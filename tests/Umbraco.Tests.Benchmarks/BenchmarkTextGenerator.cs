using System.Text;

namespace Umbraco.Tests.Benchmarks;

public static class BenchmarkTextGenerator
{
    private const int Seed = 42;

    private static readonly char[] AsciiAlphaNum =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    private static readonly char[] AsciiPunctuation =
        " .,;:!?-_'\"()".ToCharArray();

    private static readonly char[] LatinAccented =
        "àáâãäåæèéêëìíîïñòóôõöøùúûüýÿÀÁÂÃÄÅÆÈÉÊËÌÍÎÏÑÒÓÔÕÖØÙÚÛÜÝŸœŒßðÐþÞ".ToCharArray();

    private static readonly char[] Cyrillic =
        "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя".ToCharArray();

    private static readonly char[] Symbols =
        "©®™€£¥°±×÷§¶†‡•".ToCharArray();

    private static readonly char[] WorstCaseCyrillic =
        "ЩЮЯЖЧШщюяжчш".ToCharArray();

    public static string GeneratePureAscii(int length) =>
        GenerateFromCharset(length, AsciiAlphaNum);

    public static string GenerateMixed(int length)
    {
        var random = new Random(Seed);
        var sb = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            var roll = random.Next(100);
            var charset = roll switch
            {
                < 70 => AsciiAlphaNum,
                < 85 => AsciiPunctuation,
                < 95 => LatinAccented,
                < 99 => Cyrillic,
                _ => Symbols
            };
            sb.Append(charset[random.Next(charset.Length)]);
        }

        return sb.ToString();
    }

    public static string GenerateWorstCase(int length) =>
        GenerateFromCharset(length, WorstCaseCyrillic);

    private static string GenerateFromCharset(int length, char[] charset)
    {
        var random = new Random(Seed);
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            sb.Append(charset[random.Next(charset.Length)]);
        return sb.ToString();
    }
}
