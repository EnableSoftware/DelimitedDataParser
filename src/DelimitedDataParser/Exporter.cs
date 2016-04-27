using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace DelimitedDataParser
{
    /// <summary>
    /// Implements an exporter of delimited data.
    /// </summary>
    public class Exporter
    {
        /// <summary>
        /// Represents the tab character. This field is read-only.
        /// </summary>
        public static readonly char TabSeparator = '\t';

        private ISet<string> _columnNamesAsText;
        private char _fieldSeparator = ',';
        private bool _includeEscapeCharacters = true;
        private bool _outputColumnHeaders = true;
        private bool _useExtendedPropertyForColumnName = false;
        private string _extendedPropertyKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="Exporter"/> class.
        /// </summary>
        public Exporter()
        {
        }

        /// <summary>
        /// The character used as the field delimiter in the output. The default value is <c>,</c>,
        /// i.e. CSV output.
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
        /// Specifies whether each value should be escaped by wrapping in quotation marks. The
        /// default value is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// This must be set to <c>true</c> if <see cref="FieldSeparator"/> is a tab character.
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
        /// Specifies whether an initial row containing column names should be written to the
        /// output. The default value is <c>true</c>.
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
        /// Clear all "columns as text" settings.
        /// </summary>
        /// <remarks>
        /// Calling this method clears any "columns as text" settings set via the <see
        /// cref="SetColumnsAsText(IEnumerable{string})"/> method.
        /// </remarks>
        public virtual void ClearColumnsAsText()
        {
            _columnNamesAsText = null;
        }

        /// <summary>
        /// Populates column headers using the value stored on DataColumn.ExtendedProperties
        /// </summary>
        /// <remarks>
        /// If no ExtendedProperty can be found that matches the key, the default ColumnName will be used.
        /// </remarks>
        /// <param name="key">
        /// The key that the ExtendedProperties value is stored under
        /// </param>
        public virtual void UseExtendedPropertiesKeyForColumnName(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            _useExtendedPropertyForColumnName = true;
            _extendedPropertyKey = key;
        }

        /// <summary>
        /// Write the input <paramref name="DataTable"/> to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="dataTable">The <see cref="DataTable"/> containing the data to export.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to be written to.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dataTable"/> is <c>null</c> or <paramref name="writer"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="IncludeEscapeCharacters"/> is <c>false</c> and <see cref="FieldSeparator"/>
        /// is not a tab character.
        /// </exception>
        public virtual void Export(DataTable dataTable, TextWriter writer)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }

            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            // Ensure escape characters are included unless we are exporting tab separated values
            if (!_includeEscapeCharacters && _fieldSeparator != TabSeparator)
            {
                throw new InvalidOperationException();
            }

            if (dataTable.Columns.Count > 0)
            {
                if (_outputColumnHeaders)
                {
                    RenderHeaderRow(dataTable, writer);
                }

                if (dataTable.Rows.Count > 0)
                {
                    for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                    {
                        var row = dataTable.Rows[rowIndex];

                        if (rowIndex != 0 || _outputColumnHeaders)
                        {
                            writer.Write(Environment.NewLine);
                        }

                        RenderRow(dataTable, row, writer);
                    }
                }
            }
        }

        /// <summary>
        /// Set which columns should have their values quoted and preceded with an equals sign in
        /// the output.
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
        /// Escape the input <paramref name="String"/> for use in a CSV.
        /// </summary>
        /// <param name="value">The <see cref="String"/> to be escaped.</param>
        /// <param name="valueAsText">
        /// Whether the input <paramref name="String"/> should be treated as a text column.
        /// </param>
        /// <returns>The escaped string</returns>
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

        /// <summary>
        /// Whether the input <paramref name="DataColumn"/> should be treated as a text column.
        /// </summary>
        /// <param name="column">The <see cref="DataColumn"/> to be checked.</param>
        /// <returns>
        /// A <see cref="Boolean"/> specifying whether the column should be treated as a text column.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is null.</exception>
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

        /// <summary>
        /// Write an initial row containing the column names from <paramref name="DataTable"/> to
        /// the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="dataTable">The <see cref="DataTable"/> containing the columns to be written.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to be written to.</param>
        private void RenderHeaderRow(DataTable dataTable, TextWriter writer)
        {
            for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
            {
                var col = dataTable.Columns[colIndex];

                if (colIndex != 0)
                {
                    writer.Write(_fieldSeparator);
                }

                var columnName = col.ColumnName;

                if (_useExtendedPropertyForColumnName && col.ExtendedProperties.ContainsKey(_extendedPropertyKey))
                {
                    columnName = col.ExtendedProperties[_extendedPropertyKey].ToString();
                }

                writer.Write(CsvEscape(columnName, false));
            }
        }

        /// <summary>
        /// Write the <paramref name="DataRow"/> from the <paramref name="DataTable"/> to the
        /// specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="dataTable">The <see cref="DataTable"/> containing the columns to export.</param>
        /// <param name="row">The <see cref="DataRow"/> containing the data to export</param>
        /// <param name="writer">The <see cref="TextWriter"/> to be written to.</param>
        private void RenderRow(DataTable dataTable, DataRow row, TextWriter writer)
        {
            for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
            {
                var col = dataTable.Columns[colIndex];

                if (colIndex != 0)
                {
                    writer.Write(_fieldSeparator);
                }

                var valueAsText = GetIsColumnAsText(col);

                var value = row[col].ToString();

                writer.Write(CsvEscape(value, valueAsText));
            }
        }
    }
}
