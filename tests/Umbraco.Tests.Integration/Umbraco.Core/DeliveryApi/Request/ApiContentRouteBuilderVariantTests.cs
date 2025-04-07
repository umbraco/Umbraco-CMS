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

public class ApiContentRouteBuilderVariantTests : ApiContentRouteBuilderTestBase
{
    private Dictionary<string, IContent> _contentByName = new ();

    public static void ConfigureIncludeTopLevelNodeInPath(IUmbracoBuilder builder)
        => builder.Services.Configure<GlobalSettings>(config => config.HideTopLevelNodeFromPath = false);

    [SetUp]
    public async Task SetUpTest()
    {
        SetRequestHost("localhost");

        if (_contentByName.Any())
        {
            // these tests all run on the same DB to make them run faster, so we need to get the cache in a
            // predictable state with each test run.
            RefreshContentCache();
            return;
        }

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

    [TestCase("da-DK")]
    [TestCase("en-US")]
    public void First_Root(string culture)
    {
        SetVariationContext(culture);
        var publishedContent = GetPublishedContent(_contentByName[$"Root 1"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/", route.Path);
            Assert.AreEqual($"root-1-{culture.ToLowerInvariant()}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root 1"].Key, route.StartItem.Id);
        });
    }

    [TestCase("da-DK")]
    [TestCase("en-US")]
    public void Last_Root(string culture)
    {
        SetVariationContext(culture);
        var publishedContent = GetPublishedContent(_contentByName["Root 3"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual($"/root-3-{culture.ToLowerInvariant()}/", route.Path);
            Assert.AreEqual($"root-3-{culture.ToLowerInvariant()}", route.StartItem.Path);
            Assert.AreEqual(_contentByName["Root 3"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1, "en-US")]
    [TestCase(1, "da-DK")]
    [TestCase(2, "en-US")]
    [TestCase(2, "da-DK")]
    [TestCase(3, "en-US")]
    [TestCase(3, "da-DK")]
    public void First_Child(int root, string culture)
    {
        SetVariationContext(culture);
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}/Child 1"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual($"/child-1-{culture.ToLowerInvariant()}/", route.Path);
            Assert.AreEqual($"root-{root}-{culture.ToLowerInvariant()}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1, "en-US")]
    [TestCase(1, "da-DK")]
    [TestCase(2, "en-US")]
    [TestCase(2, "da-DK")]
    [TestCase(3, "en-US")]
    [TestCase(3, "da-DK")]
    public void Last_Child(int root, string culture)
    {
        SetVariationContext(culture);
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}/Child 3"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual($"/child-3-{culture.ToLowerInvariant()}/", route.Path);
            Assert.AreEqual($"root-{root}-{culture.ToLowerInvariant()}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1, "en-US")]
    [TestCase(1, "da-DK")]
    [TestCase(2, "en-US")]
    [TestCase(2, "da-DK")]
    [TestCase(3, "en-US")]
    [TestCase(3, "da-DK")]
    public void First_Grandchild(int root, string culture)
    {
        SetVariationContext(culture);
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}/Child 1/Grandchild 1"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual($"/child-1-{culture.ToLowerInvariant()}/grandchild-1-{culture.ToLowerInvariant()}/", route.Path);
            Assert.AreEqual($"root-{root}-{culture.ToLowerInvariant()}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1, "en-US")]
    [TestCase(1, "da-DK")]
    [TestCase(2, "en-US")]
    [TestCase(2, "da-DK")]
    [TestCase(3, "en-US")]
    [TestCase(3, "da-DK")]
    public void Last_Grandchild(int root, string culture)
    {
        SetVariationContext(culture);
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}/Child 3/Grandchild 3"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual($"/child-3-{culture.ToLowerInvariant()}/grandchild-3-{culture.ToLowerInvariant()}/", route.Path);
            Assert.AreEqual($"root-{root}-{culture.ToLowerInvariant()}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1, "en-US")]
    [TestCase(1, "da-DK")]
    [TestCase(2, "en-US")]
    [TestCase(2, "da-DK")]
    [TestCase(3, "en-US")]
    [TestCase(3, "da-DK")]
    [ConfigureBuilder(ActionName = nameof(ConfigureIncludeTopLevelNodeInPath))]
    public void Root_With_Top_Level_Node_Included(int root, string culture)
    {
        SetVariationContext(culture);
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/", route.Path);
            Assert.AreEqual($"root-{root}-{culture.ToLowerInvariant()}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1, "en-US")]
    [TestCase(1, "da-DK")]
    [TestCase(2, "en-US")]
    [TestCase(2, "da-DK")]
    [TestCase(3, "en-US")]
    [TestCase(3, "da-DK")]
    public void Root_With_Domain_Bindings(int root, string culture)
    {
        SetContentHost(_contentByName[$"Root {root}"], "some.host", culture);
        SetRequestHost("some.host");
        SetVariationContext(culture);
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/", route.Path);
            Assert.AreEqual($"root-{root}-{culture.ToLowerInvariant()}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }
}
