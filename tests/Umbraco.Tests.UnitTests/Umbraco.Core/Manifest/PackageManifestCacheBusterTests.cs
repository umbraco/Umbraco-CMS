using NUnit.Framework;
using Umbraco.Cms.Core.Manifest;
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
    public void ResolvePackageCacheBust_UsesVersionHashAndEnablesStamping_WhenBustingAllowed()
    {
        var manifest = new PackageManifest { Name = "Pkg", Version = "1.2.3", Extensions = [] };

        var (hash, autoStamp) = PackageManifestCacheBuster.ResolvePackageCacheBust(manifest, GlobalHash);

        Assert.Multiple(() =>
        {
            Assert.That(hash, Is.EqualTo("1.2.3".GenerateHash()));
            Assert.That(autoStamp, Is.True);
        });
    }

    [Test]
    public void ResolvePackageCacheBust_FallsBackToGlobalAndDisablesStamping_WhenBustingDisallowed()
    {
        var manifest = new PackageManifest { Name = "Pkg", Version = "1.2.3", AllowCacheBusting = false, Extensions = [] };

        var (hash, autoStamp) = PackageManifestCacheBuster.ResolvePackageCacheBust(manifest, GlobalHash);

        Assert.Multiple(() =>
        {
            Assert.That(hash, Is.EqualTo(GlobalHash));
            Assert.That(autoStamp, Is.False);
        });
    }

    [Test]
    public void ApplyCacheBust_StampsAppPluginsPath()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js", "abc", autoStamp: true);
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?umb__rnd=abc"));
    }

    [Test]
    public void ApplyCacheBust_IsCaseInsensitiveOnAppPluginsRoot()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/app_plugins/MyPkg/index.js", "abc", autoStamp: true);
        Assert.That(result, Is.EqualTo("/app_plugins/MyPkg/index.js?umb__rnd=abc"));
    }

    [Test]
    public void ApplyCacheBust_InsertsBeforeFragment()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js#frag", "abc", autoStamp: true);
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?umb__rnd=abc#frag"));
    }

    [TestCase("/App_Plugins/MyPkg/index.JS", "/App_Plugins/MyPkg/index.JS?umb__rnd=abc")]
    [TestCase("/App_Plugins/MyPkg/index.mjs.js", "/App_Plugins/MyPkg/index.mjs.js?umb__rnd=abc")]
    public void ApplyCacheBust_StampsJavaScriptPath_RegardlessOfExtensionCase(string url, string expected)
    {
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc", autoStamp: true), Is.EqualTo(expected));
    }

    [TestCase("/App_Plugins/MyPkg/styles.css")]
    [TestCase("/App_Plugins/MyPkg/data.json")]
    [TestCase("/App_Plugins/MyPkg/module.mjs")]
    [TestCase("/App_Plugins/MyPkg/icon.svg")]
    [TestCase("/App_Plugins/MyPkg/route")]
    [TestCase("/App_Plugins/MyPkg/")]
    public void ApplyCacheBust_DoesNotStampNonJavaScriptAppPluginsPath(string url)
    {
        // Only .js entrypoints are auto-stamped; anything else that merely looks like an /App_Plugins path is left alone.
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc", autoStamp: true), Is.EqualTo(url));
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
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc", autoStamp: true), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_SkipsWhenQueryAlreadyPresent()
    {
        const string url = "/App_Plugins/MyPkg/index.js?v=1";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc", autoStamp: true), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_SkipsWhenQuestionMarkInFragment()
    {
        const string url = "/App_Plugins/MyPkg/index.js#a?b";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc", autoStamp: true), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_ResolvesCacheBusterToken()
    {
        const string url = "/App_Plugins/MyPkg/index.js?v=%CACHE_BUSTER%";
        Assert.That(
            PackageManifestCacheBuster.ApplyCacheBust(url, "abc", autoStamp: true),
            Is.EqualTo("/App_Plugins/MyPkg/index.js?v=abc"));
    }

    [Test]
    public void ApplyCacheBust_ResolvesCacheBusterToken_OnAnyHost_RegardlessOfAutoStamp()
    {
        // The token is an explicit opt-in, so it resolves even on a non-/App_Plugins URL and when auto-stamping is off.
        const string url = "https://cdn.example.com/pkg/index.js?v=%CACHE_BUSTER%";
        Assert.That(
            PackageManifestCacheBuster.ApplyCacheBust(url, "abc", autoStamp: false),
            Is.EqualTo("https://cdn.example.com/pkg/index.js?v=abc"));
    }

    [Test]
    public void ApplyCacheBust_DoesNotStampCleanPath_WhenAutoStampDisabled()
    {
        const string url = "/App_Plugins/MyPkg/index.js";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc", autoStamp: false), Is.EqualTo(url));
    }
}
