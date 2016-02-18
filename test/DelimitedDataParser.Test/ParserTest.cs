using System;
using System.IO;
using System.Text;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ParserTest
    {
        [Fact]
        public void Can_Load_Input()
        {
            var parser = new Parser();
            var output = parser.Parse(GetTextReader("Test"));

            Assert.NotNull(output);
        }

        [Fact]
        public void Can_Parse_Column_Names_From_First_Row_Fields()
        {
            string input = @"Col 1,Col 2,Col 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col 3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Can_Parse_Empty_Column_Names()
        {
            string input = @",";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal(0, output.Rows.Count);
            Assert.Equal("Column1", output.Columns[0].ColumnName);
            Assert.Equal("Column2", output.Columns[1].ColumnName);
        }

        [Fact]
        public void Can_Parse_Empty_Fields()
        {
            string input = @"," + Environment.NewLine
                + @"," + Environment.NewLine
                + @"PreventBlankRow,BeingRemoved";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Rows.Count);
            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("Column1", output.Columns[0].ColumnName);
            Assert.Equal("Column2", output.Columns[1].ColumnName);
            Assert.Equal(string.Empty, output.Rows[0][0]);
            Assert.Equal(string.Empty, output.Rows[0][1]);
        }

        [Fact]
        public void Can_Parse_Empty_Stream()
        {
            var parser = new Parser();
            var output = parser.Parse(GetTextReader(string.Empty));

            Assert.Equal(0, output.Rows.Count);
            Assert.Equal(0, output.Columns.Count);
        }

        [Fact]
        public void Does_Not_Strip_Whitespace_From_Column_Names()
        {
            string input = @"Col 1, Col 2";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(" Col 2", output.Columns[1].ColumnName);
        }

        [Fact]
        public void Fails_Without_Valid_Input()
        {
            var parser = new Parser();

            Assert.Throws<ArgumentNullException>(() => parser.Parse(null));
        }

        [Fact]
        public void Removes_Blank_Rows_At_End()
        {
            string input = @"Test 1,Test 2,Test 3" + Environment.NewLine
                + @"Test 1,Test 2,Test 3" + Environment.NewLine
                + Environment.NewLine
                + @"Test 1,Test 2,Test 3" + Environment.NewLine
                + Environment.NewLine;

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal(3, output.Rows.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 3", output.Rows[0][2]);
            Assert.Equal(string.Empty, output.Rows[1][0]);
            Assert.Equal(string.Empty, output.Rows[1][1]);
            Assert.Equal(string.Empty, output.Rows[1][2]);
            Assert.Equal("Test 1", output.Rows[2][0]);
            Assert.Equal("Test 2", output.Rows[2][1]);
            Assert.Equal("Test 3", output.Rows[2][2]);
        }

        [Fact]
        public void Supports_ASCII_Text()
        {
            string input = @"Test 1,Test 2,Test 3"
                + Environment.NewLine
                + @"Test 1,Test 2,*+{|}][";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input, Encoding.ASCII));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("*+{|}][", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_BigEndianUnicode_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input, Encoding.BigEndianUnicode));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test Й", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 葉", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Cell_Containing_Quotes_After_Quoted_Content()
        {
            string input = "\"Test1\",\"Test\"2\"test,\"Test3\"";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test1", output.Rows[0][0]);
            Assert.Equal("Test2\"test", output.Rows[0][1]);
            Assert.Equal("Test3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Cell_Containing_Single_Quote()
        {
            string input = "Test1,Test\"2,Test3";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test1", output.Rows[0][0]);
            Assert.Equal("Test\"2", output.Rows[0][1]);
            Assert.Equal("Test3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Cell_Containing_Single_Quote_QuotedCell()
        {
            string input = "\"Test1\",\"Test\"2\",\"Test3\"";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test1", output.Rows[0][0]);
            Assert.Equal("Test2\"", output.Rows[0][1]);
            Assert.Equal("Test3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Colon()
        {
            string input = "Test 1:Test,2:Test 3" + Environment.NewLine
                + "Test 1:Test,2:Test 3" + Environment.NewLine;

            var parser = new Parser();
            parser.FieldSeparator = ':';
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal(1, output.Rows.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test,2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test,2", output.Rows[0][1]);
            Assert.Equal("Test 3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Pipe()
        {
            string input = @"Test 1|Test,2|Test 3" + Environment.NewLine
                + @"Test 1|Test,2|Test 3" + Environment.NewLine;

            var parser = new Parser();
            parser.FieldSeparator = '|';
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal(1, output.Rows.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test,2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test,2", output.Rows[0][1]);
            Assert.Equal("Test 3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Space()
        {
            string input = "Test1 Test,2 Test3" + Environment.NewLine
                + "Test1 Test,2 Test3" + Environment.NewLine;

            var parser = new Parser();
            parser.FieldSeparator = ' ';
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal(1, output.Rows.Count);
            Assert.Equal("Test1", output.Columns[0].ColumnName);
            Assert.Equal("Test,2", output.Columns[1].ColumnName);
            Assert.Equal("Test3", output.Columns[2].ColumnName);
            Assert.Equal("Test1", output.Rows[0][0]);
            Assert.Equal("Test,2", output.Rows[0][1]);
            Assert.Equal("Test3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Changing_Field_Separator_Char_Tab()
        {
            string input = "Test 1\tTest,2\tTest 3" + Environment.NewLine
                + "Test 1\tTest,2\tTest 3" + Environment.NewLine;

            var parser = new Parser();
            parser.FieldSeparator = '\t';
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal(1, output.Rows.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test,2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test,2", output.Rows[0][1]);
            Assert.Equal("Test 3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Duplicate_Column_Names()
        {
            string input = @"Test 1,Test 2,Test 1,Test 1,Test 3,Test 2"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 1";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(6, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Column1", output.Columns[2].ColumnName);
            Assert.Equal("Column2", output.Columns[3].ColumnName);
            Assert.Equal("Test 3", output.Columns[4].ColumnName);
            Assert.Equal("Column3", output.Columns[5].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 1", output.Rows[0][2]);
            Assert.Equal(string.Empty, output.Rows[0][3]);
            Assert.Equal(string.Empty, output.Rows[0][4]);
            Assert.Equal(string.Empty, output.Rows[0][5]);
        }

        [Fact]
        public void Supports_Field_Separator_As_Cell_Content()
        {
            string input = "\"Test1\",\",\",\"Test2\"";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test1", output.Rows[0][0]);
            Assert.Equal(",", output.Rows[0][1]);
            Assert.Equal("Test2", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_First_Row_As_Data()
        {
            string input = @"Test 1,Test 2,Test 3" + Environment.NewLine
                + @"Test 1,Test 2,Test 3" + Environment.NewLine
                + @"Test 1,Test 2,Test 3" + Environment.NewLine;

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal(3, output.Rows.Count);
            Assert.Equal("Column1", output.Columns[0].ColumnName);
            Assert.Equal("Column2", output.Columns[1].ColumnName);
            Assert.Equal("Column3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 3", output.Rows[0][2]);
            Assert.Equal("Test 1", output.Rows[1][0]);
            Assert.Equal("Test 2", output.Rows[1][1]);
            Assert.Equal("Test 3", output.Rows[1][2]);
            Assert.Equal("Test 1", output.Rows[2][0]);
            Assert.Equal("Test 2", output.Rows[2][1]);
            Assert.Equal("Test 3", output.Rows[2][2]);
        }

        [Fact]
        public void Supports_Large_Cell_Content()
        {
            int cellContentLength = 10000000;
            string input = new string('a', cellContentLength);

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(1, output.Columns.Count);
            Assert.Equal(1, output.Rows.Count);
            Assert.Equal(cellContentLength, ((string)output.Rows[0][0]).Length);
        }

        [Fact]
        public void Supports_Less_Column_Names_Than_Data_Columns()
        {
            string input = @"Col 1,Col 2,Col 3"
                + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(4, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col 3", output.Columns[2].ColumnName);
            Assert.Equal("Column1", output.Columns[3].ColumnName);
            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
            Assert.Equal("Data 3", output.Rows[0][2]);
            Assert.Equal("Data 4", output.Rows[0][3]);
        }

        [Fact]
        public void Supports_Multiple_Blank_Rows()
        {
            string input = @"Test 1,Test 2,Test 3" + Environment.NewLine
                + Environment.NewLine
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal(2, output.Rows.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal(string.Empty, output.Rows[0][0]);
            Assert.Equal(string.Empty, output.Rows[0][1]);
            Assert.Equal(string.Empty, output.Rows[0][2]);
            Assert.Equal("Test 1", output.Rows[1][0]);
            Assert.Equal("Test 2", output.Rows[1][1]);
            Assert.Equal("Test 3", output.Rows[1][2]);
        }

        [Fact]
        public void Supports_New_Row_By_Carriage_Return()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\r'
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_New_Row_By_Full_New_Line()
        {
            string input = @"Test 1,Test 2,Test 3"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_New_Row_By_Line_Feed()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\n'
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_New_Row_By_Reverse_New_Line()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\n' + '\r'
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Quoted_Column_Name_Containing_Carriage_Return()
        {
            string input = @"Col 1,Col 2,""Col" + '\r' + @"3""";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col" + '\r' + "3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Supports_Quoted_Column_Name_Containing_Comma()
        {
            string input = @"""Col,1"",Col 2,Col 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col,1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col 3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Supports_Quoted_Column_Name_Containing_Escaped_Quote()
        {
            string input = @"Col 1,""Col """"2"""""",Col 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal(@"Col ""2""", output.Columns[1].ColumnName);
            Assert.Equal("Col 3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Supports_Quoted_Column_Name_Containing_Full_New_Line()
        {
            string input = @"Col 1,Col 2,""Col"
                + Environment.NewLine
                + @"3""";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col" + Environment.NewLine + "3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Supports_Quoted_Column_Name_Containing_Line_Feed()
        {
            string input = @"Col 1,Col 2,""Col" + '\n' + @"3""";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col" + '\n' + "3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Supports_Quoted_Column_Name_Containing_Reversed_New_Line()
        {
            string input = @"Col 1,Col 2,""Col" + '\n' + '\r' + @"3""";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col" + '\n' + '\r' + "3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Supports_Quoted_Column_Name_Terminated_By_End_Of_File()
        {
            string input = @"Col 1,Col 2,""Col 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col 3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Supports_Quoted_Empty_Fields()
        {
            string input = "\"Test1\",\"\",\"Test2\"";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test1", output.Rows[0][0]);
            Assert.Equal(string.Empty, output.Rows[0][1]);
            Assert.Equal("Test2", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Quoted_First_Column_Name()
        {
            string input = @"""Col 1"",Col 2,Col 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col 3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Supports_Quoted_First_Data_Column()
        {
            string input = @"""Test 1"",Test 2,Test 3"
                + Environment.NewLine
                + @"""Test 1"",Test 2,Test 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Quoted_Last_Column_Name()
        {
            string input = @"Col 1,Col 2,""Col 3""";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col 3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Supports_Quoted_Second_Column_Name()
        {
            string input = @"Col 1,""Col 2"",Col 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col 3", output.Columns[2].ColumnName);
        }

        [Fact]
        public void Supports_Single_Quote_As_Cell_Content()
        {
            string input = "\"Test1\",\"\"\"\",\"Test2\"";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = false;
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test1", output.Rows[0][0]);
            Assert.Equal("\"", output.Rows[0][1]);
            Assert.Equal("Test2", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Unicode_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input, Encoding.Unicode));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test Й", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 葉", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Unknown_Encoding_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser();

            // TODO How is this testing an unknown encoding?
            // This test is identical to `Supports_UTF8_Text`.
            var output = parser.Parse(GetTextReader(input, Encoding.UTF8));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test Й", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 葉", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_UTF32_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input, Encoding.UTF32));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test Й", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 葉", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_UTF7_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input, Encoding.UTF7));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test Й", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 葉", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_UTF8_Text()
        {
            string input = @"Test 1,Test 2,Test Й"
                + Environment.NewLine
                + @"Test 1,Test 2,Test 葉";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input, Encoding.UTF8));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test Й", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test 葉", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Varying_Column_Counts()
        {
            string input = @"Col 1,Col 2,Col 3" + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4" + Environment.NewLine
                + @"Data 1" + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4,Data 5";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(5, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col 3", output.Columns[2].ColumnName);
            Assert.Equal("Column1", output.Columns[3].ColumnName);
            Assert.Equal("Column2", output.Columns[4].ColumnName);
            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
            Assert.Equal("Data 3", output.Rows[0][2]);
            Assert.Equal("Data 4", output.Rows[0][3]);
            Assert.Equal(string.Empty, output.Rows[0][4]);
            Assert.Equal("Data 1", output.Rows[1][0]);
            Assert.Equal(string.Empty, output.Rows[1][1]);
            Assert.Equal(string.Empty, output.Rows[1][2]);
            Assert.Equal(string.Empty, output.Rows[1][3]);
            Assert.Equal(string.Empty, output.Rows[1][4]);
            Assert.Equal("Data 1", output.Rows[2][0]);
            Assert.Equal("Data 2", output.Rows[2][1]);
            Assert.Equal("Data 3", output.Rows[2][2]);
            Assert.Equal("Data 4", output.Rows[2][3]);
            Assert.Equal("Data 5", output.Rows[2][4]);
        }

        [Fact]
        public void Supports_Windows_1252_Text()
        {
            string input = @"Test 1,Test 2,Test ½"
                + Environment.NewLine
                + @"Test 1,Test 2,Test æ";

            Encoding windows1252 = Encoding.GetEncoding(1252);

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input, windows1252));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test ½", output.Columns[2].ColumnName);
            Assert.Equal("Test 1", output.Rows[0][0]);
            Assert.Equal("Test 2", output.Rows[0][1]);
            Assert.Equal("Test æ", output.Rows[0][2]);
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
