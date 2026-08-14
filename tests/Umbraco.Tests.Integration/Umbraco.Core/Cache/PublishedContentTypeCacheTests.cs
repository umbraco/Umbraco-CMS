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
    public async Task Can_Get_Updated_Published_ElementType_By_Alias_When_Looked_Up_As_Content()
    {
        // Arrange — create an element type and prime the cache via Get(Content, alias),
        // exercising the case where the requested itemType differs from the type's actual ItemType.
        IContentType element = await CreateElementTypeAsync("myElementContent");

        IPublishedContentType initial = PublishedContentTypeCache.Get(PublishedItemType.Content, element.Alias);
        Assert.AreEqual(1, initial.PropertyTypes.Count());

        // Act — add a property and save; the save should invalidate the primed cache entry.
        await AddPropertyAndSaveAsync(element, "extra");
        IPublishedContentType updated = PublishedContentTypeCache.Get(PublishedItemType.Content, element.Alias);

        // Assert
        Assert.AreEqual(2, updated.PropertyTypes.Count());
    }

    [Test]
    public async Task Can_Get_Updated_Published_ElementType_By_Alias_When_Looked_Up_As_Element()
    {
        // Arrange — create an element type and prime the cache via Get(Element, alias),
        // the natural lookup shape for an element type.
        IContentType element = await CreateElementTypeAsync("myElementElement");

        IPublishedContentType initial = PublishedContentTypeCache.Get(PublishedItemType.Element, element.Alias);
        Assert.AreEqual(1, initial.PropertyTypes.Count());

        // Act — add a property and save; the save should invalidate the primed cache entry, and
        // the next Get must successfully resolve an element type via the alias overload.
        await AddPropertyAndSaveAsync(element, "extra");
        IPublishedContentType updated = PublishedContentTypeCache.Get(PublishedItemType.Element, element.Alias);

        // Assert
        Assert.AreEqual(2, updated.PropertyTypes.Count());
    }

    [Test]
    public async Task Can_Get_Published_ElementType_By_Id_When_Looked_Up_As_Element()
    {
        // Arrange — element type fetched on a cold cache via the int overload, exercising
        // Element handling in Get(PublishedItemType, int).
        IContentType element = await CreateElementTypeAsync("myElementById");

        // Act
        IPublishedContentType byId = PublishedContentTypeCache.Get(PublishedItemType.Element, element.Id);

        // Assert
        Assert.IsNotNull(byId);
        Assert.AreEqual(PublishedItemType.Element, byId.ItemType);
        Assert.AreEqual(element.Alias, byId.Alias);
    }

    [Test]
    public async Task ClearByDataTypeId_Removes_Element_Type_Looked_Up_As_Content()
    {
        // Arrange — element type whose property uses a data type, primed via Get(Content, alias)
        // so the alias-keyed entry uses the requested itemType prefix rather than the type's own.
        IContentType element = await CreateElementTypeAsync("myDataTypeElement");
        int dataTypeId = element.PropertyTypes.First().DataTypeId;

        IPublishedContentType primed = PublishedContentTypeCache.Get(PublishedItemType.Content, element.Alias);
        Assert.IsNotNull(primed);

        // Act — clearing by data-type id must remove every alias-keyed entry pointing at the
        // affected content type, regardless of which itemType prefix was used to insert it.
        PublishedContentTypeCache.ClearByDataTypeId(dataTypeId);
        IPublishedContentType after = PublishedContentTypeCache.Get(PublishedItemType.Content, element.Alias);

        // Assert — a fresh instance signals the cache was invalidated rather than re-served,
        // and the freshly loaded type should be structurally equivalent to the one that was cached.
        Assert.IsFalse(ReferenceEquals(primed, after));
        Assert.AreEqual(primed.PropertyTypes.Count(), after.PropertyTypes.Count());
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
