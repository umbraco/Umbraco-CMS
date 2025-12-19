# StringExtensions Refactor Design

**Date**: 2025-12-07
**Branch**: `refactor/StringExtensions`
**Status**: Approved

---

## 1. Overview

### Problem Statement

The `StringExtensions.cs` file in `Umbraco.Core` is a 1,600-line "utility dumping ground" with:
- Many unrelated string operations mixed together
- Performance issues in several methods
- Difficult to navigate and maintain

### Goals

1. **Consolidate & Organize**: Split into logical partial class files
2. **Reduce Complexity**: Each file focuses on one category of operations
3. **Fix Performance**: Optimize methods with known inefficiencies
4. **Maintain Compatibility**: Zero breaking changes

### Scope

**In Scope:**
- `src/Umbraco.Core/Extensions/StringExtensions.cs` (1,600 lines) - split and optimize

**Out of Scope (no changes):**
- `src/Umbraco.Web.Common/Extensions/StringExtensions.cs` - internal, project-specific
- `src/Umbraco.Cms.Persistence.EFCore/StringExtensions.cs` - internal, project-specific
- Test project StringExtensions files - separate concern

---

## 2. File Structure

Using **partial class** approach to prevent breaking changes:

```
src/Umbraco.Core/Extensions/
├── StringExtensions.cs                    → DELETE
├── StringExtensions.Culture.cs            → NEW
├── StringExtensions.Manipulation.cs       → NEW
├── StringExtensions.Encoding.cs           → NEW
├── StringExtensions.Parsing.cs            → NEW
└── StringExtensions.Sanitization.cs       → NEW
```

Each file follows this pattern:
```csharp
// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Extensions;

public static partial class StringExtensions
{
    // Methods for this category
}
```

---

## 3. Method Assignments

### StringExtensions.Culture.cs (~80 lines)

**Methods:**
- `InvariantEquals`
- `InvariantStartsWith`
- `InvariantEndsWith`
- `InvariantContains` (both overloads)
- `InvariantIndexOf`
- `InvariantLastIndexOf`
- `InvariantFormat`
- `ToInvariantString` (int and long overloads)
- `EnsureCultureCode`

**Static Fields:** None

---

### StringExtensions.Manipulation.cs (~300 lines)

**Methods:**
- `Trim`, `TrimStart`, `TrimEnd` (string overloads)
- `EnsureStartsWith`, `EnsureEndsWith` (all overloads)
- `ToFirstUpper`, `ToFirstLower` (all overloads including culture/invariant)
- `ReplaceMany`, `ReplaceFirst`, `Replace` (StringComparison overload)
- `ReplaceNonAlphanumericChars` (both overloads)
- `ExceptChars`
- `Truncate`, `StripWhitespace`, `StripNewLines`, `ToSingleLine`
- `MakePluralName`, `IsVowel`
- `IsLowerCase`, `IsUpperCase`
- All `IShortStringHelper` wrappers:
  - `ToSafeAlias` (3 overloads)
  - `ToUrlSegment` (2 overloads)
  - `ToCleanString` (4 overloads)
  - `SplitPascalCasing`
  - `ToSafeFileName` (2 overloads)
  - `SpaceCamelCasing` (internal)

**Static Fields:**
- `Whitespace` (Lazy<Regex>) - shared, used by `StripWhitespace`

---

### StringExtensions.Encoding.cs (~200 lines)

**Methods:**
- `GenerateHash` (both overloads)
- `ToSHA1`
- `ToUrlBase64`, `FromUrlBase64`
- `UrlTokenEncode` (note: extends `byte[]`, not `string`)
- `UrlTokenDecode`
- `ConvertToHex`, `DecodeFromHex`
- `EncodeAsGuid`, `ToGuid`
- `CreateGuidFromHash` (internal)
- `SwapByteOrder` (internal)
- `ToCSharpString`
- `EncodeJsString`
- Private `GenerateHash(string, string?)` helper

**Static Fields:**
- `ToCSharpHexDigitLower`
- `ToCSharpEscapeChars`
- `UrlNamespace`

**Static Constructor:** Yes (initializes `ToCSharpEscapeChars`)

---

### StringExtensions.Parsing.cs (~150 lines)

**Methods:**
- `IsNullOrWhiteSpace`, `IfNullOrWhiteSpace`, `OrIfNullOrWhiteSpace`, `NullOrWhiteSpaceAsNull`
- `DetectIsJson`, `DetectIsEmptyJson`
- `ParseInto` (both overloads)
- `EnumTryParse`, `EnumParse`
- `ToDelimitedList`
- `EscapedSplit`
- `ContainsAny`, `CsvContains`, `CountOccurrences`
- `GetIdsFromPathReversed`

**Static Fields:**
- `JsonEmpties`
- `DefaultEscapedStringEscapeChar` (const)

---

### StringExtensions.Sanitization.cs (~150 lines)

**Methods:**
- `CleanForXss`, `StripHtml`, `ToValidXmlString`
- `EscapeRegexSpecialCharacters`
- `StripFileExtension`, `GetFileExtension`
- `NormaliseDirectoryPath`, `IsFullPath`
- `AppendQueryStringToUrl`
- `ToFriendlyName`
- `IsEmail`
- `GenerateStreamFromString` (internal)

**Static Fields:**
- `CleanForXssChars`
- `InvalidXmlChars` (Lazy<Regex>)

---

