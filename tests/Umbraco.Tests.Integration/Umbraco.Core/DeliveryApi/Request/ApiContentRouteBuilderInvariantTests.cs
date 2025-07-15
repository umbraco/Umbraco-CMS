using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.Request;

public class ApiContentRouteBuilderInvariantTests : ApiContentRouteBuilderTestBase
{
    private readonly Dictionary<string, IContent> _contentByName = new ();

    public static void ConfigureIncludeTopLevelNodeInPath(IUmbracoBuilder builder)
        => builder.Services.Configure<GlobalSettings>(config => config.HideTopLevelNodeFromPath = false);

    public static void ConfigureOmitTrailingSlash(IUmbracoBuilder builder)
        => builder.Services.Configure<RequestHandlerSettings>(config => config.AddTrailingSlash = false);

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

        var contentType = new ContentTypeBuilder()
            .WithAlias("theContentType")
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new() { Alias = contentType.Alias, Key = contentType.Key }];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);
        foreach (var rootNumber in Enumerable.Range(1, 3))
        {
            var root = new ContentBuilder()
                .WithContentType(contentType)
                .WithName($"Root {rootNumber}")
                .Build();
            ContentService.Save(root);
            ContentService.Publish(root, ["*"]);
            _contentByName[root.Name!] = root;

            foreach (var childNumber in Enumerable.Range(1, 3))
            {
                var child = new ContentBuilder()
                    .WithContentType(contentType)
                    .WithParent(root)
                    .WithName($"Child {childNumber}")
                    .Build();
                ContentService.Save(child);
                ContentService.Publish(child, ["*"]);
                _contentByName[$"{root.Name!}/{child.Name!}"] = child;

                foreach (var grandchildNumber in Enumerable.Range(1, 3))
                {
                    var grandchild = new ContentBuilder()
                        .WithContentType(contentType)
                        .WithParent(child)
                        .WithName($"Grandchild {grandchildNumber}")
                        .Build();
                    ContentService.Save(grandchild);
                    ContentService.Publish(grandchild, ["*"]);
                    _contentByName[$"{root.Name!}/{child.Name!}/{grandchild.Name!}"] = grandchild;
                }
            }
        }
    }

    [Test]
    public void First_Root()
    {
        var publishedContent = GetPublishedContent(_contentByName["Root 1"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/", route.Path);
            Assert.AreEqual("root-1", route.StartItem.Path);
            Assert.AreEqual(_contentByName["Root 1"].Key, route.StartItem.Id);
        });
    }

    [Test]
    public void Last_Root()
    {
        var publishedContent = GetPublishedContent(_contentByName["Root 3"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/root-3/", route.Path);
            Assert.AreEqual("root-3", route.StartItem.Path);
            Assert.AreEqual(_contentByName["Root 3"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void First_Child(int root)
    {
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}/Child 1"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/child-1/", route.Path);
            Assert.AreEqual($"root-{root}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Last_Child(int root)
    {
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}/Child 3"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/child-3/", route.Path);
            Assert.AreEqual($"root-{root}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void First_Grandchild(int root)
    {
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}/Child 1/Grandchild 1"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/child-1/grandchild-1/", route.Path);
            Assert.AreEqual($"root-{root}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Last_Grandchild(int root)
    {
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}/Child 3/Grandchild 3"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/child-3/grandchild-3/", route.Path);
            Assert.AreEqual($"root-{root}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [ConfigureBuilder(ActionName = nameof(ConfigureIncludeTopLevelNodeInPath))]
    public void Root_With_Top_Level_Node_Included(int root)
    {
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/", route.Path);
            Assert.AreEqual($"root-{root}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task Root_With_Domain_Bindings(int root)
    {
        await SetContentHost(_contentByName[$"Root {root}"], "some.host", "en-US");
        SetRequestHost("some.host");

        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("/", route.Path);
            Assert.AreEqual($"root-{root}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1, "/")]
    [TestCase(2, "/root-2")]
    [TestCase(3, "/root-3")]
    [ConfigureBuilder(ActionName = nameof(ConfigureOmitTrailingSlash))]
    public void Root_Without_Trailing_Slash(int root, string expectedPath)
    {
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(expectedPath, route.Path);
            Assert.AreEqual($"root-{root}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }

    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 3)]
    [TestCase(1, 2)]
    [TestCase(2, 3)]
    [TestCase(3, 1)]
    [ConfigureBuilder(ActionName = nameof(ConfigureOmitTrailingSlash))]
    public void Child_Without_Trailing_Slash(int root, int child)
    {
        var publishedContent = GetPublishedContent(_contentByName[$"Root {root}/Child {child}"].Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.Multiple(() =>
        {
            Assert.AreEqual($"/child-{child}", route.Path);
            Assert.AreEqual($"root-{root}", route.StartItem.Path);
            Assert.AreEqual(_contentByName[$"Root {root}"].Key, route.StartItem.Id);
        });
    }
}
