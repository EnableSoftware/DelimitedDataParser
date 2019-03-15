using System;
using System.Threading;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ExporterTest
    {
        [Fact]
        public void Can_Cancel_Export()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var input = CreateDataTable();
            AddColumn(input, "C1");
            AddRow(input, "R1C1");

            var exporter = new Exporter();

            Assert.Throws<OperationCanceledException>(() => exporter.ExportToString(input, cts.Token));
        }

        [Fact]
        public void Can_Cancel_ExportReader()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var input = CreateDataTable();
            AddColumn(input, "C1");
            AddRow(input, "R1C1");

            var exporter = new Exporter();

            Assert.Throws<OperationCanceledException>(() => exporter.ExportToString(input.CreateDataReader(), cts.Token));
        }
    }
}
