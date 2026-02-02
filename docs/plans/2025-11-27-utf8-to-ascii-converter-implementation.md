# Utf8ToAsciiConverter Refactor Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Refactor Utf8ToAsciiConverter from 3,631-line switch statement to SIMD-optimized, extensible implementation with JSON-based character mappings.

**Architecture:** SIMD ASCII detection via SearchValues, Unicode normalization for accented chars, FrozenDictionary for special cases (ligatures, Cyrillic). JSON files for mappings loaded at startup. Interface + DI for extensibility. Golden file testing ensures behavioral equivalence.

**Tech Stack:** .NET 9, System.Buffers.SearchValues, FrozenDictionary, System.Text.Json, xUnit, BenchmarkDotNet

> **Implementation Note:** This plan was written before analyzing the original Umbraco implementation. The actual Cyrillic mappings use simplified transliterations for backward compatibility with existing URLs (e.g., Щ→"Sh" instead of Щ→"Shch", Ц→"F" instead of Ц→"Ts"). See `cyrillic.json` for the actual mappings used.

---

## Task 0: Establish Performance Baseline

**Files:**
- Create: `tests/Umbraco.Tests.Benchmarks/Utf8ToAsciiConverterBaselineBenchmarks.cs`
- Create: `tests/Umbraco.Tests.Benchmarks/BenchmarkTextGenerator.cs`
- Create: `docs/benchmarks/utf8-converter-baseline-2025-11-27.md`

### Step 0.1: Create BenchmarkTextGenerator

**File:** `tests/Umbraco.Tests.Benchmarks/BenchmarkTextGenerator.cs`

```csharp
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
```

### Step 0.2: Create baseline benchmarks

**File:** `tests/Umbraco.Tests.Benchmarks/Utf8ToAsciiConverterBaselineBenchmarks.cs`

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[RankColumn]
[StatisticalTestColumn]
public class Utf8ToAsciiConverterBaselineBenchmarks
{
    private static readonly string TinyAscii = BenchmarkTextGenerator.GeneratePureAscii(10);
    private static readonly string TinyMixed = BenchmarkTextGenerator.GenerateMixed(10);
    private static readonly string SmallAscii = BenchmarkTextGenerator.GeneratePureAscii(100);
    private static readonly string SmallMixed = BenchmarkTextGenerator.GenerateMixed(100);
    private static readonly string MediumAscii = BenchmarkTextGenerator.GeneratePureAscii(1024);
    private static readonly string MediumMixed = BenchmarkTextGenerator.GenerateMixed(1024);
    private static readonly string LargeAscii = BenchmarkTextGenerator.GeneratePureAscii(100 * 1024);
    private static readonly string LargeMixed = BenchmarkTextGenerator.GenerateMixed(100 * 1024);
    private static readonly string LargeWorstCase = BenchmarkTextGenerator.GenerateWorstCase(100 * 1024);

    [Benchmark]
    public string Tiny_Ascii() => Utf8ToAsciiConverter.ToAsciiString(TinyAscii);

    [Benchmark]
    public string Tiny_Mixed() => Utf8ToAsciiConverter.ToAsciiString(TinyMixed);

    [Benchmark]
    public string Small_Ascii() => Utf8ToAsciiConverter.ToAsciiString(SmallAscii);

    [Benchmark]
    public string Small_Mixed() => Utf8ToAsciiConverter.ToAsciiString(SmallMixed);

    [Benchmark]
    public string Medium_Ascii() => Utf8ToAsciiConverter.ToAsciiString(MediumAscii);

    [Benchmark]
    public string Medium_Mixed() => Utf8ToAsciiConverter.ToAsciiString(MediumMixed);

    [Benchmark]
    public string Large_Ascii() => Utf8ToAsciiConverter.ToAsciiString(LargeAscii);

    [Benchmark]
    public string Large_Mixed() => Utf8ToAsciiConverter.ToAsciiString(LargeMixed);

    [Benchmark]
    public string Large_WorstCase() => Utf8ToAsciiConverter.ToAsciiString(LargeWorstCase);

    [Benchmark]
    public char[] CharArray_Medium_Mixed() => Utf8ToAsciiConverter.ToAsciiCharArray(MediumMixed);
}
```

### Step 0.3: Run baseline benchmarks

```bash
cd /home/yv01p/Umbraco-CMS
dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*Baseline*" --exporters markdown
```

Expected: Benchmark completes and outputs results table with Mean, Allocated columns.

### Step 0.4: Save baseline results

Copy the generated markdown table to `docs/benchmarks/utf8-converter-baseline-2025-11-27.md`:

```markdown
# Utf8ToAsciiConverter Baseline Benchmarks

**Date:** 2025-11-27
**Implementation:** Original 3,631-line switch statement
**Runtime:** .NET 9

## Results

| Method | Mean | Error | StdDev | Gen0 | Allocated |
|--------|------|-------|--------|------|-----------|
| ... (paste results) ... |

## Notes

- Baseline before SIMD refactor
- Used as comparison target for Task 7
```

### Step 0.5: Commit

```bash
git add tests/Umbraco.Tests.Benchmarks/BenchmarkTextGenerator.cs \
        tests/Umbraco.Tests.Benchmarks/Utf8ToAsciiConverterBaselineBenchmarks.cs \
        docs/benchmarks/utf8-converter-baseline-2025-11-27.md
git commit -m "perf(strings): establish Utf8ToAsciiConverter baseline benchmarks"
```

---

## Task 1: Create Interfaces

**Files:**
- Create: `src/Umbraco.Core/Strings/IUtf8ToAsciiConverter.cs`
- Create: `src/Umbraco.Core/Strings/ICharacterMappingLoader.cs`
- Create: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterInterfaceTests.cs`

### Step 1.1: Write test for IUtf8ToAsciiConverter interface existence

**File:** `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterInterfaceTests.cs`

```csharp
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

public class Utf8ToAsciiConverterInterfaceTests
{
    [Fact]
    public void IUtf8ToAsciiConverter_HasConvertStringMethod()
    {
        var type = typeof(IUtf8ToAsciiConverter);
        var method = type.GetMethod("Convert", new[] { typeof(string), typeof(char) });

        Assert.NotNull(method);
        Assert.Equal(typeof(string), method.ReturnType);
    }

    [Fact]
    public void IUtf8ToAsciiConverter_HasConvertSpanMethod()
    {
        var type = typeof(IUtf8ToAsciiConverter);
        var methods = type.GetMethods().Where(m => m.Name == "Convert").ToList();

        Assert.True(methods.Count >= 2, "Should have at least 2 Convert overloads");
    }
}
```

