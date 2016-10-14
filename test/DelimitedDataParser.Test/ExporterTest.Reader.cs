using System;
using System.Data;
using System.Data.Common;
using Moq;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ExporterTest
    {
        [Fact]
        public void ExportReader_Can_Load_Input()
        {
            // Arrange
            var reader = new Mock<DbDataReader>();

            reader
                .Setup(o => o.GetSchemaTable())
                .Returns(new DataTable());

            var sut = new Exporter();

            // Act
            var output = sut.ExportToString(reader.Object);

            // Assert
            Assert.NotNull(output);
        }

        [Fact]
        public void ExportReader_Can_Parse_Empty_Fields()
        {
            // Arrange
            var reader = new Mock<DbDataReader>();

            var schema = CreateDataTable();
            AddColumn(schema, SchemaTableColumn.ColumnName);

            AddRow(schema, "Column1");
            AddRow(schema, "Column2");

            reader
                .Setup(o => o.GetSchemaTable())
                .Returns(schema);

            reader.SetupGet(o => o.HasRows).Returns(true);
            reader.SetupGet(o => o.FieldCount).Returns(2);

            reader
                .Setup(o => o.IsDBNull(It.IsAny<int>()))
                .Returns(false);

            reader
                .SetupSequence(o => o.Read())
                .Returns(true)
                .Returns(false);

            reader
                .SetupSequence(o => o.GetName(It.IsAny<int>()))
                .Returns("Column1")
                .Returns("Column2");

            reader
                .Setup(o => o.GetValue(It.IsAny<int>()))
                .Returns(string.Empty);

            var sut = new Exporter();

            // Act
            var output = sut.ExportToString(reader.Object);

            // Assert
            Assert.Equal(
                @"""Column1"",""Column2""" + Environment.NewLine
                + @""""",""""",
                output);
        }

        // TODO Add remaining test cases.
        // TODO Factor out `DbDataReader` set up.
    }
}
