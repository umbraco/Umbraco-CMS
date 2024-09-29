using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[Platform("Linux", Reason = "This uses too much memory when running both caches, should be removed when nuchache is removed")]
public class DocumentHybridCacheDocumentTypeTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [Test]
    public async Task Can_Get_Draft_Content_By_Id()
    {
        // Act
        await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);

        ContentType.RemovePropertyType("title");
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNull(newTextPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Draft_Content_By_Key()
    {
        // Act
        await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);

        ContentType.RemovePropertyType("title");
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);
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
}
