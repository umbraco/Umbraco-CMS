# StringExtensions Refactor Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Split the 1,600-line `StringExtensions.cs` into 5 logical partial class files and apply performance optimizations.

**Architecture:** Partial class approach preserves the public API while organizing methods by category. Performance fixes target regex caching, char case checks, and string allocations.

**Tech Stack:** C# 12, .NET 10.0, NUnit, BenchmarkDotNet

---

## Phase Completion Summaries

At the end of each phase, create a completion summary document:

1. Copy this plan to `docs/plans/phase-N-summary.md`
2. Update the copy with actual results:
   - Test pass/fail counts
   - Benchmark numbers
   - Any issues encountered and how they were resolved
   - Commits created
3. Mark the phase as complete in the design document

**Summary Documents:**
- `docs/plans/phase-1-baseline-testing-summary.md`
- `docs/plans/phase-2-file-split-summary.md`
- `docs/plans/phase-3-performance-fixes-summary.md`
- `docs/plans/phase-4-verification-summary.md`

---

## Phase 1: Baseline Testing

### Task 1: Run Existing Unit Tests

**Files:**
- Read: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/ShortStringHelper/StringExtensionsTests.cs`

**Step 1: Build only the unit tests project**

Run: `dotnet build tests/Umbraco.Tests.UnitTests`

**Step 2: Run existing StringExtensions tests**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~StringExtensionsTests" --no-build`

Record pass/fail count.

**Step 3: Record baseline results**

No commit needed - just record results in phase summary.

---

### Task 2: Create Performance Baseline Tests

**Files:**
- Create: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Extensions/StringExtensionsPerformanceTests.cs`

**Step 1: Write tests for methods being optimized**

```csharp
// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class StringExtensionsPerformanceTests
{
    [TestCase("hello world", "helloworld")]
    [TestCase("  spaces  everywhere  ", "spaceseverywhere")]
    [TestCase("tabs\there", "tabshere")]
    [TestCase("new\nlines", "newlines")]
    public void StripWhitespace_RemovesAllWhitespace(string input, string expected)
        => Assert.AreEqual(expected, input.StripWhitespace());

    [TestCase("file.txt", ".txt")]
    [TestCase("path/to/file.png", ".png")]
    [TestCase("file.tar.gz", ".gz")]
    [TestCase("noextension", "")]
    public void GetFileExtension_ReturnsCorrectExtension(string input, string expected)
        => Assert.AreEqual(expected, input.GetFileExtension());

    [TestCase("<p>Hello</p>", "Hello")]
    [TestCase("<div><span>Text</span></div>", "Text")]
    [TestCase("No tags here", "No tags here")]
    [TestCase("<br/>", "")]
    public void StripHtml_RemovesAllHtmlTags(string input, string expected)
        => Assert.AreEqual(expected, input.StripHtml());

    [TestCase('a', true)]
    [TestCase('z', true)]
    [TestCase('A', false)]
    [TestCase('Z', false)]
    [TestCase('5', false)]
    public void IsLowerCase_ReturnsCorrectResult(char input, bool expected)
        => Assert.AreEqual(expected, input.IsLowerCase());

    [TestCase('A', true)]
    [TestCase('Z', true)]
    [TestCase('a', false)]
    [TestCase('z', false)]
    [TestCase('5', false)]
    public void IsUpperCase_ReturnsCorrectResult(char input, bool expected)
        => Assert.AreEqual(expected, input.IsUpperCase());

    [TestCase("hello-world", "-", "helloworld")]
    [TestCase("test_123", "_", "test123")]
    [TestCase("abc!@#def", "***", "abc******def")]
    public void ReplaceNonAlphanumericChars_String_ReplacesCorrectly(string input, string replacement, string expected)
        => Assert.AreEqual(expected, input.ReplaceNonAlphanumericChars(replacement));
}
```

**Step 2: Build and run new tests to verify they pass**

Run: `dotnet build tests/Umbraco.Tests.UnitTests && dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~StringExtensionsPerformanceTests" --no-build`

Expected: All tests PASS

