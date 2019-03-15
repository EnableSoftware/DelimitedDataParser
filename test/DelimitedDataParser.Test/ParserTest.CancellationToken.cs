using System;
using System.Threading;
using Xunit;

namespace DelimitedDataParser
{
    public partial class ParserTest
    {
        [Fact]
        public void Can_Cancel_Parse()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            Assert.Throws<OperationCanceledException>(() => parser.Parse(GetTextReader("R1C1"), cts.Token));
        }

        [Fact]
        public void Can_Cancel_ParseReader_Read()
        {
            var input = @"One" + Environment.NewLine
                + @"Two";

            var cts = new CancellationTokenSource();

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            using (var reader = parser.ParseReader(GetTextReader(input), cts.Token))
            {
                reader.Read();
                cts.Cancel();
                Assert.Throws<OperationCanceledException>(() => reader.Read());
            }
        }

        [Fact]
        public void Can_Cancel_ParseReader_ReadAsync()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var parser = new Parser
            {
                UseFirstRowAsColumnHeaders = false
            };

            using (var reader = parser.ParseReader(GetTextReader("R1C1")))
            {
                Assert.ThrowsAsync<OperationCanceledException>(() => reader.ReadAsync(cts.Token));
            }
        }
    }
}