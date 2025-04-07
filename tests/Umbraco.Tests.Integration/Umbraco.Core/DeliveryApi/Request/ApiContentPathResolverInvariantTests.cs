using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.Request;

public class ApiContentPathResolverInvariantTests : ApiContentPathResolverTestBase
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
    public void First_Root_Without_StartItem()
    {
        Assert.IsEmpty(GetRequiredService<IHttpContextAccessor>().HttpContext!.Request.Headers["Start-Item"].ToString());

        var content = ApiContentPathResolver.ResolveContentPath("/");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName["Root 1"].Key, content.Key);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureIncludeTopLevelNodeInPath))]
    public void First_Root_Without_StartItem_With_Top_Level_Node_Included()
    {
        Assert.IsEmpty(GetRequiredService<IHttpContextAccessor>().HttpContext!.Request.Headers["Start-Item"].ToString());

        var content = ApiContentPathResolver.ResolveContentPath("/");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName["Root 1"].Key, content.Key);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void First_Root_Child_Without_StartItem(int child)
    {
        Assert.IsEmpty(GetRequiredService<IHttpContextAccessor>().HttpContext!.Request.Headers["Start-Item"].ToString());

        var content = ApiContentPathResolver.ResolveContentPath($"/child-{child}");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root 1/Child {child}"].Key, content.Key);
    }

    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 3)]
    [TestCase(1, 2)]
    [TestCase(2, 3)]
    [TestCase(3, 1)]
    public void First_Root_Grandchild_Without_StartItem(int child, int grandchild)
    {
        Assert.IsEmpty(GetRequiredService<IHttpContextAccessor>().HttpContext!.Request.Headers["Start-Item"].ToString());

        var content = ApiContentPathResolver.ResolveContentPath($"/child-{child}/grandchild-{grandchild}");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root 1/Child {child}/Grandchild {grandchild}"].Key, content.Key);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Root_With_StartItem(int root)
    {
        SetRequestStartItem($"root-{root}");

        var content = ApiContentPathResolver.ResolveContentPath("/");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}"].Key, content.Key);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [ConfigureBuilder(ActionName = nameof(ConfigureIncludeTopLevelNodeInPath))]
    public void Root_With_StartItem_With_Top_Level_Node_Included(int root)
    {
        SetRequestStartItem($"root-{root}");

        var content = ApiContentPathResolver.ResolveContentPath("/");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}"].Key, content.Key);
    }

    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 3)]
    [TestCase(1, 2)]
    [TestCase(2, 3)]
    [TestCase(3, 1)]
    public void Child_With_StartItem(int root, int child)
    {
        SetRequestStartItem($"root-{root}");

        var content = ApiContentPathResolver.ResolveContentPath($"/child-{child}");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}/Child {child}"].Key, content.Key);
    }

    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 3)]
    [TestCase(1, 2)]
    [TestCase(2, 3)]
    [TestCase(3, 1)]
    [ConfigureBuilder(ActionName = nameof(ConfigureIncludeTopLevelNodeInPath))]
    public void Child_With_StartItem_With_Top_Level_Node_Included(int root, int child)
    {
        SetRequestStartItem($"root-{root}");

        var content = ApiContentPathResolver.ResolveContentPath($"/child-{child}");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}/Child {child}"].Key, content.Key);
    }

    [TestCase(1, 1, 1)]
    [TestCase(2, 2, 2)]
    [TestCase(3, 3, 3)]
    [TestCase(1, 2, 3)]
    [TestCase(2, 3, 1)]
    [TestCase(3, 1, 2)]
    public void Grandchild_With_StartItem(int root, int child, int grandchild)
    {
        SetRequestStartItem($"root-{root}");

        var content = ApiContentPathResolver.ResolveContentPath($"/child-{child}/grandchild-{grandchild}");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}/Child {child}/Grandchild {grandchild}"].Key, content.Key);
    }

    [TestCase("/", 1)]
    [TestCase("/root-2", 2)]
    [TestCase("/root-3", 3)]
    public void Root_By_Path_With_StartItem(string path, int root)
    {
        SetRequestStartItem($"root-{root}");

        var content = ApiContentPathResolver.ResolveContentPath(path);
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}"].Key, content.Key);
    }

    [TestCase("/", 1)]
    [TestCase("/root-2", 2)]
    [TestCase("/root-3", 3)]
    public void Root_By_Path_Without_StartItem(string path, int root)
    {
        var content = ApiContentPathResolver.ResolveContentPath(path);
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}"].Key, content.Key);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Root_With_Domain_Bindings(int root)
    {
        SetContentHost(_contentByName[$"Root {root}"], "some.host", "en-US");
        SetRequestHost("some.host");

        var content = ApiContentPathResolver.ResolveContentPath("/");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentByName[$"Root {root}"].Key, content.Key);
    }
}