**Step 3: Commit**

```bash
git add tests/Umbraco.Tests.UnitTests/Umbraco.Core/Extensions/StringExtensionsPerformanceTests.cs
git commit -m "test: add baseline tests for StringExtensions performance methods

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 3: Create Performance Benchmarks

**Files:**
- Modify: `tests/Umbraco.Tests.Benchmarks/StringExtensionsBenchmarks.cs`

**Step 1: Add benchmark methods**

Add to the existing `StringExtensionsBenchmarks` class:

```csharp
private const string HtmlTestString = "<div><p>Hello <strong>world</strong></p></div>";
private const string WhitespaceTestString = "Hello   world\t\ntest  string";
private const string FilePathTestString = "path/to/some/file.extension?query=param";
private const string NonAlphanumericTestString = "hello-world_test!@#$%^&*()123";

[Benchmark]
public string StripWhitespace_Benchmark() => WhitespaceTestString.StripWhitespace();

[Benchmark]
public string GetFileExtension_Benchmark() => FilePathTestString.GetFileExtension();

[Benchmark]
public string StripHtml_Benchmark() => HtmlTestString.StripHtml();

[Benchmark]
public bool IsLowerCase_Benchmark() => 'a'.IsLowerCase();

[Benchmark]
public bool IsUpperCase_Benchmark() => 'A'.IsUpperCase();

[Benchmark]
public string ReplaceNonAlphanumericChars_String_Benchmark()
    => NonAlphanumericTestString.ReplaceNonAlphanumericChars("-");
```

**Step 2: Build benchmarks project**

Run: `dotnet build tests/Umbraco.Tests.Benchmarks -c Release`

Expected: Build succeeds

**Step 3: Run benchmarks and record baseline**

Run: `dotnet run --project tests/Umbraco.Tests.Benchmarks -c Release -- --filter "*StringExtensions*" --job short`

Record results for comparison after optimization.

**Step 4: Commit**

```bash
git add tests/Umbraco.Tests.Benchmarks/StringExtensionsBenchmarks.cs
git commit -m "perf: add benchmarks for StringExtensions methods to optimize

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 4: Phase 1 Completion Summary

**Files:**
- Create: `docs/plans/phase-1-baseline-testing-summary.md`

**Step 1: Create summary document**

Copy the Phase 1 section of this plan and update with actual results:

```markdown
# Phase 1: Baseline Testing - Completion Summary

**Date Completed:** [DATE]
**Status:** Complete

## Results

### Existing Unit Tests
- **Tests Run:** [NUMBER]
- **Passed:** [NUMBER]
- **Failed:** [NUMBER]

### New Performance Tests
- **File Created:** `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Extensions/StringExtensionsPerformanceTests.cs`
- **Tests Added:** 6 test methods covering StripWhitespace, GetFileExtension, StripHtml, IsLowerCase, IsUpperCase, ReplaceNonAlphanumericChars

### Benchmark Baseline
| Method | Mean | Allocated |
|--------|------|-----------|
| StripWhitespace_Benchmark | [VALUE] | [VALUE] |
| GetFileExtension_Benchmark | [VALUE] | [VALUE] |
| StripHtml_Benchmark | [VALUE] | [VALUE] |
| IsLowerCase_Benchmark | [VALUE] | [VALUE] |
| IsUpperCase_Benchmark | [VALUE] | [VALUE] |
| ReplaceNonAlphanumericChars_String_Benchmark | [VALUE] | [VALUE] |

## Commits
- [COMMIT_HASH] test: add baseline tests for StringExtensions performance methods
- [COMMIT_HASH] perf: add benchmarks for StringExtensions methods to optimize

## Issues Encountered
[None / Description of issues and resolutions]
```

**Step 2: Update design document**

Mark Phase 1 as approved in `docs/plans/2025-12-07-string-extensions-refactor-design.md`.

---

## Phase 2: File Split

### Task 5: Create StringExtensions.Culture.cs

**Files:**
- Create: `src/Umbraco.Core/Extensions/StringExtensions.Culture.cs`

