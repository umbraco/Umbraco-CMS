using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using NPoco;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.Persistence.NPocoTests;

namespace Umbraco.Tests.Benchmarks
{
    // seeing this kind of results - templates are gooood
    //
    // Method           |       Mean |     Error |    StdDev | Scaled |    Gen 0 | Allocated |
    // ---------------- |-----------:|----------:|----------:|-------:|---------:|----------:|
    // WithoutTemplate  | 2,895.0 us | 64.873 us | 99.068 us |   1.00 | 183.5938 | 762.23 KB |
    // WithTemplate     |   263.2 us |  4.581 us |  4.285 us |   0.09 |  50.2930 | 207.13 KB |
    //
    // though the difference might not be so obvious in case of WhereIn which requires parsing

    [Config(typeof(Config))]
    public class SqlTemplatesBenchmark
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                Add(new MemoryDiagnoser());
            }
        }

        public SqlTemplatesBenchmark()
        {
            var mappers = new NPoco.MapperCollection { new PocoMapper() };
            var factory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, mappers).Init());

            SqlContext = new SqlContext(new SqlCeSyntaxProvider(), DatabaseType.SQLCe, factory);
            SqlTemplates = new SqlTemplates(SqlContext);
        }

        private ISqlContext SqlContext { get; }
        private SqlTemplates SqlTemplates { get; }

        [Benchmark(Baseline = true)]
        public void WithoutTemplate()
        {
            for (var i = 0; i < 100; i++)
            {
                var sql = Sql.BuilderFor(SqlContext)
                    .Select<NPocoFetchTests.Thing1Dto>()
                    .From<NPocoFetchTests.Thing1Dto>()
                    .Where<NPocoFetchTests.Thing1Dto>(x => x.Name == "yada");

                var sqlString = sql.SQL; // force-build the SQL
            }
        }

        [Benchmark]
        public void WithTemplate()
        {
            SqlTemplates.Clear();

            for (var i = 0; i < 100; i++)
            {
                var template = SqlTemplates.Get("test", s => s
                    .Select<NPocoFetchTests.Thing1Dto>()
                    .From<NPocoFetchTests.Thing1Dto>()
                    .Where<NPocoFetchTests.Thing1Dto>(x => x.Name == SqlTemplate.Arg<string>("name")));

                var sql = template.Sql(new { name = "yada" });

                var sqlString = sql.SQL; // force-build the SQL
            }
        }

    }
}
