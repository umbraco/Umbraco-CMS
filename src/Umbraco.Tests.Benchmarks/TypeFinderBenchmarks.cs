using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks
{
    [MediumRunJob]
    [MemoryDiagnoser]
    public class TypeFinderBenchmarks
    {

        [Benchmark(Baseline = true)]
        public void WithGetReferencingAssembliesCheck()
        {
            var typeFinder1 = new TypeFinder(new NullLogger(), new DefaultUmbracoAssemblyProvider(GetType().Assembly));
            var found = typeFinder1.FindClassesOfType<IDiscoverable>().Count();
        }

        [Benchmark]
        public void WithoutGetReferencingAssembliesCheck()
        {
            var typeFinder2 = new TypeFinder(new NullLogger(), new DefaultUmbracoAssemblyProvider(GetType().Assembly));
            typeFinder2.QueryWithReferencingAssemblies = false;
            var found = typeFinder2.FindClassesOfType<IDiscoverable>().Count();
        }
    }
}