**Step 1: Create the file with culture-related methods**

```csharp
// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;

namespace Umbraco.Extensions;

/// <summary>
/// Culture and invariant comparison extensions.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Compares 2 strings with invariant culture and case ignored.
    /// </summary>
    public static bool InvariantEquals(this string? compare, string? compareTo) =>
        string.Equals(compare, compareTo, StringComparison.InvariantCultureIgnoreCase);

    public static bool InvariantStartsWith(this string compare, string compareTo) =>
        compare.StartsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);

    public static bool InvariantEndsWith(this string compare, string compareTo) =>
        compare.EndsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);

    public static bool InvariantContains(this string compare, string compareTo) =>
        compare.Contains(compareTo, StringComparison.OrdinalIgnoreCase);

    public static bool InvariantContains(this IEnumerable<string> compare, string compareTo) =>
        compare.Contains(compareTo, StringComparer.InvariantCultureIgnoreCase);

    public static int InvariantIndexOf(this string s, string value) =>
        s.IndexOf(value, StringComparison.OrdinalIgnoreCase);

    public static int InvariantLastIndexOf(this string s, string value) =>
        s.LastIndexOf(value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Formats the string with invariant culture.
    /// </summary>
    public static string InvariantFormat(this string? format, params object?[] args) =>
        string.Format(CultureInfo.InvariantCulture, format ?? string.Empty, args);

    /// <summary>
    /// Converts an integer to an invariant formatted string.
    /// </summary>
    public static string ToInvariantString(this int str) => str.ToString(CultureInfo.InvariantCulture);

    public static string ToInvariantString(this long str) => str.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Verifies the provided string is a valid culture code and returns it in a consistent casing.
    /// </summary>
    public static string? EnsureCultureCode(this string? culture)
    {
        if (string.IsNullOrEmpty(culture) || culture == "*")
        {
            return culture;
        }

        return new CultureInfo(culture).Name;
    }
}
```

**Step 2: Verify build succeeds**

Run: `dotnet build src/Umbraco.Core --no-restore`

Expected: Build succeeds (duplicate method errors expected until original file is deleted)

---

### Task 6: Create StringExtensions.Manipulation.cs

**Files:**
- Create: `src/Umbraco.Core/Extensions/StringExtensions.Manipulation.cs`

**Step 1: Create the file**

Copy methods from original file: `Trim`, `TrimStart`, `TrimEnd`, `EnsureStartsWith`, `EnsureEndsWith`, `ToFirstUpper`, `ToFirstLower`, `ToFirstUpperInvariant`, `ToFirstLowerInvariant`, `ReplaceMany`, `ReplaceFirst`, `Replace` (StringComparison overload), `ReplaceNonAlphanumericChars`, `ExceptChars`, `Truncate`, `StripWhitespace`, `StripNewLines`, `ToSingleLine`, `MakePluralName`, `IsVowel`, `IsLowerCase`, `IsUpperCase`, and all `IShortStringHelper` wrapper methods.

Include static field: `Whitespace`

**Step 2: Verify build**

Run: `dotnet build src/Umbraco.Core --no-restore`

---

### Task 7: Create StringExtensions.Encoding.cs

**Files:**
- Create: `src/Umbraco.Core/Extensions/StringExtensions.Encoding.cs`

**Step 1: Create the file**

Copy methods: `GenerateHash`, `ToSHA1`, `ToUrlBase64`, `FromUrlBase64`, `UrlTokenEncode`, `UrlTokenDecode`, `ConvertToHex`, `DecodeFromHex`, `EncodeAsGuid`, `ToGuid`, `CreateGuidFromHash`, `SwapByteOrder`, `ToCSharpString`, `EncodeJsString`.

Include static fields: `ToCSharpHexDigitLower`, `ToCSharpEscapeChars`, `UrlNamespace`

Include static constructor.

---

### Task 8: Create StringExtensions.Parsing.cs

**Files:**
- Create: `src/Umbraco.Core/Extensions/StringExtensions.Parsing.cs`

**Step 1: Create the file**

