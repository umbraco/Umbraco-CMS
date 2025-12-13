# Utf8ToAsciiConverter Refactor Design

**Date**: 2025-11-27
**Status**: Implemented
**Author**: Claude Code + Human Partner
**Benchmark Results**: [Performance Comparison](/docs/benchmarks/utf8-converter-comparison-2025-11-27.md)

## Overview

Refactor `Utf8ToAsciiConverter.cs` from a 3,631-line switch statement to a SIMD-optimized, extensible implementation with JSON-based character mappings.

### Goals

1. **Performance**: 10-25x faster for ASCII text via SIMD (AVX-512)
2. **Memory**: Reduce footprint from ~15KB to ~2KB
3. **Maintainability**: Replace 1,317 hardcoded cases with ~102 JSON entries
4. **Extensibility**: Allow custom character mappings via JSON files
5. **Backward Compatibility**: Maintain static API with `[Obsolete]` warnings

## Architecture

### Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    IUtf8ToAsciiConverter                    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Utf8ToAsciiConverter                      │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ 1. ASCII Fast Path (SIMD via SearchValues)             │ │
│  ├────────────────────────────────────────────────────────┤ │
│  │ 2. Normalize(FormD) + Strip Combining Marks            │ │
│  ├────────────────────────────────────────────────────────┤ │
│  │ 3. Special Cases (FrozenDictionary ~102 entries)       │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  ICharacterMappingLoader                    │
│  ┌─────────────────┐  ┌─────────────────┐                   │
│  │ Built-in JSON   │  │ User JSON files │                   │
│  │ (embedded)      │  │ (config/)       │                   │
│  └─────────────────┘  └─────────────────┘                   │
└─────────────────────────────────────────────────────────────┘
```

### Processing Pipeline

```
Input: "Café naïve Œuvre Москва"
        │
        ▼
┌─────────────────────────────────────────────────────────────┐
│ STAGE 1: SIMD ASCII Scan                                    │
│ SearchValues.IndexOfAnyExcept(asciiPrintable)               │
│ → Find first non-ASCII, copy ASCII prefix via SIMD          │
└─────────────────────────────────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────────────────────────────────┐
│ STAGE 2: Normalize + Strip                                  │
│ "é" → Normalize(FormD) → "e\u0301" → strip Mn → "e"         │
└─────────────────────────────────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────────────────────────────────┐
│ STAGE 3: Special Cases Lookup                               │
│ FrozenDictionary: Œ→OE, ß→ss, Д→D, Ж→Zh, etc.               │
└─────────────────────────────────────────────────────────────┘
        │
        ▼
Output: "Cafe naive OEuvre Moskva"
```

## File Structure

```
src/Umbraco.Core/Strings/
├── IUtf8ToAsciiConverter.cs           # Public interface
├── ICharacterMappingLoader.cs         # Mapping loader interface
├── Utf8ToAsciiConverter.cs            # SIMD-optimized implementation
├── CharacterMappingLoader.cs          # JSON loader
├── CharacterMappings/                 # Embedded resources
│   ├── ligatures.json
│   ├── cyrillic.json
│   └── special-latin.json
└── Utf8ToAsciiConverterStatic.cs      # Backward compat (obsolete)

config/character-mappings/             # User extensions (optional)
└── *.json

tests/Umbraco.Tests.UnitTests/Umbraco.Core/Strings/
├── Utf8ToAsciiConverterTests.cs
├── CharacterMappingLoaderTests.cs
└── Utf8ToAsciiConverterBenchmarks.cs
```

## Interfaces

### IUtf8ToAsciiConverter

```csharp
namespace Umbraco.Cms.Core.Strings;

public interface IUtf8ToAsciiConverter
{
    /// <summary>
    /// Converts text to ASCII, returning a new string.
    /// </summary>
    string Convert(string text, char fallback = '?');

    /// <summary>
    /// Converts text to ASCII, writing to output span. Returns chars written.
    /// Zero-allocation for callers who provide buffer.
    /// </summary>
    int Convert(ReadOnlySpan<char> input, Span<char> output, char fallback = '?');
}
```

### ICharacterMappingLoader

```csharp
namespace Umbraco.Cms.Core.Strings;

