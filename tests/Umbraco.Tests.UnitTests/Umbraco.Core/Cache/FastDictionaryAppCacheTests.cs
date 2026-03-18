using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// Contains unit tests for the <see cref="FastDictionaryAppCache"/> class, verifying its caching behavior and correctness.
/// These tests ensure that the cache implementation functions as expected under various scenarios.
/// </summary>
[TestFixture]
public class FastDictionaryAppCacheTests : AppCacheTests
{
    /// <summary>
    /// Sets up the test environment for <see cref="FastDictionaryAppCacheTests"/>,
    /// including initializing the <see cref="_appCache"/> field with a new <see cref="FastDictionaryAppCache"/> instance.
    /// </summary>
    public override void Setup()
    {
        base.Setup();
        _appCache = new FastDictionaryAppCache();
    }

    private FastDictionaryAppCache _appCache;

    internal override IAppCache AppCache => _appCache;

    protected override int GetTotalItemCount => _appCache.Count;
}
