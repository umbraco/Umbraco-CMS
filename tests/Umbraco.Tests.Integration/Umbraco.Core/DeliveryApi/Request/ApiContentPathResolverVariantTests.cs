using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.Request;

public class ApiContentPathResolverVariantTests : ApiContentPathResolverTestBase
{
    private Dictionary<string, IContent> _contentByName = new ();

    public static void ConfigureIncludeTopLevelNodeInPath(IUmbracoBuilder builder)
        => builder.Services.Configure<GlobalSettings>(config => config.HideTopLevelNodeFromPath = false);

    [SetUp]
    public async Task SetUpTest()
    {
        UmbracoContextFactory.EnsureUmbracoContext();
        SetRequestHost("localhost");

        if (_contentByName.Any())
        {
            // these tests all run on the same DB to make them run faster, so we need to get the cache in a
            // predictable state with each test run.
            RefreshContentCache();
            return;
        }

        await DocumentUrlService.InitAsync(true, CancellationToken.None);

        await GetRequiredService<ILanguageService>().CreateAsync(new Language("da-DK", "Danish"), Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("theContentType")
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new() { Alias = contentType.Alias, Key = contentType.Key }];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);
        foreach (var rootNumber in Enumerable.Range(1, 3))
        {
            var root = new ContentBuilder()
                .WithContentType(contentType)
                .WithCultureName("en-US", $"Root {rootNumber} en-US")
                .WithCultureName("da-DK", $"Root {rootNumber} da-DK")
                .Build();
            ContentService.Save(root);
            ContentService.Publish(root, ["*"]);
            _contentByName[$"Root {rootNumber}"] = root;

            foreach (var childNumber in Enumerable.Range(1, 3))
            {
                var child = new ContentBuilder()
                    .WithContentType(contentType)
                    .WithParent(root)
                    .WithCultureName("en-US", $"Child {childNumber} en-US")
                    .WithCultureName("da-DK", $"Child {childNumber} da-DK")
                    .Build();
                ContentService.Save(child);
                ContentService.Publish(child, ["*"]);
                _contentByName[$"Root {rootNumber}/Child {childNumber}"] = child;

                foreach (var grandchildNumber in Enumerable.Range(1, 3))
                {
                    var grandchild = new ContentBuilder()
                        .WithContentType(contentType)
                        .WithParent(child)
                        .WithCultureName("en-US", $"Grandchild {grandchildNumber} en-US")
                        .WithCultureName("da-DK", $"Grandchild {grandchildNumber} da-DK")
                        .Build();
                    ContentService.Save(grandchild);
                    ContentService.Publish(grandchild, ["*"]);
                    _contentByName[$"Root {rootNumber}/Child {childNumber}/Grandchild {grandchildNumber}"] = grandchild;
                }
            }
        }
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    public void First_Root_Without_StartItem(string culture)
    {
        Assert.IsEmpty(GetRequiredService<IHttpContextAccessor>().HttpContext!.Request.Headers["Start-Item"].ToString());
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath("/");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName["Root 1"].Key, content.Key);
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    [ConfigureBuilder(ActionName = nameof(ConfigureIncludeTopLevelNodeInPath))]
    public void First_Root_Without_StartItem_With_Top_Level_Node_Included(string culture)
    {
        Assert.IsEmpty(GetRequiredService<IHttpContextAccessor>().HttpContext!.Request.Headers["Start-Item"].ToString());
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath("/");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName["Root 1"].Key, content.Key);
    }

    [TestCase(1, "en-US")]
    [TestCase(2, "en-US")]
    [TestCase(3, "en-US")]
    [TestCase(1, "da-DK")]
    [TestCase(2, "da-DK")]
    [TestCase(3, "da-DK")]
    public void First_Root_Child_Without_StartItem(int child, string culture)
    {
        Assert.IsEmpty(GetRequiredService<IHttpContextAccessor>().HttpContext!.Request.Headers["Start-Item"].ToString());
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath($"/child-{child}-{culture.ToLowerInvariant()}");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root 1/Child {child}"].Key, content.Key);
    }

    [TestCase(1, 1, "en-US")]
    [TestCase(2, 2, "en-US")]
    [TestCase(3, 3, "en-US")]
    [TestCase(1, 2, "en-US")]
    [TestCase(2, 3, "en-US")]
    [TestCase(3, 1, "en-US")]
    [TestCase(1, 1, "da-DK")]
    [TestCase(2, 2, "da-DK")]
    [TestCase(3, 3, "da-DK")]
    [TestCase(1, 2, "da-DK")]
    [TestCase(2, 3, "da-DK")]
    [TestCase(3, 1, "da-DK")]
    public void First_Root_Grandchild_Without_StartItem(int child, int grandchild, string culture)
    {
        Assert.IsEmpty(GetRequiredService<IHttpContextAccessor>().HttpContext!.Request.Headers["Start-Item"].ToString());
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath($"/child-{child}-{culture.ToLowerInvariant()}/grandchild-{grandchild}-{culture.ToLowerInvariant()}");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root 1/Child {child}/Grandchild {grandchild}"].Key, content.Key);
    }

    [TestCase(1, "en-US")]
    [TestCase(2, "en-US")]
    [TestCase(3, "en-US")]
    [TestCase(1, "da-DK")]
    [TestCase(2, "da-DK")]
    [TestCase(3, "da-DK")]
    public void Root_With_StartItem(int root, string culture)
    {
        SetRequestStartItem($"root-{root}-{culture.ToLowerInvariant()}");
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath("/");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}"].Key, content.Key);
    }

    [TestCase(1, "en-US")]
    [TestCase(2, "en-US")]
    [TestCase(3, "en-US")]
    [TestCase(1, "da-DK")]
    [TestCase(2, "da-DK")]
    [TestCase(3, "da-DK")]
    [ConfigureBuilder(ActionName = nameof(ConfigureIncludeTopLevelNodeInPath))]
    public void Root_With_StartItem_With_Top_Level_Node_Included(int root, string culture)
    {
        SetRequestStartItem($"root-{root}-{culture.ToLowerInvariant()}");
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath("/");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}"].Key, content.Key);
    }

    [TestCase(1, 1, "en-US")]
    [TestCase(2, 2, "en-US")]
    [TestCase(3, 3, "en-US")]
    [TestCase(1, 2, "en-US")]
    [TestCase(2, 3, "en-US")]
    [TestCase(3, 1, "en-US")]
    [TestCase(1, 1, "da-DK")]
    [TestCase(2, 2, "da-DK")]
    [TestCase(3, 3, "da-DK")]
    [TestCase(1, 2, "da-DK")]
    [TestCase(2, 3, "da-DK")]
    [TestCase(3, 1, "da-DK")]
    public void Child_With_StartItem(int root, int child, string culture)
    {
        SetRequestStartItem($"root-{root}-{culture.ToLowerInvariant()}");
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath($"/child-{child}-{culture.ToLowerInvariant()}");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}/Child {child}"].Key, content.Key);
    }

    [TestCase(1, 1, "en-US")]
    [TestCase(2, 2, "en-US")]
    [TestCase(3, 3, "en-US")]
    [TestCase(1, 2, "en-US")]
    [TestCase(2, 3, "en-US")]
    [TestCase(3, 1, "en-US")]
    [TestCase(1, 1, "da-DK")]
    [TestCase(2, 2, "da-DK")]
    [TestCase(3, 3, "da-DK")]
    [TestCase(1, 2, "da-DK")]
    [TestCase(2, 3, "da-DK")]
    [TestCase(3, 1, "da-DK")]
    [ConfigureBuilder(ActionName = nameof(ConfigureIncludeTopLevelNodeInPath))]
    public void Child_With_StartItem_With_Top_Level_Node_Included(int root, int child, string culture)
    {
        SetRequestStartItem($"root-{root}-{culture.ToLowerInvariant()}");
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath($"/child-{child}-{culture.ToLowerInvariant()}");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}/Child {child}"].Key, content.Key);
    }

    [TestCase(1, 1, 1, "en-US")]
    [TestCase(2, 2, 2, "en-US")]
    [TestCase(3, 3, 3, "en-US")]
    [TestCase(1, 2, 3, "en-US")]
    [TestCase(2, 3, 1, "en-US")]
    [TestCase(3, 1, 2, "en-US")]
    [TestCase(1, 1, 1, "da-DK")]
    [TestCase(2, 2, 2, "da-DK")]
    [TestCase(3, 3, 3, "da-DK")]
    [TestCase(1, 2, 3, "da-DK")]
    [TestCase(2, 3, 1, "da-DK")]
    [TestCase(3, 1, 2, "da-DK")]
    public void Grandchild_With_StartItem(int root, int child, int grandchild, string culture)
    {
        SetRequestStartItem($"root-{root}-{culture.ToLowerInvariant()}");
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath($"/child-{child}-{culture.ToLowerInvariant()}/grandchild-{grandchild}-{culture.ToLowerInvariant()}");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}/Child {child}/Grandchild {grandchild}"].Key, content.Key);
    }

    [TestCase("/", 1, "en-US")]
    [TestCase("/root-2-en-us", 2, "en-US")]
    [TestCase("/root-3-en-us", 3, "en-US")]
    [TestCase("/", 1, "da-DK")]
    [TestCase("/root-2-da-dk", 2, "da-DK")]
    [TestCase("/root-3-da-dk", 3, "da-DK")]
    public void Root_By_Path_With_StartItem(string path, int root, string culture)
    {
        SetRequestStartItem($"root-{root}-{culture.ToLowerInvariant()}");
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath(path);
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}"].Key, content.Key);
    }

    [TestCase("/", 1, "en-US")]
    [TestCase("/root-2-en-us", 2, "en-US")]
    [TestCase("/root-3-en-us", 3, "en-US")]
    [TestCase("/", 1, "da-DK")]
    [TestCase("/root-2-da-dk", 2, "da-DK")]
    [TestCase("/root-3-da-dk", 3, "da-DK")]
    public void Root_By_Path_Without_StartItem(string path, int root, string culture)
    {
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath(path);
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}"].Key, content.Key);
    }

    [TestCase(1, "en-US")]
    [TestCase(2, "en-US")]
    [TestCase(3, "en-US")]
    [TestCase(1, "da-DK")]
    [TestCase(2, "da-DK")]
    [TestCase(3, "da-DK")]
    public void Root_With_Domain_Bindings(int root, string culture)
    {
        SetContentHost(_contentByName[$"Root {root}"], "some.host", "en-US");
        SetRequestHost("some.host");
        SetVariationContext(culture);

        var content = ApiContentPathResolver.ResolveContentPath("/");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}"].Key, content.Key);
    }

    [TestCase("/a", 1, "en-US")]
    [TestCase("/b", 1, "da-DK")]
    [TestCase("/123", 2, "en-US")]
    [TestCase("/456", 2, "da-DK")]
    [TestCase("/no-such-child", 3, "en-US")]
    [TestCase("/not-at-all", 3, "da-DK")]
    [TestCase("/a/b", 1, "en-US")]
    [TestCase("/c/d", 1, "da-DK")]
    [TestCase("/123/456", 2, "en-US")]
    [TestCase("/789/012", 2, "da-DK")]
    [TestCase("/no-such-child/no-such-grandchild", 3, "en-US")]
    [TestCase("/not-at-all/aint-no-way", 3, "da-DK")]
    public void Non_Existant_Descendant_By_Path_With_StartItem(string path, int root, string culture)
    {
        SetRequestStartItem($"root-{root}-{culture.ToLowerInvariant()}");

        var content = ApiContentPathResolver.ResolveContentPath(path);
        Assert.IsNull(content);
    }

    [TestCase("/a")]
    [TestCase("/123")]
    [TestCase("/a/b")]
    [TestCase("/123/456")]
    public void Non_Existant_Descendant_By_Path_Without_StartItem(string path)
    {
        var content = ApiContentPathResolver.ResolveContentPath(path);
        Assert.IsNull(content);
    }
}
