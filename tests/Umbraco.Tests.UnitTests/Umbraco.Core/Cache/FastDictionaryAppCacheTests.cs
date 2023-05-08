using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class FastDictionaryAppCacheTests : AppCacheTests
{
    public override void Setup()
    {
        base.Setup();
        _appCache = new FastDictionaryAppCache();
    }

    private FastDictionaryAppCache _appCache;

    internal override IAppCache AppCache => _appCache;

    protected override int GetTotalItemCount => _appCache.Count;
}
