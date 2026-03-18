// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// Contains unit tests for the <see cref="DictionaryAppCache"/> class to verify its caching behavior and functionality.
/// </summary>
[TestFixture]
public class DictionaryAppCacheTests : AppCacheTests
{
    /// <summary>
    /// Sets up the test environment for <see cref="DictionaryAppCacheTests"/>, initializing the <see cref="DictionaryAppCache"/> instance.
    /// </summary>
    public override void Setup()
    {
        base.Setup();
        _appCache = new DictionaryAppCache();
    }

    private DictionaryAppCache _appCache;

    internal override IAppCache AppCache => _appCache;

    protected override int GetTotalItemCount => _appCache.Count;
}
