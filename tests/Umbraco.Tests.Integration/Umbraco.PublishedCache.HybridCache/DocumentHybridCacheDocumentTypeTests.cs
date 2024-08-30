using NUnit.Framework;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[Ignore("These tests does not work yet")]

public class DocumentHybridCacheDocumentTypeTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    [Test]
    public async Task Can_Get_Draft_Content_By_Id()
    {
        //Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);

        var newContentType = ContentTypeBuilder.CreateBasicContentType();
        newContentType.Key = ContentType.Key;
        newContentType.Id = ContentType.Id;
        newContentType.Alias = ContentType.Alias;
        newContentType.DefaultTemplateId = ContentType.DefaultTemplateId;
        ContentTypeService.Save(newContentType);

        // Assert
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNull(newTextPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Draft_Content_By_Key()
    {
        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);

        var newContentType = ContentTypeBuilder.CreateBasicContentType();
        newContentType.Key = ContentType.Key;
        newContentType.Id = ContentType.Id;
        newContentType.Alias = ContentType.Alias;
        newContentType.DefaultTemplateId = ContentType.DefaultTemplateId;
        ContentTypeService.Save(newContentType);

        //Assert
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);
        Assert.IsNull(newTextPage.Value("title"));
    }
}
