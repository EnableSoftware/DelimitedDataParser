﻿using System;
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

            Encoding windows1252 = Encoding.GetEncoding(1252);

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
    }
}
