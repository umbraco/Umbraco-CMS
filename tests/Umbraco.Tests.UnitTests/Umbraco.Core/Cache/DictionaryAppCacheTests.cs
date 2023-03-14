// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class DictionaryAppCacheTests : AppCacheTests
{
    public override void Setup()
    {
        base.Setup();
        _appCache = new DictionaryAppCache();
    }

    private DictionaryAppCache _appCache;

    internal override IAppCache AppCache => _appCache;

    protected override int GetTotalItemCount => _appCache.Count;
}