## 4. Performance Fixes

Applied during Phase 3:

### 4.1 Regex Caching

| Method | Current Issue | Fix |
|--------|---------------|-----|
| `StripWhitespace` | `Regex.Replace(txt, @"\s", ...)` each call | Create dedicated cached `Lazy<Regex>` |
| `GetFileExtension` | `new Regex(pattern)` each call | Create cached `Lazy<Regex>` |
| `StripHtml` | `Regex.Replace(..., Compiled)` each call | Create cached `Lazy<Regex>` |

### 4.2 Char Case Checks

```csharp
// Current (allocates string)
public static bool IsLowerCase(this char ch) =>
    ch.ToString(CultureInfo.InvariantCulture) ==
    ch.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();

// Fixed (no allocation)
public static bool IsLowerCase(this char ch) => char.IsLower(ch);
public static bool IsUpperCase(this char ch) => char.IsUpper(ch);
```

### 4.3 ReplaceNonAlphanumericChars(string)

Only the `string` overload needs fixing (the `char` overload is already optimized):

```csharp
// Current (LINQ + string allocations in loop)
foreach (var c in mName.ToCharArray().Where(c => !char.IsLetterOrDigit(c)))
{
    mName = mName.Replace(c.ToString(...), replacement);
}

// Fixed (single pass with StringBuilder for multi-char replacement)
```

---

## 5. Testing Strategy

### Baseline (Phase 1)
- Run existing tests, record pass/fail state
- Run existing benchmarks, record baseline numbers
- Add missing unit tests for methods being optimized

### Verification (Phase 4)
- Run all unit tests - must match Phase 1 results
- Run benchmarks - compare against baseline
- Expected improvements:
  - Regex methods: significant (no repeated compilation)
  - Char case checks: ~10-100x faster (no allocation)
  - ReplaceNonAlphanumericChars: moderate

### New Test File
`tests/Umbraco.Tests.UnitTests/Umbraco.Core/Extensions/StringExtensionsRefactorTests.cs`

### Benchmark Additions
Add to `StringExtensionsBenchmarks.cs`:
- `StripWhitespace_Benchmark`
- `GetFileExtension_Benchmark`
- `StripHtml_Benchmark`
- `IsLowerCase_Benchmark`
- `ReplaceNonAlphanumericChars_Benchmark`

---

## 6. Implementation Phases

### Workflow for Each Phase

1. **Create Implementation Plan** - Detailed step-by-step plan
2. **User Review** - User approves the plan
3. **Save Plan** - Write to `docs/plans/` folder
4. **Execute** - Use subagent-driven development to implement
5. **Completion Summary** - Copy plan, update with results, save to `docs/plans/`

---

### Phase 1: Baseline Testing

**Objective:** Establish baseline test results and performance metrics before any changes.

**Deliverables:**
- Run existing StringExtensions unit tests
- Run existing benchmarks and record results
- Create new unit tests for methods being optimized
- Create new benchmarks for methods being optimized
- Document baseline metrics

**Plan Document:** `docs/plans/phase-1-baseline-testing-plan.md`
**Summary Document:** `docs/plans/phase-1-baseline-testing-summary.md`

---

### Phase 2: File Split

**Objective:** Split `StringExtensions.cs` into 5 partial class files with no functional changes.

**Deliverables:**
- Create 5 new partial class files
- Delete original `StringExtensions.cs`
- Verify all tests still pass (no behavioral changes)
- Single atomic commit

**Plan Document:** `docs/plans/phase-2-file-split-plan.md`
**Summary Document:** `docs/plans/phase-2-file-split-summary.md`

---

### Phase 3: Performance Fixes

**Objective:** Apply performance optimizations to identified methods.

**Deliverables:**
- Cache regex patterns for `StripWhitespace`, `GetFileExtension`, `StripHtml`
- Optimize `IsLowerCase`, `IsUpperCase`
- Optimize `ReplaceNonAlphanumericChars(string)`
- Verify all tests still pass

**Plan Document:** `docs/plans/phase-3-performance-fixes-plan.md`
**Summary Document:** `docs/plans/phase-3-performance-fixes-summary.md`

---

### Phase 4: Verification

**Objective:** Confirm refactor success through testing and benchmarking.

**Deliverables:**
- Run all unit tests - compare to Phase 1 baseline
- Run all benchmarks - compare to Phase 1 baseline
- Document performance improvements
- Final review and cleanup

**Plan Document:** `docs/plans/phase-4-verification-plan.md`
**Summary Document:** `docs/plans/phase-4-verification-summary.md`

---

## 7. Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Breaking changes | Partial class approach preserves class name |
| Method duplication | Atomic commit: create new files + delete old file together |
| Behavioral changes | Comprehensive baseline tests before changes |
| Performance regression | Benchmark before/after comparison |

---

## 8. Files Unchanged

These files remain untouched:

| File | Reason |
|------|--------|
| `src/Umbraco.Web.Common/Extensions/StringExtensions.cs` | Internal, project-specific |
| `src/Umbraco.Cms.Persistence.EFCore/StringExtensions.cs` | Internal, project-specific |
| `tests/.../StringExtensions.cs` (various) | Test utilities, separate concern |

---

## Approval

- [x] Design reviewed and approved
- [ ] Phase 1 plan approved
- [ ] Phase 2 plan approved
- [ ] Phase 3 plan approved
- [ ] Phase 4 plan approved
