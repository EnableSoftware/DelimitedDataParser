using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DelimitedDataParser
{
    public partial class ParserTest
    {
        [TestMethod]
        public void Ensure_CRLF_Equals_Environment_NewLine()
        {
            Assert.AreEqual("\r\n", Environment.NewLine);
        }

        [TestMethod]
        public void Supports_NonQuotedHeader_ThenCRLF_ThenNonQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\r\n", "Bar", "Bar");
        }

        [TestMethod]
        public void Supports_NonQuotedHeader_ThenCR_ThenNonQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\r", "Bar", "Bar");
        }

        [TestMethod]
        public void Supports_NonQuotedHeader_ThenLF_ThenNonQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\n", "Bar", "Bar");
        }

        [TestMethod]
        public void Supports_NonQuotedHeader_ThenCRLF_ThenQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\r\n", "\"Bar\"", "Bar");
        }

        [TestMethod]
        public void Supports_NonQuotedHeader_ThenCR_ThenQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\r", "\"Bar\"", "Bar");
        }

        [TestMethod]
        public void Supports_NonQuotedHeader_ThenLF_ThenQuotedContent()
        {
            PerformNewLineTest("Foo", "Foo", "\n", "\"Bar\"", "Bar");
        }

        [TestMethod]
        public void Supports_QuotedHeader_ThenCRLF_ThenNonQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\r\n", "Bar", "Bar");
        }

        [TestMethod]
        public void Supports_QuotedHeader_ThenCR_ThenNonQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\r", "Bar", "Bar");
        }

        [TestMethod]
        public void Supports_QuotedHeader_ThenLF_ThenNonQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\n", "Bar", "Bar");
        }

        [TestMethod]
        public void Supports_QuotedHeader_ThenCRLF_ThenQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\r\n", "\"Bar\"", "Bar");
        }

        [TestMethod]
        public void Supports_QuotedHeader_ThenCR_ThenQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\r", "\"Bar\"", "Bar");
        }

        [TestMethod]
        public void Supports_QuotedHeader_ThenLF_ThenQuotedContent()
        {
            PerformNewLineTest("\"Foo\"", "Foo", "\n", "\"Bar\"", "Bar");
        }

        private static void PerformNewLineTest(string contentHeaderRow, string expectedHeader, string separator, string contentDataRow, string expectedData)
        {
            var input = string.Concat(contentHeaderRow, separator, contentDataRow);

            var parser = new Parser(GetTextReader(input));
            var output = parser.Parse();

            Assert.AreEqual(1, output.Columns.Count, "Expected 1 column.");
            Assert.AreEqual(1, output.Rows.Count, "Expected 1 column.");

            var col = output.Columns[0];
            var row = output.Rows[0];

            Assert.AreEqual(expectedHeader, col.ColumnName, "Column name incorrect.");
            Assert.AreEqual(expectedData, row[col], "Row content incorrect.");
        }
    }
}