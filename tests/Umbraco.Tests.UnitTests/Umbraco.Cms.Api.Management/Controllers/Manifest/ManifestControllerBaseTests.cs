using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Manifest;
using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core;

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
    public void ReplaceCacheBusterTokens_Leaves_Extensions_Without_Token_Untouched()
    {
        var model = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"type":"js","js":"/App_Plugins/Foo/bundle.js"}"""),
            ]);

        List<ManifestResponseModel> models = [model];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        var json = JsonSerializer.Serialize(models[0].Extensions);
        Assert.That(json, Does.Contain("/App_Plugins/Foo/bundle.js"));
        Assert.That(json, Does.Not.Contain(CacheBustHash));
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

        var modelWithoutToken = CreateModel(
            [
                JsonSerializer.Deserialize<JsonElement>(
                    """{"js":"/App_Plugins/B/b.js"}"""),
            ],
            "PackageB");

        var modelEmpty = CreateModel([], "PackageC");

        List<ManifestResponseModel> models = [modelWithToken, modelWithoutToken, modelEmpty];
        TestableManifestControllerBase.TestReplaceCacheBusterTokens(models, CacheBustHash);

        Assert.Multiple(() =>
        {
            var jsonA = JsonSerializer.Serialize(models[0].Extensions);
            Assert.That(jsonA, Does.Contain(CacheBustHash));
            Assert.That(jsonA, Does.Not.Contain(Constants.Web.CacheBusterToken));

            var jsonB = JsonSerializer.Serialize(models[1].Extensions);
            Assert.That(jsonB, Does.Contain("/App_Plugins/B/b.js"));
            Assert.That(jsonB, Does.Not.Contain(CacheBustHash));

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
            Assert.That(json, Does.Contain($"/App_Plugins/Foo/one.js?v={CacheBustHash}"));
            Assert.That(json, Does.Contain($"/App_Plugins/Foo/two.js?v={CacheBustHash}"));
            Assert.That(json, Does.Contain("/App_Plugins/Foo/three.js"));
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

    private static ManifestResponseModel CreateModel(object[] extensions, string name = "TestPackage")
        => new() { Name = name, Extensions = extensions };

    /// <summary>
    /// Exposes the protected static method for testing.
    /// </summary>
    private class TestableManifestControllerBase : ManifestControllerBase
    {
        public static void TestReplaceCacheBusterTokens(
            IEnumerable<ManifestResponseModel> models, string cacheBustHash)
            => ReplaceCacheBusterTokens(models, cacheBustHash);
    }
}
