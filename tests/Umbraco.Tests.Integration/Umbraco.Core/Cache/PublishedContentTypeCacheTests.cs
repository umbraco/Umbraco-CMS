using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PublishedContentTypeCacheTests : UmbracoIntegrationTestWithContentEditing
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

    [Test]
    public async Task Can_Get_Updated_Published_ElementType_By_Alias_Via_Content()
    {
        // Arrange — create an element type, prime the cache via Get(Content, alias).
        IContentType element = await CreateElementTypeAsync("myElementContent");

        IPublishedContentType initial = PublishedContentTypeCache.Get(PublishedItemType.Content, element.Alias);
        Assert.AreEqual(1, initial.PropertyTypes.Count());

        // Act — add another property and save.
        await AddPropertyAndSaveAsync(element, "extra");

        IPublishedContentType updated = PublishedContentTypeCache.Get(PublishedItemType.Content, element.Alias);

        // Assert — element types looked up via Content must invalidate on save.
        Assert.AreEqual(2, updated.PropertyTypes.Count());
    }

    [Test]
    public async Task Can_Get_Updated_Published_ElementType_By_Alias_Via_Element()
    {
        // Arrange — create an element type, prime the cache via Get(Element, alias).
        IContentType element = await CreateElementTypeAsync("myElementElement");

        IPublishedContentType initial = PublishedContentTypeCache.Get(PublishedItemType.Element, element.Alias);
        Assert.AreEqual(1, initial.PropertyTypes.Count());

        // Act — add another property and save (also exercises CreatePublishedContentType(string)
        // for PublishedItemType.Element on the post-clear cache miss).
        await AddPropertyAndSaveAsync(element, "extra");

        IPublishedContentType updated = PublishedContentTypeCache.Get(PublishedItemType.Element, element.Alias);

        Assert.AreEqual(2, updated.PropertyTypes.Count());
    }

    private async Task<IContentType> CreateElementTypeAsync(string alias)
    {
        ContentTypeCreateModel createModel = ContentTypeEditingBuilder.CreateElementType(alias, alias);
        var attempt = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success);
        return attempt.Result!;
    }

    private async Task AddPropertyAndSaveAsync(IContentType element, string newPropertyAlias)
    {
        ContentTypeUpdateModel updateModel = ContentTypeUpdateHelper.CreateContentTypeUpdateModel(element);
        var properties = updateModel.Properties.ToList();
        properties.Add(new ContentTypePropertyTypeModel
        {
            Alias = newPropertyAlias,
            Name = newPropertyAlias,
            DataTypeKey = Constants.DataTypes.Guids.TextstringGuid,
            ContainerKey = updateModel.Containers.First().Key,
        });
        updateModel.Properties = properties;

        var updateAttempt = await ContentTypeEditingService.UpdateAsync(element, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(updateAttempt.Success);
    }
}
