﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DelimitedDataParser
{
    public class Exporter : IDisposable
    {
        public static readonly char TabSeparator = '\t';

        private readonly DataTable _dataTable;

        private bool _outputColumnHeaders = true;
        private char _fieldSeparator = ',';
        private bool _includeEscapeCharacters = true;
        private ISet<string> _columnNamesAsText;

        public Exporter(DataTable input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            _dataTable = input;
        }

        public bool OutputColumnHeaders
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

        public char FieldSeparator
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

        public bool IncludeEscapeCharacters
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

        public void SetColumnsAsText(IEnumerable<string> columnNames)
        {
            ClearColumnsAsText();

            if (columnNames != null)
            {
                _columnNamesAsText = new HashSet<string>(columnNames);
            }
        }

        public void ClearColumnsAsText()
        {
            _columnNamesAsText = null;
        }

        public string Export()
        {
            // Ensure escape characters are included unless we are exporting tab separated values
            if (!_includeEscapeCharacters && _fieldSeparator != TabSeparator)
            {
                throw new InvalidOperationException();
            }

            var output = new StringBuilder();

            if (_dataTable.Columns.Count > 0)
            {
                if (_outputColumnHeaders)
                {
                    RenderHeaderRow(output);
                }

                if (_dataTable.Rows.Count > 0)
                {
                    for (int rowIndex = 0; rowIndex < _dataTable.Rows.Count; rowIndex++)
                    {
                        var row = _dataTable.Rows[rowIndex];

                        if (rowIndex != 0 || _outputColumnHeaders)
                        {
                            output.Append(Environment.NewLine);
                        }

                        RenderRow(output, row);
                    }
                }
            }

            return output.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        private void RenderHeaderRow(StringBuilder output)
        {
            for (int colIndex = 0; colIndex < _dataTable.Columns.Count; colIndex++)
            {
                var col = _dataTable.Columns[colIndex];

                if (colIndex != 0)
                {
                    output.Append(_fieldSeparator);
                }

                output.Append(CsvEscape(col.ColumnName, false));
            }
        }

        private void RenderRow(StringBuilder output, DataRow row)
        {
            for (int colIndex = 0; colIndex < _dataTable.Columns.Count; colIndex++)
            {
                var col = _dataTable.Columns[colIndex];

                if (colIndex != 0)
                {
                    output.Append(_fieldSeparator);
                }

                var valueAsText = GetIsColumnAsText(col);

                var value = row[col].ToString();

                output.Append(CsvEscape(value, valueAsText));
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