using System;
using System.IO;
using System.Text;
using DelimitedDataParser;

namespace WindowsEncoding
{
    internal class Program
    {
        private static void Main()
        {
            // If code page based character encodings are required when using
            // DelimitedDataParser in a .NET Core app, be sure to include the
            // NuGet package System.Text.Encoding.CodePages.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Attempting to obtain the Windows-1252 encoding will fail on .NET Core
            // if the required code page based encoding is not correctly registered.
            var windows1252 = Encoding.GetEncoding(1252);

            // Try and ensure the console's encoding matches the character encoding
            // of the file input data.
            Console.OutputEncoding = windows1252;

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            using (var stream = new StreamReader("Windows1252.txt", windows1252))
            using (var reader = parser.ParseReader(stream))
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader[0]);
                }
            }
        }
    }
}