public interface ICharacterMappingLoader
{
    /// <summary>
    /// Loads all mapping files and returns combined FrozenDictionary.
    /// Higher priority mappings override lower priority.
    /// </summary>
    FrozenDictionary<char, string> LoadMappings();
}
```

## JSON Mapping Format

```json
{
  "name": "Cyrillic",
  "description": "Russian Cyrillic to Latin transliteration",
  "priority": 0,
  "mappings": {
    "А": "A",  "а": "a",
    "Б": "B",  "б": "b",
    "Ж": "Zh", "ж": "zh",
    "Щ": "Sh", "щ": "sh"
  }
}
```

### Priority System

- Built-in mappings: priority 0
- User mappings: priority > 0 (higher overrides lower)
- User config path: `config/character-mappings/*.json`

## Special Cases Dictionary

Characters that don't decompose via `Normalize(FormD)`:

| Category | Examples | Count |
|----------|----------|-------|
| Ligatures | Œ→OE, Æ→AE, ß→ss, ﬁ→fi | ~20 |
| Special Latin | Ð→D, Ł→L, Ø→O, Þ→TH | ~16 |
| Cyrillic | А→A, Ж→Zh, Щ→Sh | ~66 |
| **Total** | | **~102** |

## Performance Targets

| Scenario | Current | Target | Improvement |
|----------|---------|--------|-------------|
| ASCII (100 chars) | 312 ns | 29 ns | 10x |
| ASCII (100 KB) | 285 µs | 12 µs | 24x |
| Mixed (100 chars) | 456 ns | 112 ns | 4x |
| Mixed (100 KB) | 412 µs | 89 µs | 5x |
| Parallel (1 MB) | 8.5 ms | 890 µs | 10x |
| Memory footprint | 15 KB | 2 KB | 87% reduction |

## Benchmark Scenarios

1. **Tiny** (10 chars): Method call overhead
2. **Small** (100 chars): Typical URL slug
3. **Medium** (1 KB): Typical content field
4. **Large** (100 KB): Large document
5. **Real-world URLs**: Actual Umbraco URL slugs
6. **Streaming**: Chunked processing (1 MB)
7. **Mixed lengths**: Random 1-10 KB distribution
8. **Cached**: Repeated same input
9. **Parallel**: Multi-threaded (1 MB across 16 threads)
10. **Cold start**: First call after JIT
11. **Memory pressure**: Under GC stress
12. **Span API**: Zero-allocation path

## Backward Compatibility

```csharp
public static class Utf8ToAsciiConverterStatic
{
    private static readonly IUtf8ToAsciiConverter s_default =
        new Utf8ToAsciiConverter(new CharacterMappingLoader(...));

    [Obsolete("Use IUtf8ToAsciiConverter via DI. Will be removed in v15.")]
    public static string ToAsciiString(string text, char fail = '?')
        => s_default.Convert(text, fail);

    [Obsolete("Use IUtf8ToAsciiConverter via DI. Will be removed in v15.")]
    public static char[] ToAsciiCharArray(string text, char fail = '?')
        => s_default.Convert(text, fail).ToCharArray();
}
```

## DI Registration

```csharp
// In UmbracoBuilderExtensions.cs
builder.Services.AddSingleton<ICharacterMappingLoader, CharacterMappingLoader>();
builder.Services.AddSingleton<IUtf8ToAsciiConverter, Utf8ToAsciiConverter>();
```

## Test Coverage

### Unit Tests
- ASCII fast path (pure ASCII input)
- Normalization (accented characters)
- Ligature expansion
- Cyrillic transliteration
- Whitespace/control character handling
- Fallback character behavior
- Span API (zero allocation)
- Edge cases (null, empty, surrogates)
- Backward compatibility with original behavior

### Integration Tests
- JSON mapping file loading
- User mapping override behavior
- DI registration and injection

### Benchmark Tests
- All 12 scenarios with Original vs New comparison
- Memory allocation tracking
- Parallel throughput

## Implementation Tasks

1. Create interfaces (`IUtf8ToAsciiConverter`, `ICharacterMappingLoader`)
2. Create JSON mapping files (ligatures, cyrillic, special-latin)
3. Implement `CharacterMappingLoader`
4. Implement `Utf8ToAsciiConverter` with SIMD optimization
5. Create backward-compat static wrapper
6. Update `DefaultShortStringHelper` to use DI
7. Register services in DI container
8. Write unit tests
9. Write benchmark tests
10. Run benchmarks and validate performance targets
11. Update documentation

## Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| Behavior differences | Comprehensive backward-compat tests |
| Performance regression in edge cases | Benchmark all scenarios, including worst-case |
| JSON loading failures | Graceful degradation with logging |
| SIMD not available | Automatic fallback (handled by .NET runtime) |
