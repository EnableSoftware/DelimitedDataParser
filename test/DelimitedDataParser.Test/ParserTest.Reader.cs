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
        public void ParseReader_Fails_Without_Valid_Input()
        {
            var parser = new Parser();

            Assert.Throws<ArgumentNullException>(() => parser.ParseReader(null));
        }

        [Fact]
        public void ParseReader_Without_Valid_Encoding()
        {
            var parser = new Parser();

            Assert.Throws<ArgumentNullException>(() => parser.ParseReader(GetTextReader(string.Empty), null));
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
        public void ParseReader_Can_Index_On_Field_Names()
        {
            string input = @"Field 1,Field 2" + Environment.NewLine
                + @"Data 1,Data 2" + Environment.NewLine;

            var parser = new Parser();

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader["Field 1"]);
            Assert.Equal("Data 2", reader["Field 2"]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Can_Parse_Row()
        {
            string input = @"Data 1,Data 2,Data 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Can_Parse_Empty_Fields()
        {
            string input = ",";

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
            string input = @"Data 1,Data 2,*+{|}][";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.ASCII));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("*+{|}][", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_BigEndianUnicode_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.BigEndianUnicode));
            reader.Read();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃", reader[0]);
        }

        public void ParseReader_Supports_Cell_Containing_Quotes_After_Quoted_Content()
        {
            string input = "\"Data 1\",\"Data\"2\"data,\"Data 3\"";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data2\"data", reader[1]);
            Assert.Equal("Data 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Cell_Containing_Single_Quote()
        {
            string input = "Data 1,Data\"2,Data 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data\"2", reader[1]);
            Assert.Equal("Data 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Cell_Containing_Single_Quote_QuotedCell()
        {
            string input = "\"Data 1\",\"Data\"2\",\"Data 3\"";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data2\"", reader[1]);
            Assert.Equal("Data 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Changing_Field_Separator_Char_Colon()
        {
            string input = "Data 1:Data,2:Data 3";

            var parser = new Parser
            {
                FieldSeparator = ':',
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data,2", reader[1]);
            Assert.Equal("Data 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Changing_Field_Separator_Char_Pipe()
        {
            string input = "Data 1|Data,2|Data 3";

            var parser = new Parser
            {
                FieldSeparator = '|',
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data,2", reader[1]);
            Assert.Equal("Data 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Changing_Field_Separator_Char_Space()
        {
            string input = "Data1 Data,2 Data3";

            var parser = new Parser
            {
                FieldSeparator = ' ',
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Data1", reader[0]);
            Assert.Equal("Data,2", reader[1]);
            Assert.Equal("Data3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Changing_Field_Separator_Char_Tab()
        {
            string input = "Data 1\tData,2\tData 3";

            var parser = new Parser
            {
                FieldSeparator = '\t',
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data,2", reader[1]);
            Assert.Equal("Data 3", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Field_Separator_As_Cell_Content()
        {
            string input = "\"Data 1\",\",\",\"Data 2\"";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Data 1", reader[0]);
            Assert.Equal(",", reader[1]);
            Assert.Equal("Data 2", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_First_Row_As_Data()
        {
            string input = @"Data 1,Data 2,Data 3" + Environment.NewLine
                + @"Data 4,Data 5,Data 6" + Environment.NewLine
                + @"Data 7,Data 8,Data 9" + Environment.NewLine;

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            reader.Read();

            Assert.Equal("Data 4", reader[0]);
            Assert.Equal("Data 5", reader[1]);
            Assert.Equal("Data 6", reader[2]);

            reader.Read();

            Assert.Equal("Data 7", reader[0]);
            Assert.Equal("Data 8", reader[1]);
            Assert.Equal("Data 9", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Large_Cell_Content()
        {
            const int CellContentLength = 10000000;
            var input = new string('a', CellContentLength);

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
            string input = @"Data 1,Data 2,Data 3" + Environment.NewLine
                + Environment.NewLine
                + @"Data 4,Data 5,Data 6";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(3, reader.FieldCount);

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            reader.Read();
            reader.Read();

            Assert.Equal("Data 4", reader[0]);
            Assert.Equal("Data 5", reader[1]);
            Assert.Equal("Data 6", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_New_Row_By_Carriage_Return()
        {
            string input = @"Data 1,Data 2,Data 3"
                + '\r'
                + @"Data 4,Data 5,Data 6";

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
            string input = @"Data 1,Data 2,Data 3"
                + Environment.NewLine
                + @"Data 4,Data 5,Data 6";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();
            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            reader.Read();
            Assert.Equal("Data 4", reader[0]);
            Assert.Equal("Data 5", reader[1]);
            Assert.Equal("Data 6", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_New_Row_By_Line_Feed()
        {
            string input = @"Data 1,Data 2,Data 3"
                + '\n'
                + @"Data 4,Data 5,Data 6";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();
            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            reader.Read();
            Assert.Equal("Data 4", reader[0]);
            Assert.Equal("Data 5", reader[1]);
            Assert.Equal("Data 6", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_New_Row_By_Reverse_New_Line()
        {
            string input = @"Data 1,Data 2,Data 3"
                + '\n' + '\r'
                + @"Data 4,Data 5,Data 6";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();
            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            reader.Read();
            Assert.Equal("Data 4", reader[0]);
            Assert.Equal("Data 5", reader[1]);
            Assert.Equal("Data 6", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Carriage_Return()
        {
            string input = @"Data 1,Data 2,""Data" + '\r' + @"3""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data" + '\r' + "3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Comma()
        {
            string input = @"""Data,1"",Data 2,Data 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data,1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Escaped_Quote()
        {
            string input = @"Data 1,""Data """"2"""""",Data 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal(@"Data ""2""", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Full_New_Line()
        {
            string input = @"Field 1,Field 2,""Field"
                + Environment.NewLine
                + @"3""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Field 1", reader[0]);
            Assert.Equal("Field 2", reader[1]);
            Assert.Equal("Field" + Environment.NewLine + "3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Line_Feed()
        {
            string input = @"Field 1,Field 2,""Field" + '\n' + @"3""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Field 1", reader[0]);
            Assert.Equal("Field 2", reader[1]);
            Assert.Equal("Field" + '\n' + "3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Containing_Reversed_New_Line()
        {
            string input = @"Field 1,Field 2,""Field" + '\n' + '\r' + @"3""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Field 1", reader[0]);
            Assert.Equal("Field 2", reader[1]);
            Assert.Equal("Field" + '\n' + '\r' + "3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Column_Name_Terminated_By_End_Of_File()
        {
            string input = @"Field 1,Field 2,""Field 3";

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
        public void ParseReader_Supports_Quoted_Empty_Fields()
        {
            string input = "\"Data 1\",\"\",\"Data 2\"";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal(string.Empty, reader[1]);
            Assert.Equal("Data 2", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_First_Column()
        {
            string input = @"""Data 1"",Data 2,Data 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Last_Column()
        {
            string input = @"Data 1,Data 2,""Data 3""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Quoted_Second_Column()
        {
            string input = @"Data 1,""Data 2"",Data 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);

            var hasNextRow = reader.Read();
            Assert.False(hasNextRow);
        }

        [Fact]
        public void ParseReader_Supports_Single_Quote_As_Cell_Content()
        {
            string input = "\"Data 1\",\"\"\"\",\"Data 2\"";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("\"", reader[1]);
            Assert.Equal("Data 2", reader[2]);
        }

        [Fact]
        public void ParseReader_Supports_Unicode_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.Unicode));
            reader.Read();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃", reader[0]);
        }

        [Fact]
        public void ParseReader_Supports_UTF32_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.UTF32));
            reader.Read();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃", reader[0]);
        }

        [Fact]
        public void ParseReader_Supports_UTF7_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.UTF7));
            reader.Read();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃", reader[0]);
        }

        [Fact]
        public void ParseReader_Supports_UTF8_Text()
        {
            string input = @"Iñtërnâtiônàlizætiøn☃";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, Encoding.UTF8));
            reader.Read();

            Assert.Equal("Iñtërnâtiônàlizætiøn☃", reader[0]);
        }

        [Fact]
        public void ParseReader_Supports_Varying_Column_Counts()
        {
            string input = @"Field 1,Field 2,Field 3" + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4" + Environment.NewLine
                + @"Data 5" + Environment.NewLine
                + @"Data 6,Data 7,Data 8,Data 9,Data 10";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(3, reader.FieldCount);
            Assert.Equal("Field 1", reader[0]);
            Assert.Equal("Field 2", reader[1]);
            Assert.Equal("Field 3", reader[2]);

            reader.Read();
            Assert.Equal(4, reader.FieldCount);
            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data 3", reader[2]);
            Assert.Equal("Data 4", reader[3]);

            reader.Read();
            Assert.Equal(1, reader.FieldCount);
            Assert.Equal("Data 5", reader[0]);

            reader.Read();
            Assert.Equal(5, reader.FieldCount);
            Assert.Equal("Data 6", reader[0]);
            Assert.Equal("Data 7", reader[1]);
            Assert.Equal("Data 8", reader[2]);
            Assert.Equal("Data 9", reader[3]);
            Assert.Equal("Data 10", reader[4]);
        }

        [Fact]
        public void ParseReader_Supports_Windows_1252_Text()
        {
            string input = @"Data 1,Data 2,Data ½"
                + Environment.NewLine
                + @"Data 3,Data 4,Data æ";

            var windows1252 = Encoding.GetEncoding(1252);

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input, windows1252));
            reader.Read();

            Assert.Equal("Data 1", reader[0]);
            Assert.Equal("Data 2", reader[1]);
            Assert.Equal("Data ½", reader[2]);

            reader.Read();
            Assert.Equal("Data 3", reader[0]);
            Assert.Equal("Data 4", reader[1]);
            Assert.Equal("Data æ", reader[2]);
        }

        [Theory]
        [InlineData(4093, "\r\n")]
        [InlineData(4094, "\r\n")]
        [InlineData(4095, "\r\n")]
        [InlineData(4096, "\r\n")]
        [InlineData(4097, "\r\n")]
        [InlineData(4098, "\r\n")]
        [InlineData(4099, "\r\n")]
        [InlineData(4093, "\r")]
        [InlineData(4094, "\r")]
        [InlineData(4095, "\r")]
        [InlineData(4096, "\r")]
        [InlineData(4097, "\r")]
        [InlineData(4098, "\r")]
        [InlineData(4099, "\r")]
        [InlineData(4093, "\n")]
        [InlineData(4094, "\n")]
        [InlineData(4095, "\n")]
        [InlineData(4096, "\n")]
        [InlineData(4097, "\n")]
        [InlineData(4098, "\n")]
        [InlineData(4099, "\n")]
        public void ParseReader_Supports_NewLineAtBufferEnd(int charCount, string newLine)
        {
            var firstLine = new string('a', charCount);
            var secondLine = "b";

            string input = firstLine + newLine + secondLine;

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();
            Assert.Equal(1, reader.FieldCount);
            Assert.Equal(firstLine, reader[0]);

            reader.Read();
            Assert.Equal(1, reader.FieldCount);
            Assert.Equal(secondLine, reader[0]);
        }

        [Fact]
        public void ParseReader_Depth_Returns_Zero()
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(string.Empty));

            Assert.Equal(0, reader.Depth);
        }

        [Fact]
        public void ParseReader_FieldCount_Yields_Count_For_Header_Row()
        {
            string input = @"Field 1,Field 2,Field 3";

            var parser = new Parser();

            var reader = parser.ParseReader(GetTextReader(input));

            Assert.Equal(3, reader.FieldCount);
        }

        [Fact]
        public void ParseReader_FieldCount_Yields_Count_For_Row()
        {
            string input = @"Data 1,Data 2,Data 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            Assert.Equal(3, reader.FieldCount);
        }

        [Fact]
        public void ParseReader_FieldCount_Yields_Count_For_Each_Row()
        {
            string input = @"Data 1,Data 2,Data 3" + Environment.NewLine
                + @"Data 4,Data 5" + Environment.NewLine
                + @"Data 6,Data 7,Data 8,Data 9" + Environment.NewLine;

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();
            Assert.Equal(3, reader.FieldCount);

            reader.Read();
            Assert.Equal(2, reader.FieldCount);

            reader.Read();
            Assert.Equal(4, reader.FieldCount);
        }

        [Fact]
        public void ParseReader_VisibleFieldCount_Yields_Count_For_Header_Row()
        {
            string input = @"Field 1,Field 2,Field 3";

            var parser = new Parser();

            var reader = parser.ParseReader(GetTextReader(input));

            Assert.Equal(3, reader.VisibleFieldCount);
        }

        [Fact]
        public void ParseReader_VisibleFieldCount_Yields_Count_For_Row()
        {
            string input = @"Data 1,Data 2,Data 3";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            Assert.Equal(3, reader.VisibleFieldCount);
        }

        [Fact]
        public void ParseReader_VisibleFieldCount_Yields_Count_For_Each_Row()
        {
            string input = @"Data 1,Data 2,Data 3" + Environment.NewLine
                + @"Data 4,Data 5" + Environment.NewLine
                + @"Data 6,Data 7,Data 8,Data 9" + Environment.NewLine;

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            reader.Read();
            Assert.Equal(3, reader.VisibleFieldCount);

            reader.Read();
            Assert.Equal(2, reader.VisibleFieldCount);

            reader.Read();
            Assert.Equal(4, reader.VisibleFieldCount);
        }

        [Fact]
        public void ParseReader_RecordsAffected_Returns_Minus_One()
        {
            var parser = new Parser();

            var reader = parser.ParseReader(GetTextReader(string.Empty));

            Assert.Equal(-1, reader.RecordsAffected);
        }

        [Fact]
        public void ParseReader_HasRows_Returns_True()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));

            Assert.True(reader.HasRows);
        }

        [Fact]
        public void ParseReader_HasRows_Returns_False_For_Empty_Input()
        {
            var parser = new Parser();

            var reader = parser.ParseReader(GetTextReader(string.Empty));

            Assert.False(reader.HasRows);
        }

        [Theory]
        [InlineData("2016-05-31")]
        [InlineData("2016/05/31")]
        [InlineData("May 31 2016")]
        [InlineData("31 May 2016")]
        public void ParseReader_GetDateTime_Can_Get(string input)
        {
            var expected = new DateTime(2016, 5, 31);

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(expected, reader.GetDateTime(0));
        }

        [Fact]
        public void ParseReader_GetDateTime_Throws()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Throws<InvalidCastException>(() => reader.GetDateTime(0));
        }

        [Theory]
        [InlineData("-99999999999", -99999999999F)]
        [InlineData("0.00", 0.00F)]
        [InlineData("1.23", 1.23F)]
        [InlineData("99999999999", 99999999999F)]
        public void ParseReader_GetFloat_Can_Get(string input, float expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(expected, reader.GetFloat(0));
        }

        [Fact]
        public void ParseReader_GetFloat_Throws()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Throws<InvalidCastException>(() => reader.GetFloat(0));
        }

        [Theory]
        [InlineData("-99999999999", -99999999999)]
        [InlineData("0.00", 0.00)]
        [InlineData("1.23", 1.23)]
        [InlineData("99999999999", 99999999999)]
        public void ParseReader_GetDecimal_Can_Get(string input, decimal expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(expected, reader.GetDecimal(0));
        }

        [Fact]
        public void ParseReader_GetDecimal_Throws()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Throws<InvalidCastException>(() => reader.GetDecimal(0));
        }

        [Theory]
        [InlineData("-1700", -1.7E+3D)]
        [InlineData("0.00", 0.00D)]
        [InlineData("1.23", 1.23D)]
        [InlineData("1700", 1.7E+3D)]
        public void ParseReader_GetDouble_Can_Get(string input, double expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(expected, reader.GetDouble(0));
        }

        [Fact]
        public void ParseReader_GetDouble_Throws()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Throws<InvalidCastException>(() => reader.GetDouble(0));
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("TRUE", true)]
        [InlineData("TrUe", true)]
        [InlineData("false", false)]
        [InlineData("FALSE", false)]
        [InlineData("FaLsE", false)]
        [InlineData(" true ", true)]
        [InlineData(" false ", false)]
        public void ParseReader_GetBoolean_Can_Get(string input, bool expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(expected, reader.GetBoolean(0));
        }

        [Fact]
        public void ParseReader_GetBoolean_Throws()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Throws<InvalidCastException>(() => reader.GetBoolean(0));
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("1", 1)]
        [InlineData("42", 42)]
        [InlineData("255", 255)]
        public void ParseReader_GetByte_Can_Get(string input, byte expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(expected, reader.GetByte(0));
        }

        [Fact]
        public void ParseReader_GetByte_Throws()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Throws<InvalidCastException>(() => reader.GetByte(0));
        }

        [Theory]
        [InlineData("ca761232ed4211cebacd00aa0057b223")]
        [InlineData("CA761232-ED42-11CE-BACD-00AA0057B223")]
        [InlineData("ca761232-ed42-11ce-bacd-00aa0057b223")]
        [InlineData("{CA761232-ED42-11CE-BACD-00AA0057B223}")]
        [InlineData("(CA761232-ED42-11CE-BACD-00AA0057B223)")]
        public void ParseReader_GetGuid_Can_Get(string input)
        {
            var expected = new Guid("CA761232-ED42-11CE-BACD-00AA0057B223");

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(expected, reader.GetGuid(0));
        }

        [Fact]
        public void ParseReader_GetGuid_Throws()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Throws<InvalidCastException>(() => reader.GetGuid(0));
        }

        [Theory]
        [InlineData("42", 42)]
        [InlineData("1", 1)]
        [InlineData("0", 0)]
        [InlineData("-1", -1)]
        public void ParseReader_GetInt16_Can_Get(string input, short expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(expected, reader.GetInt16(0));
        }

        [Fact]
        public void ParseReader_GetInt16_Throws()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Throws<InvalidCastException>(() => reader.GetInt16(0));
        }

        [Fact]
        public void ParseReader_GetInt32_Can_Get()
        {
            var input = "32768";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(32768, reader.GetInt32(0));
        }

        [Fact]
        public void ParseReader_GetInt32_Throws()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Throws<InvalidCastException>(() => reader.GetInt32(0));
        }

        [Fact]
        public void ParseReader_GetInt64_Can_Get()
        {
            var input = "2147483648";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Equal(2147483648L, reader.GetInt64(0));
        }

        [Fact]
        public void ParseReader_GetInt64_Throws()
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.Throws<InvalidCastException>(() => reader.GetInt64(0));
        }

        [Fact]
        public void ParseReader_IsDBNull_Returns_True()
        {
            var input = ",";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.True(reader.IsDBNull(0));
        }

        [Fact]
        public void ParseReader_IsDBNull_Returns_False()
        {
            var input = "Data 1,Data 2";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            Assert.False(reader.IsDBNull(0));
        }

        [Theory]
        [InlineData("Data 1", -1)]
        [InlineData("Data 1", 1)]
        [InlineData("Data 1", 2)]
        [InlineData("Data 1,Data 2", 2)]
        public void ParseReader_GetBytes_OrdinalOutOfRange_Throws(string input, int ordinal)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            var buffer = new byte[1];
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetBytes(ordinal, 0, buffer, 0, 1));
        }

        [Theory]
        [InlineData("Data 1", 6, 6)]
        [InlineData("Foo", 3, 3)]
        public void ParseReader_GetBytes_ReturnsExpectedCountWithNullBuffer(string input, int length, int expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            var bytesCopied = reader.GetBytes(0, 0, null, 0, length);

            Assert.Equal(expected, bytesCopied);
        }

        [Theory]
        [InlineData("Data 1", 1, "D")]
        [InlineData("Data 1", 2, "Da")]
        [InlineData("Data 1", 6, "Data 1")]
        [InlineData("Foo", 3, "Foo")]
        [InlineData("Foo", 4, "Foo")]
        public void ParseReader_GetBytes_CopiesExpectedCountOfBytes(string input, int length, string expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            var buffer = new byte[length];
            var bytesCopied = reader.GetBytes(0, 0, buffer, 0, length);

            Assert.Equal(expected.Length, bytesCopied);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal((byte)expected[i], buffer[i]);
            }
        }

        [Theory]
        [InlineData("Data 1", 0, "Data 1")]
        [InlineData("Data 1,Data 2", 0, "Data 1")]
        [InlineData("Data 1,Data 2", 1, "Data 2")]
        [InlineData("Data 1,Data 2,Data 3", 2, "Data 3")]
        public void ParseReader_GetBytes_CopiesFromExpectedOrdinal(string input, int ordinal, string expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            var buffer = new byte[6];
            reader.GetBytes(ordinal, 0, buffer, 0, 6);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal((byte)expected[i], buffer[i]);
            }
        }

        [Theory]
        [InlineData("Data 1", -1)]
        [InlineData("Data 1", 1)]
        [InlineData("Data 1", 2)]
        [InlineData("Data 1,Data 2", 2)]
        public void ParseReader_GetChars_OrdinalOutOfRange_Throws(string input, int ordinal)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            var buffer = new char[1];
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetChars(ordinal, 0, buffer, 0, 1));
        }

        [Theory]
        [InlineData(1, -1)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        public void ParseReader_GetChars_BufferOffsetOutOfRange_Throws(int bufferLength, int bufferOffset)
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            var buffer = new char[bufferLength];
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetChars(0, 0, buffer, bufferOffset, 1));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, 2)]
        [InlineData(1, 2)]
        [InlineData(2, 3)]
        [InlineData(2, 4)]
        public void ParseReader_GetChars_BufferNotLongEnough_Throws(int bufferLength, int length)
        {
            var input = "Data 1";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            var buffer = new char[bufferLength];
            Assert.Throws<ArgumentException>(() => reader.GetChars(0, 0, buffer, 0, length));
        }

        [Theory]
        [InlineData("Data 1", 6, 6)]
        [InlineData("Foo", 3, 3)]
        public void ParseReader_GetChars_ReturnsExpectedCountWithNullBuffer(string input, int length, int expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            var charsCopied = reader.GetChars(0, 0, null, 0, length);

            Assert.Equal(expected, charsCopied);
        }

        [Theory]
        [InlineData("Data 1", 1, "D")]
        [InlineData("Data 1", 2, "Da")]
        [InlineData("Data 1", 6, "Data 1")]
        [InlineData("Foo", 3, "Foo")]
        [InlineData("Foo", 4, "Foo")]
        public void ParseReader_GetChars_CopiesExpectedCountOfChars(string input, int length, string expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            var buffer = new char[length];
            var charsCopied = reader.GetChars(0, 0, buffer, 0, length);

            Assert.Equal(expected.Length, charsCopied);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], buffer[i]);
            }
        }

        [Theory]
        [InlineData("Data 1", 0, "Data 1")]
        [InlineData("Data 1,Data 2", 0, "Data 1")]
        [InlineData("Data 1,Data 2", 1, "Data 2")]
        [InlineData("Data 1,Data 2,Data 3", 2, "Data 3")]
        public void ParseReader_GetChars_CopiesFromExpectedOrdinal(string input, int ordinal, string expected)
        {
            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var reader = parser.ParseReader(GetTextReader(input));
            reader.Read();

            var buffer = new char[6];
            reader.GetChars(ordinal, 0, buffer, 0, 6);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], buffer[i]);
            }
        }

        [Theory]
        [InlineData("田中さんにあげて下さい", 65001)]
        [InlineData("パーティーへ行かないか", 65001)]
        [InlineData("田中さんにあげて下さい", 12000)]
        [InlineData("パーティーへ行かないか", 12000)]
        [InlineData("田中さんにあげて下さい", 1200)]
        [InlineData("パーティーへ行かないか", 1200)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Nested using statements")]
        public void ParseReader_GetBytes_RoundTripsBytes(string input, int codepage)
        {
            var encoding = Encoding.GetEncoding(codepage);
            byte[] inputBytes = encoding.GetBytes(input);
            byte[] outputBytes = new byte[inputBytes.Length];

            using (var ms = new MemoryStream(inputBytes))
            using (var sr = new StreamReader(ms, encoding))
            {
                var parser = new Parser
                {
                    UseFirstRowAsColumnHeaders = false
                };

                using (var reader = parser.ParseReader(sr, encoding))
                {
                    reader.Read();

                    reader.GetBytes(0, 0, outputBytes, 0, outputBytes.Length);
                }
            }

            Assert.Equal<byte>(inputBytes, outputBytes);
        }

        [Theory]
        [InlineData(0, new[] { 97, 98, 99 }, new[] { 97, 98, 99 })]
        [InlineData(1, new[] { 97, 98, 99 }, new[] { 98, 99 })]
        [InlineData(2, new[] { 227, 129, 149 }, new[] { 149 })]
        public void ParseReader_GetBytes_CopiesFromOffset(
            int offset,
            int[] inputInts,
            int[] outputInts)
        {
            var encoding = Encoding.UTF8;
            var inputBytes = inputInts.Select(x => (byte)x).ToArray();
            var outputBytes = outputInts.Select(x => (byte)x).ToArray();
            var inputString = new string(encoding.GetChars(inputBytes));

            var readerLength = 0L;
            var readerOutput = new byte[outputBytes.Length];
            using (var sr = new StringReader(inputString))
            {
                var parser = new Parser
                {
                    UseFirstRowAsColumnHeaders = false
                };

                using (var reader = parser.ParseReader(sr, encoding))
                {
                    reader.Read();

                    readerLength = reader.GetBytes(
                        0,
                        offset,
                        readerOutput,
                        0,
                        outputBytes.Length);
                }
            }

            Assert.Equal(outputBytes.Length, readerLength);
            Assert.Equal<byte>(outputBytes, readerOutput);
        }

        [Theory]
        [InlineData(1, "123")]
        [InlineData(1, "xy")]
        [InlineData(3, "xy1234,z")]
        public void ParseReader_GetBytes_ReadsUpToBufferLength(int bufferLength, string inputText)
        {
            var buffer = new byte[bufferLength];
            var bytesRead = 0L;

            using (var sr = new StringReader(inputText))
            {
                var parser = new Parser
                {
                    UseFirstRowAsColumnHeaders = false
                };

                using (var reader = parser.ParseReader(sr))
                {
                    reader.Read();

                    bytesRead = reader.GetBytes(
                        0,
                        0,
                        buffer,
                        0,
                        inputText.Length);
                }
            }

            Assert.Equal(bufferLength, bytesRead);
        }
    }
}
