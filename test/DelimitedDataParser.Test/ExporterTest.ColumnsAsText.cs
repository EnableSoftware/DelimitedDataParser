using System;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ExporterTest
    {
        [Fact]
        public void Supports_ColumnsAsText_Data_Containing_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");

            AddRow(input, @"11""11");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "One" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One""" + Environment.NewLine
                + @"""=""""11""""11""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Data_Ending_With_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");

            AddRow(input, @"1111""");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "One" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One""" + Environment.NewLine
                + @"""=""""1111""""""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Data_Starting_With_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");

            AddRow(input, @"""1111");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "One" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One""" + Environment.NewLine
                + @"""=""""""""1111""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Multiple()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "001,002", "003,004");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "One", "Two" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""=""""001,002"""""",""=""""003,004""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_NewLine()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");

            AddRow(input, "123" + Environment.NewLine + "456");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "One" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One""" + Environment.NewLine
                + @"""=""""123" + Environment.NewLine + @"456""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_None()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "001,002", "003,004");

            var exporter = new Exporter();

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""001,002"",""003,004""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single_First()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "001,002", "003,004");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "One" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""=""""001,002"""""",""003,004""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single_MiddleOfThree()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");
            AddColumn(input, "Three");

            AddRow(input, "001,002", "003,004", "005,006");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Two" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two"",""Three""" + Environment.NewLine
                + @"""001,002"",""=""""003,004"""""",""005,006""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single_NonExistant()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "001,002", "003,004");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Three" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""001,002"",""003,004""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single_Second()
        {
            var input = CreateDataTable();
            AddColumn(input, "One");
            AddColumn(input, "Two");

            AddRow(input, "001,002", "003,004");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Two" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One"",""Two""" + Environment.NewLine
                + @"""001,002"",""=""""003,004""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_TelephoneNumber()
        {
            var input = CreateDataTable();
            AddColumn(input, "Name");
            AddColumn(input, "Tel");
            AddColumn(input, "Country");

            AddRow(input, "Enable", "0845 519 11 00", "UK");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Tel" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Name"",""Tel"",""Country""" + Environment.NewLine
                + @"""Enable"",""=""""0845 519 11 00"""""",""UK""",
                output);
        }
    }
}
