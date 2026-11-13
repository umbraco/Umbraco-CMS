using BenchmarkDotNet.Attributes;
using Moq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Tests.Benchmarks.Config;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
/// Measures the cost of <see cref="IContentCacheDataSerializerFactory.Create"/>, which runs on every database read
/// of the published content cache (single-item cache misses included), against a site with many content types.
/// </summary>
/// <remarks>
/// The content type service is backed by a real <see cref="FullDataSetRepositoryCachePolicy{TEntity,TId}"/>, whose
/// <c>GetAll</c> returns deep clones of every cached content type, as the repository does in production.
/// Run with: dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*HybridCacheSerializerFactory*"
/// </remarks>
[QuickRunWithMemoryDiagnoserConfig]
public class HybridCacheSerializerFactoryBenchmarks
{
    private const int PropertiesPerType = 15;

    private IContentCacheDataSerializerFactory _factory = null!;

    [Params(20, 200, 1000)]
    public int ContentTypeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

        IContentType[] contentTypes = Enumerable.Range(1, ContentTypeCount)
            .Select(i => CreateContentType(shortStringHelper, i))
            .ToArray();

        var accessor = new Mock<IScopeAccessor>();
        var scope = new Mock<IScope>();
        scope.Setup(x => x.RepositoryCacheMode).Returns(RepositoryCacheMode.Default);
        accessor.Setup(x => x.AmbientScope).Returns(scope.Object);

        var policy = new FullDataSetRepositoryCachePolicy<IContentType, int>(
            new BenchmarkCache(),
            accessor.Object,
            new SingleServerCacheVersionService(),
            Mock.Of<ICacheSyncService>(),
            contentType => contentType.Id,
            false);

        // Warm the cache, as a running site has after its first read.
        policy.GetAll(null, _ => contentTypes);

        var contentTypeService = new Mock<IContentTypeService>();
        contentTypeService
            .Setup(x => x.GetAll())
            .Returns(() => policy.GetAll(null, _ => contentTypes));

        var mediaTypeService = new Mock<IMediaTypeService>();
        mediaTypeService.Setup(x => x.GetAll()).Returns(Enumerable.Empty<IMediaType>());

        _factory = new MsgPackContentNestedDataSerializerFactory(
            contentTypeService.Object,
            mediaTypeService.Object,
            new PropertyEditorCollection(new DataEditorCollection(() => [])),
            Mock.Of<IPropertyCacheCompressionOptions>());
    }

    [Benchmark]
    public object Create_Document()
        => _factory.Create(ContentCacheDataSerializerEntityType.Document);

    private static IContentType CreateContentType(IShortStringHelper shortStringHelper, int id)
    {
        var contentType = new ContentType(shortStringHelper, -1)
        {
            Id = id,
            Key = Guid.NewGuid(),
            Alias = $"contentType{id}",
            Name = $"Content type {id}",
        };

        for (var i = 0; i < PropertiesPerType; i++)
        {
            contentType.AddPropertyType(
                new PropertyType(shortStringHelper, "Umbraco.TextBox", ValueStorageType.Nvarchar, $"property{i}")
                {
                    Name = $"Property {i}",
                },
                "content",
                "Content");
        }

        contentType.ResetDirtyProperties(false);
        return contentType;
    }

    private sealed class BenchmarkCache : IAppPolicyCache
    {
        private readonly Dictionary<string, object?> _cache = [];

        public object? Get(string key) => _cache.TryGetValue(key, out var value) ? value : null;

        public object? Get(string key, Func<object?> factory) => Get(key, factory, null, false);

        public object? Get(string key, Func<object?> factory, TimeSpan? timeout, bool isSliding = false)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return value;
            }

            value = factory();
            _cache[key] = value;
            return value;
        }

        public IEnumerable<object> SearchByKey(string keyStartsWith)
            => _cache
                .Where(kvp => kvp.Key.StartsWith(keyStartsWith, StringComparison.InvariantCultureIgnoreCase))
                .Select(kvp => kvp.Value!);

        public IEnumerable<object> SearchByRegex(string regex) => throw new NotImplementedException();

        public void Clear() => _cache.Clear();

        public void Clear(string key) => _cache.Remove(key);

        public void ClearOfType(Type type) => throw new NotImplementedException();

        public void ClearOfType<T>() => throw new NotImplementedException();

        public void ClearOfType<T>(Func<string, T, bool> predicate) => throw new NotImplementedException();

        public void ClearByKey(string keyStartsWith)
        {
            foreach (var key in _cache.Keys.Where(k => k.StartsWith(keyStartsWith, StringComparison.InvariantCultureIgnoreCase)).ToList())
            {
                _cache.Remove(key);
            }
        }

        public void ClearByRegex(string regex) => throw new NotImplementedException();

        public void Insert(string key, Func<object?> factory, TimeSpan? timeout = null, bool isSliding = false)
            => _cache[key] = factory();
    }
}
