using System;
using System.Data;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DelimitedDataParser
{
    [TestClass]
    public partial class ExporterTest
    {
        private const string CharPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
        private readonly Random _random = new Random();

        [TestMethod]
        public void Can_Load_Input()
        {
            var input = CreateDataTable();
            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.IsNotNull(output);
        }

        [TestMethod]
        public void Can_Parse_Empty_Table()
        {
            var exporter = new Exporter(CreateDataTable());

            var output = exporter.ExportToString();

            Assert.AreEqual(0, output.Length, "Should have zero length.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Fails_Without_Valid_Input()
        {
            using (var writer = new StringWriter())
            {
                new Exporter(null).Export(writer);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Fails_With_Invalid_Settings()
        {
            var exporter = new Exporter(CreateDataTable());
            exporter.IncludeEscapeCharacters = false;
            var output = exporter.ExportToString();
        }

        [TestMethod]
        public void Outputs_Column_Names()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");
            AddColumn(input, "Three");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""Two"",""Three""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Supports_Quoted_Column_Name()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, @"""Two""");
            AddColumn(input, "Three");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""""""Two"""""",""Three""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Supports_Column_Name_Containing_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, @"Tw""o");
            AddColumn(input, "Three");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""Tw""""o"",""Three""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Supports_Column_Name_Containing_Comma()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, @"Tw,o");
            AddColumn(input, "Three");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""Tw,o"",""Three""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Supports_Column_Name_Containing_Full_New_Line()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Tw" + Environment.NewLine + "o");
            AddColumn(input, "Three");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""Tw" + Environment.NewLine + @"o"",""Three""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Supports_Column_Name_Containing_Reversed_New_Line()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Tw\n\ro");
            AddColumn(input, "Three");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""Tw" + "\n\r" + @"o"",""Three""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Supports_Column_Name_Containing_Carriage_Return()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Tw\ro");
            AddColumn(input, "Three");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""Tw" + "\r" + @"o"",""Three""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Supports_Column_Name_Containing_Line_Feed()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Tw\no");
            AddColumn(input, "Three");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""Tw" + "\n" + @"o"",""Three""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Does_Not_Strip_Whitespace_From_Column_Names()
        {
            var input = CreateDataTable();
            AddColumn(input, " One");
            AddColumn(input, "Two ");
            AddColumn(input, " Three ");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@""" One"",""Two "","" Three """, output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Supports_Null_Column_Name()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, null);
            AddColumn(input, "Three");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""Column1"",""Three""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Supports_Empty_Column_Name()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, string.Empty);
            AddColumn(input, "Three");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""Column1"",""Three""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Supports_Empty_Last_Column_Name()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");
            AddColumn(input, string.Empty);

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(@"""One"",""Two"",""Column1""", output, "Expected output to start with column names.");
        }

        [TestMethod]
        public void Can_Parse_Empty_Fields()
        {
            var input = CreateDataTable();
            AddColumn(input, string.Empty);
            AddColumn(input, string.Empty);

            AddRow(input, string.Empty, string.Empty);

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""Column1"",""Column2""" + Environment.NewLine
                + @""""",""""",
                output);
        }

        [TestMethod]
        public void Can_Parse_Null_Fields()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, null, "Two");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""One"",""Two""" + Environment.NewLine
                + @""""",""Two""",
                output);
        }

        [TestMethod]
        public void Can_Parse_Multiple_Rows()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Three"",""Four""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [TestMethod]
        public void Supports_Quoted_Data()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, @"""Three""", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""One"",""Two""" + Environment.NewLine
                + @"""""""Three"""""",""Four""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [TestMethod]
        public void Supports_Data_Containing_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, @"Thr""ee", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Thr""""ee"",""Four""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [TestMethod]
        public void Supports_Data_Ending_With_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", @"Six""");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Three"",""Four""" + Environment.NewLine
                + @"""Five"",""Six""""""",
                output);
        }

        [TestMethod]
        public void Supports_Data_Containing_Full_New_Line()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Thr" + Environment.NewLine + "ee", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Thr" + Environment.NewLine + @"ee"",""Four""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [TestMethod]
        public void Supports_Multiple_Blank_Rows()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, string.Empty, string.Empty);
            AddRow(input, string.Empty, string.Empty);
            AddRow(input, "Five", "Six");

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Three"",""Four""" + Environment.NewLine
                + @""""",""""" + Environment.NewLine
                + @""""",""""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [TestMethod]
        public void Supports_Not_Outputting_Column_Names()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter(input);
            exporter.OutputColumnHeaders = false;
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""Three"",""Four""" + Environment.NewLine
                + @"""Five"",""Six""",
                output);
        }

        [TestMethod]
        public void Supports_Changing_Field_Separator_Char_Pipe()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter(input);
            exporter.FieldSeparator = '|';
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""One""|""Two""" + Environment.NewLine
                + @"""Three""|""Four""" + Environment.NewLine
                + @"""Five""|""Six""",
                output);
        }

        [TestMethod]
        public void Supports_Changing_Field_Separator_Char_Tab()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter(input);
            exporter.FieldSeparator = '\t';
            var output = exporter.ExportToString();

            Assert.AreEqual(
                "\"One\"\t\"Two\"" + Environment.NewLine
                + "\"Three\"\t\"Four\"" + Environment.NewLine
                + "\"Five\"\t\"Six\"",
                output);
        }

        [TestMethod]
        public void Supports_Changing_Field_Separator_Char_Colon()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter(input);
            exporter.FieldSeparator = ':';
            var output = exporter.ExportToString();

            Assert.AreEqual(
                "\"One\":\"Two\"" + Environment.NewLine
                + "\"Three\":\"Four\"" + Environment.NewLine
                + "\"Five\":\"Six\"",
                output);
        }

        [TestMethod]
        public void Supports_Changing_Field_Separator_Char_Space()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", "Four");
            AddRow(input, "Five", "Six");

            var exporter = new Exporter(input);
            exporter.FieldSeparator = ' ';
            var output = exporter.ExportToString();

            Assert.AreEqual(
                "\"One\" \"Two\"" + Environment.NewLine
                + "\"Three\" \"Four\"" + Environment.NewLine
                + "\"Five\" \"Six\"",
                output);
        }

        [TestMethod]
        public void Supports_Large_Cell_Content()
        {
            string cellContent = new string('a', 10000000);

            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "Three", cellContent);

            var exporter = new Exporter(input);
            var output = exporter.ExportToString();

            Assert.AreEqual(
                @"""One"",""Two""" + Environment.NewLine
                + @"""Three"",""" + cellContent + @"""",
                output);
        }

        [TestMethod]
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

            var exporter = new Exporter(input);

            DateTime start = DateTime.Now;

            var output = exporter.ExportToString();

            DateTime end = DateTime.Now;

            Assert.AreEqual(expected.ToString(), output);

            double duration = end.Subtract(start).TotalMilliseconds;

            Assert.IsTrue(duration / 1000 < 1);
        }

        private static DataTable CreateDataTable()
        {
            return new DataTable();
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
