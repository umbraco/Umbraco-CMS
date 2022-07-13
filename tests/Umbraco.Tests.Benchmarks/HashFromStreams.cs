using System.IO;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Umbraco.Tests.Benchmarks;

[MemoryDiagnoser]
public class HashFromStreams
{
    private readonly Stream _largeFile;
    private readonly SHA1Managed _managed;
    private readonly Stream _medFile;
    private readonly SHA1 _normal;

    private readonly Stream _smallFile;
    // according to this post: https://stackoverflow.com/a/1051777/694494
    // the SHA1CryptoServiceProvider is faster, but that is not reflected in these benchmarks.

    /*
    
    BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.1052 (20H2/October2020Update)
    Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
    .NET SDK=5.0.300-preview.21258.4
      [Host]     : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
      DefaultJob : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT


    ```
    |                  Method |         Mean |       Error |      StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    |------------------------ |-------------:|------------:|------------:|------:|------:|------:|----------:|
    |    NormalSmall100KBFile |     279.6 μs |     4.97 μs |     4.65 μs |     - |     - |     - |      1 KB |
    |     NormalMedium5MBFile |  15,051.0 μs |   242.68 μs |   215.13 μs |     - |     - |     - |      1 KB |
    |    NormalLarge100MBFile | 303,356.2 μs | 2,520.40 μs | 2,234.27 μs |     - |     - |     - |      1 KB |
    |   ManagedSmall100KBFile |     281.2 μs |     5.54 μs |     5.69 μs |     - |     - |     - |      1 KB |
    |    ManagedMedium5MBFile |  14,895.4 μs |    95.67 μs |    74.69 μs |     - |     - |     - |      1 KB |
    |   ManagedLarge100MBFile | 302,513.9 μs | 3,681.02 μs | 3,073.82 μs |     - |     - |     - |      2 KB |
    | UnmanagedSmall100KBFile |     279.1 μs |     2.91 μs |     2.43 μs |     - |     - |     - |      1 KB |
    |  UnmanagedMedium5MBFile |  14,969.7 μs |   169.22 μs |   150.01 μs |     - |     - |     - |      1 KB |
    | UnmanagedLarge100MBFile | 306,127.9 μs | 3,203.10 μs | 2,839.46 μs |     - |     - |     - |      2 KB |


    */

    private readonly SHA1CryptoServiceProvider _unmanaged;

    public HashFromStreams()
    {
        _unmanaged = new SHA1CryptoServiceProvider();
        _managed = new SHA1Managed();
        _normal = SHA1.Create();
        _smallFile = File.OpenRead(@"C:\YOUR_PATH_GOES_HERE\small-file.bin");
        _medFile = File.OpenRead(@"C:\YOUR_PATH_GOES_HERE\med-file.bin");
        _largeFile = File.OpenRead(@"C:\YOUR_PATH_GOES_HERE\large-file.bin");
    }

    private string DoHash(HashAlgorithm alg, Stream stream)
    {
        var stringBuilder = new StringBuilder();
        var hashedByteArray = alg.ComputeHash(stream);
        foreach (var b in hashedByteArray)
        {
            stringBuilder.Append(b.ToString("x2"));
        }

        return stringBuilder.ToString();
    }

    [Benchmark]
    public string NormalSmall100KBFile()
    {
        _smallFile.Seek(0, SeekOrigin.Begin);
        return DoHash(_normal, _smallFile);
    }

    [Benchmark]
    public string NormalMedium5MBFile()
    {
        _medFile.Seek(0, SeekOrigin.Begin);
        return DoHash(_normal, _medFile);
    }

    [Benchmark]
    public string NormalLarge100MBFile()
    {
        _largeFile.Seek(0, SeekOrigin.Begin);
        return DoHash(_normal, _largeFile);
    }

    [Benchmark]
    public string ManagedSmall100KBFile()
    {
        _smallFile.Seek(0, SeekOrigin.Begin);
        return DoHash(_managed, _smallFile);
    }

    [Benchmark]
    public string ManagedMedium5MBFile()
    {
        _medFile.Seek(0, SeekOrigin.Begin);
        return DoHash(_managed, _medFile);
    }

    [Benchmark]
    public string ManagedLarge100MBFile()
    {
        _largeFile.Seek(0, SeekOrigin.Begin);
        return DoHash(_managed, _largeFile);
    }

    [Benchmark]
    public string UnmanagedSmall100KBFile()
    {
        _smallFile.Seek(0, SeekOrigin.Begin);
        return DoHash(_unmanaged, _smallFile);
    }

    [Benchmark]
    public string UnmanagedMedium5MBFile()
    {
        _medFile.Seek(0, SeekOrigin.Begin);
        return DoHash(_unmanaged, _medFile);
    }

    [Benchmark]
    public string UnmanagedLarge100MBFile()
    {
        _largeFile.Seek(0, SeekOrigin.Begin);
        return DoHash(_unmanaged, _largeFile);
    }
}
