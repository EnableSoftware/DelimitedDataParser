using System;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ParserTest
    {
        [Fact]
        public void Supports_ColumnsAsText_DataNotInFormat()
        {
            string input = @"""One"",""Two""" + Environment.NewLine +
                @"""=""""Three"""""",""=""""Four""""""" + Environment.NewLine +
                @"""Five"",""=""""Six""""""";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = true;

            parser.SetColumnsAsText(new[] { "One", "Two" });

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("One", output.Columns[0].ColumnName);
            Assert.Equal("Two", output.Columns[1].ColumnName);
            Assert.Equal("Three", output.Rows[0][0]);
            Assert.Equal("Four", output.Rows[0][1]);
            Assert.Equal("Five", output.Rows[1][0]);
            Assert.Equal("Six", output.Rows[1][1]);
        }

        [Fact]
        public void Supports_ColumnsAsText_Format()
        {
            string input = @"""=""""x"""""",""y""";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = false;

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal(@"=""x""", output.Rows[0][0]);
            Assert.Equal(@"y", output.Rows[0][1]);
        }

        [Fact]
        public void Supports_ColumnsAsText_FormatInLastCell()
        {
            string input = @"""=""""x""""""";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = false;

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(1, output.Columns.Count);
            Assert.Equal(@"=""x""", output.Rows[0][0]);
        }

        [Fact]
        public void Supports_ColumnsAsText_Multiple()
        {
            string input = @"""One"",""Two""" + Environment.NewLine +
                @"""=""""Three"""""",""=""""Four""""""";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = true;

            parser.SetColumnsAsText(new[] { "One", "Two" });

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("One", output.Columns[0].ColumnName);
            Assert.Equal("Two", output.Columns[1].ColumnName);
            Assert.Equal("Three", output.Rows[0][0]);
            Assert.Equal("Four", output.Rows[0][1]);
        }

        [Fact]
        public void Supports_ColumnsAsText_None()
        {
            string input = @"""One"",""Two""" + Environment.NewLine +
                @"""Three"",""=""""Four""""""";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = true;

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("One", output.Columns[0].ColumnName);
            Assert.Equal("Two", output.Columns[1].ColumnName);
            Assert.Equal("Three", output.Rows[0][0]);
            Assert.Equal(@"=""Four""", output.Rows[0][1]);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single()
        {
            string input = @"""One"",""Two""" + Environment.NewLine +
                @"""Three"",""=""""Four""""""";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = true;

            parser.SetColumnsAsText(new[] { "Two" });

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("One", output.Columns[0].ColumnName);
            Assert.Equal("Two", output.Columns[1].ColumnName);
            Assert.Equal("Three", output.Rows[0][0]);
            Assert.Equal("Four", output.Rows[0][1]);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single_NonExistant()
        {
            string input = @"""One"",""Two""" + Environment.NewLine +
                @"""Three"",""=""""Four""""""";

            var parser = new Parser();
            parser.UseFirstRowAsColumnHeaders = true;

            parser.SetColumnsAsText(new[] { "Three" });

            var output = parser.Parse(GetTextReader(input));

            Assert.Equal(2, output.Columns.Count);
            Assert.Equal("One", output.Columns[0].ColumnName);
            Assert.Equal("Two", output.Columns[1].ColumnName);
            Assert.Equal("Three", output.Rows[0][0]);
            Assert.Equal(@"=""Four""", output.Rows[0][1]);
        }
    }
}
