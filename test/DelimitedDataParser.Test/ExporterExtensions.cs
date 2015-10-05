using System.Data;
using System.Globalization;
using System.IO;

namespace DelimitedDataParser
{
    internal static class ExporterExtensions
    {
        public static string ExportToString(this Exporter exporter, DataTable dataTable)
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                exporter.Export(dataTable, sw);
                return sw.ToString();
            }
        }
    }
}
