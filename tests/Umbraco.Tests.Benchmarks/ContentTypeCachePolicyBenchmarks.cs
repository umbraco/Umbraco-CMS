// Copyright (c) Umbraco.
// See LICENSE for more details.

using BenchmarkDotNet.Attributes;
using Moq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Tests.Benchmarks.Config;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
/// Benchmarks comparing the performance of <see cref="FullDataSetRepositoryCachePolicy{TEntity,TId}.FindCached"/>
/// (clones only the matched entity) vs <see cref="FullDataSetRepositoryCachePolicy{TEntity,TId}.GetAll"/>
/// (clones all cached entities then filters).
/// </summary>
/// <remarks>
/// Run with: dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*ContentTypeCachePolicy*"
/// </remarks>
[QuickRunWithMemoryDiagnoserConfig]
public class ContentTypeCachePolicyBenchmarks
{
    private FullDataSetRepositoryCachePolicy<AuditItem, object> _policy = null!;
    private AuditItem[] _entities = null!;

    [Params(10, 50, 200)]
    public int EntityCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _entities = Enumerable.Range(1, EntityCount)
            .Select(i => new AuditItem(i, AuditType.Copy, 123, "test", $"item{i}"))
            .ToArray();

        var accessor = new Mock<IScopeAccessor>();
        var scope = new Mock<IScope>();
        scope.Setup(x => x.RepositoryCacheMode).Returns(RepositoryCacheMode.Default);
        accessor.Setup(x => x.AmbientScope).Returns(scope.Object);

        var cache = new BenchmarkCache();

        _policy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(
            cache,
            accessor.Object,
            new SingleServerCacheVersionService(),
            Mock.Of<ICacheSyncService>(),
            item => item.Id,
            false);

        // Warm the cache.
        _policy.GetAll(null, _ => _entities);
    }

    /// <summary>
    /// Baseline: simulates the old code path where GetAll() clones every entity,
    /// then the caller filters with FirstOrDefault to find one.
    /// </summary>
    [Benchmark(Baseline = true)]
    public AuditItem? GetAll_ThenFilter()
    {
        var all = _policy.GetAll(null, _ => _entities);
        return all.FirstOrDefault(x => x.Id.Equals(EntityCount / 2));
    }

    /// <summary>
    /// Optimized: FindCached searches non-cloned cache and clones only the single match.
    /// </summary>
    [Benchmark]
    public AuditItem? FindCached_SingleClone()
    {
        return _policy.FindCached(x => x.Id.Equals(EntityCount / 2), _ => _entities);
    }

    /// <summary>
    /// Optimized: ExistsCached searches non-cloned cache without cloning at all.
    /// </summary>
    [Benchmark]
    public bool ExistsCached_NoClone()
    {
        return _policy.ExistsCached(x => x.Id.Equals(EntityCount / 2), _ => _entities);
    }

    /// <summary>
    /// A simple cache backed by a dictionary, suitable for benchmarking.
    /// </summary>
    private sealed class BenchmarkCache : IAppPolicyCache
    {
        private readonly Dictionary<string, object?> _cache = [];

        public object? Get(string key)
            => _cache.TryGetValue(key, out var value) ? value : null;

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
            => _cache.Where(kvp => kvp.Key.StartsWith(keyStartsWith, StringComparison.InvariantCultureIgnoreCase)).Select(kvp => kvp.Value!);

        public IEnumerable<object> SearchByRegex(string regex) => throw new NotImplementedException();

        public void Clear() => _cache.Clear();

        public void Clear(string key) => _cache.Remove(key);

        public void ClearOfType(Type type) => throw new NotImplementedException();

        public void ClearOfType<T>() => throw new NotImplementedException();

        public void ClearOfType<T>(Func<string, T, bool> predicate) => throw new NotImplementedException();

        public void ClearByKey(string keyStartsWith)
        {
            var keysToRemove = _cache.Keys.Where(k => k.StartsWith(keyStartsWith, StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }

        public void ClearByRegex(string regex) => throw new NotImplementedException();

        public void Insert(string key, Func<object?> factory, TimeSpan? timeout = null, bool isSliding = false)
            => _cache[key] = factory();
    }
}
