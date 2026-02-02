using System.Globalization;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

/// <summary>
/// Analyzes which character mappings are covered by Unicode normalization
/// vs which require explicit dictionary mappings.
/// </summary>
[TestFixture]
public class Utf8ToAsciiConverterNormalizationCoverageTests
{
    private static readonly Dictionary<string, string> GoldenMappings;

    static Utf8ToAsciiConverterNormalizationCoverageTests()
    {
        var testDataPath = Path.Combine(
            AppContext.BaseDirectory,
            "Umbraco.Core",
            "Strings",
            "TestData",
            "golden-mappings.json");

        if (!File.Exists(testDataPath))
        {
            throw new InvalidOperationException(
                $"Golden mappings file not found at: {testDataPath}");
        }

        var json = File.ReadAllText(testDataPath);
        var doc = JsonDocument.Parse(json);
        GoldenMappings = doc.RootElement
            .GetProperty("mappings")
            .EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetString() ?? "");
    }

    /// <summary>
    /// Test that demonstrates normalization-covered characters.
    /// This is the analysis test that generates the coverage report.
    /// </summary>
    [Test]
    public void AnalyzeNormalizationCoverage()
    {
        var normalizationCovered = new List<(string Char, string Expected)>();
        var dictionaryRequired = new List<(string Char, string Expected)>();

        foreach (var (inputChar, expected) in GoldenMappings)
        {
            if (inputChar.Length != 1)
            {
                // Skip multi-char inputs
                dictionaryRequired.Add((inputChar, expected));
                continue;
            }

            var normalizedResult = TryNormalize(inputChar[0]);

            if (normalizedResult == expected)
            {
                normalizationCovered.Add((inputChar, expected));
            }
            else
            {
                dictionaryRequired.Add((inputChar, expected));
            }
        }

        // Print summary to console for documentation purposes
        Console.WriteLine("=== UTF8 TO ASCII CONVERTER NORMALIZATION COVERAGE ===\n");
        Console.WriteLine($"Total original mappings: {GoldenMappings.Count}");
        Console.WriteLine($"Covered by normalization: {normalizationCovered.Count}");
        Console.WriteLine($"Require dictionary: {dictionaryRequired.Count}");
        Console.WriteLine($"Coverage ratio: {normalizationCovered.Count * 100.0 / GoldenMappings.Count:F1}%\n");

        // Print dictionary-required characters by category
        Console.WriteLine("=== DICTIONARY-REQUIRED CHARACTERS ===\n");

        // Categorize dictionary-required characters
        var ligatures = dictionaryRequired.Where(x =>
            x.Char is "Æ" or "æ" or "Œ" or "œ" or "ß" or "Ĳ" or "ĳ" ||
            x.Char.StartsWith('ﬀ') || // ff, fi, fl, ffi, ffl, st
            x.Expected.Length > 1 && x.Char.Length == 1).ToList();

        var specialLatin = dictionaryRequired.Where(x =>
            x.Char is "Ð" or "ð" or "Đ" or "đ" or "Ħ" or "ħ" or
            "Ł" or "ł" or "Ŀ" or "ŀ" or "Ø" or "ø" or
            "Þ" or "þ" or "Ŧ" or "ŧ").ToList();

        var cyrillic = dictionaryRequired.Where(x =>
        {
            if (x.Char.Length != 1) return false;
            var code = (int)x.Char[0];
            return code >= 0x0400 && code <= 0x04FF; // Cyrillic Unicode block
        }).ToList();

        var punctuationAndSymbols = dictionaryRequired.Where(x =>
        {
            if (x.Char.Length != 1) return false;
            var category = CharUnicodeInfo.GetUnicodeCategory(x.Char[0]);
            return category is
                UnicodeCategory.DashPunctuation or
                UnicodeCategory.OpenPunctuation or
                UnicodeCategory.ClosePunctuation or
                UnicodeCategory.InitialQuotePunctuation or
                UnicodeCategory.FinalQuotePunctuation or
                UnicodeCategory.OtherPunctuation or
                UnicodeCategory.MathSymbol or
                UnicodeCategory.CurrencySymbol or
                UnicodeCategory.ModifierSymbol or
                UnicodeCategory.OtherSymbol;
        }).ToList();

        var numbers = dictionaryRequired.Where(x =>
        {
            if (x.Char.Length != 1) return false;
            var category = CharUnicodeInfo.GetUnicodeCategory(x.Char[0]);
            return category is UnicodeCategory.OtherNumber or UnicodeCategory.LetterNumber;
        }).ToList();

        var other = dictionaryRequired.Except(ligatures)
            .Except(specialLatin)
            .Except(cyrillic)
            .Except(punctuationAndSymbols)
            .Except(numbers)
            .ToList();

        Console.WriteLine($"Ligatures: {ligatures.Count}");
        PrintCategory(ligatures.Take(20));

        Console.WriteLine($"\nSpecial Latin: {specialLatin.Count}");
        PrintCategory(specialLatin);

        Console.WriteLine($"\nCyrillic: {cyrillic.Count}");
        PrintCategory(cyrillic.Take(20));

        Console.WriteLine($"\nPunctuation & Symbols: {punctuationAndSymbols.Count}");
        PrintCategory(punctuationAndSymbols.Take(20));

        Console.WriteLine($"\nNumbers: {numbers.Count}");
        PrintCategory(numbers.Take(20));

        Console.WriteLine($"\nOther: {other.Count}");
        PrintCategory(other.Take(20));

        // Print examples of normalization-covered characters
        Console.WriteLine("\n=== NORMALIZATION-COVERED EXAMPLES ===\n");
        var accentedSamples = normalizationCovered
            .Where(x => x.Char.Length == 1 && x.Char[0] >= 'À' && x.Char[0] <= 'ÿ')
            .Take(30);
        PrintCategory(accentedSamples);

        // This test always passes - it's for analysis only
        Assert.Pass($"Analysis complete. {normalizationCovered.Count}/{GoldenMappings.Count} covered by normalization.");
    }

    private void PrintCategory(IEnumerable<(string Char, string Expected)> items)
    {
        foreach (var (ch, expected) in items)
        {
            var unicodeInfo = ch.Length == 1
                ? $"U+{((int)ch[0]):X4}"
                : $"{string.Join(", ", ch.Select(c => $"U+{((int)c):X4}"))}";
            Console.WriteLine($"  {ch} → {expected} ({unicodeInfo})");
        }
    }

    /// <summary>
    /// Tries to normalize a character using Unicode normalization (FormD).
    /// Returns the base character(s) after stripping combining marks.
    /// </summary>
    private static string TryNormalize(char c)
    {
        // Skip characters that won't normalize to ASCII
        if (c < '\u00C0')
        {
            return c.ToString();
        }

        // Normalize to FormD (decomposed form)
        ReadOnlySpan<char> input = stackalloc char[] { c };
        var normalized = input.ToString().Normalize(NormalizationForm.FormD);

        if (normalized.Length == 0)
        {
            return string.Empty;
        }

        // Copy only base characters (skip combining marks)
        var result = new StringBuilder();
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);

            // Skip combining marks (diacritics)
            if (category == UnicodeCategory.NonSpacingMark ||
                category == UnicodeCategory.SpacingCombiningMark ||
                category == UnicodeCategory.EnclosingMark)
            {
                continue;
            }

            // Only keep if it's now ASCII
            if (ch < '\u0080')
            {
                result.Append(ch);
            }
        }

        return result.ToString();
    }
}
