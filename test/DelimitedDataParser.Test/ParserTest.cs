using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DelimitedDataParser
{
    [TestClass]
    public partial class ParserTest
    {
        [TestMethod]
        public void Can_Load_Input()
        {
            var parser = new Parser(GetTextReader("Test"));
            var output = parser.Parse();

            Assert.IsNotNull(output, "Parser did not generate any output.");
        }

        [TestMethod]
        public void Can_Parse_Empty_Stream()
        {
            var parser = new Parser(GetTextReader(string.Empty));
            var output = parser.Parse();

            Assert.AreEqual(0, output.Rows.Count, "Should have zero rows.");
            Assert.AreEqual(0, output.Columns.Count, "Should have zero columns.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Fails_Without_Valid_Input()
        {
            var output = new Parser(null).Parse();
        }

        [TestMethod]
        public void Can_Parse_Column_Names_From_First_Row_Fields()
        {
            string input = @"Col 1,Col 2,Col 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_First_Column_Name()
        {
            string input = @"""Col 1"",Col 2,Col 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_Second_Column_Name()
        {
            string input = @"Col 1,""Col 2"",Col 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_Last_Column_Name()
        {
            string input = @"Col 1,Col 2,""Col 3""";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_Column_Name_Containing_Escaped_Quote()
        {
            string input = @"Col 1,""Col """"2"""""",Col 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual(@"Col ""2""", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_Column_Name_Containing_Comma()
        {
            string input = @"""Col,1"",Col 2,Col 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col,1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_Column_Name_Containing_Full_New_Line()
        {
            string input = @"Col 1,Col 2,""Col"
                + Environment.NewLine
                + @"3""";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col" + Environment.NewLine + "3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_Column_Name_Containing_Reversed_New_Line()
        {
            string input = @"Col 1,Col 2,""Col" + '\n' + '\r' + @"3""";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col" + '\n' + '\r' + "3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_Column_Name_Containing_Carriage_Return()
        {
            string input = @"Col 1,Col 2,""Col" + '\r' + @"3""";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col" + '\r' + "3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_Column_Name_Containing_Line_Feed()
        {
            string input = @"Col 1,Col 2,""Col" + '\n' + @"3""";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col" + '\n' + "3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_Column_Name_Terminated_By_End_Of_File()
        {
            string input = @"Col 1,Col 2,""Col 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 3", output.Columns[2].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_Empty_Fields()
        {
            string input = "\"Test1\",\"\",\"Test2\"";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test2", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Single_Quote_As_Cell_Content()
        {
            string input = "\"Test1\",\"\"\"\",\"Test2\"";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("\"", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test2", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Cell_Containing_Single_Quote()
        {
            string input = "Test1,Test\"2,Test3";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test\"2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Cell_Containing_Single_Quote_QuotedCell()
        {
            string input = "\"Test1\",\"Test\"2\",\"Test3\"";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test2\"", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Cell_Containing_Quotes_After_Quoted_Content()
        {
            string input = "\"Test1\",\"Test\"2\"test,\"Test3\"";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test2\"test", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Field_Separator_As_Cell_Content()
        {
            string input = "\"Test1\",\",\",\"Test2\"";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual(",", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test2", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Does_Not_Strip_Whitespace_From_Column_Names()
        {
            string input = @"Col 1, Col 2";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(" Col 2", output.Columns[1].ColumnName);
        }

        [TestMethod]
        public void Can_Parse_Empty_Column_Names()
        {
            string input = @",";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(2, output.Columns.Count, "Expected 2 columns.");
            Assert.AreEqual(0, output.Rows.Count, "Expected 0 rows.");
            Assert.AreEqual("Column1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Column2", output.Columns[1].ColumnName, "Column name incorrect.");
        }

        [TestMethod]
        public void Can_Parse_Empty_Fields()
        {
            string input = @"," + Environment.NewLine
                + @"," + Environment.NewLine
                + @"PreventBlankRow,BeingRemoved";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(2, output.Rows.Count, "Expected 2 rows.");
            Assert.AreEqual(2, output.Columns.Count, "Expected 2 columns.");
            Assert.AreEqual("Column1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Column2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[0][1], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Less_Column_Names_Than_Data_Columns()
        {
            string input = @"Col 1,Col 2,Col 3"
                + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(4, output.Columns.Count, "Expected 4 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Column1", output.Columns[3].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Data 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Data 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Data 3", output.Rows[0][2], "Field data incorrect.");
            Assert.AreEqual("Data 4", output.Rows[0][3], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Varying_Column_Counts()
        {
            string input = @"Col 1,Col 2,Col 3" + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4" + Environment.NewLine
                + @"Data 1" + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4,Data 5";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(5, output.Columns.Count, "Expected 5 columns.");
            Assert.AreEqual("Col 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Col 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Column1", output.Columns[3].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Column2", output.Columns[4].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Data 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Data 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Data 3", output.Rows[0][2], "Field data incorrect.");
            Assert.AreEqual("Data 4", output.Rows[0][3], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[0][4], "Field data incorrect.");
            Assert.AreEqual("Data 1", output.Rows[1][0], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[1][1], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[1][2], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[1][3], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[1][4], "Field data incorrect.");
            Assert.AreEqual("Data 1", output.Rows[2][0], "Field data incorrect.");
            Assert.AreEqual("Data 2", output.Rows[2][1], "Field data incorrect.");
            Assert.AreEqual("Data 3", output.Rows[2][2], "Field data incorrect.");
            Assert.AreEqual("Data 4", output.Rows[2][3], "Field data incorrect.");
            Assert.AreEqual("Data 5", output.Rows[2][4], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Quoted_First_Data_Column()
        {
            string input = @"""Test 1"",Test 2,Test 3"
                + Environment.NewLine
                + @"""Test 1"",Test 2,Test 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_New_Row_By_Full_New_Line()
        {
            string input = @"Test 1,Test 2,Test 3"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_New_Row_By_Reverse_New_Line()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\n' + '\r'
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_New_Row_By_Carriage_Return()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\r'
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_New_Row_By_Line_Feed()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\n'
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Multiple_Blank_Rows()
        {
            string input = @"Test 1,Test 2,Test 3" + Environment.NewLine
                + Environment.NewLine
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual(2, output.Rows.Count, "Expected 2 rows.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[0][2], "Field data incorrect.");
            Assert.AreEqual("Test 1", output.Rows[1][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[1][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[1][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_ASCII_Text()
        {
            string input = @"Test 1,Test 2,Test 3"
                + Environment.NewLine
                + @"Test 1,Test 2,*+{|}][";

            var parser = new Parser(GetTextReader(input, Encoding.ASCII));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("*+{|}][", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_BigEndianUnicode_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser(GetTextReader(input, Encoding.BigEndianUnicode));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test Й", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 葉", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Unicode_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser(GetTextReader(input, Encoding.Unicode));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test Й", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 葉", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_UTF32_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser(GetTextReader(input, Encoding.UTF32));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test Й", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 葉", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_UTF7_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser(GetTextReader(input, Encoding.UTF7));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test Й", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 葉", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_UTF8_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser(GetTextReader(input, Encoding.UTF8));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test Й", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 葉", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Windows_1252_Text()
        {
            string input = @"Test 1,Test 2,Test ½"
                + Environment.NewLine
                + @"Test 1,Test 2,Test æ";

            Encoding windows1252 = Encoding.GetEncoding(1252);

            var parser = new Parser(GetTextReader(input, windows1252));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test ½", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test æ", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Unknown_Encoding_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser(GetTextReader(input, Encoding.UTF8));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test Й", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 葉", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Duplicate_Column_Names()
        {
            string input = @"Test 1,Test 2,Test 1,Test 1,Test 3,Test 2"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 1";

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(6, output.Columns.Count, "Expected 6 columns.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Column1", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Column2", output.Columns[3].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[4].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Column3", output.Columns[5].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][2], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[0][3], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[0][4], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[0][5], "Field data incorrect.");
        }

        [TestMethod]
        public void Removes_Blank_Rows_At_End()
        {
            string input = @"Test 1,Test 2,Test 3" + Environment.NewLine
                + @"Test 1,Test 2,Test 3" + Environment.NewLine
                + Environment.NewLine
                + @"Test 1,Test 2,Test 3" + Environment.NewLine
                + Environment.NewLine;

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual(3, output.Rows.Count, "Expected 3 rows.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[0][2], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[1][0], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[1][1], "Field data incorrect.");
            Assert.AreEqual(string.Empty, output.Rows[1][2], "Field data incorrect.");
            Assert.AreEqual("Test 1", output.Rows[2][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[2][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[2][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_First_Row_As_Data()
        {
            string input = @"Test 1,Test 2,Test 3" + Environment.NewLine
                + @"Test 1,Test 2,Test 3" + Environment.NewLine
                + @"Test 1,Test 2,Test 3" + Environment.NewLine;

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual(3, output.Rows.Count, "Expected 3 rows.");
            Assert.AreEqual("Column1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Column2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Column3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[0][2], "Field data incorrect.");
            Assert.AreEqual("Test 1", output.Rows[1][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[1][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[1][2], "Field data incorrect.");
            Assert.AreEqual("Test 1", output.Rows[2][0], "Field data incorrect.");
            Assert.AreEqual("Test 2", output.Rows[2][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[2][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Changing_Field_Separator_Char_Pipe()
        {
            string input = @"Test 1|Test,2|Test 3" + Environment.NewLine
                + @"Test 1|Test,2|Test 3" + Environment.NewLine;

            var parser = new Parser(GetTextReader(input));
            parser.FieldSeparator = '|';
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual(1, output.Rows.Count, "Expected 1 rows.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test,2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test,2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Changing_Field_Separator_Char_Tab()
        {
            string input = "Test 1\tTest,2\tTest 3" + Environment.NewLine
                + "Test 1\tTest,2\tTest 3" + Environment.NewLine;

            var parser = new Parser(GetTextReader(input));
            parser.FieldSeparator = '\t';
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual(1, output.Rows.Count, "Expected 1 rows.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test,2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test,2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Changing_Field_Separator_Char_Colon()
        {
            string input = "Test 1:Test,2:Test 3" + Environment.NewLine
                + "Test 1:Test,2:Test 3" + Environment.NewLine;

            var parser = new Parser(GetTextReader(input));
            parser.FieldSeparator = ':';
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual(1, output.Rows.Count, "Expected 1 rows.");
            Assert.AreEqual("Test 1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test,2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test 1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test,2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test 3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Changing_Field_Separator_Char_Space()
        {
            string input = "Test1 Test,2 Test3" + Environment.NewLine
                + "Test1 Test,2 Test3" + Environment.NewLine;

            var parser = new Parser(GetTextReader(input));
            parser.FieldSeparator = ' ';
            var output = parser.Parse();

            Assert.AreEqual(3, output.Columns.Count, "Expected 3 columns.");
            Assert.AreEqual(1, output.Rows.Count, "Expected 1 rows.");
            Assert.AreEqual("Test1", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test,2", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test3", output.Columns[2].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Test1", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Test,2", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Test3", output.Rows[0][2], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_Large_Cell_Content()
        {
            int cellContentLength = 10000000;
            string input = new string('a', cellContentLength);

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse();

            Assert.AreEqual(1, output.Columns.Count, "Expected 1 column.");
            Assert.AreEqual(1, output.Rows.Count, "Expected 1 row.");
            Assert.AreEqual(cellContentLength, ((string)output.Rows[0][0]).Length, "Column content length incorrect.");
        }

        private static TextReader GetTextReader(string input)
        {
            if (input == null)
            {
                input = string.Empty;
            }

            return new StringReader(input);
        }

        private static TextReader GetTextReader(string input, Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            if (input == null)
            {
                input = string.Empty;
            }

            var buffer = encoding.GetBytes(input);

            return new StreamReader(new MemoryStream(buffer), encoding);
        }
    }
}
