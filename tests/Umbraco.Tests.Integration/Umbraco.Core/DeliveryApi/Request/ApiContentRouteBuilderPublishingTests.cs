using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.Request;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ApiContentRouteBuilderPublishingTests : ApiContentRouteBuilderTestBase
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Handle_Broken_Variant_Publish_Path(bool breakPublishedPath)
    {
        SetRequestHost("localhost");

        await GetRequiredService<ILanguageService>().CreateAsync(new Language("da-DK", "Danish"), Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("theContentType")
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new() { Alias = contentType.Alias, Key = contentType.Key }];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        var root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", $"Root en-US")
            .WithCultureName("da-DK", $"Root da-DK")
            .Build();
        ContentService.Save(root);
        ContentService.Publish(root, ["*"]);

        var child = new ContentBuilder()
            .WithContentType(contentType)
            .WithParent(root)
            .WithCultureName("en-US", $"Child en-US")
            .WithCultureName("da-DK", $"Child da-DK")
            .Build();
        ContentService.Save(child);
        ContentService.Publish(child, ["*"]);

        var grandchild = new ContentBuilder()
            .WithContentType(contentType)
            .WithParent(child)
            .WithCultureName("en-US", $"Grandchild en-US")
            .WithCultureName("da-DK", $"Grandchild da-DK")
            .Build();
        ContentService.Save(grandchild);
        ContentService.Publish(grandchild, ["*"]);

        if (breakPublishedPath)
        {
            ContentService.Unpublish(child, "da-DK");
        }

        SetVariationContext("en-US");
        var publishedContent = GetPublishedContent(grandchild.Key);
        var route = ApiContentRouteBuilder.Build(publishedContent);
        Assert.IsNotNull(route);
        Assert.AreEqual("/child-en-us/grandchild-en-us/", route.Path);

        SetVariationContext("da-DK");
        publishedContent = GetPublishedContent(grandchild.Key);
        route = ApiContentRouteBuilder.Build(publishedContent);

        if (breakPublishedPath)
        {
            Assert.IsNull(route);
        }
        else
        {
            Assert.IsNotNull(route);
            Assert.AreEqual("/child-da-dk/grandchild-da-dk/", route.Path);
        }
    }
}
