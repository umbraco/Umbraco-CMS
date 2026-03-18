// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// Contains unit tests for the <see cref="ObjectCacheAppCache"/> class to verify its caching functionality.
/// </summary>
[TestFixture]
public class ObjectAppCacheTests : RuntimeAppCacheTests
{
    /// <summary>
    /// Initializes the test environment and sets up the ObjectCacheAppCache provider for ObjectAppCacheTests.
    /// </summary>
    public override void Setup()
    {
        base.Setup();
        _provider = new ObjectCacheAppCache();
    }

    private ObjectCacheAppCache _provider;

    protected override int GetTotalItemCount => _provider.MemoryCache.Count;

    internal override IAppCache AppCache => _provider;

    internal override IAppPolicyCache AppPolicyCache => _provider;
}
