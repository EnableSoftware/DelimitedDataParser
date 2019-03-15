using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Threading;

namespace DelimitedDataParser
{
    internal static class ExporterExtensions
    {
        public static string ExportToString(this Exporter exporter, DbDataReader dataReader, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                exporter.ExportReader(dataReader, sw, cancellationToken);
                return sw.ToString();
            }
        }

        public static string ExportToString(this Exporter exporter, DataTable dataTable, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                exporter.Export(dataTable, sw, cancellationToken);
                return sw.ToString();
            }
        }
    }
}