Copy methods: `IsNullOrWhiteSpace`, `IfNullOrWhiteSpace`, `OrIfNullOrWhiteSpace`, `NullOrWhiteSpaceAsNull`, `DetectIsJson`, `DetectIsEmptyJson`, `ParseInto`, `EnumTryParse`, `EnumParse`, `ToDelimitedList`, `EscapedSplit`, `ContainsAny`, `CsvContains`, `CountOccurrences`, `GetIdsFromPathReversed`.

Include static fields: `JsonEmpties`, `DefaultEscapedStringEscapeChar`

---

### Task 9: Create StringExtensions.Sanitization.cs

**Files:**
- Create: `src/Umbraco.Core/Extensions/StringExtensions.Sanitization.cs`

**Step 1: Create the file**

Copy methods: `CleanForXss`, `StripHtml`, `ToValidXmlString`, `EscapeRegexSpecialCharacters`, `StripFileExtension`, `GetFileExtension`, `NormaliseDirectoryPath`, `IsFullPath`, `AppendQueryStringToUrl`, `ToFriendlyName`, `IsEmail`, `GenerateStreamFromString`.

Include static fields: `CleanForXssChars`, `InvalidXmlChars`

---

### Task 10: Delete Original and Verify

**Files:**
- Delete: `src/Umbraco.Core/Extensions/StringExtensions.cs`

**Step 1: Delete the original file**

Run: `rm src/Umbraco.Core/Extensions/StringExtensions.cs`

**Step 2: Build to verify no duplicate definitions**

Run: `dotnet build src/Umbraco.Core --no-restore`

Expected: Build succeeds with no errors

**Step 3: Build and run all tests**

Run: `dotnet build tests/Umbraco.Tests.UnitTests && dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~StringExtensions" --no-build`

Expected: All tests pass

**Step 4: Commit atomically**

```bash
git add src/Umbraco.Core/Extensions/StringExtensions*.cs
git add -u src/Umbraco.Core/Extensions/StringExtensions.cs
git commit -m "refactor: split StringExtensions into 5 partial class files

Split the 1,600-line StringExtensions.cs into logical categories:
- StringExtensions.Culture.cs - invariant comparison methods
- StringExtensions.Manipulation.cs - string modification methods
- StringExtensions.Encoding.cs - hashing, base64, guid encoding
- StringExtensions.Parsing.cs - parsing and detection methods
- StringExtensions.Sanitization.cs - XSS, HTML, file path methods

No functional changes. All existing tests pass.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 11: Phase 2 Completion Summary

**Files:**
- Create: `docs/plans/phase-2-file-split-summary.md`

**Step 1: Create summary document**

```markdown
# Phase 2: File Split - Completion Summary

**Date Completed:** [DATE]
**Status:** Complete

## Results

### Files Created
- `src/Umbraco.Core/Extensions/StringExtensions.Culture.cs` - [X] methods
- `src/Umbraco.Core/Extensions/StringExtensions.Manipulation.cs` - [X] methods
- `src/Umbraco.Core/Extensions/StringExtensions.Encoding.cs` - [X] methods
- `src/Umbraco.Core/Extensions/StringExtensions.Parsing.cs` - [X] methods
- `src/Umbraco.Core/Extensions/StringExtensions.Sanitization.cs` - [X] methods

### Files Deleted
- `src/Umbraco.Core/Extensions/StringExtensions.cs` - [X] lines removed

### Test Results
- **Tests Run:** [NUMBER]
- **Passed:** [NUMBER]
- **Failed:** [NUMBER]
- **Comparison to Phase 1:** [SAME/DIFFERENT]

## Commits
- [COMMIT_HASH] refactor: split StringExtensions into 5 partial class files

