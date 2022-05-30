using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Extensions;

namespace Umbraco.Tests.Benchmarks;
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
    public SqlTemplatesBenchmark()
    {
        var mappers = new MapperCollection();
        var factory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, mappers).Init(), mappers);

        SqlContext = new SqlContext(new SqlServerSyntaxProvider(Options.Create(new GlobalSettings())), DatabaseType.SQLCe, factory);
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
                .Select<Thing1Dto>()
                .From<Thing1Dto>()
                .Where<Thing1Dto>(x => x.Name == "yada");

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
                .Select<Thing1Dto>()
                .From<Thing1Dto>()
                .Where<Thing1Dto>(x => x.Name == SqlTemplate.Arg<string>("name")));

            var sql = template.Sql(new { name = "yada" });

            var sqlString = sql.SQL; // force-build the SQL
        }
    }

    private class Config : ManualConfig
    {
        public Config() => Add(MemoryDiagnoser.Default);
    }

    [TableName("zbThing1")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Thing1Dto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}
