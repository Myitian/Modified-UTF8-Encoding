using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Myitian.Text;
public partial class ModifiedUTF8Encoding
{
    public class ModifiedUTF8Encoder : Encoder
    {
        #region Convert
        public override unsafe void Convert(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            if (chars is null || bytes is null)
                throw new ArgumentNullException((chars is null) ? nameof(chars) : nameof(bytes), ExceptionResource.ArgumentNull_Array);
            if ((charIndex | charCount) < 0)
                throw new ArgumentOutOfRangeException((charIndex < 0) ? nameof(charIndex) : nameof(charCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if ((byteIndex | byteCount) < 0)
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? nameof(byteIndex) : nameof(byteCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (chars.Length - charIndex < charCount)
                throw new ArgumentOutOfRangeException(nameof(chars), ExceptionResource.ArgumentOutOfRange_IndexCount);
            if (bytes.Length - byteIndex < byteCount)
                throw new ArgumentOutOfRangeException(nameof(bytes), ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);

            fixed (char* pChars = chars)
            fixed (byte* pBytes = bytes)
                ConvertCommon(pChars + charIndex, charCount, pBytes + byteIndex, bytes.Length - byteIndex, out charsUsed, out bytesUsed);
            completed = true;
        }
#if NETSTANDARD2_0_OR_GREATER
        override
#endif
        public unsafe void Convert(char* chars, int charCount, byte* bytes, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
        {
            if (chars is null || bytes is null)
                throw new ArgumentNullException((chars is null) ? nameof(chars) : nameof(bytes), ExceptionResource.ArgumentNull_Array);
            if ((charCount | byteCount) < 0)
                throw new ArgumentOutOfRangeException((charCount < 0) ? nameof(charCount) : nameof(byteCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

            ConvertCommon(chars, charCount, bytes, byteCount, out charsUsed, out bytesUsed);
            completed = true;
        }
#if NETSTANDARD2_1_OR_GREATER
        public override unsafe void Convert(ReadOnlySpan<char> chars, Span<byte> bytes, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
        {
            fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
            fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
                Convert(charsPtr, chars.Length, bytesPtr, bytes.Length, flush, out bytesUsed, out charsUsed, out completed);
        }
#endif
        protected unsafe void ConvertCommon(char* pChars, int charCount, byte* pBytes, int byteCount, out int charsUsed, out int bytesUsed)
        {
            ;
            bytesUsed = 0;
            for (charsUsed = 0; charsUsed < charCount; charsUsed++)
            {
                char c = *pChars;
                int current = ModifiedUTF8Encoding.GetByteCount(c);
                if (bytesUsed + current <= byteCount)
                {
                    bytesUsed += current;
                    if (c == 0)
                    {
                        *pBytes++ = 0xC0;
                        *pBytes++ = 0x80;
                    }
                    else if (c < 128)
                    {
                        *pBytes++ = (byte)c;
                    }
                    else if (c < 2048)
                    {
                        *pBytes++ = (byte)((c >> 6) | 0b_1100_0000);
                        *pBytes++ = (byte)((c & 0b_0011_1111) | 0b_1000_0000);
                    }
                    else
                    {
                        *pBytes++ = (byte)((c >> 12) | 0b_1110_0000);
                        *pBytes++ = (byte)(((c >> 6) & 0b_0011_1111) | 0b_1000_0000);
                        *pBytes++ = (byte)((c & 0b_0011_1111) | 0b_1000_0000);
                    }
                }
                else
                {
                    break;
                }
                pChars++;
            }
        }
        #endregion Convert

        #region GetByteCount
        public override unsafe int GetByteCount(char[] chars, int index, int count, bool flush)
        {
            if (chars is null)
                throw new ArgumentNullException(nameof(chars), ExceptionResource.ArgumentNull_Array);
            if ((index | count) < 0)
                throw new ArgumentOutOfRangeException((index < 0) ? nameof(index) : nameof(count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (chars.Length - index < count)
                throw new ArgumentOutOfRangeException(nameof(chars), ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);

            fixed (char* pChars = chars)
                return GetByteCountCommon(pChars + index, count);
        }
#if NETSTANDARD2_0_OR_GREATER
        override
#endif
        public unsafe int GetByteCount(char* chars, int count, bool flush)
        {
            if (chars is null)
                throw new ArgumentOutOfRangeException(nameof(chars), ExceptionResource.ArgumentNull_Array);
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

            return GetByteCountCommon(chars, count);
        }
#if NETSTANDARD2_1_OR_GREATER
        public override unsafe int GetByteCount(ReadOnlySpan<char> chars, bool flush)
        {
            fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
                return GetByteCountCommon(charsPtr, chars.Length);
        }
#endif
        #endregion GetByteCount

        public override unsafe int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
        {
            if (chars is null || bytes is null)
                throw new ArgumentNullException((chars is null) ? nameof(chars) : nameof(bytes), ExceptionResource.ArgumentNull_Array);
            if ((charIndex | charCount) < 0)
                throw new ArgumentOutOfRangeException((charIndex < 0) ? nameof(charIndex) : nameof(charCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (chars.Length - charIndex < charCount)
                throw new ArgumentOutOfRangeException(nameof(chars), ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            if ((uint)byteIndex > (uint)bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(byteIndex), ExceptionResource.ArgumentOutOfRange_IndexMustBeLessOrEqual);

            fixed (char* pChars = chars)
            fixed (byte* pBytes = bytes)
                return GetBytesCommon(pChars + charIndex, charCount, pBytes + byteIndex, bytes.Length - byteIndex);
        }
#if NETSTANDARD2_0_OR_GREATER
        override
#endif
        public unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
        {
            if (chars is null || bytes is null)
                throw new ArgumentNullException((chars is null) ? nameof(chars) : nameof(bytes), ExceptionResource.ArgumentNull_Array);
            if ((charCount | byteCount) < 0)
                throw new ArgumentOutOfRangeException((charCount < 0) ? nameof(charCount) : nameof(byteCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

            return GetBytesCommon(chars, charCount, bytes, byteCount);
        }
#if NETSTANDARD2_1_OR_GREATER
        public override unsafe int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes, bool flush)
        {
            fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
            fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
                return GetBytesCommon(charsPtr, chars.Length, bytesPtr, bytes.Length);
        }
#endif
        public unsafe int GetBytesCommon(char* pChars, int charCount, byte* pBytes, int byteCount)
        {
            ConvertCommon(pChars, charCount, pBytes, byteCount, out int charUsed, out int byteUsed);
            if (charUsed != charCount)
                throw new ArgumentException(ExceptionResource.Argument_EncodingConversionOverflowChars, nameof(pChars));
            return byteUsed;
        }
    }
}