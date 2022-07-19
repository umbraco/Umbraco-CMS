using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Tests.Benchmarks;

[MediumRunJob]
[MemoryDiagnoser]
public class TypeLoaderBenchmarks
{
    private readonly TypeLoader _typeLoader1;
    private readonly TypeLoader _typeLoader2;

    public TypeLoaderBenchmarks()
    {
        var typeFinder1 = new TypeFinder(
            new NullLogger<TypeFinder>(),
            new DefaultUmbracoAssemblyProvider(GetType().Assembly, NullLoggerFactory.Instance));

        _typeLoader1 = new TypeLoader(typeFinder1, NullLogger<TypeLoader>.Instance);
        _typeLoader2 = new TypeLoader(typeFinder1, NullLogger<TypeLoader>.Instance);
    }

    /// <summary>
    ///     Use a predefined cache of types (names) - this is an example of what is saved to disk and loaded on startup
    /// </summary>
    /// <returns></returns>
    private Dictionary<(string, string), IEnumerable<string>> GetCache()
        => new()
        {
            [(typeof(ITypeLoaderBenchmarkPlugin).FullName, null)] = new List<string>
            {
                typeof(CustomAssemblyProvider1).FullName,
                typeof(CustomAssemblyProvider2).FullName,
                typeof(CustomAssemblyProvider3).FullName
            }
        };

    [Benchmark(Baseline = true)]
    public void WithTypesCache()
    {
        var found = _typeLoader1.GetTypes<ITypeLoaderBenchmarkPlugin>().Count();
    }

    [Benchmark]
    public void WithoutTypesCache()
    {
        var found = _typeLoader2.GetTypes<ITypeLoaderBenchmarkPlugin>(false).Count();
    }
}

// These are the types we'll find for the benchmark

public interface ITypeLoaderBenchmarkPlugin
{
}

public class CustomAssemblyProvider1 : ITypeLoaderBenchmarkPlugin
{
}

public class CustomAssemblyProvider2 : ITypeLoaderBenchmarkPlugin
{
}

public class CustomAssemblyProvider3 : ITypeLoaderBenchmarkPlugin
{
}
