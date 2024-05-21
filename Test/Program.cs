using Myitian.Text;
using System.Security.Cryptography;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

ModifiedUTF8Encoding encMU8 = new();
UTF8Encoding encU8 = new();

string s = "😊🐱 ASCII 中文 ㈠㈡㈢ null(\0)";
Console.WriteLine("\e[97m* Original String:\e[39m");
Console.WriteLine(s);
string ns = s;
byte[] u16b = Encoding.Unicode.GetBytes(ns);
Console.WriteLine($"\e[90mEncoding={Encoding.Unicode.EncodingName},Length={u16b.Length},SHA1={Convert.ToHexString(SHA1.HashData(u16b))}\e[39m");
byte[] mu8b = encMU8.GetBytes(s);
byte[] u8b = encU8.GetBytes(s);

{
    Console.WriteLine("\n\e[97m* ModifiedUTF8 -> ModifiedUTF8:\e[39m");
    Console.WriteLine($"\e[90mEncoding={encMU8.EncodingName},Length={mu8b.Length},Content={Convert.ToHexString(mu8b)}\e[39m");
    Console.WriteLine(ns = encMU8.GetString(mu8b));
    u16b = Encoding.Unicode.GetBytes(ns);
    Console.WriteLine($"\e[90mEncoding={Encoding.Unicode.EncodingName},Length={u16b.Length},SHA1={Convert.ToHexString(SHA1.HashData(u16b))}\e[39m");
}
{
    Console.WriteLine("\n\e[97m* UTF8 -> ModifiedUTF8:\e[39m");
    Console.WriteLine($"\e[90mEncoding={encU8.EncodingName},Length={u8b.Length},Content={Convert.ToHexString(u8b)}\e[39m");
    Console.WriteLine(ns = encMU8.GetString(u8b));
    u16b = Encoding.Unicode.GetBytes(ns);
    Console.WriteLine($"\e[90mEncoding={Encoding.Unicode.EncodingName},Length={u16b.Length},SHA1={Convert.ToHexString(SHA1.HashData(u16b))}\e[39m");
}
{
    Console.WriteLine("\n\e[97m* ModifiedUTF8 -> UTF8:\e[39m");
    Console.WriteLine($"\e[90mEncoding={encMU8.EncodingName},Length={mu8b.Length},Content={Convert.ToHexString(mu8b)}\e[39m");
    Console.WriteLine(ns = encU8.GetString(mu8b));
    u16b = Encoding.Unicode.GetBytes(ns);
    Console.WriteLine($"\e[90mEncoding={Encoding.Unicode.EncodingName},Length={u16b.Length},SHA1={Convert.ToHexString(SHA1.HashData(u16b))}\e[39m");
}
{
    using MemoryStream ms = new();
    Console.WriteLine("\n\e[97m* ModifiedUTF8(Encoder) -> ModifiedUTF8:\e[39m");
    Encoder encoder = encMU8.GetEncoder();
    byte[] b = new byte[4];
    foreach (char c in s)
    {
        int count = encoder.GetBytes([c], b, false);
        ms.Write(b, 0, count);
    }
    byte[] arr = ms.ToArray();
    Console.WriteLine($"\e[90mEncoding={encMU8.EncodingName},Length={arr.Length},Content={Convert.ToHexString(arr)}\e[39m");
    Console.WriteLine(ns = encMU8.GetString(arr));
    u16b = Encoding.Unicode.GetBytes(ns);
    Console.WriteLine($"\e[90mEncoding={Encoding.Unicode.EncodingName},Length={u16b.Length},SHA1={Convert.ToHexString(SHA1.HashData(u16b))}\e[39m");
}
{
    StringBuilder sb = new();
    Console.WriteLine("\n\e[97m* ModifiedUTF8 -> ModifiedUTF8(Decoder):\e[39m");
    Console.WriteLine($"\e[90mEncoding={encMU8.EncodingName},Length={mu8b.Length},Content={Convert.ToHexString(mu8b)}\e[39m");
    Decoder decoder = encMU8.GetDecoder();
    char[] c = new char[2];
    foreach (byte b in mu8b)
    {
        int count = decoder.GetChars([b], c, false);
        sb.Append(c, 0, count);
    }
    Console.WriteLine(ns = sb.ToString());
    u16b = Encoding.Unicode.GetBytes(ns);
    Console.WriteLine($"\e[90mEncoding={Encoding.Unicode.EncodingName},Length={u16b.Length},SHA1={Convert.ToHexString(SHA1.HashData(u16b))}\e[39m");
}