## Issues Encountered
[None / Description of issues and resolutions]
```

**Step 2: Update design document**

Mark Phase 2 as approved in `docs/plans/2025-12-07-string-extensions-refactor-design.md`.

---

## Phase 3: Performance Fixes

### Task 12: Cache Regex in StripWhitespace

**Files:**
- Modify: `src/Umbraco.Core/Extensions/StringExtensions.Manipulation.cs`

**Step 1: Update StripWhitespace to use cached regex**

Replace:
```csharp
public static string StripWhitespace(this string txt) => Regex.Replace(txt, @"\s", string.Empty);
```

With:
```csharp
public static string StripWhitespace(this string txt) => Whitespace.Value.Replace(txt, string.Empty);
```

**Step 2: Build and run tests**

Run: `dotnet build tests/Umbraco.Tests.UnitTests && dotnet test tests/Umbraco.Tests.UnitTests --filter "StripWhitespace" --no-build`

Expected: PASS

**Step 3: Commit**

```bash
git add src/Umbraco.Core/Extensions/StringExtensions.Manipulation.cs
git commit -m "perf: cache regex in StripWhitespace

Use existing Whitespace Lazy<Regex> instead of creating new Regex each call.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 13: Cache Regex in GetFileExtension

**Files:**
- Modify: `src/Umbraco.Core/Extensions/StringExtensions.Sanitization.cs`

**Step 1: Add cached regex field**

```csharp
private static readonly Lazy<Regex> FileExtensionRegex = new(() =>
    new Regex(@"(?<extension>\.[^\.\?]+)(\?.*|$)", RegexOptions.Compiled));
```

**Step 2: Update method**

Replace:
```csharp
public static string GetFileExtension(this string file)
{
    const string pattern = @"(?<extension>\.[^\.\?]+)(\?.*|$)";
    Match match = Regex.Match(file, pattern);
    return match.Success ? match.Groups["extension"].Value : string.Empty;
}
```

With:
```csharp
public static string GetFileExtension(this string file)
{
    Match match = FileExtensionRegex.Value.Match(file);
    return match.Success ? match.Groups["extension"].Value : string.Empty;
}
```

**Step 3: Build and run tests**

Run: `dotnet build tests/Umbraco.Tests.UnitTests && dotnet test tests/Umbraco.Tests.UnitTests --filter "GetFileExtension" --no-build`

Expected: PASS

**Step 4: Commit**

```bash
git add src/Umbraco.Core/Extensions/StringExtensions.Sanitization.cs
git commit -m "perf: cache regex in GetFileExtension

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 14: Cache Regex in StripHtml

**Files:**
- Modify: `src/Umbraco.Core/Extensions/StringExtensions.Sanitization.cs`

**Step 1: Add cached regex field**

```csharp
private static readonly Lazy<Regex> HtmlTagRegex = new(() =>
    new Regex(@"<(.|\n)*?>", RegexOptions.Compiled));
```

**Step 2: Update method**

Replace:
```csharp
public static string StripHtml(this string text)
{
    const string pattern = @"<(.|\n)*?>";
    return Regex.Replace(text, pattern, string.Empty, RegexOptions.Compiled);
}
```

With:
```csharp
public static string StripHtml(this string text) => HtmlTagRegex.Value.Replace(text, string.Empty);
```

**Step 3: Build and run tests**

Run: `dotnet build tests/Umbraco.Tests.UnitTests && dotnet test tests/Umbraco.Tests.UnitTests --filter "StripHtml" --no-build`

Expected: PASS

**Step 4: Commit**

```bash
git add src/Umbraco.Core/Extensions/StringExtensions.Sanitization.cs
git commit -m "perf: cache regex in StripHtml

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 15: Optimize IsLowerCase and IsUpperCase

**Files:**
- Modify: `src/Umbraco.Core/Extensions/StringExtensions.Manipulation.cs`

**Step 1: Update methods**

Replace:
```csharp
public static bool IsLowerCase(this char ch) => ch.ToString(CultureInfo.InvariantCulture) ==
                                                ch.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();

public static bool IsUpperCase(this char ch) => ch.ToString(CultureInfo.InvariantCulture) ==
                                                ch.ToString(CultureInfo.InvariantCulture).ToUpperInvariant();
```

With:
```csharp
public static bool IsLowerCase(this char ch) => char.IsLower(ch);

public static bool IsUpperCase(this char ch) => char.IsUpper(ch);
```