### Step 1.2: Run test to verify it fails

```bash
cd /home/yv01p/Umbraco-CMS
dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~Utf8ToAsciiConverterInterfaceTests" --no-build 2>&1 | head -20
```

Expected: Build failure - `IUtf8ToAsciiConverter` does not exist

### Step 1.3: Create IUtf8ToAsciiConverter interface

**File:** `src/Umbraco.Core/Strings/IUtf8ToAsciiConverter.cs`

```csharp
namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Converts UTF-8 text to ASCII, handling accented characters and transliteration.
/// </summary>
public interface IUtf8ToAsciiConverter
{
    /// <summary>
    /// Converts text to ASCII, returning a new string.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="fallback">Character to use for unmappable characters. Default '?'.</param>
    /// <returns>The ASCII-converted string.</returns>
    string Convert(string? text, char fallback = '?');

    /// <summary>
    /// Converts text to ASCII, writing to output span.
    /// Zero-allocation for callers who provide buffer.
    /// </summary>
    /// <param name="input">The input text span.</param>
    /// <param name="output">The output buffer. Must be at least input.Length * 4.</param>
    /// <param name="fallback">Character to use for unmappable characters. Default '?'.</param>
    /// <returns>Number of characters written to output.</returns>
    int Convert(ReadOnlySpan<char> input, Span<char> output, char fallback = '?');
}
```

### Step 1.4: Create ICharacterMappingLoader interface

**File:** `src/Umbraco.Core/Strings/ICharacterMappingLoader.cs`

```csharp
using System.Collections.Frozen;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Loads character mappings from JSON files.
/// </summary>
public interface ICharacterMappingLoader
{
    /// <summary>
    /// Loads all mapping files and returns combined FrozenDictionary.
    /// Higher priority mappings override lower priority.
    /// </summary>
    /// <returns>Frozen dictionary of character to string mappings.</returns>
    FrozenDictionary<char, string> LoadMappings();
}
```

### Step 1.5: Run test to verify it passes

```bash
cd /home/yv01p/Umbraco-CMS
dotnet build src/Umbraco.Core
dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~Utf8ToAsciiConverterInterfaceTests"
```

Expected: PASS

### Step 1.6: Commit

```bash
git add src/Umbraco.Core/Strings/IUtf8ToAsciiConverter.cs \
        src/Umbraco.Core/Strings/ICharacterMappingLoader.cs \
        tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterInterfaceTests.cs
git commit -m "feat(strings): add IUtf8ToAsciiConverter and ICharacterMappingLoader interfaces"
```

---

## Task 2: Extract Golden Mappings and Create JSON Files

