using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Text;

namespace DelimitedDataParser
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    internal class DelimitedDataReader : DbDataReader
    {
        private const char CarriageReturn = '\r';
        private const char LineFeed = '\n';
        private const char Quote = '"';

        private readonly TextReader _textReader;
        private readonly char _fieldSeparator;
        private readonly bool _useFirstRowAsColumnHeaders;
        private readonly char[] _buffer = new char[4096];

        private IReadOnlyList<string> _fieldNameLookup = null;
        private IReadOnlyList<string> _currentRow = null;

        private bool _isClosed = false;
        private bool _firstRowRead = false;
        private bool _yieldExistingRow = false;
        private int _charsInBuffer;
        private int _bufferIndex;

        public DelimitedDataReader(TextReader textReader, char fieldSeparator, bool useFirstRowAsColumnHeaders)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            _textReader = textReader;
            _fieldSeparator = fieldSeparator;
            _useFirstRowAsColumnHeaders = useFirstRowAsColumnHeaders;
        }

        public override int Depth
        {
            get
            {
                return 0;
            }
        }

        public override int FieldCount
        {
            get
            {
                EnsureInitialised();

                // If we are not on a valid row, return -1.
                if (_currentRow == null)
                {
                    return -1;
                }

                // Otherwise, return the column count for the current record.
                return _currentRow.Count;
            }
        }

        public override bool HasRows
        {
            get
            {
                EnsureInitialised();

                return _currentRow != null;
            }
        }

        public override bool IsClosed
        {
            get
            {
                return _isClosed;
            }
        }

        public override int RecordsAffected
        {
            get
            {
                return -1;
            }
        }

        public override object this[string name]
        {
            get
            {
                return _currentRow[GetOrdinal(name)];
            }
        }

        public override object this[int ordinal]
        {
            get
            {
                return _currentRow[ordinal];
            }
        }

        public override void Close()
        {
            _isClosed = true;
        }

        public override bool GetBoolean(int ordinal)
        {
            return bool.Parse(_currentRow[ordinal]);
        }

        public override byte GetByte(int ordinal)
        {
            return byte.Parse(_currentRow[ordinal], CultureInfo.InvariantCulture);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            // TODO See https://github.com/Microsoft/referencesource/blob/e458f8df6ded689323d4bd1a2a725ad32668aaec/System.Data/System/Data/Common/DataRecordInternal.cs#L108
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            var str = _currentRow[ordinal];

            if (str.Length != 1)
            {
                throw new InvalidCastException();
            }

            return str[0];
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            // TODO See https://github.com/Microsoft/referencesource/blob/e458f8df6ded689323d4bd1a2a725ad32668aaec/System.Data/System/Data/Common/DataRecordInternal.cs#L176
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            if (ordinal < 0 || ordinal > _currentRow.Count - 1)
            {
                throw new ArgumentOutOfRangeException("ordinal");
            }

            return typeof(string).Name;
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return DateTime.Parse(_currentRow[ordinal], CultureInfo.InvariantCulture);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return decimal.Parse(_currentRow[ordinal], CultureInfo.InvariantCulture);
        }

        public override double GetDouble(int ordinal)
        {
            return double.Parse(_currentRow[ordinal], CultureInfo.InvariantCulture);
        }

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this);
        }

        public override Type GetFieldType(int ordinal)
        {
            if (ordinal < 0 || ordinal > _currentRow.Count - 1)
            {
                throw new ArgumentOutOfRangeException("ordinal");
            }

            return typeof(string);
        }

        public override float GetFloat(int ordinal)
        {
            return float.Parse(_currentRow[ordinal], CultureInfo.InvariantCulture);
        }

        public override Guid GetGuid(int ordinal)
        {
            Guid value;

            if (!Guid.TryParse(_currentRow[ordinal], out value))
            {
                throw new InvalidCastException();
            }

            return value;
        }

        public override short GetInt16(int ordinal)
        {
            short value;

            if (!short.TryParse(_currentRow[ordinal], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                throw new InvalidCastException();
            }

             return value;
        }

        public override int GetInt32(int ordinal)
        {
            int value;

            if (!int.TryParse(_currentRow[ordinal], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                throw new InvalidCastException();
            }

            return value;
        }

        public override long GetInt64(int ordinal)
        {
            long value;

            if (!long.TryParse(_currentRow[ordinal], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                throw new InvalidCastException();
            }

            return value;
        }

        public override string GetName(int ordinal)
        {
            return _fieldNameLookup[ordinal];
        }

        public override int GetOrdinal(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            for (int i = 0; i < _fieldNameLookup.Count; i++)
            {
                if (string.Equals(name, _fieldNameLookup[i], StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            throw new ArgumentOutOfRangeException("name");
        }

        public override DataTable GetSchemaTable()
        {
            if (_isClosed)
            {
                throw new InvalidOperationException();
            }

            EnsureInitialised();

            var schemaTable = new DataTable("SchemaTable")
            {
                Locale = CultureInfo.InvariantCulture
            };

            var allowDBNull = new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool));
            var baseColumnName = new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string));
            var baseSchemaName = new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string));
            var baseTableName = new DataColumn(SchemaTableColumn.BaseTableName, typeof(string));
            var columnName = new DataColumn(SchemaTableColumn.ColumnName, typeof(string));
            var columnOrdinal = new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int));
            var columnSize = new DataColumn(SchemaTableColumn.ColumnSize, typeof(int));
            var dataType = new DataColumn(SchemaTableColumn.DataType, typeof(Type));
            var isAliased = new DataColumn(SchemaTableColumn.IsAliased, typeof(bool));
            var isExpression = new DataColumn(SchemaTableColumn.IsExpression, typeof(bool));
            var isKey = new DataColumn(SchemaTableColumn.IsKey, typeof(bool));
            var isLong = new DataColumn(SchemaTableColumn.IsLong, typeof(bool));
            var isUnique = new DataColumn(SchemaTableColumn.IsUnique, typeof(bool));
            var nonVersionedProviderType = new DataColumn(SchemaTableColumn.NonVersionedProviderType, typeof(int));
            var numericPrecision = new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short));
            var numericScale = new DataColumn(SchemaTableColumn.NumericScale, typeof(short));
            var providerType = new DataColumn(SchemaTableColumn.ProviderType, typeof(int));

            columnOrdinal.DefaultValue = 0;
            isLong.DefaultValue = false;

            var columns = schemaTable.Columns;

            columns.Add(columnName);
            columns.Add(columnOrdinal);
            columns.Add(columnSize);
            columns.Add(numericPrecision);
            columns.Add(numericScale);
            columns.Add(isUnique);
            columns.Add(isKey);
            columns.Add(baseColumnName);
            columns.Add(baseSchemaName);
            columns.Add(baseTableName);
            columns.Add(dataType);
            columns.Add(allowDBNull);
            columns.Add(providerType);
            columns.Add(isAliased);
            columns.Add(isExpression);
            columns.Add(isLong);
            columns.Add(nonVersionedProviderType);

            for (int i = 0; i < _fieldNameLookup.Count; i++)
            {
                var schemaRow = schemaTable.NewRow();

                schemaRow[allowDBNull] = true;
                schemaRow[baseColumnName] = _fieldNameLookup[i];
                schemaRow[columnName] = _fieldNameLookup[i];
                schemaRow[columnOrdinal] = i;
                schemaRow[columnSize] = int.MaxValue;
                schemaRow[dataType] = typeof(string);
                schemaRow[isAliased] = false;
                schemaRow[isExpression] = false;
                schemaRow[isKey] = false;
                schemaRow[isLong] = false;
                schemaRow[isUnique] = false;
                schemaRow[nonVersionedProviderType] = DbType.String;
                schemaRow[numericScale] = byte.MaxValue;
                schemaRow[numericPrecision] = byte.MaxValue;
                schemaRow[providerType] = DbType.String;

                schemaTable.Rows.Add(schemaRow);

                schemaRow.AcceptChanges();
            }

            // Mark all columns as read-only.
            foreach (DataColumn column in columns)
            {
                column.ReadOnly = true;
            }

            return schemaTable;
        }

        public override string GetString(int ordinal)
        {
            return _currentRow[ordinal];
        }

        public override object GetValue(int ordinal)
        {
            return _currentRow[ordinal];
        }

        public override int GetValues(object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = i < _currentRow.Count ? _currentRow[i] : null;
            }

            return Math.Min(values.Length, _currentRow.Count);
        }

        public override bool IsDBNull(int ordinal)
        {
            return string.IsNullOrEmpty(_currentRow[ordinal]);
        }

        public override bool NextResult()
        {
            return false;
        }

        public override bool Read()
        {
            EnsureInitialised();

            if (_yieldExistingRow)
            {
                _yieldExistingRow = false;

                return _currentRow != null;
            }

            return ReadInternal();
        }

        /// <summary>
        /// Handle quote characters that have been encountered whilst processing the character string.
        /// </summary>
        /// <param name="field">The <see cref="StringBuilder"/> to be used.</param>
        /// <param name="quoteCount">How many repeated quote characters have been read.</param>
        /// <param name="quotedMode">
        /// A <see cref="Boolean"/> to identify whether the current operation is within a quoted string.
        /// </param>
        /// <param name="quotedModeHasPassed">
        /// A <see cref="Boolean"/> specifying whether the current operation has finished parsing a
        /// quoted value or has just left 'Quoted Mode'.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="field"/> is null.</exception>
        private static void HandleQuotes(StringBuilder field, int quoteCount, ref bool quotedMode, ref bool quotedModeHasPassed)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            if (quotedModeHasPassed)
            {
                field.Append(Quote, quoteCount);
            }
            else
            {
                var escapedQuoteCount = quoteCount / 2;
                var oddQuotes = quoteCount % 2 > 0;

                if (quotedMode)
                {
                    if (oddQuotes)
                    {
                        quotedMode = false;
                        quotedModeHasPassed = true;
                    }
                }
                else
                {
                    if (oddQuotes)
                    {
                        quotedMode = true;
                    }
                    else
                    {
                        escapedQuoteCount--;
                    }
                }

                field.Append(Quote, escapedQuoteCount);
            }
        }

        private void EnsureInitialised()
        {
            if (!_firstRowRead)
            {
                if (ReadInternal())
                {
                    if (_useFirstRowAsColumnHeaders)
                    {
                        GenerateFieldLookup();
                    }
                    else
                    {
                        GenerateDefaultFieldNameLookup();
                    }
                }
                else
                {
                    _fieldNameLookup = new List<string>(0).AsReadOnly();
                }

                _firstRowRead = true;
                _yieldExistingRow = !_useFirstRowAsColumnHeaders;
            }
        }

        private void GenerateFieldLookup()
        {
            // Here we assume that the current row is the header row.
            _fieldNameLookup = new List<string>(_currentRow).AsReadOnly();
        }

        private void GenerateDefaultFieldNameLookup()
        {
            var defaultColumnHeaders = new List<string>(_currentRow.Count);

            for (int i = 0; i < _currentRow.Count; i++)
            {
                defaultColumnHeaders.Add(string.Concat("Column", i + 1));
            }

            _fieldNameLookup = defaultColumnHeaders.AsReadOnly();
        }

        private bool ReadInternal()
        {
            var quotedMode = false;
            var quotedModeHasPassed = false;
            var newLineCharacterSequenceCount = 0;
            var quoteCount = 0;
            var currentCell = new StringBuilder();
            var row = new List<string>(_currentRow != null ? _currentRow.Count : 4);
            char c;
            var readAnyChar = false;

            while (ReadNextChar(out c))
            {
                if (newLineCharacterSequenceCount > 0)
                {
                    if (newLineCharacterSequenceCount == 1 && (c == CarriageReturn || c == LineFeed))
                    {
                        newLineCharacterSequenceCount++;
                        continue;
                    }

                    if (currentCell.Length != 0 || quoteCount > 0 || row.Count > 0)
                    {
                        row.Add(currentCell.ToString());
                    }

                    currentCell.Clear();
                    
                    _bufferIndex--;

                    _currentRow = row.AsReadOnly();
                    return true;
                }

                readAnyChar = true;

                if (c == Quote)
                {
                    quoteCount++;
                    continue;
                }

                if (quoteCount > 0)
                {
                    HandleQuotes(currentCell, quoteCount, ref quotedMode, ref quotedModeHasPassed);
                }

                quoteCount = 0;

                if (c == _fieldSeparator && !quotedMode)
                {
                    // Handle Field Separator when not in quoted mode - End cell
                    row.Add(currentCell.ToString());
                    currentCell.Clear();

                    quotedModeHasPassed = false;
                }
                else if ((c == CarriageReturn || c == LineFeed) && !quotedMode)
                {
                    // Handle new line when not in quoted mode - Start collecting new line char sequence
                    newLineCharacterSequenceCount++;
                }
                else
                {
                    // Cell content
                    currentCell.Append(c);

                    if (!quotedMode)
                    {
                        quotedModeHasPassed = true;
                    }
                }
            }

            if (!readAnyChar)
            {
                _currentRow = null;
                return false;
            }

            if (quoteCount > 0)
            {
                // Tidy up any quotes at end of last cell
                HandleQuotes(currentCell, quoteCount, ref quotedMode, ref quotedModeHasPassed);
            }

            // Add current cell contents to final row.
            row.Add(currentCell.ToString());

            _currentRow = row.AsReadOnly();
            return true;
        }

        private bool ReadNextChar(out char c)
        {
            _bufferIndex++;

            if (_charsInBuffer == 0 || _bufferIndex > _charsInBuffer - 1)
            {
                if (_textReader.Peek() == -1)
                {
                    c = default(char);
                    return false;
                }

                _charsInBuffer = _textReader.Read(_buffer, 0, _buffer.Length);

                if (_charsInBuffer == 0)
                {
                    c = default(char);
                    return false;
                }

                _bufferIndex = 0;
            }

            c = _buffer[_bufferIndex];
            return true;
        }
    }
}
