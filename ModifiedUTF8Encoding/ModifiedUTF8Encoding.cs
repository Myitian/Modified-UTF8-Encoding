using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Myitian.Text;
public class ModifiedUTF8Encoding : Encoding
{
    private const int UTF8_CODEPAGE = 65001;
    private const char FALLBACK_CHAR = '\uFFFD';
    private const string FALLBACK_STR = "\uFFFD";
    private readonly bool _throwOnInvalidBytes;

#if NETSTANDARD1_3_OR_GREATER
    private readonly static DecoderReplacementFallback _fb = new(FALLBACK_STR);

    public override string EncodingName => "Unicode (Modified UTF-8)";
    public ModifiedUTF8Encoding() : this(false) { }
    public ModifiedUTF8Encoding(bool throwOnInvalidBytes) : base(UTF8_CODEPAGE, null, throwOnInvalidBytes ? DecoderFallback.ExceptionFallback : _fb)
    {
        _throwOnInvalidBytes = throwOnInvalidBytes;
    }
#else
    public ModifiedUTF8Encoding() : this(false) { }
    public ModifiedUTF8Encoding(bool throwOnInvalidBytes) : base()
    {
        _throwOnInvalidBytes = throwOnInvalidBytes;
    }
#endif

    #region GetByteCount
    public override unsafe int GetByteCount(char[] chars, int index, int count)
    {
        if (chars is null)
            throw new ArgumentNullException(nameof(chars), ExceptionResource.ArgumentNull_Array);
        if ((index | count) < 0)
            throw new ArgumentOutOfRangeException((index < 0) ? nameof(index) : nameof(count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        if (chars.Length - index < count)
            throw new ArgumentOutOfRangeException(nameof(chars), ExceptionResource.ArgumentOutOfRange_IndexCount);

        fixed (char* pChars = chars)
            return GetByteCountCommon(pChars, chars.Length);
    }
    public override unsafe int GetByteCount(string chars)
    {
        if (chars is null)
            throw new ArgumentNullException(nameof(chars));

        fixed (char* pChars = chars)
            return GetByteCountCommon(pChars, chars.Length);
    }
#if NETSTANDARD1_3_OR_GREATER
    override
#endif
    public unsafe int GetByteCount(char* chars, int count)
    {
        if (chars is null)
            throw new ArgumentNullException(nameof(chars));
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

        return GetByteCountCommon(chars, count);
    }
#if NETSTANDARD2_1_OR_GREATER
    public override unsafe int GetByteCount(ReadOnlySpan<char> chars)
    {
        fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
            return GetByteCountCommon(charsPtr, chars.Length);
    }
#endif
    protected unsafe int GetByteCountCommon(char* pChars, int count)
    {
        int byteCount = 0;
        for (int i = 0; i < count; i++)
        {
            char c = *pChars;
            if (c == 0)
                byteCount += 2;
            else if (c < 128)
                byteCount++;
            else if (c < 2048)
                byteCount += 2;
            else
                byteCount += 3;
            pChars++;
        }
        return byteCount;
    }
    #endregion GetByteCount

    #region GetBytes
    public override unsafe int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        if (s is null || bytes is null)
            throw new ArgumentNullException((s is null) ? nameof(s) : nameof(bytes), ExceptionResource.ArgumentNull_Array);
        if ((charIndex | charCount) < 0)
            throw new ArgumentOutOfRangeException((charIndex < 0) ? nameof(charIndex) : nameof(charCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        if (s.Length - charIndex < charCount)
            throw new ArgumentOutOfRangeException(nameof(s), ExceptionResource.ArgumentOutOfRange_IndexCount);
        if ((uint)byteIndex > bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(byteIndex), ExceptionResource.ArgumentOutOfRange_IndexMustBeLessOrEqual);

        fixed (char* pChars = s)
        fixed (byte* pBytes = bytes)
            return GetBytesCommon(pChars + charIndex, charCount, pBytes + byteIndex, bytes.Length - byteIndex);
    }
    public override unsafe int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        if (chars is null || bytes is null)
            throw new ArgumentNullException((chars is null) ? nameof(chars) : nameof(bytes), ExceptionResource.ArgumentNull_Array);
        if ((charIndex | charCount) < 0)
            throw new ArgumentOutOfRangeException((charIndex < 0) ? nameof(charIndex) : nameof(charCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        if (chars.Length - charIndex < charCount)
            throw new ArgumentOutOfRangeException(nameof(chars), ExceptionResource.ArgumentOutOfRange_IndexCount);
        if ((uint)byteIndex > bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(byteIndex), ExceptionResource.ArgumentOutOfRange_IndexMustBeLessOrEqual);

        fixed (char* pChars = chars)
        fixed (byte* pBytes = bytes)
            return GetBytesCommon(pChars + charIndex, charCount, pBytes + byteIndex, bytes.Length - byteIndex);
    }
#if NETSTANDARD1_3_OR_GREATER
    override
#endif
    public unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount)
    {
        if (chars is null || bytes is null)
            throw new ArgumentNullException((chars is null) ? nameof(chars) : nameof(bytes), ExceptionResource.ArgumentNull_Array);
        if ((charCount | byteCount) < 0)
            throw new ArgumentOutOfRangeException((charCount < 0) ? nameof(charCount) : nameof(byteCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

        return GetBytesCommon(chars, charCount, bytes, byteCount);
    }
#if NETSTANDARD2_1_OR_GREATER
    public override unsafe int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes)
    {
        fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
        fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
            return GetBytesCommon(charsPtr, chars.Length, bytesPtr, bytes.Length);
    }
#endif
    protected unsafe int GetBytesCommon(char* pChars, int charCount, byte* pBytes, int byteCount, bool throwForDestinationOverflow = true)
    {
        byte* p0 = pBytes;
        byte* pe = pBytes + byteCount;
        for (int i = 0; i < charCount; i++)
        {
            char c = *pChars++;
            if (c == 0)
            {
                if (throwForDestinationOverflow && pBytes + 2 > pe)
                    Overflow();
                *pBytes++ = 0xC0;
                *pBytes++ = 0x80;
            }
            else if (c < 128)
            {
                if (throwForDestinationOverflow && pBytes + 1 > pe)
                    Overflow();
                *pBytes++ = (byte)c;
            }
            else if (c < 2048)
            {
                if (throwForDestinationOverflow && pBytes + 2 > pe)
                    Overflow();
                *pBytes++ = (byte)((c >> 6) | 0b_1100_0000);
                *pBytes++ = (byte)((c & 0b_0011_1111) | 0b_1000_0000);
            }
            else
            {
                if (throwForDestinationOverflow && pBytes + 3 > pe)
                    Overflow();
                *pBytes++ = (byte)((c >> 12) | 0b_1110_0000);
                *pBytes++ = (byte)(((c >> 6) & 0b_0011_1111) | 0b_1000_0000);
                *pBytes++ = (byte)((c & 0b_0011_1111) | 0b_1000_0000);
            }
        }
        return (int)(pBytes - p0);

        static void Overflow()
            => throw new ArgumentException(ExceptionResource.Argument_EncodingConversionOverflowBytes, nameof(pBytes));
    }
    #endregion GetBytes

    #region GetCharCount
    public override unsafe int GetCharCount(byte[] bytes, int index, int count)
    {
        if (bytes is null)
            throw new ArgumentNullException(nameof(bytes), ExceptionResource.ArgumentNull_Array);
        if ((index | count) < 0)
            throw new ArgumentOutOfRangeException((index < 0) ? nameof(index) : nameof(count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        if (bytes.Length - index < count)
            throw new ArgumentOutOfRangeException(nameof(bytes), ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);

        fixed (byte* pBytes = bytes)
            return GetCharCountCommon(pBytes + index, count);
    }
#if NETSTANDARD1_3_OR_GREATER
    override
#endif
    public unsafe int GetCharCount(byte* bytes, int count)
    {
        if (bytes is null)
            throw new ArgumentNullException(nameof(bytes), ExceptionResource.ArgumentNull_Array);
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

        return GetCharCountCommon(bytes, count);
    }
#if NETSTANDARD2_1_OR_GREATER
    public override unsafe int GetCharCount(ReadOnlySpan<byte> bytes)
    {
        fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
            return GetCharCountCommon(bytesPtr, bytes.Length);
    }
#endif
    protected unsafe int GetCharCountCommon(byte* pBytes, int count)
    {
#if NETSTANDARD1_3_OR_GREATER
        DecoderFallbackBuffer decoderFallbackBuffer = DecoderFallback.CreateFallbackBuffer();
        char c;
#endif
        byte[] bs = new byte[4];
        byte[] b1 = new byte[1];
        int chr = 0;
        int bsi = 0;
        int mode = 0;
        int status = 0;
        int charCount = 0;
        for (int i = 0; i < count; i++)
        {
            byte b = *pBytes++;
            if (status == 0)
            {
                bs[0] = b;
                bsi = 0;
                if ((b & 0b_1000_0000) == 0b_0000_0000)
                {
                    charCount++;
                }
                else if ((b & 0b_1110_0000) == 0b_1100_0000)
                {
                    status = 1;
                    mode = 1;
                    chr = b & 0b_0001_1111;
                }
                else if ((b & 0b_1111_0000) == 0b_1110_0000)
                {
                    status = 2;
                    mode = 2;
                    chr = b & 0b_0000_1111;
                }
                else if ((b & 0b_1111_1000) == 0b_1111_0000)
                {
                    status = 3;
                    mode = 3;
                    chr = b & 0b_0000_0111;
                }
                else
                {
                    b1[0] = b;
#if NETSTANDARD1_3_OR_GREATER
                    decoderFallbackBuffer.Fallback(b1, i);
                    while ((c = decoderFallbackBuffer.GetNextChar()) > 0)
                        charCount++;
#else
                    if (_throwOnInvalidBytes)
                        Throw(b1, i);
                    else
                        charCount++;
#endif
                }
            }
            else if ((b & 0b_1100_0000) == 0b_1000_0000)
            {
                status--;
                bs[++bsi] = b;
                chr = (chr << 6) | (b & 0b_0011_1111);
                if (status == 0)
                {
                    if (chr < char.MaxValue)
                        charCount++;
                    else
                        charCount += 2;
                }
            }
            else
            {
                status = 0;
                for (bsi = 0; bsi <= mode; bsi++)
                {
                    b1[0] = bs[bsi];
#if NETSTANDARD1_3_OR_GREATER
                    decoderFallbackBuffer.Fallback(b1, i - mode + bsi + 1);
                    while ((c = decoderFallbackBuffer.GetNextChar()) > 0)
                        charCount++;
#else
                    if (_throwOnInvalidBytes)
                        Throw(b1, i - mode + bsi + 1);
                    else
                        charCount++;
#endif
                }
            }
        }
        if (status != 0)
        {
            for (bsi = 0; bsi <= mode; bsi++)
            {
                b1[0] = bs[bsi];
#if NETSTANDARD1_3_OR_GREATER
                decoderFallbackBuffer.Fallback(b1, count - mode + bsi + 1);
                while ((c = decoderFallbackBuffer.GetNextChar()) > 0)
                    charCount++;
#else
                if (_throwOnInvalidBytes)
                    Throw(b1, count - mode + bsi + 1);
                else
                    charCount++;
#endif
            }
        }
        return charCount;
    }
    #endregion GetCharCount

    #region GetChars
    public override unsafe int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        if (bytes is null || chars is null)
            throw new ArgumentNullException((bytes is null) ? nameof(bytes) : nameof(chars), ExceptionResource.ArgumentNull_Array);
        if ((byteIndex | byteCount) < 0)
            throw new ArgumentOutOfRangeException((byteIndex < 0) ? nameof(byteIndex) : nameof(byteCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        if (bytes.Length - byteIndex < byteCount)
            throw new ArgumentOutOfRangeException(nameof(chars), ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
        if ((uint)charIndex > (uint)chars.Length)
            throw new ArgumentOutOfRangeException(nameof(charIndex), ExceptionResource.ArgumentOutOfRange_IndexMustBeLessOrEqual);

        fixed (byte* pBytes = bytes)
        fixed (char* pChars = chars)
            return GetCharsCommon(pBytes + byteIndex, byteCount, pChars + charIndex, chars.Length - charIndex);
    }
#if NETSTANDARD1_3_OR_GREATER
    override
#endif
    public unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount)
    {
        if (bytes is null || chars is null)
            throw new ArgumentNullException((bytes is null) ? nameof(bytes) : nameof(chars), ExceptionResource.ArgumentNull_Array);
        if ((byteCount | charCount) < 0)
            throw new ArgumentOutOfRangeException((byteCount < 0) ? nameof(byteCount) : nameof(charCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

        return GetCharsCommon(bytes, byteCount, chars, charCount);
    }
#if NETSTANDARD2_1_OR_GREATER
    public override unsafe int GetChars(ReadOnlySpan<byte> bytes, Span<char> chars)
    {
        fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
        fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
            return GetCharsCommon(bytesPtr, bytes.Length, charsPtr, chars.Length);
    }
#endif
    protected unsafe int GetCharsCommon(byte* pBytes, int byteCount, char* pChars, int charCount, bool throwForDestinationOverflow = true)
    {
#if NETSTANDARD1_3_OR_GREATER
        DecoderFallbackBuffer decoderFallbackBuffer = DecoderFallback.CreateFallbackBuffer();
        char c;
#endif
        byte[] bs = new byte[4];
        byte[] b1 = new byte[1];
        int bsi = 0;
        byte mode = 0;
        int status = 0;
        char* p0 = pChars;
        char* pe = pChars + charCount;
        int chr = 0;
        for (int i = 0; i < byteCount; i++)
        {
            byte b = *pBytes++;
            if (status == 0)
            {
                bs[0] = b;
                bsi = 0;
                if ((b & 0b_1000_0000) == 0b_0000_0000)
                {
                    if (throwForDestinationOverflow && pChars + 1 > pe)
                        Overflow();
                    *pChars++ = (char)b;
                }
                else if ((b & 0b_1110_0000) == 0b_1100_0000)
                {
                    status = 1;
                    mode = 1;
                    chr = b & 0b_0001_1111;
                }
                else if ((b & 0b_1111_0000) == 0b_1110_0000)
                {
                    status = 2;
                    mode = 2;
                    chr = b & 0b_0000_1111;
                }
                else if ((b & 0b_1111_1000) == 0b_1111_0000)
                {
                    status = 3;
                    mode = 3;
                    chr = b & 0b_0000_0111;
                }
                else
                {
                    b1[0] = b;
#if NETSTANDARD1_3_OR_GREATER
                    decoderFallbackBuffer.Fallback(b1, byteCount - mode + bsi + 1);
                    while ((c = decoderFallbackBuffer.GetNextChar()) > 0)
                    {
                        if (throwForDestinationOverflow && pChars + 1 > pe)
                            Overflow();
                        *pChars++ = c;
                    }
#else
                    if (_throwOnInvalidBytes)
                        Throw(b1, byteCount - mode + bsi + 1);
                    else
                    {
                        if (throwForDestinationOverflow && pChars + 1 > pe)
                            Overflow();
                        *pChars++ = FALLBACK_CHAR;
                    }
#endif
                }
            }
            else if ((b & 0b_1100_0000) == 0b_1000_0000)
            {
                status--;
                bs[++bsi] = b;
                chr = (chr << 6) | (b & 0b_0011_1111);
                if (status == 0)
                {
                    if (throwForDestinationOverflow && pChars + 1 > pe)
                        Overflow();
                    if (chr < char.MaxValue)
                        *pChars++ = (char)chr;
                    else
                    {
                        uint value = (uint)chr;
                        *pChars++ = (char)((value + 0x35F0000) >> 10);
                        *pChars++ = (char)((value & 0x3FF) + 0xDC00);
                    }
                }
            }
            else
            {
                for (bsi = 0; bsi <= mode; bsi++)
                {
                    b1[0] = bs[bsi];
#if NETSTANDARD1_3_OR_GREATER
                    decoderFallbackBuffer.Fallback(b1, i - mode + bsi + 1);
                    while ((c = decoderFallbackBuffer.GetNextChar()) > 0)
                    {
                        if (throwForDestinationOverflow && pChars + 1 > pe)
                            Overflow();
                        *pChars++ = c;
                    }
#else
                    if (_throwOnInvalidBytes)
                        Throw(b1, i - mode + bsi + 1);
                    else
                    {
                        if (throwForDestinationOverflow && pChars + 1 > pe)
                            Overflow();
                        *pChars++ = FALLBACK_CHAR;
                    }
#endif
                }
            }
        }
        if (status != 0)
        {
            for (bsi = 0; bsi <= mode; bsi++)
            {
                b1[0] = bs[bsi];
#if NETSTANDARD1_3_OR_GREATER
                decoderFallbackBuffer.Fallback(b1, byteCount - mode + bsi + 1);
                while ((c = decoderFallbackBuffer.GetNextChar()) > 0)
                {
                    if (throwForDestinationOverflow && pChars + 1 > pe)
                        Overflow();
                    *pChars++ = c;
                }
#else
                if (_throwOnInvalidBytes)
                    Throw(b1, byteCount - mode + bsi + 1);
                else
                {
                    if (throwForDestinationOverflow && pChars + 1 > pe)
                        Overflow();
                    *pChars++ = FALLBACK_CHAR;
                }
#endif
            }
        }
        return (int)(pChars - p0);

        static void Overflow()
            => throw new ArgumentException(ExceptionResource.Argument_EncodingConversionOverflowChars, nameof(pChars));
    }
    #endregion GetChars

    public override int GetMaxByteCount(int charCount)
    {
        long byteCount = charCount * 3L;

        if (byteCount > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(charCount), ExceptionResource.ArgumentOutOfRange_GetByteCountOverflow);

        return (int)byteCount;
    }

    public override int GetMaxCharCount(int byteCount)
    {
        long charCount = (long)byteCount + 1;

#if NETSTANDARD1_3_OR_GREATER
        if (DecoderFallback.MaxCharCount > 1)
            charCount *= DecoderFallback.MaxCharCount;
#endif
        if (charCount > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(byteCount), ExceptionResource.ArgumentOutOfRange_GetCharCountOverflow);

        return (int)charCount;
    }

    public override Decoder GetDecoder()
    {
        return new ModifiedUTF8Decoder(_throwOnInvalidBytes)
#if NETSTANDARD1_3_OR_GREATER
        {
            Fallback = DecoderFallback
        }
#endif
        ;
    }

    private static readonly byte[] _emptyBytes = [];
    private static void Throw(byte[] bytesUnknown, int index)
    {
        bytesUnknown ??= _emptyBytes;

        StringBuilder strBytes = new(bytesUnknown.Length * 4);

        const int MaxLength = 20;
        for (int i = 0; i < bytesUnknown.Length && i < MaxLength; i++)
            strBytes.Append($"[{bytesUnknown[i]:X2}]");

        if (bytesUnknown.Length > MaxLength)
            strBytes.Append(" ...");

        throw new DecoderFallbackException(
            string.Format(ExceptionResource.Argument_InvalidCodePageBytesIndex,
               strBytes, index), bytesUnknown, index);
    }

    public static class ExceptionResource
    {
        public const string Argument_EncodingConversionOverflowBytes = "The output byte buffer is too small to contain the encoded data.";
        public const string Argument_EncodingConversionOverflowChars = "The output char buffer is too small to contain the decoded characters.";
        public const string Argument_InvalidCodePageBytesIndex = "Unable to translate bytes {0} at index {1} from specified code page to Unicode.";
        public const string ArgumentNull_Array = "Array cannot be null.";
        public const string ArgumentOutOfRange_GetByteCountOverflow = "Too many characters. The resulting number of bytes is larger than what can be returned as an int.";
        public const string ArgumentOutOfRange_GetCharCountOverflow = "Too many bytes. The resulting number of chars is larger than what can be returned as an int.";
        public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
        public const string ArgumentOutOfRange_IndexCount = "Index and count must refer to a location within the string.";
        public const string ArgumentOutOfRange_IndexCountBuffer = "Index and count must refer to a location within the buffer.";
        public const string ArgumentOutOfRange_IndexMustBeLessOrEqual = "Index was out of range. Must be non-negative and less than the length of the string.";
    }

    public class ModifiedUTF8Decoder : Decoder
    {
        private readonly bool _throwOnInvalidBytes;

        byte[] _bs = new byte[4];
        byte[] _b1 = new byte[1];
        int _bsi = 0;
        int _chr = 0;
        byte _mode = 0;
        int _status = 0;
        bool _hasNext = false;
        char _next;

        public ModifiedUTF8Decoder() : base() { }
        public ModifiedUTF8Decoder(bool throwOnInvalidBytes) : base()
        {
            _throwOnInvalidBytes = throwOnInvalidBytes;
        }

        #region Convert
        public override unsafe void Convert(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            if (bytes is null || chars is null)
                throw new ArgumentNullException((bytes is null) ? nameof(bytes) : nameof(chars), ExceptionResource.ArgumentNull_Array);
            if ((byteIndex | byteCount) < 0)
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? nameof(byteIndex) : nameof(byteCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (bytes.Length - byteIndex < byteCount)
                throw new ArgumentOutOfRangeException(nameof(chars), ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            if ((uint)charIndex > (uint)chars.Length)
                throw new ArgumentOutOfRangeException(nameof(charIndex), ExceptionResource.ArgumentOutOfRange_IndexMustBeLessOrEqual);

            fixed (byte* pBytes = bytes)
            fixed (char* pChars = chars)
                ConvertCommon(pBytes + byteIndex, byteCount, pChars + charIndex, chars.Length - charIndex, flush, out bytesUsed, out charsUsed, out completed);
        }
#if NETSTANDARD2_0_OR_GREATER
        override
#endif
        public unsafe void Convert(byte* bytes, int byteCount, char* chars, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            if (bytes is null || chars is null)
                throw new ArgumentNullException((bytes is null) ? nameof(bytes) : nameof(chars), ExceptionResource.ArgumentNull_Array);
            if ((byteCount | charCount) < 0)
                throw new ArgumentOutOfRangeException((byteCount < 0) ? nameof(byteCount) : nameof(charCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

            ConvertCommon(bytes, byteCount, chars, charCount, flush, out bytesUsed, out charsUsed, out completed);
        }
#if NETSTANDARD2_1_OR_GREATER
        public override unsafe void Convert(ReadOnlySpan<byte> bytes, Span<char> chars, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
            fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
                Convert(bytesPtr, bytes.Length, charsPtr, chars.Length, flush, out bytesUsed, out charsUsed, out completed);
        }
#endif
        protected unsafe void ConvertCommon(byte* pBytes, int byteCount, char* pChars, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            char* p0 = pChars;
            char* pe = pChars + charCount;
            int i = 0;
            if (_hasNext)
            {
                if (pChars + 1 > pe)
                    goto NotCompleted;

                _hasNext = false;
                *pChars++ = _next;
            }
#if NETSTANDARD1_3_OR_GREATER
            char c;
            while ((c = FallbackBuffer.GetNextChar()) > 0)
            {
                if (pChars + 1 > pe)
                {
                    _next = c;
                    goto NotCompleted;
                }
                *pChars++ = c;
            }
#endif
            for (; i < byteCount; i++)
            {
                byte b = *pBytes++;
                if (_status == 0)
                {
                    _bs[0] = b;
                    _bsi = 0;
                    if ((b & 0b_1000_0000) == 0b_0000_0000)
                    {
                        if (pChars + 1 > pe)
                        {
                            _next = (char)b;
                            goto NotCompleted;
                        }
                        *pChars++ = (char)b;
                    }
                    else if ((b & 0b_1110_0000) == 0b_1100_0000)
                    {
                        _status = 1;
                        _mode = 1;
                        _chr = b & 0b_0001_1111;
                    }
                    else if ((b & 0b_1111_0000) == 0b_1110_0000)
                    {
                        _status = 2;
                        _mode = 2;
                        _chr = b & 0b_0000_1111;
                    }
                    else if ((b & 0b_1111_1000) == 0b_1111_0000)
                    {
                        _status = 3;
                        _mode = 3;
                        _chr = b & 0b_0000_0111;
                    }
                    else
                    {
                        _b1[0] = b;
#if NETSTANDARD1_3_OR_GREATER
                        FallbackBuffer.Fallback(_b1, byteCount - _mode + _bsi + 1);
                        while ((c = FallbackBuffer.GetNextChar()) > 0)
                        {
                            if (pChars + 1 > pe)
                            {
                                _next = c;
                                goto NotCompleted;
                            }
                            *pChars++ = c;
                        }
#else
                        if (_throwOnInvalidBytes)
                            Throw(_b1, byteCount - _mode + _bsi + 1);
                        else
                        {
                            if (pChars + 1 > pe)
                            {
                                _next = FALLBACK_CHAR;
                                goto NotCompleted;
                            }
                            *pChars++ = FALLBACK_CHAR;
                        }
#endif
                    }
                }
                else if ((b & 0b_1100_0000) == 0b_1000_0000)
                {
                    _status--;
                    _bs[++_bsi] = b;
                    _chr = (_chr << 6) | (b & 0b_0011_1111);
                    if (_status == 0)
                    {
                        if (pChars + 1 > pe)
                        {
                            _next = (char)_chr;
                            goto NotCompleted;
                        }
                        if (_chr < char.MaxValue)
                            *pChars++ = (char)_chr;
                        else
                        {
                            uint value = (uint)_chr;
                            *pChars++ = (char)((value + 0x35F0000) >> 10);
                            if (pChars + 1 > pe)
                            {
                                _next = (char)((value & 0x3FF) + 0xDC00);
                                goto NotCompleted;
                            }
                            *pChars++ = (char)((value & 0x3FF) + 0xDC00);
                        }
                    }
                }
                else
                {
                    for (_bsi = 0; _bsi <= _mode; _bsi++)
                    {
                        _b1[0] = _bs[_bsi];
#if NETSTANDARD1_3_OR_GREATER
                        FallbackBuffer.Fallback(_b1, i - _mode + _bsi + 1);
                        while ((c = FallbackBuffer.GetNextChar()) > 0)
                        {
                            if (pChars + 1 > pe)
                            {
                                _next = c;
                                goto NotCompleted;
                            }
                            *pChars++ = c;
                        }
#else
                        if (_throwOnInvalidBytes)
                            Throw(_b1, i - _mode + _bsi + 1);
                        else
                        {
                            if (pChars + 1 > pe)
                            {
                                _next = FALLBACK_CHAR;
                                goto NotCompleted;
                            }
                            *pChars++ = FALLBACK_CHAR;
                        }
#endif
                    }
                }
            }
            if (_status != 0)
            {
                if (flush)
                {
                    for (_bsi = 0; _bsi <= _mode; _bsi++)
                    {
                        _b1[0] = _bs[_bsi];
#if NETSTANDARD1_3_OR_GREATER
                        FallbackBuffer.Fallback(_b1, byteCount - _mode + _bsi + 1);
                        while ((c = FallbackBuffer.GetNextChar()) > 0)
                        {
                            if (pChars + 1 > pe)
                            {
                                _next = c;
                                goto NotCompleted;
                            }
                            *pChars++ = c;
                        }
#else
                        if (_throwOnInvalidBytes)
                            Throw(_b1, byteCount - _mode + _bsi + 1);
                        else
                        {
                            if (pChars + 1 > pe)
                            {
                                _next = FALLBACK_CHAR;
                                goto NotCompleted;
                            }
                            *pChars++ = FALLBACK_CHAR;
                        }
#endif
                    }
                    Reset();
                }
                else
                {
                    bytesUsed = i;
                    charsUsed = (int)(pChars - p0);
                    completed = false;
                    _hasNext = false;
                    return;
                }
            }
            bytesUsed = i;
            charsUsed = (int)(pChars - p0);
            completed = true;
            _hasNext = false;
            return;
        NotCompleted:
            bytesUsed = i;
            charsUsed = (int)(pChars - p0);
            completed = false;
            _hasNext = true;
        }
        #endregion Convert

        #region GetCharCount
        public override unsafe int GetCharCount(byte[] bytes, int index, int count)
        {
            return GetCharCount(bytes, index, count, true);
        }
        public override unsafe int GetCharCount(byte[] bytes, int index, int count, bool flush)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes), ExceptionResource.ArgumentNull_Array);
            if ((index | count) < 0)
                throw new ArgumentOutOfRangeException((index < 0) ? nameof(index) : nameof(count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (bytes.Length - index < count)
                throw new ArgumentOutOfRangeException(nameof(bytes), ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);

            fixed (byte* pBytes = bytes)
                return GetCharCountCommon(pBytes + index, count, flush);
        }
#if NETSTANDARD2_0_OR_GREATER
        override
#endif
        public unsafe int GetCharCount(byte* bytes, int count, bool flush)
        {
            if (bytes is null)
                throw new ArgumentOutOfRangeException(nameof(bytes), ExceptionResource.ArgumentNull_Array);
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

            return GetCharCountCommon(bytes, count, flush);
        }
#if NETSTANDARD2_1_OR_GREATER
        public override unsafe int GetCharCount(ReadOnlySpan<byte> bytes, bool flush)
        {
            fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
                return GetCharCountCommon(bytesPtr, bytes.Length, flush);
        }
#endif
        public unsafe int GetCharCountCommon(byte* pBytes, int count, bool flush)
        {
            int status = _status;
            int mode = _mode;
            int bsi = _bsi;
            byte[] bs = new byte[_bs.Length];
            _bs.CopyTo(bs, 0);
            int charCount = 0;
            int chr = _chr;
            int i = 0;
            if (_hasNext)
                charCount++;
#if NETSTANDARD1_3_OR_GREATER
            char c;
            while ((c = FallbackBuffer.GetNextChar()) > 0)
                charCount++;
#endif
            for (; i < count; i++)
            {
                byte b = *pBytes++;
                if (status == 0)
                {
                    bs[0] = b;
                    bsi = 0;
                    if ((b & 0b_1000_0000) == 0b_0000_0000)
                    {
                        charCount++;
                    }
                    else if ((b & 0b_1110_0000) == 0b_1100_0000)
                    {
                        status = 1;
                        mode = 1;
                        chr = b & 0b_0001_1111;
                    }
                    else if ((b & 0b_1111_0000) == 0b_1110_0000)
                    {
                        status = 2;
                        mode = 2;
                        chr = b & 0b_0000_1111;
                    }
                    else if ((b & 0b_1111_1000) == 0b_1111_0000)
                    {
                        status = 3;
                        mode = 3;
                        chr = b & 0b_0000_0111;
                    }
                    else
                    {
                        _b1[0] = b;
#if NETSTANDARD1_3_OR_GREATER
                        FallbackBuffer.Fallback(_b1, count - mode + bsi + 1);
                        while ((c = FallbackBuffer.GetNextChar()) > 0)
                            charCount++;
#else
                        if (_throwOnInvalidBytes)
                            Throw(_b1, count - mode + bsi + 1);
                        else
                            charCount++;
#endif
                    }
                }
                else if ((b & 0b_1100_0000) == 0b_1000_0000)
                {
                    status--;
                    bs[++bsi] = b;
                    chr = (chr << 6) | (b & 0b_0011_1111);
                    if (_status == 0)
                    {
                        if (chr < char.MaxValue)
                            charCount++;
                        else
                            charCount += 2;
                    }
                }
                else
                {
                    for (bsi = 0; bsi <= mode; bsi++)
                    {
                        _b1[0] = bs[_bsi];
#if NETSTANDARD1_3_OR_GREATER
                        FallbackBuffer.Fallback(_b1, i - mode + bsi + 1);
                        while ((c = FallbackBuffer.GetNextChar()) > 0)
                            charCount++;
#else
                        if (_throwOnInvalidBytes)
                            Throw(_b1, i - mode + bsi + 1);
                        else
                            charCount++;
#endif
                    }
                }
            }
            if (_status != 0)
            {
                if (flush)
                {
                    for (bsi = 0; bsi <= mode; bsi++)
                    {
                        _b1[0] = bs[bsi];
#if NETSTANDARD1_3_OR_GREATER
                        FallbackBuffer.Fallback(_b1, count - _mode + _bsi + 1);
                        while ((c = FallbackBuffer.GetNextChar()) > 0)
                            charCount++;
#else
                        if (_throwOnInvalidBytes)
                            Throw(_b1, count - mode + bsi + 1);
                        else
                            charCount++;
#endif
                    }
                }
            }
            return charCount;
        }
        #endregion GetCharCount


        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return GetChars(bytes, byteIndex, byteCount, chars, charIndex, true);
        }
        public override unsafe int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush)
        {
            if (bytes is null || chars is null)
                throw new ArgumentNullException((bytes is null) ? nameof(bytes) : nameof(chars), ExceptionResource.ArgumentNull_Array);
            if ((byteIndex | byteCount) < 0)
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? nameof(byteIndex) : nameof(byteCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (bytes.Length - byteIndex < byteCount)
                throw new ArgumentOutOfRangeException(nameof(chars), ExceptionResource.ArgumentOutOfRange_IndexCountBuffer);
            if ((uint)charIndex > (uint)chars.Length)
                throw new ArgumentOutOfRangeException(nameof(charIndex), ExceptionResource.ArgumentOutOfRange_IndexMustBeLessOrEqual);

            fixed (byte* pBytes = bytes)
            fixed (char* pChars = chars)
                return GetCharsCommon(pBytes + byteIndex, byteCount, pChars + charIndex, chars.Length - charIndex, flush);
        }
#if NETSTANDARD2_0_OR_GREATER
        override
#endif
        public unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, bool flush)
        {
            if (bytes is null || chars is null)
                throw new ArgumentNullException((bytes is null) ? nameof(bytes) : nameof(chars), ExceptionResource.ArgumentNull_Array);
            if ((byteCount | charCount) < 0)
                throw new ArgumentOutOfRangeException((byteCount < 0) ? nameof(byteCount) : nameof(charCount), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

            return GetCharsCommon(bytes, byteCount, chars, charCount, flush);
        }
#if NETSTANDARD2_1_OR_GREATER
        public override unsafe int GetChars(ReadOnlySpan<byte> bytes, Span<char> chars, bool flush)
        {
            fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
            fixed (char* charsPtr = &MemoryMarshal.GetReference(chars))
                return GetCharsCommon(bytesPtr, bytes.Length, charsPtr, chars.Length, flush);
        }
#endif
        public unsafe int GetCharsCommon(byte* pBytes, int byteCount, char* pChars, int charCount, bool flush)
        {
            ConvertCommon(pBytes, byteCount, pChars, charCount, flush, out int byteUsed, out int charsUsed, out _);
            if (byteUsed != byteCount)
                throw new ArgumentException(ExceptionResource.Argument_EncodingConversionOverflowChars, nameof(pChars));
            return charsUsed;
        }
        public override void Reset()
        {
            _bsi = 0;
            _mode = 0;
            _status = 0;
            _hasNext = false;
        }
    }

}