**Files:**
- Create: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/TestData/golden-mappings.json`
- Create: `src/Umbraco.Core/Strings/CharacterMappings/ligatures.json`
- Create: `src/Umbraco.Core/Strings/CharacterMappings/special-latin.json`
- Create: `src/Umbraco.Core/Strings/CharacterMappings/cyrillic.json`
- Modify: `src/Umbraco.Core/Umbraco.Core.csproj`

### Step 2.1: Extract all mappings from original switch statement

Parse `src/Umbraco.Core/Strings/Utf8ToAsciiConverter.cs` and extract every case mapping.

**File:** `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/TestData/golden-mappings.json`

```json
{
  "source": "Extracted from Utf8ToAsciiConverter.cs switch statement",
  "extracted_date": "2025-11-27",
  "total_mappings": 1317,
  "mappings": {
    "À": "A",
    "Á": "A",
    "Â": "A",
    "Ã": "A",
    "Ä": "A",
    "Å": "A",
    "Æ": "AE",
    "Ç": "C",
    "È": "E",
    "É": "E",
    "Ê": "E",
    "Ë": "E",
    "Ì": "I",
    "Í": "I",
    "Î": "I",
    "Ï": "I",
    "Ð": "D",
    "Ñ": "N",
    "Ò": "O",
    "Ó": "O",
    "Ô": "O",
    "Õ": "O",
    "Ö": "O",
    "Ø": "O",
    "Ù": "U",
    "Ú": "U",
    "Û": "U",
    "Ü": "U",
    "Ý": "Y",
    "Þ": "TH",
    "ß": "ss",
    "à": "a",
    "á": "a",
    "â": "a",
    "ã": "a",
    "ä": "a",
    "å": "a",
    "æ": "ae",
    "ç": "c",
    "è": "e",
    "é": "e",
    "ê": "e",
    "ë": "e",
    "ì": "i",
    "í": "i",
    "î": "i",
    "ï": "i",
    "ð": "d",
    "ñ": "n",
    "ò": "o",
    "ó": "o",
    "ô": "o",
    "õ": "o",
    "ö": "o",
    "ø": "o",
    "ù": "u",
    "ú": "u",
    "û": "u",
    "ü": "u",
    "ý": "y",
    "þ": "th",
    "ÿ": "y",
    "Œ": "OE",
    "œ": "oe",
    "Ł": "L",
    "ł": "l",
    "Ħ": "H",
    "ħ": "h",
    "Đ": "D",
    "đ": "d",
    "Ŧ": "T",
    "ŧ": "t",
    "Ŀ": "L",
    "ŀ": "l",
    "А": "A",
    "а": "a",
    "Б": "B",
    "б": "b",
    "В": "V",
    "в": "v",
    "Г": "G",
    "г": "g",
    "Д": "D",
    "д": "d",
    "Е": "E",
    "е": "e",
    "Ё": "Yo",
    "ё": "yo",
    "Ж": "Zh",
    "ж": "zh",
    "З": "Z",
    "з": "z",
    "И": "I",
    "и": "i",
    "Й": "Y",
    "й": "y",
    "К": "K",
    "к": "k",
    "Л": "L",
    "л": "l",
    "М": "M",
    "м": "m",
    "Н": "N",
    "н": "n",
    "О": "O",
    "о": "o",
    "П": "P",
    "п": "p",
    "Р": "R",
    "р": "r",
    "С": "S",
    "с": "s",
    "Т": "T",
    "т": "t",
    "У": "U",
    "у": "u",
    "Ф": "F",
    "ф": "f",
    "Х": "Kh",
    "х": "kh",
    "Ц": "Ts",
    "ц": "ts",
    "Ч": "Ch",
    "ч": "ch",
    "Ш": "Sh",
    "ш": "sh",
    "Щ": "Shch",
    "щ": "shch",
    "Ъ": "",
    "ъ": "",
    "Ы": "Y",
    "ы": "y",
    "Ь": "",
    "ь": "",
    "Э": "E",
    "э": "e",
    "Ю": "Yu",
    "ю": "yu",
    "Я": "Ya",
    "я": "ya",
    "ﬀ": "ff",
    "ﬁ": "fi",
    "ﬂ": "fl",
    "ﬃ": "ffi",
    "ﬄ": "ffl",
    "ﬅ": "st",
    "ﬆ": "st",
    "Ĳ": "IJ",
    "ĳ": "ij"
  }
}
```

**Note:** The above is a representative subset. The full extraction should include all 1,317 mappings from the original switch statement. Use a script or manual extraction to complete.

### Step 2.2: Create CharacterMappings directory

```bash
mkdir -p src/Umbraco.Core/Strings/CharacterMappings
```

### Step 2.3: Create ligatures.json

**File:** `src/Umbraco.Core/Strings/CharacterMappings/ligatures.json`

```json
{
  "name": "Ligatures",
  "description": "Ligature characters expanded to component letters",
  "priority": 0,
  "mappings": {
    "Æ": "AE",
    "æ": "ae",
    "Œ": "OE",
    "œ": "oe",
    "Ĳ": "IJ",
    "ĳ": "ij",
    "ß": "ss",
    "ﬀ": "ff",
    "ﬁ": "fi",
    "ﬂ": "fl",
    "ﬃ": "ffi",
    "ﬄ": "ffl",
    "ﬅ": "st",
    "ﬆ": "st"
  }
}
```

### Step 2.4: Create special-latin.json

**File:** `src/Umbraco.Core/Strings/CharacterMappings/special-latin.json`

```json
{
  "name": "Special Latin",
  "description": "Latin characters that do not decompose via Unicode normalization",
  "priority": 0,
  "mappings": {
    "Ð": "D",
    "ð": "d",
    "Đ": "D",
    "đ": "d",
    "Ħ": "H",
    "ħ": "h",
    "Ł": "L",
    "ł": "l",
    "Ŀ": "L",
    "ŀ": "l",
    "Ø": "O",
    "ø": "o",
    "Þ": "TH",
    "þ": "th",
    "Ŧ": "T",
    "ŧ": "t"
  }
}
```

### Step 2.5: Create cyrillic.json

**File:** `src/Umbraco.Core/Strings/CharacterMappings/cyrillic.json`

```json
{
  "name": "Cyrillic",
  "description": "Russian Cyrillic to Latin transliteration",
  "priority": 0,
  "mappings": {
    "А": "A",
    "а": "a",
    "Б": "B",
    "б": "b",
    "В": "V",
    "в": "v",
    "Г": "G",
    "г": "g",
    "Д": "D",
    "д": "d",
    "Е": "E",
    "е": "e",
    "Ё": "Yo",
    "ё": "yo",
    "Ж": "Zh",
    "ж": "zh",
    "З": "Z",
    "з": "z",
    "И": "I",
    "и": "i",
    "Й": "Y",
    "й": "y",
    "К": "K",
    "к": "k",
    "Л": "L",
    "л": "l",
    "М": "M",
    "м": "m",
    "Н": "N",
    "н": "n",
    "О": "O",
    "о": "o",
    "П": "P",
    "п": "p",
    "Р": "R",
    "р": "r",
    "С": "S",
    "с": "s",
    "Т": "T",
    "т": "t",
    "У": "U",
    "у": "u",
    "Ф": "F",
    "ф": "f",
    "Х": "Kh",
    "х": "kh",
    "Ц": "Ts",
    "ц": "ts",
    "Ч": "Ch",
    "ч": "ch",
    "Ш": "Sh",
    "ш": "sh",
    "Щ": "Shch",
    "щ": "shch",
    "Ъ": "",
    "ъ": "",
    "Ы": "Y",
    "ы": "y",
    "Ь": "",
    "ь": "",
    "Э": "E",
    "э": "e",
    "Ю": "Yu",
    "ю": "yu",
    "Я": "Ya",
    "я": "ya"
  }
}
```

### Step 2.6: Update csproj to embed JSON files

**File:** `src/Umbraco.Core/Umbraco.Core.csproj` - Add this ItemGroup:

```xml
  <ItemGroup>
    <EmbeddedResource Include="Strings\CharacterMappings\*.json" />
  </ItemGroup>
