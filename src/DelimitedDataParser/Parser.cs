using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        public Parser()
        {
        }

        /// <summary>
        /// The character used as the field delimiter in the text file. The default value is
        /// <c>,</c>, i.e. CSV input.
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
        /// Specifies whether the first row of the text file should be treated as a header row. The
        /// default value is <c>true</c>.
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
        /// Parse the input <paramref name="TextReader"/> as a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="textReader">
        /// The <see cref="TextReader"/> containing the delimited data to read.
        /// </param>
        /// <returns>The <see cref="DataTable"/> containing the parsed data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public virtual DataTable Parse(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            var output = new DataTable
            {
                Locale = CultureInfo.CurrentCulture
            };

            var reader = ParseReader(textReader);
            output.Load(reader);

            if (_columnNamesAsText != null && _columnNamesAsText.Any())
            {
                ResolveColumnsAsText(output);
            }

            output.AcceptChanges();

            return output;
        }

        /// <summary>
        /// Create a data reader that will read from the <paramref name="TextReader"/>.
        /// </summary>
        /// <param name="textReader">
        /// The <see cref="TextReader"/> containing the delimited data to read.
        /// </param>
        /// <returns>A <see cref="DbDataReader"/> that will read rows of data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is null.</exception>
        public virtual DbDataReader ParseReader(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            return new DelimitedDataReader(textReader, _fieldSeparator, _useFirstRowAsColumnHeaders);
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
        /// Parse the input <paramref name="String"/> where values may be wrapped in quotes and
        /// preceded with an equals sign.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to be parsed.</param>
        /// <returns>The parsed <see cref="String"/>.</returns>
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
