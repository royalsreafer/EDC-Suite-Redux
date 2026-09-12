using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace EDCSuiteRedux.Core.Tests
{
    public class EDC15ParserTests
    {
        public static IEnumerable<object[]> ReadableEdc15Exports()
        {
            yield return new object[]
            {
                "Audi_A3_D1900_R42VT_96kW_0281010308_038906019CK_1243_1037360044_crc33B3CFFC.bin",
                "0281001308"
            };
            yield return new object[]
            {
                "Audi_A3_D1900_R42VT_96kW_0281010561_038906019EF_1331_1037360585_crc3ED18F25.bin",
                "0281010561"
            };
            yield return new object[]
            {
                "Audi_A3_D1900_R42VT_96kW_0281010981_038906019FT_1527_1037363228_crcB9EF1D69.bin",
                "0281010981"
            };
            yield return new object[]
            {
                "Audi_A3_D1900_R4V2T_96kW_0281001308_038906019CK_1243_1037360044_crcF03BD0AF.bin",
                "0281001308"
            };
        }

        [Theory]
        [MemberData(nameof(ReadableEdc15Exports))]
        public void ExtractBoschPartnumberReadsReadableExport(string fileName, string expectedBoschPartnumber)
        {
            var filePath = FindBinary(fileName);
            var parser = new EDC15PFileParser();

            var boschPartnumber = parser.ExtractBoschPartnumber(File.ReadAllBytes(filePath));

            Assert.Equal(expectedBoschPartnumber, boschPartnumber);
        }

        [Fact]
        public void ParseReadableEdc15ExportProducesParserResults()
        {
            var filePath = FindBinary("Audi_A3_D1900_R42VT_96kW_0281010308_038906019CK_1243_1037360044_crc33B3CFFC.bin");
            var parser = new EDC15PFileParser();

            var symbols = parser.parseFile(filePath, out var codeBlocks, out var axes);

            Assert.NotNull(symbols);
            Assert.NotNull(codeBlocks);
            Assert.NotNull(axes);
            Assert.NotEmpty(codeBlocks);
        }

        private static string FindBinary(string fileName)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "binaries", fileName);
                if (File.Exists(candidate)) return candidate;
                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not find the readable EDC15 export.", fileName);
        }
    }
}