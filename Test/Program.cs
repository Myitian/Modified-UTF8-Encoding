using Myitian.Text;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

ModifiedUTF8Encoding encMU8 = new();
UTF8Encoding encU8 = new();

string s = "😊🐱 ASCII 中文 ㈠㈡㈢ null(\0)";
Console.WriteLine("Original String:");
Console.WriteLine(s);

byte[] mu8b = encMU8.GetBytes(s);
byte[] u8b = encU8.GetBytes(s);

Console.WriteLine("\nModifiedUTF8 -> ModifiedUTF8:");
Console.WriteLine($"Encoding={encMU8.EncodingName},Length={mu8b.Length},Content={Convert.ToHexString(mu8b)}");
Console.WriteLine(encMU8.GetString(mu8b));

Console.WriteLine("\nUTF8 -> ModifiedUTF8:");
Console.WriteLine($"Encoding={encU8.EncodingName},Length={u8b.Length},Content={Convert.ToHexString(u8b)}");
Console.WriteLine(encMU8.GetString(u8b));

Console.WriteLine("\nModifiedUTF8 -> UTF8:");
Console.WriteLine($"Encoding={encMU8.EncodingName},Length={mu8b.Length},Content={Convert.ToHexString(mu8b)}");
Console.WriteLine(encU8.GetString(mu8b));