**Step 2: Build and run tests**

Run: `dotnet build tests/Umbraco.Tests.UnitTests && dotnet test tests/Umbraco.Tests.UnitTests --filter "IsLowerCase or IsUpperCase" --no-build`

Expected: PASS

**Step 3: Commit**

```bash
git add src/Umbraco.Core/Extensions/StringExtensions.Manipulation.cs
git commit -m "perf: use char.IsLower/IsUpper instead of string allocation

Eliminates string allocations for simple char case checks.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 16: Optimize ReplaceNonAlphanumericChars(string)

**Files:**
- Modify: `src/Umbraco.Core/Extensions/StringExtensions.Manipulation.cs`

**Step 1: Update method**

Replace:
```csharp
public static string ReplaceNonAlphanumericChars(this string input, string replacement)
{
    var mName = input;
    foreach (var c in mName.ToCharArray().Where(c => !char.IsLetterOrDigit(c)))
    {
        mName = mName.Replace(c.ToString(CultureInfo.InvariantCulture), replacement);
    }
    return mName;
}
```

With:
```csharp
public static string ReplaceNonAlphanumericChars(this string input, string replacement)
{
    if (string.IsNullOrEmpty(input))
    {
        return input;
    }

    // Single-char replacement can use the optimized char overload
    if (replacement.Length == 1)
    {
        return input.ReplaceNonAlphanumericChars(replacement[0]);
    }

    // Multi-char replacement: single pass with StringBuilder
    var sb = new StringBuilder(input.Length);
    foreach (var c in input)
    {
        if (char.IsLetterOrDigit(c))
        {
            sb.Append(c);
        }
        else
        {
            sb.Append(replacement);
        }
    }
    return sb.ToString();
}
```

**Step 2: Build and run tests**

Run: `dotnet build tests/Umbraco.Tests.UnitTests && dotnet test tests/Umbraco.Tests.UnitTests --filter "ReplaceNonAlphanumericChars" --no-build`

Expected: PASS

**Step 3: Commit**

```bash
git add src/Umbraco.Core/Extensions/StringExtensions.Manipulation.cs
git commit -m "perf: optimize ReplaceNonAlphanumericChars string overload

Single-pass StringBuilder instead of multiple string.Replace calls.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 17: Phase 3 Completion Summary

**Files:**
- Create: `docs/plans/phase-3-performance-fixes-summary.md`

**Step 1: Create summary document**

```markdown
# Phase 3: Performance Fixes - Completion Summary

**Date Completed:** [DATE]
**Status:** Complete

## Results

### Optimizations Applied

| Method | Change | File |
|--------|--------|------|
| StripWhitespace | Cached regex | StringExtensions.Manipulation.cs |
| GetFileExtension | Cached regex | StringExtensions.Sanitization.cs |
| StripHtml | Cached regex | StringExtensions.Sanitization.cs |
| IsLowerCase | char.IsLower() | StringExtensions.Manipulation.cs |
| IsUpperCase | char.IsUpper() | StringExtensions.Manipulation.cs |
| ReplaceNonAlphanumericChars | StringBuilder single-pass | StringExtensions.Manipulation.cs |

### Test Results
- **Tests Run:** [NUMBER]
- **Passed:** [NUMBER]
- **Failed:** [NUMBER]

## Commits
- [COMMIT_HASH] perf: cache regex in StripWhitespace
- [COMMIT_HASH] perf: cache regex in GetFileExtension
- [COMMIT_HASH] perf: cache regex in StripHtml
- [COMMIT_HASH] perf: use char.IsLower/IsUpper instead of string allocation
- [COMMIT_HASH] perf: optimize ReplaceNonAlphanumericChars string overload

## Issues Encountered
[None / Description of issues and resolutions]
```

**Step 2: Update design document**

Mark Phase 3 as approved in `docs/plans/2025-12-07-string-extensions-refactor-design.md`.

---

## Phase 4: Verification

### Task 18: Run All Tests

**Step 1: Build and run all StringExtensions tests**

