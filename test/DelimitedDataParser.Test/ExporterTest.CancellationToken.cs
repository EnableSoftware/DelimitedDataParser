using System;
using System.Data;
using System.Globalization;
using System.IO;
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

            var input = new DataTable();
            AddColumn(input, "C1");
            AddRow(input, "R1C1");

            var exporter = new Exporter();

            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                Assert.Throws<OperationCanceledException>(() => exporter.Export(input, sw, cts.Token));
            }
        }

        [Fact]
        public void Can_Cancel_ExportReader()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var input = new DataTable();
            AddColumn(input, "C1");
            AddRow(input, "R1C1");

            var exporter = new Exporter();

            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                Assert.Throws<OperationCanceledException>(() => exporter.ExportReader(input.CreateDataReader(), sw, cts.Token));
            }
        }
    }
}
