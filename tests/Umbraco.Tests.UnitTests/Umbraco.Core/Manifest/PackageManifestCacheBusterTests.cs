using NUnit.Framework;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class PackageManifestCacheBusterTests
{
    [Test]
    public void ApplyCacheBust_AppendsVersionAndCacheBuster()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js", "1.2.3", "seed", autoStamp: true);
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?v=1.2.3&umb__rnd=seed"));
    }

    [Test]
    public void ApplyCacheBust_AppendsVersionOnly_WhenNoCacheBuster()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js", "1.2.3", string.Empty, autoStamp: true);
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?v=1.2.3"));
    }

    [Test]
    public void ApplyCacheBust_AppendsCacheBusterOnly_WhenNoVersion()
    {
        // The host cache-buster works even when a package has no version (version is optional).
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js", null, "seed", autoStamp: true);
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?umb__rnd=seed"));
    }

    [Test]
    public void ApplyCacheBust_LeavesUnchanged_WhenNeitherVersionNorCacheBuster()
    {
        const string url = "/App_Plugins/MyPkg/index.js";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, null, string.Empty, autoStamp: true), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_EscapesValues()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js", "1.0 beta", "a/b", autoStamp: true);
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?v=1.0%20beta&umb__rnd=a%2Fb"));
    }

    [Test]
    public void ApplyCacheBust_IsCaseInsensitiveOnAppPluginsRoot()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/app_plugins/MyPkg/index.js", "1.2.3", null, autoStamp: true);
        Assert.That(result, Is.EqualTo("/app_plugins/MyPkg/index.js?v=1.2.3"));
    }

    [Test]
    public void ApplyCacheBust_InsertsBeforeFragment()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js#frag", "1.2.3", null, autoStamp: true);
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?v=1.2.3#frag"));
    }

    [TestCase("/umbraco/backoffice/apps/app/index.js")]
    [TestCase("@umbraco-cms/backoffice/router")]
    [TestCase("https://cdn.example.com/pkg/index.js")]
    [TestCase("//cdn.example.com/pkg/index.js")]
    [TestCase("./relative/index.js")]
    [TestCase("/App_PluginsFoo/index.js")]
    [TestCase("/App_Plugins")]
    public void ApplyCacheBust_LeavesNonAppPluginsPathsUnchanged(string url)
    {
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "1.2.3", "seed", autoStamp: true), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_SkipsWhenQueryAlreadyPresent()
    {
        const string url = "/App_Plugins/MyPkg/index.js?foo=1";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "1.2.3", "seed", autoStamp: true), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_SkipsWhenQuestionMarkInFragment()
    {
        const string url = "/App_Plugins/MyPkg/index.js#a?b";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "1.2.3", "seed", autoStamp: true), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_DoesNotStampCleanPath_WhenAutoStampDisabled()
    {
        const string url = "/App_Plugins/MyPkg/index.js";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "1.2.3", "seed", autoStamp: false), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_ResolvesCacheBusterToken_ToVersion()
    {
        const string url = "/App_Plugins/MyPkg/index.js?cb=%CACHE_BUSTER%";
        Assert.That(
            PackageManifestCacheBuster.ApplyCacheBust(url, "1.2.3", "seed", autoStamp: true),
            Is.EqualTo("/App_Plugins/MyPkg/index.js?cb=1.2.3"));
    }

    [Test]
    public void ApplyCacheBust_ResolvesCacheBusterToken_ToCacheBuster_WhenNoVersion()
    {
        const string url = "/App_Plugins/MyPkg/index.js?cb=%CACHE_BUSTER%";
        Assert.That(
            PackageManifestCacheBuster.ApplyCacheBust(url, null, "seed", autoStamp: true),
            Is.EqualTo("/App_Plugins/MyPkg/index.js?cb=seed"));
    }

    [Test]
    public void ApplyCacheBust_ResolvesCacheBusterToken_OnAnyHost_RegardlessOfAutoStamp()
    {
        // The token is an explicit opt-in, so it resolves even on a non-/App_Plugins URL and when auto-stamping is off.
        const string url = "https://cdn.example.com/pkg/index.js?cb=%CACHE_BUSTER%";
        Assert.That(
            PackageManifestCacheBuster.ApplyCacheBust(url, "1.2.3", "seed", autoStamp: false),
            Is.EqualTo("https://cdn.example.com/pkg/index.js?cb=1.2.3"));
    }
}
