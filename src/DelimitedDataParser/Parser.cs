using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DelimitedDataParser
{
    /// <summary>
    /// Implements a parser of delimited data.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class with
        /// the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="input">
        /// The <see cref="TextReader"/> containing the delimited data to read.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="input"/> is null.
        /// </exception>
        public Parser(TextReader input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            _textReader = input;
        }

        /// <summary>
        /// Specifies whether the first row of the text file should be treated
        /// as a header row. The default value is <c>true</c>.
        /// </summary>
        public virtual bool UseFirstRowAsColumnHeaders
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

        /// <summary>
        /// The character used as the field delimiter in the text file. The
        /// default value is <c>,</c>, i.e. CSV input.
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
        /// Specifies which column values are wrapped in quotes and preceded
        /// with an equals sign in the input.
        /// </summary>
        /// <param name="columnNames">
        /// The names of the columns whose values are quoted in the input.
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

        /// <summary>
        /// Parse the input <see cref="TextReader"/> as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> containing the parsed data.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public virtual DataTable Parse()
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

        /// <summary>
        /// Releases all resources used by the current instance of the
        /// <see cref="Parser"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Parser"/>
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
