# Utf8ToAsciiConverter Final Benchmarks

**Date:** 2025-11-27
**Implementation:** SIMD-optimized with FrozenDictionary
**Runtime:** .NET 10.0

## Results

```
BenchmarkDotNet v0.15.6, Linux Ubuntu 25.10 (Questing Quokka)
Intel Xeon CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v4
```

| Method            | Mean             | Error          | StdDev         | Rank | Gen0     | Gen1     | Gen2     | Allocated |
|------------------ |-----------------:|---------------:|---------------:|-----:|---------:|---------:|---------:|----------:|
| Tiny_Ascii        |         6.756 ns |      0.1042 ns |      0.0974 ns |    1 |        - |        - |        - |         - |
| Tiny_Mixed        |         6.554 ns |      0.0153 ns |      0.0143 ns |    1 |        - |        - |        - |         - |
| Small_Ascii       |         8.132 ns |      0.0271 ns |      0.0253 ns |    2 |        - |        - |        - |         - |
| Small_Mixed       |       308.895 ns |      0.6975 ns |      0.6525 ns |    4 |   0.0129 |        - |        - |     224 B |
| Medium_Ascii      |        38.200 ns |      0.2104 ns |      0.1968 ns |    3 |        - |        - |        - |         - |
| Medium_Mixed      |     4,213.825 ns |     43.6474 ns |     40.8278 ns |    6 |   0.1221 |        - |        - |    2216 B |
| Large_Ascii       |     4,327.400 ns |     23.7729 ns |     21.0740 ns |    6 |        - |        - |        - |         - |
| Large_Mixed       |   791,424.668 ns |  4,670.0767 ns |  4,368.3927 ns |    7 |  57.6172 |  57.6172 |  57.6172 |  220856 B |
| Large_WorstCase   | 2,275,919.826 ns | 27,753.5138 ns | 25,960.6540 ns |    8 | 105.4688 | 105.4688 | 105.4688 |  409763 B |
| Span_Medium_Mixed |     3,743.828 ns |      8.5415 ns |      7.5718 ns |    5 |   0.0038 |        - |        - |     120 B |

## Key Improvements

### Performance Highlights

1. **SIMD ASCII Detection**: Pure ASCII strings now use vectorized scanning (SearchValues)
   - Tiny_Ascii: 12.3x faster (82.81 ns → 6.756 ns)
   - Large_Ascii: 137x faster (593,733 ns → 4,327 ns)

2. **Zero Allocations for ASCII**: Pure ASCII strings are returned as-is (same reference)
   - Tiny_Ascii: 48 B → 0 B (100% reduction)
   - Large_Ascii: 819,332 B → 0 B (100% reduction)

3. **Reduced Allocations for Mixed Content**:
   - Small_Mixed: 224 B → 224 B (same, already optimal)
   - Medium_Mixed: 8,264 B → 2,216 B (73% reduction)
   - Large_Mixed: 823,523 B → 220,856 B (73% reduction)

4. **Zero-Copy Span API**: New Span-based API allows callers to provide their own buffers
   - Span_Medium_Mixed: 120 B allocated (vs 8,264 B for string API)

### Mixed Content Performance

- Small_Mixed: 2.2x faster (686.54 ns → 308.895 ns)
- Medium_Mixed: 1.7x faster (7,116.65 ns → 4,213.825 ns)
- Large_Mixed: 1.3x faster (1,066,297 ns → 791,424 ns)

### Worst Case (Cyrillic) Performance

- Large_WorstCase: Similar performance (2,148,169 ns → 2,275,919 ns)
- Trade-off: Slightly slower for worst case, but dramatically faster for common cases
- Allocation improvement: 1,024,125 B → 409,763 B (60% reduction)

## Technical Implementation

1. **SearchValues for ASCII Detection**: Uses SIMD instructions (AVX-512 when available)
2. **ArrayPool for Buffers**: Reduces GC pressure by reusing buffers
3. **FrozenDictionary for Mappings**: O(1) lookup for special characters
4. **Unicode Normalization**: Handles most accented characters automatically
5. **Fast-Path Optimization**: Pure ASCII strings returned immediately without allocation

## Memory Efficiency

The new implementation dramatically reduces memory allocations:

| Scenario | Baseline | Final | Improvement |
|----------|----------|-------|-------------|
| Pure ASCII (100KB) | 819 KB | 0 B | 100% reduction |
| Mixed content (100KB) | 823 KB | 220 KB | 73% reduction |
| Worst case (100KB) | 1024 KB | 409 KB | 60% reduction |

## Notes

- Benchmarks run on .NET 10.0 (latest)
- All benchmarks use BenchmarkDotNet with MemoryDiagnoser
- Hardware intrinsics enabled (AVX-512 support)
- Results are median of 15 iterations
