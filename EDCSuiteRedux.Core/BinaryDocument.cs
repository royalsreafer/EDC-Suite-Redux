using System;

namespace EDCSuiteRedux.Core
{
    /// <summary>
    /// Mutable binary content with bounds-checked access for parser and editor code.
    /// </summary>
    public sealed class BinaryDocument
    {
        private readonly byte[] _bytes;

        public BinaryDocument(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            _bytes = (byte[])bytes.Clone();
        }

        public int Length => _bytes.Length;

        public byte ReadByte(int offset)
        {
            ValidateRange(offset, 1);
            return _bytes[offset];
        }

        public byte[] ReadBytes(int offset, int count)
        {
            ValidateRange(offset, count);
            var result = new byte[count];
            Buffer.BlockCopy(_bytes, offset, result, 0, count);
            return result;
        }

        public void WriteByte(int offset, byte value)
        {
            ValidateRange(offset, 1);
            _bytes[offset] = value;
        }

        public void WriteBytes(int offset, byte[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            ValidateRange(offset, values.Length);
            Buffer.BlockCopy(values, 0, _bytes, offset, values.Length);
        }

        public byte[] ToArray()
        {
            return (byte[])_bytes.Clone();
        }

        private void ValidateRange(int offset, int count)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset > _bytes.Length - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }
    }
}