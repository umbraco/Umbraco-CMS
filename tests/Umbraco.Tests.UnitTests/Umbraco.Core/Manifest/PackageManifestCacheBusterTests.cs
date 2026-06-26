using NUnit.Framework;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class PackageManifestCacheBusterTests
{
    private static string ShortHash(string value) => value.GenerateHash()[..7];

    [Test]
    public void ComputeCacheBuster_CombinesVersionWithShortHostHash()
    {
        var result = PackageManifestCacheBuster.ComputeCacheBuster("1.2.3", "deploy-1");
        Assert.That(result, Is.EqualTo($"1.2.3-{ShortHash("deploy-1")}"));
    }

    [Test]
    public void ComputeCacheBuster_UsesShortHashOfSevenCharacters()
        => Assert.That(PackageManifestCacheBuster.ComputeCacheBuster("1.2.3", "deploy-1")!.Split('-')[^1], Has.Length.EqualTo(7));

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ComputeCacheBuster_FallsBackToVersion_WhenNoHostCacheBuster(string? hostCacheBuster)
        => Assert.That(PackageManifestCacheBuster.ComputeCacheBuster("1.2.3", hostCacheBuster), Is.EqualTo("1.2.3"));

    [Test]
    public void ComputeCacheBuster_UsesShortHashAlone_WhenNoVersion()
        => Assert.That(PackageManifestCacheBuster.ComputeCacheBuster(null, "deploy-1"), Is.EqualTo(ShortHash("deploy-1")));

    [Test]
    public void ComputeCacheBuster_ReturnsNull_WhenNoVersionAndNoHostCacheBuster()
        => Assert.That(PackageManifestCacheBuster.ComputeCacheBuster(null, null), Is.Null);

    [Test]
    public void ApplyCacheBust_AppendsCacheBusterToCleanAppPluginsPath()
        => Assert.That(
            PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js", "1.2.3-a1b2c3d"),
            Is.EqualTo("/App_Plugins/MyPkg/index.js?umb__rnd=1.2.3-a1b2c3d"));

    [Test]
    public void ApplyCacheBust_IsCaseInsensitiveOnAppPluginsRoot()
        => Assert.That(
            PackageManifestCacheBuster.ApplyCacheBust("/app_plugins/MyPkg/index.js", "1.2.3"),
            Is.EqualTo("/app_plugins/MyPkg/index.js?umb__rnd=1.2.3"));

    [Test]
    public void ApplyCacheBust_InsertsBeforeFragment()
        => Assert.That(
            PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js#frag", "1.2.3"),
            Is.EqualTo("/App_Plugins/MyPkg/index.js?umb__rnd=1.2.3#frag"));

    [Test]
    public void ApplyCacheBust_EscapesValue()
        => Assert.That(
            PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js", "a/b c"),
            Is.EqualTo("/App_Plugins/MyPkg/index.js?umb__rnd=a%2Fb%20c"));

    [TestCase(null)]
    [TestCase("")]
    public void ApplyCacheBust_LeavesUrlUnchanged_WhenNoCacheBuster(string? cacheBuster)
    {
        const string url = "/App_Plugins/MyPkg/index.js";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, cacheBuster), Is.EqualTo(url));
    }

    [TestCase("/umbraco/backoffice/apps/app/index.js")]
    [TestCase("@umbraco-cms/backoffice/router")]
    [TestCase("https://cdn.example.com/pkg/index.js")]
    [TestCase("//cdn.example.com/pkg/index.js")]
    [TestCase("/App_PluginsFoo/index.js")]
    public void ApplyCacheBust_LeavesNonAppPluginsPathsUnchanged(string url)
        => Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "1.2.3"), Is.EqualTo(url));

    [Test]
    public void ApplyCacheBust_SkipsWhenQueryAlreadyPresent()
    {
        // The %CACHE_BUSTER% token is resolved server-side; a URL that already carries a query is left alone.
        const string url = "/App_Plugins/MyPkg/index.js?cb=%CACHE_BUSTER%";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "1.2.3"), Is.EqualTo(url));
    }
}
