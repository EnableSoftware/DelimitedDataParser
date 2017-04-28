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
        public void Can_Parse_Empty_Stream()
        {
            var parser = new Parser();
            var output = parser.Parse(GetTextReader(string.Empty));

            Assert.Equal(0, output.Rows.Count);
            Assert.Equal(0, output.Columns.Count);
        }

        [Fact]
        public void Fails_Without_Valid_Input()
        {
            var parser = new Parser();

            Assert.Throws<ArgumentNullException>(() => parser.Parse(null));
        }

        [Fact]
        public void Fails_Without_Valid_Encoding()
        {
            var parser = new Parser();

            Assert.Throws<ArgumentNullException>(() => parser.Parse(GetTextReader(string.Empty), null));
        }

        [Fact]
        public void Can_Parse_Column_Names_From_First_Row()
        {
            string input = @"Field 1,Field 2,Field 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Field 3", output.Columns[2].ColumnName);
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
        public void Supports_Duplicate_Column_Names()
        {
            string input = @"Field 1,Field 2,Field 1,Field 1,Field 3,Field 2"
                + Environment.NewLine
                + @"Data 1,Data 2,Data 1";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(6, output.Columns.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Field 11", output.Columns[2].ColumnName);
            Assert.Equal("Field 12", output.Columns[3].ColumnName);
            Assert.Equal("Field 3", output.Columns[4].ColumnName);
            Assert.Equal("Field 21", output.Columns[5].ColumnName);
            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
            Assert.Equal("Data 1", output.Rows[0][2]);
            Assert.Equal(DBNull.Value, output.Rows[0][3]);
            Assert.Equal(DBNull.Value, output.Rows[0][4]);
            Assert.Equal(DBNull.Value, output.Rows[0][5]);
        }

        [Fact]
        public void Can_Parse_Fields()
        {
            string input = @"Field 1,Field 2,Field 3" + Environment.NewLine
                + @"Data 1,Data 2,Data 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(1, output.Rows.Count);
            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Field 3", output.Columns[2].ColumnName);

            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
            Assert.Equal("Data 3", output.Rows[0][2]);
        }

        [Fact]
        public void Can_Parse_Empty_Fields()
        {
            string input = @"Col 1,Col 2,Col 3" + Environment.NewLine
                + @"Data 1,," + Environment.NewLine
                + @"," + Environment.NewLine
                + @"""""";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Rows.Count);
            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Col 1", output.Columns[0].ColumnName);
            Assert.Equal("Col 2", output.Columns[1].ColumnName);
            Assert.Equal("Col 3", output.Columns[2].ColumnName);

            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal(string.Empty, output.Rows[0][1]);
            Assert.Equal(string.Empty, output.Rows[0][2]);

            Assert.Equal(string.Empty, output.Rows[1][0]);
            Assert.Equal(string.Empty, output.Rows[1][1]);
            Assert.Equal(DBNull.Value, output.Rows[1][2]);

            Assert.Equal(string.Empty, output.Rows[2][0]);
            Assert.Equal(DBNull.Value, output.Rows[2][1]);
            Assert.Equal(DBNull.Value, output.Rows[2][2]);
        }

        [Fact]
        public void Supports_First_Row_As_Data()
        {
            var input = @"Data 1,Data 2,Data 3" + Environment.NewLine
                + @"Data 4,Data 5,Data 6" + Environment.NewLine
                + @"Data 7,Data 8,Data 9" + Environment.NewLine;

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal(3, output.Rows.Count);
            Assert.Equal("Column1", output.Columns[0].ColumnName);
            Assert.Equal("Column2", output.Columns[1].ColumnName);
            Assert.Equal("Column3", output.Columns[2].ColumnName);
            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
            Assert.Equal("Data 3", output.Rows[0][2]);
            Assert.Equal("Data 4", output.Rows[1][0]);
            Assert.Equal("Data 5", output.Rows[1][1]);
            Assert.Equal("Data 6", output.Rows[1][2]);
            Assert.Equal("Data 7", output.Rows[2][0]);
            Assert.Equal("Data 8", output.Rows[2][1]);
            Assert.Equal("Data 9", output.Rows[2][2]);
        }

        [Fact]
        public void Supports_Large_Cell_Content()
        {
            var cellContentLength = 10000000;
            var input = new string('a', cellContentLength);

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(1, output.Columns.Count);
            Assert.Equal(1, output.Rows.Count);
            Assert.Equal(cellContentLength, ((string)output.Rows[0][0]).Length);
        }

        [Fact]
        public void Does_Not_Support_More_Data_Columns_Than_Header_Columns()
        {
            var input = @"Field 1,Field 2,Field 3"
                + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Field 3", output.Columns[2].ColumnName);
            Assert.Equal(3, output.Rows[0].ItemArray.Length);
            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
            Assert.Equal("Data 3", output.Rows[0][2]);
        }

        [Fact]
        public void Supports_Multiple_Blank_Rows()
        {
            var input = @"Field 1,Field 2,Field 3" + Environment.NewLine
                + Environment.NewLine
                + @"Data 1,Data 2,Data 3";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));
            
            Assert.Equal(3, output.Columns.Count);
            Assert.Equal(2, output.Rows.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Field 3", output.Columns[2].ColumnName);
            Assert.Equal(DBNull.Value, output.Rows[0][0]);
            Assert.Equal(DBNull.Value, output.Rows[0][1]);
            Assert.Equal(DBNull.Value, output.Rows[0][2]);
            Assert.Equal("Data 1", output.Rows[1][0]);
            Assert.Equal("Data 2", output.Rows[1][1]);
            Assert.Equal("Data 3", output.Rows[1][2]);
        }

        [Fact]
        public void Supports_Varying_Column_Counts()
        {
            var input = @"Field 1,Field 2,Field 3" + Environment.NewLine
                + @"Data 1,Data 2,Data 3,Data 4" + Environment.NewLine
                + @"Data 5" + Environment.NewLine
                + @"Data 6,Data 7,Data 8,Data 9,Data 10";

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(3, output.Columns.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Field 3", output.Columns[2].ColumnName);

            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
            Assert.Equal("Data 3", output.Rows[0][2]);

            Assert.Equal("Data 5", output.Rows[1][0]);
            Assert.Equal(DBNull.Value, output.Rows[1][1]);
            Assert.Equal(DBNull.Value, output.Rows[1][2]);

            Assert.Equal("Data 6", output.Rows[2][0]);
            Assert.Equal("Data 7", output.Rows[2][1]);
            Assert.Equal("Data 8", output.Rows[2][2]);
        }

        private static TextReader GetTextReader(string input)
        {
            if (input == null)
            {
                input = string.Empty;
            }

            return new StringReader(input);
        }

        private static StreamReader GetTextReader(string input, Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
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
