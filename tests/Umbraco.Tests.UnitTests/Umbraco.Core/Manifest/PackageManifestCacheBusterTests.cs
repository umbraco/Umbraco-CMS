using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Semver;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class PackageManifestCacheBusterTests
{
    private const string GlobalHash = "globalhash";

    [Test]
    public void ResolvePackageCacheBustHash_UsesVersionHash_WhenVersionPresent()
    {
        var result = PackageManifestCacheBuster.ResolvePackageCacheBustHash("1.2.3", GlobalHash);
        Assert.That(result, Is.EqualTo("1.2.3".GenerateHash()));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ResolvePackageCacheBustHash_FallsBackToGlobal_WhenVersionMissing(string? version)
    {
        var result = PackageManifestCacheBuster.ResolvePackageCacheBustHash(version, GlobalHash);
        Assert.That(result, Is.EqualTo(GlobalHash));
    }

    [Test]
    public void GetGlobalCacheBustHash_UsesVersionHash_WhenNotInDebugMode()
    {
        var hostingEnv = new Mock<IHostingEnvironment>();
        hostingEnv.SetupGet(h => h.IsDebugMode).Returns(false);

        var umbracoVersion = new Mock<IUmbracoVersion>();
        umbracoVersion.SetupGet(v => v.SemanticVersion).Returns(new SemVersion(17, 0, 0));

        var expected = new SemVersion(17, 0, 0).ToSemanticString().GenerateHash();
        Assert.That(PackageManifestCacheBuster.GetGlobalCacheBustHash(hostingEnv.Object, umbracoVersion.Object), Is.EqualTo(expected));
    }

    [Test]
    public void ApplyCacheBust_StampsAppPluginsPath()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js", "abc");
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?umb__rnd=abc"));
    }

    [Test]
    public void ApplyCacheBust_IsCaseInsensitiveOnAppPluginsRoot()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/app_plugins/MyPkg/index.js", "abc");
        Assert.That(result, Is.EqualTo("/app_plugins/MyPkg/index.js?umb__rnd=abc"));
    }

    [Test]
    public void ApplyCacheBust_InsertsBeforeFragment()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js#frag", "abc");
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?umb__rnd=abc#frag"));
    }

    [TestCase("/umbraco/backoffice/apps/app/index.js")]
    [TestCase("@umbraco-cms/backoffice/router")]
    [TestCase("https://cdn.example.com/pkg/index.js")]
    [TestCase("//cdn.example.com/pkg/index.js")]
    [TestCase("./relative/index.js")]
    public void ApplyCacheBust_LeavesNonAppPluginsPathsUnchanged(string url)
    {
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc"), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_SkipsWhenQueryAlreadyPresent()
    {
        const string url = "/App_Plugins/MyPkg/index.js?v=1";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc"), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_SkipsWhenCacheBusterTokenPresent()
    {
        const string url = "/App_Plugins/MyPkg/index.js?v=%CACHE_BUSTER%";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc"), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_SkipsWhenQuestionMarkInFragment()
    {
        const string url = "/App_Plugins/MyPkg/index.js#a?b";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc"), Is.EqualTo(url));
    }
}
