namespace Myitian.Text;
public partial class ModifiedUTF8Encoding
{
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

}