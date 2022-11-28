using System;
using BenchmarkDotNet.Attributes;
using Umbraco.Cms.Core;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks
{
    [QuickRunWithMemoryDiagnoserConfig]
    public class GuidUdiBenchmarks
    {
        private readonly Guid _guid = Guid.NewGuid();
        private readonly string _entityType = Constants.UdiEntityType.DocumentType;

        [Benchmark(Baseline = true)]
        public Udi CurrentInit()
        {
            return new OldGuidUdi(_entityType, _guid);
        }

        [Benchmark()]
        public Udi StringPolationInit()
        {
            return new StringPolationGuidUdi(_entityType, _guid);
        }

        [Benchmark]
        public Udi NewInit()
        {
            return new NewGuidUdi(_entityType, _guid);
        }

        public class OldGuidUdi : Udi
        {
            public Guid Guid { get; }

            public override bool IsRoot => throw new NotImplementedException();

            public OldGuidUdi(string entityType, Guid guid)
            : base(entityType, "umb://" + entityType + "/" + guid.ToString("N")) =>
            Guid = guid;
        }

        public class StringPolationGuidUdi : Udi
        {
            public Guid Guid { get; }

            public override bool IsRoot => throw new NotImplementedException();

            public StringPolationGuidUdi(string entityType, Guid guid)
            : base(entityType, $"umb://{entityType}/{guid:N}") =>
            Guid = guid;
        }

        public class NewGuidUdi : Udi
        {
            public Guid Guid { get; }

            public override bool IsRoot => throw new NotImplementedException();

            public NewGuidUdi(string entityType, Guid guid) : base(entityType, GetStringValue(entityType, guid))
            {
                Guid = guid;
            }

            public static string GetStringValue(ReadOnlySpan<char> entityType, Guid guid)
            {
                var startUdiLength = Constants.Conventions.Udi.Prefix.Length;
                var outputSize = entityType.Length + startUdiLength + 32 + 1;
                Span<char> output = stackalloc char[outputSize];

                Constants.Conventions.Udi.Prefix.CopyTo(output[..startUdiLength]);
                entityType.CopyTo(output.Slice(startUdiLength, entityType.Length));
                output[startUdiLength + entityType.Length] = '/';
                guid.TryFormat(output.Slice(outputSize - 32, 32), out _, "N");

                return new string(output);
            }
        }

        // I think we are currently bottlenecked by the 'new Udi(string)' not accepting a span. If it did, then we wouldn't have to create a string mid creation and we would be able to cut back on the allocated data
        //
        //|             Method |     Mean |     Error |   StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
        //|------------------- |---------:|----------:|---------:|------:|--------:|-------:|----------:|
        //|        CurrentInit | 562.2 ns | 242.84 ns | 13.31 ns |  1.00 |    0.00 | 0.1113 |     352 B |
        //| StringPolationInit | 589.3 ns | 530.08 ns | 29.06 ns |  1.05 |    0.07 | 0.0811 |     264 B |
        //|            NewInit | 533.6 ns |  40.55 ns |  2.22 ns |  0.95 |    0.03 | 0.0808 |     264 B |
    }
}
