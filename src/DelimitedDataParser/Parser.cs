using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DelimitedDataParser
{
    public class Parser : IDisposable
    {
        private const char CarriageReturn = '\r';
        private const char LineFeed = '\n';
        private const char Quotes = '"';
        private const int BufferSize = 4096;

        private readonly TextReader _textReader;

        private bool _useFirstRowAsColumnHeaders = true;
        private char _fieldSeparator = ',';
        private ISet<string> _columnNamesAsText;

        public Parser(TextReader input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            _textReader = input;
        }

        public bool UseFirstRowAsColumnHeaders
        {
            get
            {
                return _useFirstRowAsColumnHeaders;
            }

            set
            {
                _useFirstRowAsColumnHeaders = value;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public DataTable Parse()
        {
            if (_textReader.Peek() == -1)
            {
                return new DataTable
                {
                    Locale = CultureInfo.CurrentCulture
                };
            }

            var table = new Table
            {
                UseFirstRowAsColumnHeaders = _useFirstRowAsColumnHeaders
            };

            var quotedModeHasPassed = false;
            var quotedMode = false;
            var newLineCharacterSequenceCount = 0;
            var quoteCount = 0;

            var buffer = new char[BufferSize];
            char c;
            int charCount;

            while ((charCount = _textReader.Read(buffer, 0, BufferSize)) > 0)
            {
                for (int i = 0; i < charCount; i++)
                {
                    c = buffer[i];

                    if (newLineCharacterSequenceCount > 0)
                    {
                        if (newLineCharacterSequenceCount == 1 && (c == CarriageReturn || c == LineFeed))
                        {
                            newLineCharacterSequenceCount++;
                            continue;
                        }
                        else
                        {
                            table.FlushCell();
                            table.FlushRow();
                            quotedModeHasPassed = false;
                            newLineCharacterSequenceCount = 0;
                        }
                    }

                    if (c == Quotes)
                    {
                        quoteCount++;
                        continue;
                    }

                    if (quoteCount > 0)
                    {
                        HandleQuotes(table, quoteCount, ref quotedMode, ref quotedModeHasPassed);
                    }

                    quoteCount = 0;

                    if (c == _fieldSeparator && !quotedMode)
                    {
                        // Handle Field Separator when not in quoted mode - End cell
                        table.FlushCell();
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
                        table.AddToCurrentCell(c);

                        if (!quotedMode)
                        {
                            quotedModeHasPassed = true;
                        }
                    }
                }
            }

            if (quoteCount > 0)
            {
                // Tidy up any quotes at end of last cell
                HandleQuotes(table, quoteCount, ref quotedMode, ref quotedModeHasPassed);
            }

            var output = table.ToDataTable();

            if (_columnNamesAsText != null && _columnNamesAsText.Any())
            {
                ResolveColumnsAsText(output);
            }

            output.AcceptChanges();

            return output;
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
                if (_textReader != null)
                {
                    _textReader.Dispose();
                }
            }
        }

        private static void HandleQuotes(Table table, int quoteCount, ref bool quotedMode, ref bool quotedModeHasPassed)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            if (quotedModeHasPassed)
            {
                table.AddToCurrentCell('\"', quoteCount);
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

                table.AddToCurrentCell('\"', escapedQuoteCount);
            }
        }

        private static string ParseValueAsText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.Length < 3 || !value.StartsWith("=\"", StringComparison.Ordinal) || !value.EndsWith("\"", StringComparison.Ordinal))
            {
                return value;
            }

            return value.Substring(2, value.Length - 3);
        }

        private void ResolveColumnsAsText(DataTable dataTable)
        {
            foreach (DataColumn column in dataTable.Columns)
            {
                if (_columnNamesAsText.Contains(column.ColumnName))
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        row[column] = ParseValueAsText(row[column].ToString());
                    }
                }
            }
        }
    }
}
