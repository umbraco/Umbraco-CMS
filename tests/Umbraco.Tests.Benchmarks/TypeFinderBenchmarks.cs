using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Tests.Benchmarks;

[MediumRunJob]
[MemoryDiagnoser]
public class TypeFinderBenchmarks
{
    private readonly TypeFinder _typeFinder1;
    private readonly TypeFinder _typeFinder2;

    public TypeFinderBenchmarks()
    {
        _typeFinder1 = new TypeFinder(new NullLogger<TypeFinder>(), new DefaultUmbracoAssemblyProvider(GetType().Assembly, NullLoggerFactory.Instance));
        _typeFinder2 = new TypeFinder(new NullLogger<TypeFinder>(), new DefaultUmbracoAssemblyProvider(GetType().Assembly, NullLoggerFactory.Instance))
            {
                QueryWithReferencingAssemblies = false
            };
    }

    // TODO: This setting seems to make no difference anymore

    [Benchmark(Baseline = true)]
    public void WithGetReferencingAssembliesCheck()
    {
        var found = _typeFinder1.FindClassesOfType<IDiscoverable>().Count();
    }

    [Benchmark]
    public void WithoutGetReferencingAssembliesCheck()
    {
        var found = _typeFinder2.FindClassesOfType<IDiscoverable>().Count();
    }
}