```

### Step 2.7: Verify embedded resources

```bash
cd /home/yv01p/Umbraco-CMS
dotnet build src/Umbraco.Core
unzip -l src/Umbraco.Core/bin/Debug/net9.0/Umbraco.Cms.Core.dll | grep -i json
```

Expected: Should show the three embedded JSON files.

### Step 2.8: Commit

```bash
git add src/Umbraco.Core/Strings/CharacterMappings/*.json \
        src/Umbraco.Core/Umbraco.Core.csproj \
        tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/TestData/golden-mappings.json
git commit -m "feat(strings): add character mapping JSON files and golden test data"
```

---

## Task 3: Implement CharacterMappingLoader

**Files:**
- Create: `src/Umbraco.Core/Strings/CharacterMappingLoader.cs`
- Create: `src/Umbraco.Core/Strings/CharacterMappingFile.cs`
- Create: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/CharacterMappingLoaderTests.cs`

### Step 3.1: Write failing test for CharacterMappingLoader

**File:** `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/CharacterMappingLoaderTests.cs`

```csharp
using System.Collections.Frozen;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

public class CharacterMappingLoaderTests
{
    [Fact]
    public void LoadMappings_LoadsBuiltInMappings()
    {
        // Arrange
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        // Act
        var mappings = loader.LoadMappings();

        // Assert
        Assert.NotNull(mappings);
        Assert.True(mappings.Count > 0, "Should have loaded mappings");
    }

    [Fact]
    public void LoadMappings_ContainsLigatures()
    {
        // Arrange
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        // Act
        var mappings = loader.LoadMappings();

        // Assert
        Assert.Equal("OE", mappings['Œ']);
        Assert.Equal("ae", mappings['æ']);
        Assert.Equal("ss", mappings['ß']);
    }

    [Fact]
    public void LoadMappings_ContainsCyrillic()
    {
        // Arrange
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        // Act
        var mappings = loader.LoadMappings();

        // Assert
        Assert.Equal("Shch", mappings['Щ']);
        Assert.Equal("zh", mappings['ж']);
        Assert.Equal("Ya", mappings['Я']);
    }

    [Fact]
    public void LoadMappings_ContainsSpecialLatin()
    {
        // Arrange
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        // Act
        var mappings = loader.LoadMappings();

        // Assert
        Assert.Equal("L", mappings['Ł']);
        Assert.Equal("O", mappings['Ø']);
        Assert.Equal("TH", mappings['Þ']);
    }
}
```

### Step 3.2: Run test to verify it fails

```bash
cd /home/yv01p/Umbraco-CMS
dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~CharacterMappingLoaderTests" --no-build 2>&1 | head -20
```

Expected: Build failure - `CharacterMappingLoader` does not exist

### Step 3.3: Create CharacterMappingFile model

**File:** `src/Umbraco.Core/Strings/CharacterMappingFile.cs`

```csharp
namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Represents a character mapping JSON file.
/// </summary>
internal sealed class CharacterMappingFile
{
    /// <summary>
    /// Name of the mapping set.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Priority for override ordering. Higher values override lower.
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Character to string mappings.
    /// </summary>
    public required Dictionary<string, string> Mappings { get; init; }
}
```

### Step 3.4: Implement CharacterMappingLoader

**File:** `src/Umbraco.Core/Strings/CharacterMappingLoader.cs`

```csharp
using System.Collections.Frozen;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Loads character mappings from embedded JSON files and user configuration.
/// </summary>
public sealed class CharacterMappingLoader : ICharacterMappingLoader
{
    private static readonly string[] BuiltInFiles =
        ["ligatures.json", "special-latin.json", "cyrillic.json"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<CharacterMappingLoader> _logger;

    public CharacterMappingLoader(
        IHostEnvironment hostEnvironment,
        ILogger<CharacterMappingLoader> logger)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    /// <inheritdoc />
    public FrozenDictionary<char, string> LoadMappings()
    {
        var allMappings = new List<(int Priority, string Name, Dictionary<string, string> Mappings)>();

        // 1. Load built-in mappings from embedded resources
        foreach (var file in BuiltInFiles)
        {
            var mapping = LoadEmbeddedMapping(file);
            if (mapping != null)
            {
                allMappings.Add((mapping.Priority, mapping.Name, mapping.Mappings));
                _logger.LogDebug(
                    "Loaded built-in character mappings: {Name} ({Count} entries)",
                    mapping.Name, mapping.Mappings.Count);
            }
        }

        // 2. Load user mappings from config directory
        var userPath = Path.Combine(
            _hostEnvironment.ContentRootPath,
            "config",
            "character-mappings");

        if (Directory.Exists(userPath))
        {
            foreach (var file in Directory.GetFiles(userPath, "*.json"))
            {
                var mapping = LoadJsonFile(file);
                if (mapping != null)
                {
                    allMappings.Add((mapping.Priority, mapping.Name, mapping.Mappings));
                    _logger.LogInformation(
                        "Loaded user character mappings: {Name} ({Count} entries, priority {Priority})",
                        mapping.Name, mapping.Mappings.Count, mapping.Priority);
                }
            }
        }

        // 3. Merge by priority (higher priority wins)
        return MergeMappings(allMappings);
    }

    private static FrozenDictionary<char, string> MergeMappings(
        List<(int Priority, string Name, Dictionary<string, string> Mappings)> allMappings)
    {
        var merged = new Dictionary<char, string>();

        foreach (var (_, _, mappings) in allMappings.OrderBy(m => m.Priority))
        {
            foreach (var (key, value) in mappings)
            {
                if (key.Length == 1)
                {
                    merged[key[0]] = value;
                }
            }
        }

        return merged.ToFrozenDictionary();
    }

    private CharacterMappingFile? LoadEmbeddedMapping(string fileName)
    {
        var assembly = typeof(CharacterMappingLoader).Assembly;
        var resourceName = $"Umbraco.Cms.Core.Strings.CharacterMappings.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            _logger.LogWarning(
                "Built-in character mapping file not found: {ResourceName}",
                resourceName);
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CharacterMappingFile>(stream, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse embedded mapping: {ResourceName}", resourceName);
            return null;
        }
    }

    private CharacterMappingFile? LoadJsonFile(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            var mapping = JsonSerializer.Deserialize<CharacterMappingFile>(json, JsonOptions);

            if (mapping?.Mappings == null)
            {
                _logger.LogWarning(
                    "Invalid mapping file {Path}: missing 'mappings' property", path);
                return null;
            }

            return mapping;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse character mappings from {Path}", path);
            return null;
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to read character mappings from {Path}", path);
            return null;
        }
    }
}
```

### Step 3.5: Run tests to verify they pass

```bash
cd /home/yv01p/Umbraco-CMS
dotnet build src/Umbraco.Core
dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~CharacterMappingLoaderTests"
```

Expected: All tests PASS

### Step 3.6: Commit

```bash
git add src/Umbraco.Core/Strings/CharacterMappingFile.cs \
        src/Umbraco.Core/Strings/CharacterMappingLoader.cs \
        tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/CharacterMappingLoaderTests.cs
git commit -m "feat(strings): implement CharacterMappingLoader for JSON-based character mappings"
```

---

## Task 4: Implement Utf8ToAsciiConverterNew with Golden File Tests

**Files:**
- Create: `src/Umbraco.Core/Strings/Utf8ToAsciiConverterNew.cs`
- Create: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterNewTests.cs`
- Create: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterGoldenTests.cs`

### Step 4.1: Write failing unit tests for converter

**File:** `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterNewTests.cs`

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

public class Utf8ToAsciiConverterNewTests
{
    private readonly IUtf8ToAsciiConverter _converter;

    public Utf8ToAsciiConverterNewTests()
    {
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        _converter = new Utf8ToAsciiConverterNew(loader);
    }

    // === Null/Empty ===

    [Fact]
    public void Convert_Null_ReturnsEmpty()
        => Assert.Equal(string.Empty, _converter.Convert(null));

    [Fact]
    public void Convert_Empty_ReturnsEmpty()
        => Assert.Equal(string.Empty, _converter.Convert(string.Empty));

    // === ASCII Fast Path ===

    [Theory]
    [InlineData("hello world", "hello world")]
    [InlineData("ABC123", "ABC123")]
    [InlineData("The quick brown fox", "The quick brown fox")]
    public void Convert_AsciiOnly_ReturnsSameString(string input, string expected)
        => Assert.Equal(expected, _converter.Convert(input));

    // === Normalization (Accented Characters) ===

    [Theory]
    [InlineData("cafe", "cafe")]
    [InlineData("naive", "naive")]
    [InlineData("resume", "resume")]
    public void Convert_AccentedChars_NormalizesCorrectly(string input, string expected)
        => Assert.Equal(expected, _converter.Convert(input));

    // === Ligatures ===

    [Theory]
    [InlineData("Œuvre", "OEuvre")]
    [InlineData("Ærodynamic", "AErodynamic")]
    [InlineData("straße", "strasse")]
    public void Convert_Ligatures_ExpandsCorrectly(string input, string expected)
        => Assert.Equal(expected, _converter.Convert(input));

    // === Cyrillic ===

    [Theory]
    [InlineData("Москва", "Moskva")]
    [InlineData("Борщ", "Borshch")]
    [InlineData("Щука", "Shchuka")]
    [InlineData("Привет", "Privet")]
    public void Convert_Cyrillic_TransliteratesCorrectly(string input, string expected)
        => Assert.Equal(expected, _converter.Convert(input));

    // === Special Latin ===

    [Theory]
    [InlineData("Łódź", "Lodz")]
    [InlineData("Ørsted", "Orsted")]
    [InlineData("Þórr", "Thorr")]
    public void Convert_SpecialLatin_ConvertsCorrectly(string input, string expected)
        => Assert.Equal(expected, _converter.Convert(input));

    // === Span API ===

    [Fact]
    public void Convert_SpanApi_WritesToOutputBuffer()
    {
        ReadOnlySpan<char> input = "cafe";
        Span<char> output = stackalloc char[20];

        var written = _converter.Convert(input, output);

        Assert.Equal(4, written);
        Assert.Equal("cafe", new string(output[..written]));
    }

    [Fact]
    public void Convert_SpanApi_HandlesExpansion()
    {
        ReadOnlySpan<char> input = "Щ"; // Expands to "Shch" (4 chars)
        Span<char> output = stackalloc char[20];

        var written = _converter.Convert(input, output);

        Assert.Equal(4, written);
        Assert.Equal("Shch", new string(output[..written]));
    }

    // === Mixed Content ===

    [Fact]
    public void Convert_MixedContent_HandlesCorrectly()
    {
        var input = "Cafe Muller in Moskva";
        var expected = "Cafe Muller in Moskva";

        Assert.Equal(expected, _converter.Convert(input));
    }
}
```

### Step 4.2: Write golden file tests

**File:** `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterGoldenTests.cs`

```csharp
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

public class Utf8ToAsciiConverterGoldenTests
{
    private readonly IUtf8ToAsciiConverter _newConverter;
    private static readonly Dictionary<string, string> GoldenMappings;

    static Utf8ToAsciiConverterGoldenTests()
    {
        var testDataPath = Path.Combine(
            AppContext.BaseDirectory,
            "Umbraco.Core",
            "Strings",
            "TestData",
            "golden-mappings.json");

        if (File.Exists(testDataPath))
        {
            var json = File.ReadAllText(testDataPath);
            var doc = JsonDocument.Parse(json);
            GoldenMappings = doc.RootElement
                .GetProperty("mappings")
                .EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.GetString() ?? "");
        }
        else
        {
            GoldenMappings = new Dictionary<string, string>();
        }
    }

    public Utf8ToAsciiConverterGoldenTests()
    {
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        _newConverter = new Utf8ToAsciiConverterNew(loader);
    }

    public static IEnumerable<object[]> GetGoldenMappings()
    {
        foreach (var (input, expected) in GoldenMappings)
        {
            yield return new object[] { input, expected };
        }
    }

    [Theory]
    [MemberData(nameof(GetGoldenMappings))]
    public void NewConverter_MatchesGoldenMapping(string input, string expected)
    {
        var result = _newConverter.Convert(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(GetGoldenMappings))]
    public void NewConverter_MatchesOriginalBehavior(string input, string expected)
    {
        // Compare new implementation against original
        var originalResult = Utf8ToAsciiConverter.ToAsciiString(input);
        var newResult = _newConverter.Convert(input);

        Assert.Equal(originalResult, newResult);
    }
}
```

### Step 4.3: Run tests to verify they fail

```bash
cd /home/yv01p/Umbraco-CMS
dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~Utf8ToAsciiConverterNewTests" --no-build 2>&1 | head -20
```

Expected: Build failure - `Utf8ToAsciiConverterNew` does not exist

### Step 4.4: Implement Utf8ToAsciiConverterNew

**File:** `src/Umbraco.Core/Strings/Utf8ToAsciiConverterNew.cs`

```csharp
using System.Buffers;
using System.Collections.Frozen;
using System.Globalization;
using System.Text;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// SIMD-optimized UTF-8 to ASCII converter with extensible character mappings.
/// </summary>
public sealed class Utf8ToAsciiConverterNew : IUtf8ToAsciiConverter
{
    // SIMD-optimized ASCII detection (uses AVX-512 when available)
    private static readonly SearchValues<char> AsciiPrintable =
        SearchValues.Create(" !\"#$%&'()*+,-./0123456789:;<=>?@" +
                           "ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`" +
                           "abcdefghijklmnopqrstuvwxyz{|}~");

    private readonly FrozenDictionary<char, string> _mappings;

    public Utf8ToAsciiConverterNew(ICharacterMappingLoader mappingLoader)
    {
        _mappings = mappingLoader.LoadMappings();
    }

    /// <inheritdoc />
    public string Convert(string? text, char fallback = '?')
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var input = text.AsSpan();

        // Fast path: all ASCII - no conversion needed
        if (input.IndexOfAnyExcept(AsciiPrintable) == -1)
        {
            return text;
        }

        // Allocate output buffer (worst case: each char becomes 4, e.g., Щ→Shch)
        var maxLen = text.Length * 4;
        char[] arrayBuffer = ArrayPool<char>.Shared.Rent(maxLen);
        try
        {
            var written = Convert(input, arrayBuffer.AsSpan(), fallback);
            return new string(arrayBuffer, 0, written);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arrayBuffer);
        }
    }

    /// <inheritdoc />
    public int Convert(ReadOnlySpan<char> input, Span<char> output, char fallback = '?')
    {
        if (input.IsEmpty)
        {
            return 0;
        }

        var opos = 0;
        var ipos = 0;

        while (ipos < input.Length)
        {
            // Find next non-ASCII character using SIMD
            var remaining = input[ipos..];
            var asciiLen = remaining.IndexOfAnyExcept(AsciiPrintable);

            if (asciiLen == -1)
            {
                // Rest is all ASCII - bulk copy
                remaining.CopyTo(output[opos..]);
                return opos + remaining.Length;
            }

            if (asciiLen > 0)
            {
                // Copy ASCII prefix
                remaining[..asciiLen].CopyTo(output[opos..]);
                opos += asciiLen;
                ipos += asciiLen;
            }

            // Process non-ASCII character
            var c = input[ipos];

            // Handle surrogate pairs (emoji, etc.)
            if (char.IsSurrogate(c))
            {
                output[opos++] = fallback;
                ipos++;
                if (ipos < input.Length && char.IsLowSurrogate(input[ipos]))
                {
                    ipos++; // Skip low surrogate
                }
                continue;
            }

            opos += ProcessNonAscii(c, output[opos..], fallback);
            ipos++;
        }

        return opos;
    }

    private int ProcessNonAscii(char c, Span<char> output, char fallback)
    {
        // 1. Check special cases dictionary (ligatures, Cyrillic, etc.)
        if (_mappings.TryGetValue(c, out var mapped))
        {
            if (mapped.Length == 0)
            {
                return 0; // Empty mapping = strip character
            }
            mapped.AsSpan().CopyTo(output);
            return mapped.Length;
        }

        // 2. Try Unicode normalization (handles most accented chars)
        var normLen = TryNormalize(c, output);
        if (normLen > 0)
        {
            return normLen;
        }

        // 3. Control character handling
        if (char.IsControl(c))
        {
            return 0; // Strip control characters
        }

        // 4. Whitespace normalization
        if (char.IsWhiteSpace(c))
        {
            output[0] = ' ';
            return 1;
        }

        // 5. Fallback for unmapped characters
        output[0] = fallback;
        return 1;
    }

    private static int TryNormalize(char c, Span<char> output)
    {
        // Skip characters that won't normalize to ASCII
        if (c < '\u00C0')
        {
            return 0;
        }

        // Normalize to FormD (decomposed form)
        ReadOnlySpan<char> input = stackalloc char[] { c };
        var normalized = input.ToString().Normalize(NormalizationForm.FormD);

        if (normalized.Length == 0)
        {
            return 0;
        }

        // Copy only base characters (skip combining marks)
        var len = 0;
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
                output[len++] = ch;
            }
        }

        return len;
    }
}
```

### Step 4.5: Run tests to verify they pass

```bash
cd /home/yv01p/Umbraco-CMS
dotnet build src/Umbraco.Core
dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~Utf8ToAsciiConverterNewTests"
dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~Utf8ToAsciiConverterGoldenTests"
```

Expected: All tests PASS

### Step 4.6: Fix any failing tests

If tests fail, debug and fix. Common issues:
- Normalization edge cases
- Character category detection
- Span bounds
- Missing mappings in JSON files

### Step 4.7: Commit

```bash
git add src/Umbraco.Core/Strings/Utf8ToAsciiConverterNew.cs \
        tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterNewTests.cs \
        tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterGoldenTests.cs
git commit -m "feat(strings): implement SIMD-optimized Utf8ToAsciiConverterNew with golden file tests"
```

---

## Task 5: Replace Original and Create Backward Compatibility Wrapper

**Files:**
- Rename: `src/Umbraco.Core/Strings/Utf8ToAsciiConverter.cs` → `Utf8ToAsciiConverterOriginal.cs`
- Rename: `src/Umbraco.Core/Strings/Utf8ToAsciiConverterNew.cs` → `Utf8ToAsciiConverter.cs`
- Create: `src/Umbraco.Core/Strings/Utf8ToAsciiConverterStatic.cs`

### Step 5.1: Rename original to keep as reference

```bash
cd /home/yv01p/Umbraco-CMS
mv src/Umbraco.Core/Strings/Utf8ToAsciiConverter.cs \
   src/Umbraco.Core/Strings/Utf8ToAsciiConverterOriginal.cs
```

### Step 5.2: Mark original as not compiled

Edit `src/Umbraco.Core/Strings/Utf8ToAsciiConverterOriginal.cs`:

Add at the top of the file:
```csharp
#if false // Kept for historical reference only - not compiled
```

Add at the bottom of the file:
```csharp
#endif
```

### Step 5.3: Rename new implementation to replace original

```bash
mv src/Umbraco.Core/Strings/Utf8ToAsciiConverterNew.cs \
   src/Umbraco.Core/Strings/Utf8ToAsciiConverter.cs
```

### Step 5.4: Update class name in new file

In `src/Umbraco.Core/Strings/Utf8ToAsciiConverter.cs`:
- Change `class Utf8ToAsciiConverterNew` to `class Utf8ToAsciiConverter`

### Step 5.5: Create static wrapper for backward compatibility

**File:** `src/Umbraco.Core/Strings/Utf8ToAsciiConverterStatic.cs`

```csharp
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Static wrapper for backward compatibility with existing code.
/// </summary>
/// <remarks>
/// Use <see cref="IUtf8ToAsciiConverter"/> via dependency injection for new code.
/// </remarks>
public static class Utf8ToAsciiConverterStatic
{
    private static readonly Lazy<IUtf8ToAsciiConverter> DefaultConverter = new(() =>
    {
        var hostEnv = new HostingEnvironment { ContentRootPath = AppContext.BaseDirectory };
        var loader = new CharacterMappingLoader(hostEnv, NullLogger<CharacterMappingLoader>.Instance);
        return new Utf8ToAsciiConverter(loader);
    });

    /// <summary>
    /// Converts an UTF-8 string into an ASCII string.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="fail">The character to use to replace characters that cannot be converted.</param>
    /// <returns>The converted text.</returns>
    [Obsolete("Use IUtf8ToAsciiConverter via dependency injection. This will be removed in v15.")]
    public static string ToAsciiString(string text, char fail = '?')
        => DefaultConverter.Value.Convert(text, fail);

    /// <summary>
    /// Converts an UTF-8 string into an array of ASCII characters.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="fail">The character to use to replace characters that cannot be converted.</param>
    /// <returns>The converted text as char array.</returns>
    [Obsolete("Use IUtf8ToAsciiConverter via dependency injection. This will be removed in v15.")]
    public static char[] ToAsciiCharArray(string text, char fail = '?')
        => DefaultConverter.Value.Convert(text, fail).ToCharArray();
}
```

### Step 5.6: Update test files

Rename test file and update references:

```bash
mv tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterNewTests.cs \
   tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/Utf8ToAsciiConverterTests.cs
```

In the test file:
- Change class name from `Utf8ToAsciiConverterNewTests` to `Utf8ToAsciiConverterTests`
- Change `new Utf8ToAsciiConverterNew(loader)` to `new Utf8ToAsciiConverter(loader)`

Update golden tests similarly.

### Step 5.7: Run all tests

```bash
cd /home/yv01p/Umbraco-CMS
dotnet build src/Umbraco.Core
dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~Utf8ToAsciiConverter"
```

Expected: All tests PASS

### Step 5.8: Commit

```bash
git add -A src/Umbraco.Core/Strings/
git add tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/
git commit -m "refactor(strings): replace Utf8ToAsciiConverter with SIMD-optimized implementation

- Rename original to Utf8ToAsciiConverterOriginal.cs (kept as reference, not compiled)
- Rename Utf8ToAsciiConverterNew to Utf8ToAsciiConverter
- Add Utf8ToAsciiConverterStatic with [Obsolete] static methods for backward compat"
```

---

## Task 6: Update Consumers and DI Registration

**Files:**
- Modify: `src/Umbraco.Core/Strings/DefaultShortStringHelper.cs`
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.CoreServices.cs` (or similar)

### Step 6.1: Find DefaultShortStringHelper usage

```bash
grep -n "Utf8ToAsciiConverter" src/Umbraco.Core/Strings/DefaultShortStringHelper.cs
```

### Step 6.2: Update DefaultShortStringHelper to use DI

Add constructor parameter and field:

```csharp
private readonly IUtf8ToAsciiConverter _asciiConverter;

public DefaultShortStringHelper(
    IOptionsMonitor<RequestHandlerSettings> requestHandlerSettings,
    IUtf8ToAsciiConverter asciiConverter)  // Add this parameter
{
    _asciiConverter = asciiConverter;
    // ... existing code
}
```

Replace static calls:

```csharp
// Before:
text = Utf8ToAsciiConverter.ToAsciiString(text);

// After:
text = _asciiConverter.Convert(text);
```

### Step 6.3: Find DI registration file

```bash
grep -rn "AddSingleton.*IShortStringHelper" src/Umbraco.Core/
```

### Step 6.4: Register services in DI

Add to the appropriate DI registration file:

```csharp
services.AddSingleton<ICharacterMappingLoader, CharacterMappingLoader>();
services.AddSingleton<IUtf8ToAsciiConverter, Utf8ToAsciiConverter>();
```

### Step 6.5: Run full test suite

```bash
cd /home/yv01p/Umbraco-CMS
dotnet build
dotnet test tests/Umbraco.Tests.UnitTests
```

### Step 6.6: Commit

```bash
git add src/Umbraco.Core/Strings/DefaultShortStringHelper.cs \
        src/Umbraco.Core/DependencyInjection/*.cs
git commit -m "refactor(strings): update DefaultShortStringHelper to use IUtf8ToAsciiConverter via DI"
```

---

## Task 7: Run Final Benchmarks and Compare

**Files:**
- Create: `docs/benchmarks/utf8-converter-final-2025-11-27.md`
- Create: `docs/benchmarks/utf8-converter-comparison-2025-11-27.md`

### Step 7.1: Update benchmarks to use new implementation

**File:** `tests/Umbraco.Tests.Benchmarks/Utf8ToAsciiConverterBenchmarks.cs`

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[RankColumn]
[StatisticalTestColumn]
public class Utf8ToAsciiConverterBenchmarks
{
    private static readonly string TinyAscii = BenchmarkTextGenerator.GeneratePureAscii(10);
    private static readonly string TinyMixed = BenchmarkTextGenerator.GenerateMixed(10);
    private static readonly string SmallAscii = BenchmarkTextGenerator.GeneratePureAscii(100);
    private static readonly string SmallMixed = BenchmarkTextGenerator.GenerateMixed(100);
    private static readonly string MediumAscii = BenchmarkTextGenerator.GeneratePureAscii(1024);
    private static readonly string MediumMixed = BenchmarkTextGenerator.GenerateMixed(1024);
    private static readonly string LargeAscii = BenchmarkTextGenerator.GeneratePureAscii(100 * 1024);
    private static readonly string LargeMixed = BenchmarkTextGenerator.GenerateMixed(100 * 1024);
    private static readonly string LargeWorstCase = BenchmarkTextGenerator.GenerateWorstCase(100 * 1024);

    private IUtf8ToAsciiConverter _converter = null!;

    [GlobalSetup]
    public void Setup()
    {
        var hostEnv = new HostingEnvironment { ContentRootPath = AppContext.BaseDirectory };
        var loader = new CharacterMappingLoader(hostEnv, NullLogger<CharacterMappingLoader>.Instance);
        _converter = new Utf8ToAsciiConverter(loader);
    }

    [Benchmark]
    public string Tiny_Ascii() => _converter.Convert(TinyAscii);

    [Benchmark]
    public string Tiny_Mixed() => _converter.Convert(TinyMixed);

    [Benchmark]
    public string Small_Ascii() => _converter.Convert(SmallAscii);

    [Benchmark]
    public string Small_Mixed() => _converter.Convert(SmallMixed);

    [Benchmark]
    public string Medium_Ascii() => _converter.Convert(MediumAscii);

    [Benchmark]
    public string Medium_Mixed() => _converter.Convert(MediumMixed);

    [Benchmark]
    public string Large_Ascii() => _converter.Convert(LargeAscii);

    [Benchmark]
    public string Large_Mixed() => _converter.Convert(LargeMixed);

    [Benchmark]
    public string Large_WorstCase() => _converter.Convert(LargeWorstCase);

    [Benchmark]
    public int Span_Medium_Mixed()
    {
        Span<char> buffer = stackalloc char[4096];
        return _converter.Convert(MediumMixed.AsSpan(), buffer);
    }
}
```

### Step 7.2: Run final benchmarks

```bash
cd /home/yv01p/Umbraco-CMS
dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*Utf8ToAsciiConverterBenchmarks*" --exporters markdown
```

### Step 7.3: Save final results

Copy the generated markdown table to `docs/benchmarks/utf8-converter-final-2025-11-27.md`:

```markdown
# Utf8ToAsciiConverter Final Benchmarks

**Date:** 2025-11-27
**Implementation:** SIMD-optimized with FrozenDictionary
**Runtime:** .NET 9

## Results

| Method | Mean | Error | StdDev | Gen0 | Allocated |
|--------|------|-------|--------|------|-----------|
| ... (paste results) ... |
```

### Step 7.4: Create comparison document

**File:** `docs/benchmarks/utf8-converter-comparison-2025-11-27.md`

```markdown
# Utf8ToAsciiConverter Performance Comparison

**Date:** 2025-11-27

## Summary

| Scenario | Baseline | Final | Speedup | Memory Δ |
|----------|----------|-------|---------|----------|
| Tiny_Ascii | X ns | Y ns | Zx | -N% |
| Large_Ascii | X µs | Y µs | Zx | -N% |
| Large_Mixed | X µs | Y µs | Zx | -N% |
| ... | ... | ... | ... | ... |

## Targets vs Actual

| Target | Expected | Actual | Met? |
|--------|----------|--------|------|
| Pure ASCII improvement | 5x+ | ?x | ✓/✗ |
| Mixed content improvement | 2x+ | ?x | ✓/✗ |
| Memory reduction | Yes | ?% | ✓/✗ |

## Detailed Results

### Baseline (Original)

(Copy from utf8-converter-baseline-2025-11-27.md)

### Final (SIMD-optimized)

(Copy from utf8-converter-final-2025-11-27.md)
```

### Step 7.5: Commit

```bash
git add tests/Umbraco.Tests.Benchmarks/Utf8ToAsciiConverterBenchmarks.cs \
        docs/benchmarks/utf8-converter-final-2025-11-27.md \
        docs/benchmarks/utf8-converter-comparison-2025-11-27.md
git commit -m "perf(strings): add final benchmarks and performance comparison"
```

---

## Task 8: Prune JSON Mappings and Document

**Files:**
- Modify: `src/Umbraco.Core/Strings/CharacterMappings/*.json`
- Create: `docs/plans/utf8-converter-normalization-coverage.md`

### Step 8.1: Identify normalization-covered mappings

Create a test or script to check each golden mapping:

```csharp
// For each mapping in golden-mappings.json:
// 1. Try normalization only (no dictionary)
// 2. If result matches expected, mark as "normalization-covered"
// 3. Output list of mappings that require dictionary
```

### Step 8.2: Document normalization coverage

**File:** `docs/plans/utf8-converter-normalization-coverage.md`

```markdown
# Utf8ToAsciiConverter Normalization Coverage

## Summary

- **Total original mappings:** 1,317
- **Covered by normalization:** ~1,200
- **Require dictionary:** ~100

## Dictionary-Required Characters

### Ligatures (cannot decompose)
- Æ → AE
- Œ → OE
- ß → ss
- ﬁ → fi
- ...

### Special Latin (no combining marks)
- Ð → D
- Ł → L
- Ø → O
- Þ → TH
- ...

### Cyrillic (different script)
- А → A
- Ж → Zh
- Щ → Shch
- ...

## Normalization-Covered Characters (examples)

These characters decompose correctly via Normalize(FormD):
- À → A (À = A + combining grave)
- é → e (é = e + combining acute)
- ñ → n (ñ = n + combining tilde)
- ...
```

### Step 8.3: Verify pruned JSON still passes golden tests

```bash
cd /home/yv01p/Umbraco-CMS
dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~Utf8ToAsciiConverterGoldenTests"
```

Expected: All tests PASS

### Step 8.4: Commit

```bash
git add src/Umbraco.Core/Strings/CharacterMappings/*.json \
        docs/plans/utf8-converter-normalization-coverage.md
git commit -m "docs(strings): document normalization coverage and prune JSON mappings"
```

---

## Task 9: Final Cleanup and Documentation

### Step 9.1: Update design doc with completion status

Update `docs/plans/2025-11-27-utf8-to-ascii-converter-refactor-design.md`:
- Change status to "Implemented"
- Add link to benchmark comparison

### Step 9.2: Run full test suite

```bash
cd /home/yv01p/Umbraco-CMS
dotnet test
```

Expected: All tests PASS

### Step 9.3: Final commit

```bash
git add -A
git commit -m "docs: mark Utf8ToAsciiConverter refactor as complete"
```

---

## Summary

| Task | Description | Key Deliverables |
|------|-------------|------------------|
| 0 | Baseline benchmarks | Performance baseline before changes |
| 1 | Create interfaces | IUtf8ToAsciiConverter, ICharacterMappingLoader |
| 2 | Extract mappings + JSON | golden-mappings.json, 3 JSON files |
| 3 | Implement loader | CharacterMappingLoader |
| 4 | Implement converter | Utf8ToAsciiConverterNew + golden tests |
| 5 | Replace original | Rename files, static wrapper |
| 6 | Update consumers | DefaultShortStringHelper, DI registration |
| 7 | Final benchmarks | Comparison document |
| 8 | Prune mappings | Normalization coverage docs |
| 9 | Cleanup | Final documentation |
