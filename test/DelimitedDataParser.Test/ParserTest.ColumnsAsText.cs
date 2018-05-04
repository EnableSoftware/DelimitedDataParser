using System;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ParserTest
    {
        [Fact]
        public void Supports_ColumnsAsText_DataNotInFormat()
        {
            string input = @"""Field 1"",""Field 2""" + Environment.NewLine +
                @"""=""""Data 1"""""",""=""""Data 2""""""" + Environment.NewLine +
                @"""Data 3"",""=""""Data 4""""""";

            var parser = new Parser();

            parser.SetColumnsAsText(new[] { "Field 1", "Field 2" });

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
            Assert.Equal("Data 3", output.Rows[1][0]);
            Assert.Equal("Data 4", output.Rows[1][1]);
        }

        [Fact]
        public void Supports_ColumnsAsText_Format()
        {
            string input = @"""=""""Data 1"""""",""Data 2""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal(@"=""Data 1""", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
        }

        [Fact]
        public void Supports_ColumnsAsText_FormatInLastCell()
        {
            string input = @"""=""""Data 1""""""";

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            var output = parser.Parse(GetTextReader(input));

            Assert.Single(output.Columns);
            Assert.Equal(@"=""Data 1""", output.Rows[0][0]);
        }

        [Fact]
        public void Supports_ColumnsAsText_Multiple()
        {
            string input = @"""Field 1"",""Field 2""" + Environment.NewLine +
                @"""=""""Data 1"""""",""=""""Data 2""""""";

            var parser = new Parser();

            parser.SetColumnsAsText(new[] { "Field 1", "Field 2" });

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
        }

        [Fact]
        public void Supports_ColumnsAsText_None()
        {
            string input = @"""Field 1"",""Field 2""" + Environment.NewLine +
                @"""Data 1"",""=""""Data 2""""""";

            var parser = new Parser();

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal(@"=""Data 2""", output.Rows[0][1]);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single()
        {
            string input = @"""Field 1"",""Field 2""" + Environment.NewLine +
                @"""Data 1"",""=""""Data 2""""""";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = true;

            parser.SetColumnsAsText(new[] { "Field 2" });

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal("Data 2", output.Rows[0][1]);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single_NonExistant()
        {
            string input = @"""Field 1"",""Field 2""" + Environment.NewLine +
                @"""Data 1"",""=""""Data 2""""""";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = true;

            parser.SetColumnsAsText(new[] { "Field 3" });

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("Field 1", output.Columns[0].ColumnName);
            Assert.Equal("Field 2", output.Columns[1].ColumnName);
            Assert.Equal("Data 1", output.Rows[0][0]);
            Assert.Equal(@"=""Data 2""", output.Rows[0][1]);
        }
    }
}
