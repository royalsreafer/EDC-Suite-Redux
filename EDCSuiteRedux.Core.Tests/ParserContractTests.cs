using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace EDCSuiteRedux.Core.Tests;

public class ParserContractTests
{
    public static IEnumerable<object[]> Parsers()
    {
        yield return new object[] { new EDC15PFileParser() };
        yield return new object[] { new EDC15P6FileParser() };
        yield return new object[] { new EDC15VFileParser() };
        yield return new object[] { new EDC15CFileParser() };
        yield return new object[] { new EDC15MFileParser() };
        yield return new object[] { new EDC16FileParser() };
        yield return new object[] { new EDC17FileParser() };
        yield return new object[] { new MSA15FileParser() };
        yield return new object[] { new MSA6FileParser() };
    }

    [Theory]
    [MemberData(nameof(Parsers))]
    public void MetadataExtractionHandlesEmptyInput(IEDCFileParser parser)
    {
        var emptyBytes = Array.Empty<byte>();

        var exception = Record.Exception(() =>
        {
            Assert.Equal(string.Empty, parser.ExtractBoschPartnumber(emptyBytes));
            Assert.Equal(string.Empty, parser.ExtractSoftwareNumber(emptyBytes));
            Assert.Equal(string.Empty, parser.ExtractPartnumber(emptyBytes));
            Assert.Equal(string.Empty, parser.ExtractInfo(emptyBytes));
        });

        Assert.Null(exception);
    }

    [Fact]
    public void Edc16ParserDetectsSyntheticTwoByTwoMap()
    {
        var bytes = new byte[64];
        bytes[0] = 0;
        bytes[1] = 2;
        bytes[2] = 0;
        bytes[3] = 2;

        var filePath = Path.Combine(Path.GetTempPath(), $"edc16-parser-{Guid.NewGuid():N}.bin");
        File.WriteAllBytes(filePath, bytes);

        try
        {
            var parser = new EDC16FileParser();
            var symbols = parser.parseFile(filePath, out var codeBlocks, out var axes);

            Assert.NotEmpty(symbols);
            var symbol = symbols[0];
            Assert.Equal(2, symbol.X_axis_length);
            Assert.Equal(2, symbol.Y_axis_length);
            Assert.Equal(12, symbol.Flash_start_address);
            Assert.Equal(8, symbol.Length);
            Assert.Empty(codeBlocks);
            Assert.Empty(axes);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void ReadingFileDataReleasesFileHandle()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"file-read-{Guid.NewGuid():N}.bin");
        File.WriteAllBytes(filePath, new byte[] { 0x01, 0x02, 0x03, 0x04 });

        try
        {
            var values = Tools.Instance.readdatafromfileasint(filePath, 0, 1, EDCFileType.EDC15P);

            Assert.Equal(0x0201, values[0]);
            using var exclusiveStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.Equal(4, exclusiveStream.Length);
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
