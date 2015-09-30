using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;

namespace DelimitedDataParser
{
    /// <summary>
    /// Implements an exporter of delimited data.
    /// </summary>
    public class Exporter : IDisposable
    {
        /// <summary>
        /// Represent the tab character. This field is read-only.
        /// </summary>
        public static readonly char TabSeparator = '\t';

        private readonly DataTable _dataTable;

        private bool _outputColumnHeaders = true;
        private char _fieldSeparator = ',';
        private bool _includeEscapeCharacters = true;
        private ISet<string> _columnNamesAsText;

        /// <summary>
        /// Initializes a new instance of the <see cref="Exporter"/> class with
        /// the specified <see cref="DataTable"/>.
        /// </summary>
        /// <param name="input">
        /// The <see cref="DataTable"/> containing the data to export.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="input"/> is null.
        /// </exception>
        public Exporter(DataTable input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            _dataTable = input;
        }

        /// <summary>
        /// Specifies whether an initial row containing column names should be
        /// written to the output. The default value is <c>true</c>.
        /// </summary>
        public virtual bool OutputColumnHeaders
        {
            get
            {
                return _outputColumnHeaders;
            }

            set
            {
                _outputColumnHeaders = value;
            }
        }

        /// <summary>
        /// The character used as the field delimiter in the output. The
        /// default value is <c>,</c>, i.e. CSV output.
        /// </summary>
        public virtual char FieldSeparator
        {
            get
            {
                return _fieldSeparator;
            }

            set
            {
                _fieldSeparator = value;
            }
        }

        /// <summary>
        /// Specifies whether each value should be escaped by wrapping in
        /// quotation marks. The default value is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// This must be set to <c>true</c> if <see cref="FieldSeparator"/>
        /// is a tab character.
        /// </remarks>
        public virtual bool IncludeEscapeCharacters
        {
            get
            {
                return _includeEscapeCharacters;
            }

            set
            {
                _includeEscapeCharacters = value;
            }
        }

        /// <summary>
        /// Set which columns should have their values quoted and preceded
        /// with an equals sign in the output.
        /// </summary>
        /// <param name="columnNames">
        /// The names of the columns whose values should quoted in the output.
        /// </param>
        public virtual void SetColumnsAsText(IEnumerable<string> columnNames)
        {
            ClearColumnsAsText();

            if (columnNames != null)
            {
                _columnNamesAsText = new HashSet<string>(columnNames);
            }
        }

        /// <summary>
        /// Clear all "columns as text" settings.
        /// </summary>
        /// <remarks>
        /// Calling this method clears any "columns as text" settings set via
        /// the <see cref="SetColumnsAsText(IEnumerable{string})"/> method.
        /// </remarks>
        public virtual void ClearColumnsAsText()
        {
            _columnNamesAsText = null;
        }

        [Obsolete("Exporting to string is deprecated, please use TextWriter instead.")]
        public virtual string Export()
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                this.Export(sw);
                return sw.ToString();
            }
        }

        /// <summary>
        /// Write the input <see cref="DataTable"/> to the specified
        /// <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="TextWriter"/> to be written to.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="writer"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="IncludeEscapeCharacters"/> is <c>false</c> and
        /// <see cref="FieldSeparator"/> is not a tab character.
        /// </exception>
        public virtual void Export(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            // Ensure escape characters are included unless we are exporting tab separated values
            if (!_includeEscapeCharacters && _fieldSeparator != TabSeparator)
            {
                throw new InvalidOperationException();
            }

            if (_dataTable.Columns.Count > 0)
            {
                if (_outputColumnHeaders)
                {
                    RenderHeaderRow(writer);
                }

                if (_dataTable.Rows.Count > 0)
                {
                    for (int rowIndex = 0; rowIndex < _dataTable.Rows.Count; rowIndex++)
                    {
                        var row = _dataTable.Rows[rowIndex];

                        if (rowIndex != 0 || _outputColumnHeaders)
                        {
                            writer.Write(Environment.NewLine);
                        }

                        RenderRow(writer, row);
                    }
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the
        /// <see cref="Exporter"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Exporter"/>
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dataTable != null)
                {
                    _dataTable.Dispose();
                }
            }
        }

        private void RenderHeaderRow(TextWriter writer)
        {
            for (int colIndex = 0; colIndex < _dataTable.Columns.Count; colIndex++)
            {
                var col = _dataTable.Columns[colIndex];

                if (colIndex != 0)
                {
                    writer.Write(_fieldSeparator);
                }

                writer.Write(CsvEscape(col.ColumnName, false));
            }
        }

        private void RenderRow(TextWriter writer, DataRow row)
        {
            for (int colIndex = 0; colIndex < _dataTable.Columns.Count; colIndex++)
            {
                var col = _dataTable.Columns[colIndex];

                if (colIndex != 0)
                {
                    writer.Write(_fieldSeparator);
                }

                var valueAsText = GetIsColumnAsText(col);

                var value = row[col].ToString();

                writer.Write(CsvEscape(value, valueAsText));
            }
        }

        private bool GetIsColumnAsText(DataColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            if (_columnNamesAsText == null)
            {
                return false;
            }

            return _columnNamesAsText.Contains(column.ColumnName);
        }

        private string CsvEscape(string value, bool valueAsText)
        {
            if (!_includeEscapeCharacters)
            {
                return value;
            }

            value = value.Replace(@"""", @"""""");

            if (valueAsText)
            {
                value = string.Concat(@"""=""""", value, @"""""""");
            }
            else
            {
                value = string.Concat(@"""", value, @"""");
            }

            return value;
        }
    }
}
