using System;
using System.Data;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ExporterTest
    {
        [Fact]
        public void Supports_Use_ExtendedProperty()
        {
            var input = CreateDataTable();
            
            var exporter = new Exporter();

            exporter.UseExtendedPropertyForColumnName("ColumnKey");

            var output = exporter.ExportToString(input);

            Assert.NotNull(output);
        }

        [Fact]
        public void Supports_Replaces_ColumnNames_With_ExtendedProperty()
        {
            var input = CreateDataTable();
            var dataColumn = new DataColumn();
            dataColumn.ColumnName = "One";
            dataColumn.ExtendedProperties.Add("ColumnKey", "Two");
            input.Columns.Add(dataColumn);
            
            var exporter = new Exporter();

            exporter.UseExtendedPropertyForColumnName("ColumnKey");

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""Two""",
                output);
        }

        [Fact]
        public void Supports_Uses_Original_ColumnName_If_ExtendedProperty_Is_Missing()
        {
            var input = CreateDataTable();
            var dataColumn = new DataColumn();
            dataColumn.ColumnName = "One";
            input.Columns.Add(dataColumn);

            var exporter = new Exporter();

            exporter.UseExtendedPropertyForColumnName("ColumnKey");

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""One""",
                output);
        }

        [Fact]
        public void Supports_Mixture_Of_ColumnName_And_ExtendedProperty()
        {
            var input = CreateDataTable();
            var dataColumnOne = new DataColumn();
            dataColumnOne.ColumnName = "One";
            dataColumnOne.ExtendedProperties.Add("ColumnKey", "ExtendedProperty");
            input.Columns.Add(dataColumnOne);

            var dataColumnTwo = new DataColumn();
            dataColumnTwo.ColumnName = "Two";
            input.Columns.Add(dataColumnTwo);

            var exporter = new Exporter();

            exporter.UseExtendedPropertyForColumnName("ColumnKey");

            var output = exporter.ExportToString(input);

            Assert.Equal(
                @"""ExtendedProperty"",""Two""",
                output);
        }

        [Fact]
        public void Fails_With_Invalid_ExtendedProperty()
        {
            var exporter = new Exporter();

            Assert.Throws<ArgumentNullException>(() => exporter.UseExtendedPropertyForColumnName(null));
        }
    }
}
