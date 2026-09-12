using System;
using Xunit;

namespace EDCSuiteRedux.Core.Tests
{
    public class BinaryDocumentTests
    {
        [Fact]
        public void ConstructorClonesInput()
        {
            var source = new byte[] { 1, 2, 3 };
            var document = new BinaryDocument(source);
            source[0] = 9;

            Assert.Equal(1, document.ReadByte(0));
        }

        [Fact]
        public void WriteBytesAndToArrayReturnIndependentData()
        {
            var document = new BinaryDocument(new byte[] { 0, 0, 0 });
            document.WriteBytes(1, new byte[] { 4, 5 });

            var result = document.ToArray();
            result[1] = 8;

            Assert.Equal(new byte[] { 0, 4, 5 }, document.ToArray());
        }

        [Theory]
        [InlineData(-1, 1)]
        [InlineData(2, 2)]
        public void AccessOutsideDocumentThrows(int offset, int count)
        {
            var document = new BinaryDocument(new byte[] { 1, 2, 3 });

            Assert.Throws<ArgumentOutOfRangeException>(() => document.ReadBytes(offset, count));
        }
    }
}
