using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]

public class DocumentHybridCacheDocumentTypeTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IPublishedContentTypeCache PublishedContentTypeCache => GetRequiredService<IPublishedContentTypeCache>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [Test]
    public async Task Can_Get_Draft_Content_By_Id()
    {
        //Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);

        ContentType.RemovePropertyType("title");
        ContentTypeService.Save(ContentType);

        // Assert
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNull(newTextPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Draft_Content_By_Key()
    {
        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);

        ContentType.RemovePropertyType("title");
        ContentTypeService.Save(ContentType);
        //Assert
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);
        Assert.IsNull(newTextPage.Value("title"));
    }

    [Test]
    public async Task Content_Gets_Removed_When_DocumentType_Is_Deleted()
    {
        // Load into cache
        var textpage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, preview: true);
        Assert.IsNotNull(textpage);

        await ContentTypeService.DeleteAsync(textpage.ContentType.Key, Constants.Security.SuperUserKey);

        var textpageAgain = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, preview: true);
        Assert.IsNull(textpageAgain);
    }


    // TODO: Copy this into PublishedContentTypeCache
    [Test]
    public async Task Can_Get_Published_DocumentType_By_Key()
    {
        var contentType = PublishedContentTypeCache.Get(PublishedItemType.Content, Textpage.ContentTypeKey);
        Assert.IsNotNull(contentType);
        var contentTypeAgain = PublishedContentTypeCache.Get(PublishedItemType.Content, Textpage.ContentTypeKey);
        Assert.IsNotNull(contentType);
    }

    [Test]
    public async Task Published_DocumentType_Gets_Deleted()
    {
        var contentType = PublishedContentTypeCache.Get(PublishedItemType.Content, Textpage.ContentTypeKey);
        Assert.IsNotNull(contentType);

        await ContentTypeService.DeleteAsync(contentType.Key, Constants.Security.SuperUserKey);
        // PublishedContentTypeCache just explodes if it doesn't exist
        Assert.Catch(() => PublishedContentTypeCache.Get(PublishedItemType.Content, Textpage.ContentTypeKey));
    }
}
