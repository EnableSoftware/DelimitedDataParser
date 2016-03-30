using System;
using System.Text;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ParserTest
    {
        [Fact]
        public void ParseReader_Fails_Without_Valid_Input()
        {
            var parser = new Parser();

            Assert.Throws<ArgumentNullException>(() => parser.ParseReader(null));
        }

        [Fact]
        public void ParseReader_Can_Parse_Empty_Stream()
        {
            var parser = new Parser();
            var reader = parser.ParseReader(GetTextReader(string.Empty));
            var hasRow = reader.Read();

            Assert.False(hasRow);
        }

        [Fact]
        public void ParseReader_Can_Parse_Header()
        {
            string input = @"Field 1,Field 2,Field 3";

            var parser = new Parser();

            var reader = parser.ParseReader(GetTextReader(input));

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);

            Assert.Equal("Field 1", reader.GetName(0));
            Assert.Equal("Field 2", reader.GetName(1));
            Assert.Equal("Field 3", reader.GetName(2));
        }

        [Fact]
        public void ParseReader_Can_Parse_Duplicate_Header_Names()
        {
            string input = @"Field 1,Field 1";

            var parser = new Parser();

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Field 1", reader.GetName(0));
            Assert.Equal("Field 1", reader.GetName(1));
        }

        [Fact]
        public void ParseReader_Can_Look_Up_Header_Ordinals()
        {
            string input = @"Field 1,Field 2,Field 3";

            var parser = new Parser();

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();
            
            Assert.Equal(0, reader.GetOrdinal("Field 1"));
            Assert.Equal(1, reader.GetOrdinal("Field 2"));
            Assert.Equal(2, reader.GetOrdinal("Field 3"));
        }

        [Fact]
        public void ParseReader_Can_Look_Up_Duplicated_Header_Ordinals()
        {
            string input = @"Field 1,Field 1";

            var parser = new Parser();

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();
            
            Assert.Equal(0, reader.GetOrdinal("Field 1"));
        }

        [Fact]
        public void ParseReader_Can_Parse_Row()
        {
            string input = @"Field 1,Field 2,Field 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Field 1", reader[0]);
            Assert.Equal("Field 2", reader[1]);
            Assert.Equal("Field 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }
        
        [Fact]
        public void ParseReader_Can_Parse_Empty_Fields()
        {
            string input = ",";
            var expected = new string[] { string.Empty, string.Empty };

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };
            
            var reader = parser.ParseReader(GetTextReader(input)); 
            reader.Read();

            Assert.Equal(2, reader.FieldCount);
            Assert.Equal(string.Empty, reader[0]);
            Assert.Equal(string.Empty, reader[1]);
        }

        [Fact]
        public void ParseReader_Does_Not_Strip_Whitespace_From_Fields()
        {
            string input = @"Field 1, Field 2";
            
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input)); 
            reader.Read();

            Assert.Equal(" Field 2", reader[1]);
        }

        [Fact]
        public void ParseReader_Supports_ASCII_Text()
        {
            string input = @"Test 1,Test 2,*+{|}][";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.ASCII)); 
            reader.Read();

            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("*+{|}][", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_BigEndianUnicode_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃💩";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.BigEndianUnicode)); 
            reader.Read();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃💩", reader[0]);
        }

        public void ParseReader_Supports_Cell_Containing_Quotes_After_Quoted_Content()
        {
            string input = "\"Test1\",\"Test\"2\"test,\"Test3\"";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input)); 
            reader.Read();

            Assert.Equal("Test1", reader[0]);
            Assert.Equal("Test2\"test", reader[1]);
            Assert.Equal("Test3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Cell_Containing_Single_Quote()
        {
            string input = "Test1,Test\"2,Test3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Test1", reader[0]);
            Assert.Equal("Test\"2", reader[1]);
            Assert.Equal("Test3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Cell_Containing_Single_Quote_QuotedCell()
        {
            string input = "\"Test1\",\"Test\"2\",\"Test3\"";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Test1", reader[0]);
            Assert.Equal("Test2\"", reader[1]);
            Assert.Equal("Test3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Changing_Field_Separator_Char_Colon()
        {
            string input = "Test 1:Test,2:Test 3";

            var parser = new Parser
            {
                FieldSeparator = ':',
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input)); 
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test,2", reader[1]);
            Assert.Equal("Test 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Changing_Field_Separator_Char_Pipe()
        {
            string input = "Test 1|Test,2|Test 3";

            var parser = new Parser
            {
                FieldSeparator = '|',
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input)); 
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test,2", reader[1]);
            Assert.Equal("Test 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Changing_Field_Separator_Char_Space()
        {
            string input = "Test1 Test,2 Test3";

            var parser = new Parser
            {
                FieldSeparator = ' ',
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input)); 
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Test1", reader[0]);
            Assert.Equal("Test,2", reader[1]);
            Assert.Equal("Test3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Changing_Field_Separator_Char_Tab()
        {
            string input = "Test 1\tTest,2\tTest 3";

            var parser = new Parser
            {
                FieldSeparator = '\t',
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input)); 
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test,2", reader[1]);
            Assert.Equal("Test 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Field_Separator_As_Cell_Content()
        {
            string input = "\"Test1\",\",\",\"Test2\"";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Test1", reader[0]);
            Assert.Equal(",", reader[1]);
            Assert.Equal("Test2", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_First_Row_As_Data()
        {
            string input = @"Test 1,Test 2,Test 3" + Environment.NewLine
                + @"Test 1,Test 2,Test 3" + Environment.NewLine
                + @"Test 1,Test 2,Test 3" + Environment.NewLine;

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();

            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("Test 3", reader[2]);

            reader.Read();

            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("Test 3", reader[2]);

            reader.Read();

            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("Test 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Large_Cell_Content()
        {
            const int CellContentLength = 10000000;
            string input = new string('a', CellContentLength);

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input)); 
            reader.Read();

            Assert.Equal(CellContentLength, reader.GetString(0).Length);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }
        
        [Fact]
        public void ParseReader_Supports_Multiple_Blank_Rows()
        {
            string input = @"Test 1,Test 2,Test 3" + Environment.NewLine
                + Environment.NewLine
                + @"Test 1,Test 2,Test 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(3, reader.FieldCount);

            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("Test 3", reader[2]);

            reader.Read();
            reader.Read();

            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("Test 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_New_Row_By_Carriage_Return()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\r'
                + @"Test 4,Test 5,Test 6";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.False(reader.Read());
        }

        [Fact]
        public void ParseReader_Supports_New_Row_By_Full_New_Line()
        {
            string input = @"Test 1,Test 2,Test 3"
                + Environment.NewLine
                + @"Test 4,Test 5,Test 6";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();
            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("Test 3", reader[2]);

            reader.Read();
            Assert.Equal("Test 4", reader[0]);
            Assert.Equal("Test 5", reader[1]);
            Assert.Equal("Test 6", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_New_Row_By_Line_Feed()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\n'
                + @"Test 4,Test 5,Test 6";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();
            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("Test 3", reader[2]);

            reader.Read();
            Assert.Equal("Test 4", reader[0]);
            Assert.Equal("Test 5", reader[1]);
            Assert.Equal("Test 6", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_New_Row_By_Reverse_New_Line()
        {
            string input = @"Test 1,Test 2,Test 3"
                + '\n' + '\r'
                + @"Test 4,Test 5,Test 6";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();
            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("Test 3", reader[2]);

            reader.Read();
            Assert.Equal("Test 4", reader[0]);
            Assert.Equal("Test 5", reader[1]);
            Assert.Equal("Test 6", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Carriage_Return()
        {
            string input = @"Col 1,Col 2,""Col" + '\r' + @"3""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Col 1", reader[0]);
            Assert.Equal("Col 2", reader[1]);
            Assert.Equal("Col" + '\r' + "3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Comma()
        {
            string input = @"""Col,1"",Col 2,Col 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Col,1", reader[0]);
            Assert.Equal("Col 2", reader[1]);
            Assert.Equal("Col 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Escaped_Quote()
        {
            string input = @"Col 1,""Col """"2"""""",Col 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Col 1", reader[0]);
            Assert.Equal(@"Col ""2""", reader[1]);
            Assert.Equal("Col 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Full_New_Line()
        {
            string input = @"Col 1,Col 2,""Col"
                + Environment.NewLine
                + @"3""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Col 1", reader[0]);
            Assert.Equal("Col 2", reader[1]);
            Assert.Equal("Col" + Environment.NewLine + "3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Line_Feed()
        {
            string input = @"Col 1,Col 2,""Col" + '\n' + @"3""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Col 1", reader[0]);
            Assert.Equal("Col 2", reader[1]);
            Assert.Equal("Col" + '\n' + "3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Reversed_New_Line()
        {
            string input = @"Col 1,Col 2,""Col" + '\n' + '\r' + @"3""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Col 1", reader[0]);
            Assert.Equal("Col 2", reader[1]);
            Assert.Equal("Col" + '\n' + '\r' + "3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Terminated_By_End_Of_File()
        {
            string input = @"Col 1,Col 2,""Col 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Col 1", reader[0]);
            Assert.Equal("Col 2", reader[1]);
            Assert.Equal("Col 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Empty_Fields()
        {
            string input = "\"Test1\",\"\",\"Test2\"";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Test1", reader[0]);
            Assert.Equal(string.Empty, reader[1]);
            Assert.Equal("Test2", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_First_Column()
        {
            string input = @"""Col 1"",Col 2,Col 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Col 1", reader[0]);
            Assert.Equal("Col 2", reader[1]);
            Assert.Equal("Col 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Last_Column()
        {
            string input = @"Col 1,Col 2,""Col 3""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Col 1", reader[0]);
            Assert.Equal("Col 2", reader[1]);
            Assert.Equal("Col 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Second_Column()
        {
            string input = @"Col 1,""Col 2"",Col 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Col 1", reader[0]);
            Assert.Equal("Col 2", reader[1]);
            Assert.Equal("Col 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Single_Quote_As_Cell_Content()
        {
            string input = "\"Test1\",\"\"\"\",\"Test2\"";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input)); 
            reader.Read();

            Assert.Equal("Test1", reader[0]);
            Assert.Equal("\"", reader[1]);
            Assert.Equal("Test2", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Unicode_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃💩";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.Unicode)); 
            reader.Read();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃💩", reader[0]);
        }

        [Fact]
        public void ParseReader_Supports_UTF32_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃💩";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.UTF32)); 
            reader.Read();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃💩", reader[0]);
        }

        [Fact]
        public void ParseReader_Supports_UTF7_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃💩";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.UTF7)); 
            reader.Read();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃💩", reader[0]);
        }

        [Fact]
        public void ParseReader_Supports_UTF8_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃💩";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.UTF8));
            reader.Read();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃💩", reader[0]);
        }

        [Fact]
        public void ParseReader_Supports_Varying_Column_Counts()
        {
            string input = @"Col 1,Col 2,Col 3" + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4" + Environment.NewLine
                + @"Data 1" + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4,Data 5";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Col 1", reader[0]);
            Assert.Equal("Col 2", reader[1]);
            Assert.Equal("Col 3", reader[2]);

            reader.Read();
            Assert.Equal(4, reader.FieldCount);
            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);
            Assert.Equal("Data 4", reader[3]);

            reader.Read();
            Assert.Equal(1, reader.FieldCount);
            Assert.Equal("Data 1", reader[0]);

            reader.Read();
            Assert.Equal(5, reader.FieldCount);
            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);
            Assert.Equal("Data 4", reader[3]);
            Assert.Equal("Data 5", reader[4]);
        }

        [Fact]
        public void ParseReader_Supports_Windows_1252_Text()
        {
            string input = @"Test 1,Test 2,Test ½"
                + Environment.NewLine
                + @"Test 1,Test 2,Test æ";

            Encoding windows1252 = Encoding.GetEncoding(1252);

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, windows1252));
            reader.Read();
            
            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("Test ½", reader[2]);

            reader.Read();
            Assert.Equal("Test 1", reader[0]);
            Assert.Equal("Test 2", reader[1]);
            Assert.Equal("Test æ", reader[2]);
        }
    }
}
