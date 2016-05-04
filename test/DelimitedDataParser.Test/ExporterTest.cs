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

            Assert.Empty(output);
        }

        [Fact]
        public void Can_Parse_Multiple_Rows()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Data 1", "Data 2");
            AddRow(input, "Data 3", "Data 4");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Data 1"",""Data 2""" + Environment.NewLine
                + @"""Data 3"",""Data 4""",
                output);
        }

        [Fact]
        public void Can_Parse_Null_Fields()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, null, "Data 1");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @""""",""Data 1""",
                output);
        }

        [Fact]
        public void Does_Not_Strip_Whitespace_From_Column_Names()
        {
            var input = CreateDataTable();
            AddColumn(input, " Field 1");
            AddColumn(input, "Field 2 ");
            AddColumn(input, " Field 3 ");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@""" Field 1"",""Field 2 "","" Field 3 """, output);
        }

        [Fact]
        public void Fails_With_Invalid_Settings()
        {
            var exporter = new Exporter();
            exporter.IncludeEscapeCharacters = false;

            Assert.Throws<InvalidOperationException>(() => exporter.ExportToString(CreateDataTable()));
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
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");
            AddColumn(input, "Field 3");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""Field 1"",""Field 2"",""Field 3""", output);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Colon()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Data 1", "Data 2");
            AddRow(input, "Data 3", "Data 4");

            var exporter = new Exporter();
            exporter.FieldSeparator = ':';
            var output = exporter.ExportToString(input);

            Assert.Equal(
                "\"Field 1\":\"Field 2\"" + Environment.NewLine
                + "\"Data 1\":\"Data 2\"" + Environment.NewLine
                + "\"Data 3\":\"Data 4\"",
                output);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Pipe()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Data 1", "Data 2");
            AddRow(input, "Data 3", "Data 4");

            var exporter = new Exporter();
            exporter.FieldSeparator = '|';
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1""|""Field 2""" + Environment.NewLine
                + @"""Data 1""|""Data 2""" + Environment.NewLine
                + @"""Data 3""|""Data 4""",
                output);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Space()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Data 1", "Data 2");
            AddRow(input, "Data 3", "Data 4");

            var exporter = new Exporter();
            exporter.FieldSeparator = ' ';
            var output = exporter.ExportToString(input);

            Assert.Equal(
                "\"Field 1\" \"Field 2\"" + Environment.NewLine
                + "\"Data 1\" \"Data 2\"" + Environment.NewLine
                + "\"Data 3\" \"Data 4\"",
                output);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Tab()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Data 1", "Data 2");
            AddRow(input, "Data 3", "Data 4");

            var exporter = new Exporter();
            exporter.FieldSeparator = '\t';
            var output = exporter.ExportToString(input);

            Assert.Equal(
                "\"Field 1\"\t\"Field 2\"" + Environment.NewLine
                + "\"Data 1\"\t\"Data 2\"" + Environment.NewLine
                + "\"Data 3\"\t\"Data 4\"",
                output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Carriage_Return()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Fie\rld 2");
            AddColumn(input, "Field 3");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""Field 1"",""Fie" + "\r" + @"ld 2"",""Field 3""", output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Comma()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, @"Fie,ld 2");
            AddColumn(input, "Field 3");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""Field 1"",""Fie,ld 2"",""Field 3""", output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Full_New_Line()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Fie" + Environment.NewLine + "ld 2");
            AddColumn(input, "Field 3");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""Field 1"",""Fie" + Environment.NewLine + @"ld 2"",""Field 3""", output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Line_Feed()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Fie\nld 2");
            AddColumn(input, "Field 3");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""Field 1"",""Fie" + "\n" + @"ld 2"",""Field 3""", output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, @"Fie""ld 2");
            AddColumn(input, "Field 3");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""Field 1"",""Fie""""ld 2"",""Field 3""", output);
        }

        [Fact]
        public void Supports_Column_Name_Containing_Reversed_New_Line()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Fie\n\rld 2");
            AddColumn(input, "Field 3");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""Field 1"",""Fie" + "\n\r" + @"ld 2"",""Field 3""", output);
        }

        [Fact]
        public void Supports_Data_Containing_Full_New_Line()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Dat" + Environment.NewLine + "a 1", "Data 2");
            AddRow(input, "Data 2", "Data 3");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Dat" + Environment.NewLine + @"a 1"",""Data 2""" + Environment.NewLine
                + @"""Data 2"",""Data 3""",
                output);
        }

        [Fact]
        public void Supports_Data_Containing_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, @"Data""1", "Data 2");
            AddRow(input, "Data 3", "Data 4");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Data""""1"",""Data 2""" + Environment.NewLine
                + @"""Data 3"",""Data 4""",
                output);
        }

        [Fact]
        public void Supports_Data_Ending_With_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Data 1", "Data 2");
            AddRow(input, "Data 3", @"Data 4""");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Data 1"",""Data 2""" + Environment.NewLine
                + @"""Data 3"",""Data 4""""""",
                output);
        }

        [Fact]
        public void Supports_Empty_Column_Name()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, string.Empty);
            AddColumn(input, "Field 3");

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""Field 1"",""Column1"",""Field 3""", output);
        }

        [Fact]
        public void Supports_Empty_Last_Column_Name()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");
            AddColumn(input, string.Empty);

            var exporter = new Exporter();
            var output = exporter.ExportToString(input);

            Assert.Equal(@"""Field 1"",""Field 2"",""Column1""", output);
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
