using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ParserTest
    {
        [Fact]
        public void ParseRows_Fails_Without_Valid_Input()
        {
            var parser = new Parser();

            Assert.Throws<ArgumentNullException>(() => parser.ParseRows(null));
        }

        [Fact]
        public void ParseRows_Can_Parse_Empty_Stream()
        {
            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(string.Empty));

            Assert.Empty(output);
        }

        [Fact(Skip = "Consider for implementation")]
        public void ParseRows_Execution_Is_Deferred()
        {
            // TODO Should we test that execution is deferred?
            // See http://codeblog.jonskeet.uk/2010/09/03/reimplementing-linq-to-objects-part-2-quot-where-quot/
        }

        [Fact]
        public void ParseRows_Can_Parse_Row()
        {
            string input = @"Field 1,Field 2,Field 3";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Field 1", output[0][0]);
            Assert.Equal("Field 2", output[0][1]);
            Assert.Equal("Field 3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Can_Parse_Empty_Fields()
        {
            string input = ",";
            var expected = new string[] { string.Empty, string.Empty };

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal(expected, output);
        }

        [Fact]
        public void ParseRows_Does_Not_Strip_Whitespace_From_Fields()
        {
            string input = @"Field 1, Field 2";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal(" Field 2", output[1]);
        }

        [Fact(Skip = "Consider for implementation")]
        public void ParseRows_Removes_Blank_Rows_At_End()
        {
            // TODO Do we want to support this?
            // We could leave this behaviour to the existing `Parse` method only.
        }

        [Fact]
        public void ParseRows_Supports_ASCII_Text()
        {
            string input = @"Test 1,Test 2,*+{|}][";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input, Encoding.ASCII)).Single();

            Assert.Equal("Test 1", output[0]);
            Assert.Equal("Test 2", output[1]);
            Assert.Equal("*+{|}][", output[2]);
        }

        [Fact]
        public void ParseRows_Supports_BigEndianUnicode_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃💩";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input, Encoding.BigEndianUnicode)).Single();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃💩", output[0]);
        }

        public void ParseRows_Supports_Cell_Containing_Quotes_After_Quoted_Content()
        {
            string input = "\"Test1\",\"Test\"2\"test,\"Test3\"";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal("Test1", output[0]);
            Assert.Equal("Test2\"test", output[1]);
            Assert.Equal("Test3", output[2]);
        }

        [Fact]
        public void ParseRows_Supports_Cell_Containing_Single_Quote()
        {
            string input = "Test1,Test\"2,Test3";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal("Test1", output[0]);
            Assert.Equal("Test\"2", output[1]);
            Assert.Equal("Test3", output[2]);
        }

        [Fact]
        public void ParseRows_Supports_Cell_Containing_Single_Quote_QuotedCell()
        {
            string input = "\"Test1\",\"Test\"2\",\"Test3\"";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal("Test1", output[0]);
            Assert.Equal("Test2\"", output[1]);
            Assert.Equal("Test3", output[2]);
        }

        [Fact]
        public void ParseRows_Supports_Changing_Field_Separator_Char_Colon()
        {
            string input = "Test 1:Test,2:Test 3";

            var parser = new Parser();
            parser.FieldSeparator = ':';
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal(3, output.Length);
            Assert.Equal("Test 1", output[0]);
            Assert.Equal("Test,2", output[1]);
            Assert.Equal("Test 3", output[2]);
        }

        [Fact]
        public void ParseRows_Supports_Changing_Field_Separator_Char_Pipe()
        {
            string input = "Test 1|Test,2|Test 3";

            var parser = new Parser();
            parser.FieldSeparator = '|';
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal(3, output.Length);
            Assert.Equal("Test 1", output[0]);
            Assert.Equal("Test,2", output[1]);
            Assert.Equal("Test 3", output[2]);
        }

        [Fact]
        public void ParseRows_Supports_Changing_Field_Separator_Char_Space()
        {
            string input = "Test1 Test,2 Test3";

            var parser = new Parser();
            parser.FieldSeparator = ' ';
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal(3, output.Length);
            Assert.Equal("Test1", output[0]);
            Assert.Equal("Test,2", output[1]);
            Assert.Equal("Test3", output[2]);
        }

        [Fact]
        public void ParseRows_Supports_Changing_Field_Separator_Char_Tab()
        {
            string input = "Test 1\tTest,2\tTest 3";

            var parser = new Parser();
            parser.FieldSeparator = '\t';
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal(3, output.Length);
            Assert.Equal("Test 1", output[0]);
            Assert.Equal("Test,2", output[1]);
            Assert.Equal("Test 3", output[2]);
        }

        [Fact]
        public void ParseRows_Supports_Field_Separator_As_Cell_Content()
        {
            string input = "\"Test1\",\",\",\"Test2\"";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal(3, output.Length);
            Assert.Equal("Test1", output[0]);
            Assert.Equal(",", output[1]);
            Assert.Equal("Test2", output[2]);
        }

        [Fact]
        public void ParseRows_Supports_Large_Cell_Content()
        {
            int cellContentLength = 10000000;
            string input = new string('a', cellContentLength);

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Single(output);
            Assert.Equal(cellContentLength, output[0].Length);
        }

        [Fact(Skip = "Consider appropriate test cases here")]
        public void ParseRows_Supports_Multiple_Blank_Rows()
        {
            string input = @"Test 1,Test 2,Test 3" + Environment.NewLine
                + Environment.NewLine
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Equal(3, output.Length);

            Assert.Equal("Test 1", output[0][0]);
            Assert.Equal("Test 2", output[0][1]);
            Assert.Equal("Test 3", output[0][2]);

            Assert.Empty(output[1]);

            Assert.Equal("Test 1", output[2][0]);
            Assert.Equal("Test 2", output[2][1]);
            Assert.Equal("Test 3", output[2][2]);
        }

        [Fact]
        public void ParseRows_Supports_New_Row_By_Carriage_Return()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\r'
                + @"Test 4,Test 5,Test 6";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Equal(2, output.Length);
        }

        [Fact]
        public void ParseRows_Supports_New_Row_By_Full_New_Line()
        {
            string input = @"Test 1,Test 2,Test 3"
                + Environment.NewLine
                + @"Test 4,Test 5,Test 6";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Equal(2, output.Length);

            Assert.Equal("Test 1", output[0][0]);
            Assert.Equal("Test 2", output[0][1]);
            Assert.Equal("Test 3", output[0][2]);
            Assert.Equal("Test 4", output[1][0]);
            Assert.Equal("Test 5", output[1][1]);
            Assert.Equal("Test 6", output[1][2]);
        }

        [Fact]
        public void ParseRows_Supports_New_Row_By_Line_Feed()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\n'
                + @"Test 4,Test 5,Test 6";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 4", output.Rows[0][0]);
            Assert.Equal("Test 5", output.Rows[0][1]);
            Assert.Equal("Test 6", output.Rows[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_New_Row_By_Reverse_New_Line()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\n' + '\r'
                + @"Test 4,Test 5,Test 6";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Test 1", output.Columns[0].ColumnName);
            Assert.Equal("Test 2", output.Columns[1].ColumnName);
            Assert.Equal("Test 3", output.Columns[2].ColumnName);
            Assert.Equal("Test 4", output.Rows[0][0]);
            Assert.Equal("Test 5", output.Rows[0][1]);
            Assert.Equal("Test 6", output.Rows[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_Column_Name_Containing_Carriage_Return()
        {
            string input = @"Col 1,Col 2,""Col" + '\r' + @"3""";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Col 1", output[0][0]);
            Assert.Equal("Col 2", output[0][1]);
            Assert.Equal("Col" + '\r' + "3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_Column_Name_Containing_Comma()
        {
            string input = @"""Col,1"",Col 2,Col 3";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Col,1", output[0][0]);
            Assert.Equal("Col 2", output[0][1]);
            Assert.Equal("Col 3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_Column_Name_Containing_Escaped_Quote()
        {
            string input = @"Col 1,""Col """"2"""""",Col 3";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Col 1", output[0][0]);
            Assert.Equal(@"Col ""2""", output[0][1]);
            Assert.Equal("Col 3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_Column_Name_Containing_Full_New_Line()
        {
            string input = @"Col 1,Col 2,""Col"
                + Environment.NewLine
                + @"3""";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Col 1", output[0][0]);
            Assert.Equal("Col 2", output[0][1]);
            Assert.Equal("Col" + Environment.NewLine + "3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_Column_Name_Containing_Line_Feed()
        {
            string input = @"Col 1,Col 2,""Col" + '\n' + @"3""";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Col 1", output[0][0]);
            Assert.Equal("Col 2", output[0][1]);
            Assert.Equal("Col" + '\n' + "3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_Column_Name_Containing_Reversed_New_Line()
        {
            string input = @"Col 1,Col 2,""Col" + '\n' + '\r' + @"3""";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Col 1", output[0][0]);
            Assert.Equal("Col 2", output[0][1]);
            Assert.Equal("Col" + '\n' + '\r' + "3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_Column_Name_Terminated_By_End_Of_File()
        {
            string input = @"Col 1,Col 2,""Col 3";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Col 1", output[0][0]);
            Assert.Equal("Col 2", output[0][1]);
            Assert.Equal("Col 3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_Empty_Fields()
        {
            string input = "\"Test1\",\"\",\"Test2\"";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Test1", output[0][0]);
            Assert.Equal(string.Empty, output[0][1]);
            Assert.Equal("Test2", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_First_Column()
        {
            string input = @"""Col 1"",Col 2,Col 3";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Col 1", output[0][0]);
            Assert.Equal("Col 2", output[0][1]);
            Assert.Equal("Col 3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_Last_Column()
        {
            string input = @"Col 1,Col 2,""Col 3""";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Col 1", output[0][0]);
            Assert.Equal("Col 2", output[0][1]);
            Assert.Equal("Col 3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Quoted_Second_Column()
        {
            string input = @"Col 1,""Col 2"",Col 3";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Single(output);
            Assert.Equal("Col 1", output[0][0]);
            Assert.Equal("Col 2", output[0][1]);
            Assert.Equal("Col 3", output[0][2]);
        }

        [Fact]
        public void ParseRows_Supports_Single_Quote_As_Cell_Content()
        {
            string input = "\"Test1\",\"\"\"\",\"Test2\"";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).Single();

            Assert.Equal("Test1", output[0]);
            Assert.Equal("\"", output[1]);
            Assert.Equal("Test2", output[2]);
        }

        [Fact]
        public void ParseRows_Supports_Unicode_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃💩";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input, Encoding.Unicode)).Single();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃💩", output[0]);
        }

        [Fact]
        public void ParseRows_Supports_UTF32_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃💩";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input, Encoding.UTF32)).Single();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃💩", output[0]);
        }

        [Fact]
        public void ParseRows_Supports_UTF7_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃💩";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input, Encoding.UTF7)).Single();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃💩", output[0]);
        }

        [Fact]
        public void ParseRows_Supports_UTF8_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃💩";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input, Encoding.UTF8)).Single();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃💩", output[0]);
        }

        [Fact]
        public void ParseRows_Supports_Varying_Column_Counts()
        {
            string input = @"Col 1,Col 2,Col 3" + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4" + Environment.NewLine
                + @"Data 1" + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4,Data 5";

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input)).ToArray();

            Assert.Equal(3, output[0].Length);
            Assert.Equal("Col 1", output[0][0]);
            Assert.Equal("Col 2", output[0][1]);
            Assert.Equal("Col 3", output[0][2]);

            Assert.Equal(4, output[1].Length);
            Assert.Equal("Data 1", output[1][0]);
            Assert.Equal("Data 2", output[1][1]);
            Assert.Equal("Data 3", output[1][2]);
            Assert.Equal("Data 4", output[1][3]);

            Assert.Equal(1, output[2].Length);
            Assert.Equal("Data 1", output[2][0]);

            Assert.Equal(5, output[3].Length);
            Assert.Equal("Data 1", output[3][0]);
            Assert.Equal("Data 2", output[3][1]);
            Assert.Equal("Data 3", output[3][2]);
            Assert.Equal("Data 4", output[3][3]);
            Assert.Equal("Data 5", output[3][4]);
        }

        [Fact]
        public void ParseRows_Supports_Windows_1252_Text()
        {
            string input = @"Test 1,Test 2,Test ½"
                + Environment.NewLine
                + @"Test 1,Test 2,Test æ";

            Encoding windows1252 = Encoding.GetEncoding(1252);

            var parser = new Parser();
            var output = parser.ParseRows(GetTextReader(input, windows1252)).ToArray();

            Assert.Equal("Test 1", output[0][0]);
            Assert.Equal("Test 2", output[0][1]);
            Assert.Equal("Test ½", output[0][2]);
            Assert.Equal("Test 1", output[1][0]);
            Assert.Equal("Test 2", output[1][1]);
            Assert.Equal("Test æ", output[1][2]);
        }
    }
}
