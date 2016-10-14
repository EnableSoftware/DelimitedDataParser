using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;

namespace DelimitedDataParser
{
    internal static class ExporterExtensions
    {
        public static string ExportToString(this Exporter exporter, DbDataReader dataReader)
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                exporter.ExportReader(dataReader, sw);
                return sw.ToString();
            }
        }

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
