using System.Globalization;
using System.IO;

namespace DelimitedDataParser
{
    internal static class ExporterExtensions
    {
        public static string ExportToString(this Exporter exporter)
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                exporter.Export(sw);
                return sw.ToString();
            }
        }
    }
}
