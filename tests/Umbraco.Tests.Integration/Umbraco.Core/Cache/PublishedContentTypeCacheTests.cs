using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class PublishedContentTypeCacheTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedContentTypeCache PublishedContentTypeCache => GetRequiredService<IPublishedContentTypeCache>();
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [Test]
    public async Task Can_Get_Published_DocumentType_By_Key()
    {
        // Act
        var contentType = PublishedContentTypeCache.Get(PublishedItemType.Content, ContentType.Key);

        // Assert
        Assert.IsNotNull(contentType);
    }

    [Test]
    public async Task Can_Get_Updated_Published_DocumentType_By_Key()
    {
        // Arrange
        var contentType = PublishedContentTypeCache.Get(PublishedItemType.Content, Textpage.ContentTypeKey);
        Assert.IsNotNull(contentType);
        Assert.AreEqual(1, ContentType.PropertyTypes.Count());
        // Update the content type
        var updateModel = ContentTypeUpdateHelper.CreateContentTypeUpdateModel(ContentType);
        updateModel.Properties = new List<ContentTypePropertyTypeModel>();
        await ContentTypeEditingService.UpdateAsync(ContentType, updateModel, Constants.Security.SuperUserKey);

        // Act
        var updatedContentType = PublishedContentTypeCache.Get(PublishedItemType.Content, ContentType.Key);

        // Assert
        Assert.IsNotNull(updatedContentType);
        Assert.AreEqual(0, updatedContentType.PropertyTypes.Count());
    }

    [Test]
    public async Task Published_DocumentType_Gets_Deleted()
    {
        var contentType = PublishedContentTypeCache.Get(PublishedItemType.Content, ContentType.Key);
        Assert.IsNotNull(contentType);

        await ContentTypeService.DeleteAsync(contentType.Key, Constants.Security.SuperUserKey);
        Assert.Catch(() => PublishedContentTypeCache.Get(PublishedItemType.Content, ContentType.Key));
    }
}
