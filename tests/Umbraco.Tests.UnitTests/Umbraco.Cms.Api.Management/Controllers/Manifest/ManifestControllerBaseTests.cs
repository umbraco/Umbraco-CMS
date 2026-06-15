using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Manifest;
using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.Manifest;

[TestFixture]
public class ManifestControllerBaseTests
{
    private const string CacheBustHash = "abc123hash";

    [Test]
    public void ReplaceCacheBusterTokens_Replaces_Token_In_Extension_Properties()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"type":"js","js":"/App_Plugins/Foo/bundle.js?v=%CACHE_BUSTER%"}"""),
            ]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.That(json, Does.Contain(CacheBustHash));
        Assert.That(json, Does.Not.Contain(Constants.Web.CacheBusterToken));
    }

    [Test]
    public void ReplaceCacheBusterTokens_Replaces_Token_In_Multiple_Properties()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"type":"js","js":"/App_Plugins/Foo/bundle.js?v=%CACHE_BUSTER%","element":"/App_Plugins/Foo/element.js?v=%CACHE_BUSTER%"}"""),
            ]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.That(json, Does.Contain($"/App_Plugins/Foo/bundle.js?v={CacheBustHash}"));
        Assert.That(json, Does.Contain($"/App_Plugins/Foo/element.js?v={CacheBustHash}"));
        Assert.That(json, Does.Not.Contain(Constants.Web.CacheBusterToken));
    }

    [Test]
    public void ReplaceCacheBusterTokens_Stamps_Clean_AppPlugins_Url()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"type":"js","js":"/App_Plugins/Foo/bundle.js"}"""),
            ]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.That(json, Does.Contain($"/App_Plugins/Foo/bundle.js?umb__rnd={CacheBustHash}"));
    }

    [Test]
    public void ReplaceCacheBusterTokens_Stamps_Clean_Url_Nested_In_Extension()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"type":"bundle","meta":{"loader":"/App_Plugins/Foo/loader.js"}}"""),
            ]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.That(json, Does.Contain($"/App_Plugins/Foo/loader.js?umb__rnd={CacheBustHash}"));
    }

    [Test]
    public void ReplaceCacheBusterTokens_Leaves_Author_Managed_Query_Untouched()
    {
        // The author already provided a query string, so we assume they manage cache-busting themselves.
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"type":"js","js":"/App_Plugins/Foo/bundle.js?v=1"}"""),
            ]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("/App_Plugins/Foo/bundle.js?v=1"));
            Assert.That(json, Does.Not.Contain("umb__rnd"));
            Assert.That(json, Does.Not.Contain(CacheBustHash));
        });
    }

    [Test]
    public void ReplaceCacheBusterTokens_Leaves_Non_JavaScript_AppPlugins_Assets_Untouched()
    {
        // Stylesheets, icons and other non-.js /App_Plugins strings are exactly the false positives the .js filter guards against.
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"type":"css","css":"/App_Plugins/Foo/styles.css","meta":{"icon":"/App_Plugins/Foo/icon.svg"}}"""),
            ]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("/App_Plugins/Foo/styles.css"));
            Assert.That(json, Does.Contain("/App_Plugins/Foo/icon.svg"));
            Assert.That(json, Does.Not.Contain("umb__rnd"));
        });
    }

    [Test]
    public void ReplaceCacheBusterTokens_Leaves_Non_AppPlugins_Url_Untouched()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"type":"js","js":"https://cdn.example.com/foo.js","element":"@my/bare-specifier"}"""),
            ]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("https://cdn.example.com/foo.js"));
            Assert.That(json, Does.Contain("@my/bare-specifier"));
            Assert.That(json, Does.Not.Contain("umb__rnd"));
        });
    }

    [Test]
    public void ReplaceCacheBusterTokens_Handles_Empty_Extensions()
    {
        var model = CreateModel([]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        Assert.That(models[0].Extensions, Is.Empty);
    }

    [Test]
    public void ReplaceCacheBusterTokens_Handles_Multiple_Models()
    {
        var modelWithToken = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"js":"/App_Plugins/A/a.js?v=%CACHE_BUSTER%"}"""),
            ],
            "PackageA");

        var modelWithCleanUrl = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"js":"/App_Plugins/B/b.js"}"""),
            ],
            "PackageB");

        var modelEmpty = CreateModel([], "PackageC");

        List<ManifestResponseModel> models = [modelWithToken, modelWithCleanUrl, modelEmpty];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        Assert.Multiple(() =>
        {
            var jsonA = JsonSerializer.Serialize(models[0].Extensions);
            Assert.That(jsonA, Does.Contain(CacheBustHash));
            Assert.That(jsonA, Does.Not.Contain(Constants.Web.CacheBusterToken));

            var jsonB = JsonSerializer.Serialize(models[1].Extensions);
            Assert.That(jsonB, Does.Contain($"/App_Plugins/B/b.js?umb__rnd={CacheBustHash}"));

            Assert.That(models[2].Extensions, Is.Empty);
        });
    }

    [Test]
    public void ReplaceCacheBusterTokens_Replaces_Token_In_Nested_Extension_Objects()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"type":"bundle","meta":{"loader":"/App_Plugins/Foo/loader.js?v=%CACHE_BUSTER%"}}"""),
            ]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.That(json, Does.Contain($"/App_Plugins/Foo/loader.js?v={CacheBustHash}"));
        Assert.That(json, Does.Not.Contain(Constants.Web.CacheBusterToken));
    }

    [Test]
    public void ReplaceCacheBusterTokens_Preserves_All_Models_In_Output()
    {
        List<ManifestResponseModel> models = Enumerable.Range(0, 5)
            .Select(i => CreateModel([], $"Package{i}"))
            .ToList();

        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        Assert.That(models, Has.Count.EqualTo(5));
        for (var i = 0; i < 5; i++)
        {
            Assert.That(models[i].Name, Is.EqualTo($"Package{i}"));
        }
    }

    [Test]
    public void ReplaceCacheBusterTokens_Handles_Multiple_Extensions_In_Single_Model()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"js":"/App_Plugins/Foo/one.js?v=%CACHE_BUSTER%"}"""),
                JsonSerializer.Deserialize<JsonElement>(
                    """{"js":"/App_Plugins/Foo/two.js?v=%CACHE_BUSTER%"}"""),
                JsonSerializer.Deserialize<JsonElement>(
                    """{"js":"/App_Plugins/Foo/three.js"}"""),
            ]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.Multiple(() =>
        {
            // The tokenised URLs resolve the token; the clean URL is auto-stamped.
            Assert.That(json, Does.Contain($"/App_Plugins/Foo/one.js?v={CacheBustHash}"));
            Assert.That(json, Does.Contain($"/App_Plugins/Foo/two.js?v={CacheBustHash}"));
            Assert.That(json, Does.Contain($"/App_Plugins/Foo/three.js?umb__rnd={CacheBustHash}"));
            Assert.That(json, Does.Not.Contain(Constants.Web.CacheBusterToken));
        });
    }

    [Test]
    public void ReplaceCacheBusterTokens_Produces_Valid_Json_When_Hash_Contains_Special_Characters()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"js":"/App_Plugins/Foo/bundle.js?v=%CACHE_BUSTER%"}"""),
            ]);

        const string unsafeHash = """hash"with\special""";

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, unsafeHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.That(json, Does.Not.Contain(Constants.Web.CacheBusterToken));

        // Verify the result is still valid JSON by round-tripping
        Assert.DoesNotThrow(() => JsonSerializer.Deserialize<object[]>(json));
    }

    [Test]
    public void ReplaceCacheBusterTokens_Uses_Package_Version_Hash_When_Available()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"js":"/App_Plugins/Foo/bundle.js?v=%CACHE_BUSTER%"}"""),
            ],
            "Foo");
        var manifest = new PackageManifest { Name = "Foo", Version = "1.2.3", Extensions = model.Extensions };

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, [manifest], CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.Multiple(() =>
        {
            // The package's own version drives the hash, not the global Umbraco hash.
            Assert.That(json, Does.Contain($"/App_Plugins/Foo/bundle.js?v={"1.2.3".GenerateHash()}"));
            Assert.That(json, Does.Not.Contain(CacheBustHash));
            Assert.That(json, Does.Not.Contain(Constants.Web.CacheBusterToken));
        });
    }

    [Test]
    public void ReplaceCacheBusterTokens_ResolvesTokenToVersionHash_WhenBustingDisabled()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"js":"/App_Plugins/Foo/bundle.js?v=%CACHE_BUSTER%"}"""),
            ],
            "Foo");
        var manifest = new PackageManifest { Name = "Foo", Version = "1.2.3", AllowCacheBusting = false, Extensions = model.Extensions };

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, [manifest], CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.Multiple(() =>
        {
            // Disabling busting turns off auto-stamping only; an explicit token still resolves to the package version hash.
            Assert.That(json, Does.Contain($"/App_Plugins/Foo/bundle.js?v={"1.2.3".GenerateHash()}"));
            Assert.That(json, Does.Not.Contain(CacheBustHash));
            Assert.That(json, Does.Not.Contain(Constants.Web.CacheBusterToken));
        });
    }

    [Test]
    public void ReplaceCacheBusterTokens_Stamps_Clean_Url_With_Package_Version_Hash()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>("""{"js":"/App_Plugins/Foo/bundle.js"}"""),
            ],
            "Foo");
        var manifest = new PackageManifest { Name = "Foo", Version = "1.2.3", Extensions = model.Extensions };

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, [manifest], CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.Multiple(() =>
        {
            // The clean URL is auto-stamped with the package's own version hash, not the global Umbraco hash.
            Assert.That(json, Does.Contain($"/App_Plugins/Foo/bundle.js?umb__rnd={"1.2.3".GenerateHash()}"));
            Assert.That(json, Does.Not.Contain(CacheBustHash));
        });
    }

    [Test]
    public void ReplaceCacheBusterTokens_DoesNotStamp_CleanUrl_WhenBustingDisabled()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>("""{"js":"/App_Plugins/Foo/bundle.js"}"""),
            ],
            "Foo");
        var manifest = new PackageManifest { Name = "Foo", Version = "1.2.3", AllowCacheBusting = false, Extensions = model.Extensions };

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, [manifest], CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.Multiple(() =>
        {
            // Busting disabled: a clean URL without an explicit token is left exactly as authored.
            Assert.That(json, Does.Contain("/App_Plugins/Foo/bundle.js"));
            Assert.That(json, Does.Not.Contain("umb__rnd"));
        });
    }

    private static ManifestResponseModel CreateModel(object[] extensions, string name = "TestPackage")
        => new() { Name = name, Extensions = extensions };

    /// <summary>
    /// Exposes the protected static method for testing.
    /// </summary>
    private class TestableManifestControllerBase : ManifestControllerBase
    {
        // Builds version-less manifests so the %CACHE_BUSTER% token resolves to the supplied global hash —
        // the behaviour the bulk of these tests assert. Per-package version behaviour is covered via the overload below.
        public static void TestReplaceCacheBusterTokens(
            IEnumerable<ManifestResponseModel> models, string globalHash)
        {
            List<ManifestResponseModel> modelList = models.ToList();
            List<PackageManifest> manifests = modelList
                .Select(m => new PackageManifest { Name = m.Name, Extensions = m.Extensions })
                .ToList();
            ReplaceCacheBusterTokens(modelList, manifests, globalHash);
        }

        public static void TestReplaceCacheBusterTokens(
            IEnumerable<ManifestResponseModel> models, IEnumerable<PackageManifest> manifests, string globalHash)
            => ReplaceCacheBusterTokens(models, manifests, globalHash);
    }
}