Run: `dotnet build tests/Umbraco.Tests.UnitTests && dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~StringExtensions" --no-build`

Expected: Build succeeds, all tests pass, same count as Phase 1 baseline

**Step 2: Document results**

Compare pass count to Phase 1 baseline.

---

### Task 19: Run Benchmarks and Compare

**Step 1: Run benchmarks**

Run: `dotnet run --project tests/Umbraco.Tests.Benchmarks -c Release -- --filter "*StringExtensions*" --job short`

**Step 2: Document improvements**

Compare to Phase 1 baseline. Expected improvements:
- `StripWhitespace`: Significant (cached regex)
- `GetFileExtension`: Significant (cached regex)
- `StripHtml`: Significant (cached regex)
- `IsLowerCase`/`IsUpperCase`: ~10-100x faster (no allocation)
- `ReplaceNonAlphanumericChars`: Moderate (single pass)

---

### Task 20: Final Review

**Step 1: Verify file structure**

Run: `ls -la src/Umbraco.Core/Extensions/StringExtensions*.cs`

Expected:
```
StringExtensions.Culture.cs
StringExtensions.Encoding.cs
StringExtensions.Manipulation.cs
StringExtensions.Parsing.cs
StringExtensions.Sanitization.cs
```

**Step 2: Verify no StringExtensions.cs remains**

Run: `test ! -f src/Umbraco.Core/Extensions/StringExtensions.cs && echo "OK: Original file deleted"`

Expected: "OK: Original file deleted"

---

### Task 21: Phase 4 Completion Summary

**Files:**
- Create: `docs/plans/phase-4-verification-summary.md`

**Step 1: Create summary document**

```markdown
# Phase 4: Verification - Completion Summary

**Date Completed:** [DATE]
**Status:** Complete

## Results

### Final Test Results
- **Tests Run:** [NUMBER]
- **Passed:** [NUMBER]
- **Failed:** [NUMBER]
- **Comparison to Phase 1 Baseline:** [SAME/DIFFERENT]

### Benchmark Comparison

| Method | Before | After | Improvement |
|--------|--------|-------|-------------|
| StripWhitespace_Benchmark | [VALUE] | [VALUE] | [X%] |
| GetFileExtension_Benchmark | [VALUE] | [VALUE] | [X%] |
| StripHtml_Benchmark | [VALUE] | [VALUE] | [X%] |
| IsLowerCase_Benchmark | [VALUE] | [VALUE] | [X%] |
| IsUpperCase_Benchmark | [VALUE] | [VALUE] | [X%] |
| ReplaceNonAlphanumericChars_String_Benchmark | [VALUE] | [VALUE] | [X%] |

### File Structure Verification
- [x] StringExtensions.Culture.cs exists
- [x] StringExtensions.Encoding.cs exists
- [x] StringExtensions.Manipulation.cs exists
- [x] StringExtensions.Parsing.cs exists
- [x] StringExtensions.Sanitization.cs exists
- [x] Original StringExtensions.cs deleted

## Overall Summary

**Refactoring Goals Achieved:**
- [x] Split into 5 logical partial class files
- [x] No breaking changes (all tests pass)
- [x] Performance improvements applied
- [x] Measurable benchmark improvements

## Issues Encountered
[None / Description of issues and resolutions]
```

**Step 2: Update design document**

Mark Phase 4 as approved and all checkboxes complete in `docs/plans/2025-12-07-string-extensions-refactor-design.md`.

---

## Summary

**Total Tasks:** 21
**Phases:** 4

| Phase | Tasks | Description |
|-------|-------|-------------|
| 1 | 1-4 | Baseline testing, benchmarks, and summary |
| 2 | 5-11 | File split into 5 partial classes and summary |
| 3 | 12-17 | Performance optimizations and summary |
| 4 | 18-21 | Verification, documentation, and summary |

**Expected Outcomes:**
- 5 well-organized partial class files
- Cached regex patterns (3 methods)
- Zero-allocation char case checks
- Optimized string replacement
- All existing tests passing
- Measurable performance improvements
- Completion summary for each phase
