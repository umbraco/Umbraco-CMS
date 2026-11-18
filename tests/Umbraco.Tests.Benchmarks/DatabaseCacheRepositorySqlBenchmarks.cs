using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
/// Measures building the SQL of the published content and media reads of the database cache repository, which every
/// cache miss does once per read.
/// </summary>
/// <remarks>
/// Run with: <c>dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*DatabaseCacheRepositorySql*" --inProcess</c>.
/// </remarks>
[QuickRunWithMemoryDiagnoserConfig]
public class DatabaseCacheRepositorySqlBenchmarks
{
    private DatabaseCacheRepository _repository = null!;
    private readonly Guid _key = Guid.NewGuid();
    private Guid[] _keys = null!;

    [Params(2, 8)]
    public int KeyCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var mappers = new MapperCollection();
        var pocoDataFactory = new FluentPocoDataFactory((type, _) => new PocoDataBuilder(type, mappers).Init(), mappers);
        var sqlContext = new SqlContext(new SqlServerSyntaxProvider(Options.Create(new GlobalSettings())), DatabaseType.SqlServer2012, pocoDataFactory);

        var databaseFactory = new Mock<IUmbracoDatabaseFactory>();
        databaseFactory.SetupGet(x => x.SqlContext).Returns(sqlContext);

        _repository = new DatabaseCacheRepository(
            Mock.Of<IScopeAccessor>(),
            databaseFactory.Object,
            AppCaches.NoCache,
            NullLogger<DatabaseCacheRepository>.Instance,
            Mock.Of<IShortStringHelper>(),
            new UrlSegmentProviderCollection(() => []),
            Mock.Of<IContentCacheDataSerializerFactory>(),
            Options.Create(new NuCacheSettings()));

        _keys = Enumerable.Range(0, KeyCount).Select(_ => Guid.NewGuid()).ToArray();

        // Build each query once, so a cached template is created before measuring.
        _ = ContentSource();
        _ = ContentSources();
        _ = MediaSource();
        _ = MediaSources();
    }

    [Benchmark]
    public string ContentSource() => _repository.GetContentSourceSql(_key).SQL;

    [Benchmark]
    public string ContentSources() => _repository.GetContentSourcesSql(_keys).SQL;

    [Benchmark]
    public string MediaSource() => _repository.GetMediaSourceSql(_key).SQL;

    [Benchmark]
    public string MediaSources() => _repository.GetMediaSourcesSql(_keys).SQL;
}
