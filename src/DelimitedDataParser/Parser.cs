﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DelimitedDataParser
{
    /// <summary>
    /// Implements a parser of delimited data.
    /// </summary>
    public class Parser
    {
        private const int BufferSize = 4096;
        private const char CarriageReturn = '\r';
        private const char LineFeed = '\n';
        private const char Quotes = '"';
        private ISet<string> _columnNamesAsText;
        private char _fieldSeparator = ',';
        private bool _useFirstRowAsColumnHeaders = true;
        private bool _trimColumnHeaders = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        public Parser()
        {
        }

        /// <summary>
        /// Gets or sets the character used as the field delimiter in the text file. The default value is
        /// '<c>,</c>', i.e. CSV input.
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
        /// Gets or sets a value indicating whether the first row of the text file should be treated as a header row.
        /// The default value is <c>true</c>.
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
        /// Gets or sets a value indicating whether the column headers, if present, should have whitespace trimmed before being used as a key
        /// The default value is <c>true</c>.
        /// </summary>
        public virtual bool TrimColumnHeaders
        {
            get
            {
                return _trimColumnHeaders;
            }

            set
            {
                _trimColumnHeaders = value;
            }
        }

        /// <summary>
        /// Clear all "columns as text" settings.
        /// </summary>
        /// <remarks>
        /// Calling this method clears any "columns as text" settings set via the
        /// <see cref="SetColumnsAsText(IEnumerable{string})"/> method.
        /// </remarks>
        public virtual void ClearColumnsAsText()
        {
            _columnNamesAsText = null;
        }

        /// <summary>
        /// Parse the input <see cref="TextReader"/> as a <see cref="DataTable"/>.
        /// </summary>
        /// <remarks>
        /// This method assumes an encoding of <see cref="Encoding.Default"/>.
        /// </remarks>
        /// <param name="textReader">
        /// The <see cref="TextReader"/> containing the delimited data to read.
        /// </param>
        /// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled.</param>
        /// <returns>The <see cref="DataTable"/> containing the parsed data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is null.</exception>
        public virtual DataTable Parse(TextReader textReader, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Parse(textReader, Encoding.Default, cancellationToken);
        }

        /// <summary>
        /// Parse the input <see cref="TextReader"/> as a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="textReader">
        /// The <see cref="TextReader"/> containing the delimited data to read.
        /// </param>
        /// <param name="encoding">
        /// The character encoding to use.
        /// </param>
        /// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled.</param>
        /// <returns>The <see cref="DataTable"/> containing the parsed data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> or <paramref name="encoding"/> is null.</exception>
        public virtual DataTable Parse(TextReader textReader, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (textReader == null)
            {
                throw new ArgumentNullException(nameof(textReader));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            var output = new DataTable
            {
                Locale = CultureInfo.CurrentCulture
            };

            using (var reader = ParseReader(textReader, encoding, cancellationToken))
            {
                output.Load(reader);
            }

            if (_columnNamesAsText != null && _columnNamesAsText.Any())
            {
                ResolveColumnsAsText(output);
            }

            output.AcceptChanges();

            return output;
        }

        /// <summary>
        /// Create a data reader that will read from the <paramref name="textReader"/>.
        /// </summary>
        /// <remarks>
        /// This method assumes an encoding of <see cref="Encoding.Default"/>.
        /// </remarks>
        /// <param name="textReader">
        /// The <see cref="TextReader"/> containing the delimited data to read.
        /// </param>
        /// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled.</param>
        /// <returns>A <see cref="DbDataReader"/> that will read rows of data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is null.</exception>
        public virtual DbDataReader ParseReader(TextReader textReader, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ParseReader(textReader, Encoding.Default, cancellationToken);
        }

        /// <summary>
        /// Create a data reader that will read from the <paramref name="textReader"/>.
        /// </summary>
        /// <param name="textReader">
        /// The <see cref="TextReader"/> containing the delimited data to read.
        /// </param>
        /// <param name="encoding">
        /// The character encoding to use.
        /// </param>
        /// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled.</param>
        /// <returns>A <see cref="DbDataReader"/> that will read rows of data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> or <paramref name="encoding"/> is null.</exception>
        public virtual DbDataReader ParseReader(TextReader textReader, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (textReader == null)
            {
                throw new ArgumentNullException(nameof(textReader));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return new DelimitedDataReader(
                textReader,
                encoding,
                _fieldSeparator,
                _useFirstRowAsColumnHeaders,
                _trimColumnHeaders,
                cancellationToken);
        }

        /// <summary>
        /// Create a data reader that will read from the <paramref name="streamReader"/>.
        /// </summary>
        /// <param name="streamReader">
        /// The <see cref="StreamReader"/> containing the delimited data to read. The stream encoding is used to read this data.
        /// </param>
        /// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled.</param>
        /// <returns>A <see cref="DbDataReader"/> that will read rows of data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="streamReader"/> is null.</exception>
        public virtual DbDataReader ParseReader(StreamReader streamReader, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (streamReader == null)
            {
                throw new ArgumentNullException(nameof(streamReader));
            }

            return new DelimitedDataReader(
                streamReader,
                streamReader.CurrentEncoding,
                _fieldSeparator,
                _useFirstRowAsColumnHeaders,
                _trimColumnHeaders,
                cancellationToken);
        }

        /// <summary>
        /// Specifies which column values are wrapped in quotes and preceded with an equals sign in
        /// the input.
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
        /// Parse the input <paramref name="value"/> where values may be wrapped in quotes and
        /// preceded with an equals sign.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value to be parsed.</param>
        /// <returns>The parsed <see cref="string"/>.</returns>
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

        /// <summary>
        /// Parse the row values as text for each column where values are wrapped in quotes and
        /// preceded with an equals sign.
        /// </summary>
        /// <param name="dataTable">
        /// The <see cref="DataTable"/> containing the columns for which the row values need to be
        /// parsed as text.
        /// </param>
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
