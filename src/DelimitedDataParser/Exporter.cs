﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;

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

        private static readonly string[] UnsafeLeadingCharacters = { "=", "+", "-", "@", "|" };

        private readonly IProgress<int> _progress;

        private ISet<string> _columnNamesAsText;
        private ISet<string> _columnNamesSanitizationPrevented;
        private IDictionary<string, string> _extendedPropertyValueLookup;

        private char _fieldSeparator = ',';
        private bool _includeEscapeCharacters = true;
        private bool _sanitizeStrings = true;
        private bool _outputColumnHeaders = true;
        private bool _useExtendedPropertyForColumnName;
        private string _extendedPropertyKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="Exporter"/> class.
        /// </summary>
        public Exporter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exporter"/> class.
        /// </summary>
        /// <param name="progress">
        /// The provider for receiving progress updates.
        /// </param>
        public Exporter(IProgress<int> progress)
        {
            _progress = progress;
        }

        /// <summary>
        /// Gets or sets the character used as the field delimiter in the output. The default value is
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
        /// Gets or sets a value indicating whether each value should be escaped by wrapping in quotation marks. The
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
        /// Gets or sets a value indicating whether strings should be sanitized, prepending blacklisted characters at
        /// the start of the string with a single quote "'". The default value is <c>false</c>.
        /// </summary>
        public virtual bool SanitizeStrings
        {
            get
            {
                return _sanitizeStrings;
            }

            set
            {
                _sanitizeStrings = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether an initial row containing column names should be written to the
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
        /// Calling this method clears any "columns as text" settings set via the
        /// <see cref="SetColumnsAsText(IEnumerable{string})"/> method.
        /// </remarks>
        public virtual void ClearColumnsAsText()
        {
            _columnNamesAsText = null;
        }

        /// <summary>
        /// Clear all "sanitization prevented" settings.
        /// </summary>
        /// <remarks>
        /// Calling this method clears any "sanitization prevented" settings set via the
        /// <see cref="SetColumnsAsSanitizationPrevented(IEnumerable{string})"/> method.
        /// </remarks>
        public virtual void ClearColumnsSanitizationPrevented()
        {
            _columnNamesSanitizationPrevented = null;
        }

        /// <summary>
        /// Clear the extended property key setting.
        /// </summary>
        /// <remarks>
        /// Calling this method clears the extended property key setting set via the
        /// the <see cref="UseExtendedPropertyForColumnName(string)"/> method.
        /// </remarks>
        public virtual void ClearExtendedPropertyForColumnName()
        {
            _useExtendedPropertyForColumnName = false;
            _extendedPropertyKey = default(string);
        }

        /// <summary>
        /// Populates column headers using the value stored on DataColumn.ExtendedProperties.
        /// </summary>
        /// <remarks>
        /// If no ExtendedProperty can be found that matches the key, the default ColumnName will be used.
        /// </remarks>
        /// <param name="key">
        /// The key that the ExtendedProperties value is stored under.
        /// </param>
        public virtual void UseExtendedPropertyForColumnName(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            _useExtendedPropertyForColumnName = true;
            _extendedPropertyKey = key;
        }

        /// <summary>
        /// Write the input <see cref="DbDataReader"/> to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="reader">The <see cref="DbDataReader"/> containing the data to export.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to be written to.</param>
        /// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="reader"/> is <c>null</c> or <paramref name="writer"/> is <c>null</c>.
        /// </exception>
        public virtual void ExportReader(DbDataReader reader, TextWriter writer, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (_outputColumnHeaders)
            {
                RenderHeaderRow(reader, writer);
            }

            if (reader.HasRows)
            {
                var rowIndex = 0;

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (rowIndex != 0 || _outputColumnHeaders)
                    {
                        writer.Write(Environment.NewLine);
                    }

                    RenderRow(reader, writer);

                    rowIndex++;

                    _progress?.Report(rowIndex);
                }
            }
        }

        /// <summary>
        /// Write the input <paramref name="dataTable"/> to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="dataTable">The <see cref="DataTable"/> containing the data to export.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to be written to.</param>
        /// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dataTable"/> is <c>null</c> or <paramref name="writer"/> is <c>null</c>.
        /// </exception>
        public virtual void Export(DataTable dataTable, TextWriter writer, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (dataTable.Columns.Count > 0)
            {
                if (_useExtendedPropertyForColumnName)
                {
                    _extendedPropertyValueLookup = new Dictionary<string, string>();

                    // If using an extended property for column names, we create
                    // a lookup of column names to extended property values.
                    // This is used when rendering the header row.
                    for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
                    {
                        var column = dataTable.Columns[colIndex];

                        if (column.ExtendedProperties.ContainsKey(_extendedPropertyKey))
                        {
                            var columnName = column.ColumnName;
                            var extendedPropertyValue = column.ExtendedProperties[_extendedPropertyKey].ToString();

                            _extendedPropertyValueLookup[columnName] = extendedPropertyValue;
                        }
                    }
                }

                using (var reader = dataTable.CreateDataReader())
                {
                    ExportReader(reader, writer, cancellationToken);
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
        /// Set which columns should not be sanitized.
        /// </summary>
        /// <param name="columnNames">
        /// The names of the columns whose values should not be sanitized.
        /// </param>
        public virtual void SetColumnsAsSanitizationPrevented(IEnumerable<string> columnNames)
        {
            ClearColumnsSanitizationPrevented();

            if (columnNames != null)
            {
                _columnNamesSanitizationPrevented = new HashSet<string>(columnNames);
            }
        }

        /// <summary>
        /// Sanitize the input <paramref name="value"/> for use in a CSV.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to be sanitized.</param>
        /// <returns>The sanitized string</returns>
        private static string Sanitize(string value)
        {
            if (value != null && UnsafeLeadingCharacters.Any(value.StartsWith))
            {
                return string.Concat("'", value);
            }

            return value;
        }

        /// <summary>
        /// Escape the input <paramref name="value"/> for use in a CSV.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to be escaped.</param>
        /// <param name="valueAsText">
        /// Whether the input <paramref name="value"/> should be treated as a text column.
        /// </param>
        /// <param name="preventSanitization">
        /// Whether the input <paramref name="value"/> should be blocked from sanitization.
        /// </param>
        /// <returns>The escaped string</returns>
        private string CsvEscape(string value, bool valueAsText, bool preventSanitization)
        {
            if (_sanitizeStrings && !preventSanitization)
            {
                value = Sanitize(value);
            }

            if (_includeEscapeCharacters)
            {
                value = value.Replace(@"""", @"""""");

                if (valueAsText)
                {
                    value = string.Concat(@"""=""""", value, @"""""""");
                }
                else
                {
                    value = string.Concat(@"""", value, @"""");
                }
            }

            return value;
        }

        /// <summary>
        /// Whether the input <paramref name="columnName"/> should be treated as a text column.
        /// </summary>
        /// <param name="columnName">The name of the column to be checked.</param>
        /// <returns>
        /// A <see cref="bool"/> specifying whether the column should be treated as a text column.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is null.</exception>
        private bool GetIsColumnAsText(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            if (_columnNamesAsText == null)
            {
                return false;
            }

            return _columnNamesAsText.Contains(columnName);
        }

        /// <summary>
        /// Whether the input <paramref name="columnName"/> should not be sanitized.
        /// </summary>
        /// <param name="columnName">The name of the column to be checked.</param>
        /// <returns>
        /// A <see cref="bool"/> specifying whether the column should not be sanitized.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is null.</exception>
        private bool GetIsColumnSanitizationPrevented(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            if (_columnNamesSanitizationPrevented == null)
            {
                return false;
            }

            return _columnNamesSanitizationPrevented.Contains(columnName);
        }

        /// <summary>
        /// Write an initial row containing the column names from the specified
        /// <see cref="DbDataReader"/> to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="reader">The <see cref="DbDataReader"/> containing the columns to be written.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to be written to.</param>
        private void RenderHeaderRow(DbDataReader reader, TextWriter writer)
        {
            var schemaTable = reader.GetSchemaTable();
            var colIndex = 0;

            foreach (DataRow row in schemaTable.Rows)
            {
                // `colIndex` is not a typo: `GetSchemaTable` returns a row per
                // column in the `DbDataReader`.
                if (colIndex != 0)
                {
                    writer.Write(_fieldSeparator);
                }

                var columnName = row[SchemaTableColumn.ColumnName].ToString();

                if (_useExtendedPropertyForColumnName &&
                    _extendedPropertyValueLookup != null)
                {
                    string extendedPropertyValue;

                    if (_extendedPropertyValueLookup.TryGetValue(columnName, out extendedPropertyValue))
                    {
                        columnName = extendedPropertyValue;
                    }
                }

                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = "Column" + (colIndex + 1);
                }

                var preventSanitization = GetIsColumnSanitizationPrevented(columnName);

                writer.Write(CsvEscape(columnName, valueAsText: false, preventSanitization: preventSanitization));

                colIndex++;
            }
        }

        /// <summary>
        /// Write the <see cref="DbDataReader"/> record to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="reader">The <see cref="DbDataReader"/> containing the data to export.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to be written to.</param>
        private void RenderRow(DbDataReader reader, TextWriter writer)
        {
            for (int colIndex = 0; colIndex < reader.FieldCount; colIndex++)
            {
                if (colIndex != 0)
                {
                    writer.Write(_fieldSeparator);
                }

                var columnName = reader.GetName(colIndex);

                var valueAsText = GetIsColumnAsText(columnName);
                var preventSanitization = GetIsColumnSanitizationPrevented(columnName);

                string valueAsString;

                if (!reader.IsDBNull(colIndex))
                {
                    var rawValue = reader.GetValue(colIndex);

                    valueAsString = rawValue != null
                        ? rawValue.ToString()
                        : string.Empty;
                }
                else
                {
                    valueAsString = string.Empty;
                }

                writer.Write(CsvEscape(valueAsString, valueAsText, preventSanitization));
            }
        }
    }
}
