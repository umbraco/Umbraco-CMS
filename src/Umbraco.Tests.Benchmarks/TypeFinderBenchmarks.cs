﻿using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Core.Composing;
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
            var typeFinder1 = new TypeFinder(new NullLogger<TypeFinder>(), new DefaultUmbracoAssemblyProvider(GetType().Assembly), new VaryingRuntimeHash());
            var found = typeFinder1.FindClassesOfType<IDiscoverable>().Count();
        }

        [Benchmark]
        public void WithoutGetReferencingAssembliesCheck()
        {
            var typeFinder2 = new TypeFinder(new NullLogger<TypeFinder>(), new DefaultUmbracoAssemblyProvider(GetType().Assembly), new VaryingRuntimeHash());
            typeFinder2.QueryWithReferencingAssemblies = false;
            var found = typeFinder2.FindClassesOfType<IDiscoverable>().Count();
        }

    }
}
