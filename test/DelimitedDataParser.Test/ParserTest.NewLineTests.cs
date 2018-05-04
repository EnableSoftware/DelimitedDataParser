using System;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ParserTest
    {
        [Fact]
        public void Ensure_CRLF_Equals_Environment_NewLine()
        {
            Assert.Equal("\r\n", Environment.NewLine);
        }

        [Fact]
        public void Supports_NonQuotedHeader_ThenCR_ThenNonQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\r", "Bar", "Bar");
        }

        [Fact]
        public void Supports_NonQuotedHeader_ThenCR_ThenQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\r", "\"Bar\"", "Bar");
        }

        [Fact]
        public void Supports_NonQuotedHeader_ThenCRLF_ThenNonQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\r\n", "Bar", "Bar");
        }

        [Fact]
        public void Supports_NonQuotedHeader_ThenCRLF_ThenQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\r\n", "\"Bar\"", "Bar");
        }

        [Fact]
        public void Supports_NonQuotedHeader_ThenLF_ThenNonQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\n", "Bar", "Bar");
        }

        [Fact]
        public void Supports_NonQuotedHeader_ThenLF_ThenQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\n", "\"Bar\"", "Bar");
        }

        [Fact]
        public void Supports_QuotedHeader_ThenCR_ThenNonQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\r", "Bar", "Bar");
        }

        [Fact]
        public void Supports_QuotedHeader_ThenCR_ThenQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\r", "\"Bar\"", "Bar");
        }

        [Fact]
        public void Supports_QuotedHeader_ThenCRLF_ThenNonQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\r\n", "Bar", "Bar");
        }

        [Fact]
        public void Supports_QuotedHeader_ThenCRLF_ThenQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\r\n", "\"Bar\"", "Bar");
        }

        [Fact]
        public void Supports_QuotedHeader_ThenLF_ThenNonQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\n", "Bar", "Bar");
        }

        [Fact]
        public void Supports_QuotedHeader_ThenLF_ThenQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\n", "\"Bar\"", "Bar");
        }

        private static void PerformNewLineTest(string contentHeaderRow, string expectedHeader, string separator, string contentDataRow, string expectedData)
        {
            var input = string.Concat(contentHeaderRow, separator, contentDataRow);

            var parser = new Parser();
            var output = parser.Parse(GetTextReader(input));

            Assert.Single(output.Columns);
            Assert.Single(output.Rows);

            var col = output.Columns[0];
            var row = output.Rows[0];

            Assert.Equal(expectedHeader, col.ColumnName);
            Assert.Equal(expectedData, row[col]);
        }
    }
}
