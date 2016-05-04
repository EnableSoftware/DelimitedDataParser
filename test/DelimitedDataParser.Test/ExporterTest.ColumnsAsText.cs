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
            AddColumn(input, "Field 1");

            AddRow(input, @"Dat""a 1");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Field 1" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1""" + Environment.NewLine
                + @"""=""""Dat""""a 1""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Data_Ending_With_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");

            AddRow(input, @"Data 1""");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Field 1" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1""" + Environment.NewLine
                + @"""=""""Data 1""""""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Data_Starting_With_Quote()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");

            AddRow(input, @"""Data 1");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Field 1" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1""" + Environment.NewLine
                + @"""=""""""""Data 1""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Multiple()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Dat,a 1", "Dat,a 2");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Field 1", "Field 2" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""=""""Dat,a 1"""""",""=""""Dat,a 2""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_NewLine()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");

            AddRow(input, "Dat" + Environment.NewLine + "a 1");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Field 1" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1""" + Environment.NewLine
                + @"""=""""Dat" + Environment.NewLine + @"a 1""""""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_None()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Dat,a 1", "Dat,a 2");

            var exporter = new Exporter();

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Dat,a 1"",""Dat,a 2""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single_First()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Dat,a 1", "Dat,a 2");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Field 1" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""=""""Dat,a 1"""""",""Dat,a 2""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single_MiddleOfThree()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");
            AddColumn(input, "Field 3");

            AddRow(input, "Dat,a 1", "Dat,a 2", "Dat,a 3");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Field 2" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2"",""Field 3""" + Environment.NewLine
                + @"""Dat,a 1"",""=""""Dat,a 2"""""",""Dat,a 3""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single_NonExistant()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Dat,a 1", "Dat,a 2");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Field 3" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Dat,a 1"",""Dat,a 2""",
                output);
        }

        [Fact]
        public void Supports_ColumnsAsText_Single_Second()
        {
            var input = CreateDataTable();
            AddColumn(input, "Field 1");
            AddColumn(input, "Field 2");

            AddRow(input, "Dat,a 1", "Dat,a 2");

            var exporter = new Exporter();

            exporter.SetColumnsAsText(new[] { "Field 2" });

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Field 1"",""Field 2""" + Environment.NewLine
                + @"""Dat,a 1"",""=""""Dat,a 2""""""",
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
