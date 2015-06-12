using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DelimitedDataParser
{
    public partial class ParserTest
    {
        [TestMethod]
        public void Supports_ColumnsAsText_Format()
        {
            string input = @"""=""""x"""""",""y""";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = false;

            var output = parser.Parse();

            Assert.AreEqual(2, output.Columns.Count, "Expected 2 columns.");
            Assert.AreEqual(@"=""x""", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual(@"y", output.Rows[0][1], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_ColumnsAsText_FormatInLastCell()
        {
            string input = @"""=""""x""""""";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = false;

            var output = parser.Parse();

            Assert.AreEqual(1, output.Columns.Count, "Expected 1 column.");
            Assert.AreEqual(@"=""x""", output.Rows[0][0], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_ColumnsAsText_None()
        {
            string input = @"""One"",""Two""" + Environment.NewLine +
                @"""Three"",""=""""Four""""""";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = true;

            var output = parser.Parse();

            Assert.AreEqual(2, output.Columns.Count, "Expected 2 columns.");
            Assert.AreEqual("One", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Two", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Three", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual(@"=""Four""", output.Rows[0][1], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_ColumnsAsText_Single_NonExistant()
        {
            string input = @"""One"",""Two""" + Environment.NewLine +
                @"""Three"",""=""""Four""""""";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = true;

            parser.SetColumnsAsText(new[] { "Three" });

            var output = parser.Parse();

            Assert.AreEqual(2, output.Columns.Count, "Expected 2 columns.");
            Assert.AreEqual("One", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Two", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Three", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual(@"=""Four""", output.Rows[0][1], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_ColumnsAsText_Single()
        {
            string input = @"""One"",""Two""" + Environment.NewLine +
                @"""Three"",""=""""Four""""""";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = true;

            parser.SetColumnsAsText(new[] { "Two" });

            var output = parser.Parse();

            Assert.AreEqual(2, output.Columns.Count, "Expected 2 columns.");
            Assert.AreEqual("One", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Two", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Three", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Four", output.Rows[0][1], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_ColumnsAsText_Multiple()
        {
            string input = @"""One"",""Two""" + Environment.NewLine +
                @"""=""""Three"""""",""=""""Four""""""";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = true;

            parser.SetColumnsAsText(new[] { "One", "Two" });

            var output = parser.Parse();

            Assert.AreEqual(2, output.Columns.Count, "Expected 2 columns.");
            Assert.AreEqual("One", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Two", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Three", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Four", output.Rows[0][1], "Field data incorrect.");
        }

        [TestMethod]
        public void Supports_ColumnsAsText_DataNotInFormat()
        {
            string input = @"""One"",""Two""" + Environment.NewLine +
                @"""=""""Three"""""",""=""""Four""""""" + Environment.NewLine +
                @"""Five"",""=""""Six""""""";

            var parser = new Parser(GetTextReader(input));
            parser.UseFirstRowAsColumnHeaders = true;

            parser.SetColumnsAsText(new[] { "One", "Two" });

            var output = parser.Parse();

            Assert.AreEqual(2, output.Columns.Count, "Expected 2 columns.");
            Assert.AreEqual("One", output.Columns[0].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Two", output.Columns[1].ColumnName, "Column name incorrect.");
            Assert.AreEqual("Three", output.Rows[0][0], "Field data incorrect.");
            Assert.AreEqual("Four", output.Rows[0][1], "Field data incorrect.");
            Assert.AreEqual("Five", output.Rows[1][0], "Field data incorrect.");
            Assert.AreEqual("Six", output.Rows[1][1], "Field data incorrect.");
        }
    }
}