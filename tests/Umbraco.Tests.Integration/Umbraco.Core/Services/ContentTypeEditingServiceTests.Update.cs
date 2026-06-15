using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class ContentTypeEditingServiceTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Update_All_Basic_Settings(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        createModel.Description = "This is the Test description";
        createModel.Icon = "icon icon-something";
        createModel.AllowedAsRoot = true;
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test updated", "test", isElement: isElement);
        updateModel.Description = "This is the Test description updated";
        updateModel.Icon = "icon icon-something-updated";
        updateModel.AllowedAsRoot = false;

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.That(contentType, Is.Not.Null);

        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.Alias, Is.EqualTo("test"));
        Assert.That(contentType.Name, Is.EqualTo("Test updated"));
        Assert.That(contentType.Id, Is.EqualTo(result.Result.Id));
        Assert.That(contentType.Key, Is.EqualTo(result.Result.Key));
        Assert.That(contentType.Description, Is.EqualTo("This is the Test description updated"));
        Assert.That(contentType.Icon, Is.EqualTo("icon icon-something-updated"));
        Assert.That(contentType.AllowedAsRoot, Is.False);

        // expect RefreshOther when changing basic settings only
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Update_Alias(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test updated", "testUpdated", isElement: isElement);
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.That(contentType, Is.Not.Null);

        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.Alias, Is.EqualTo("testUpdated"));

        // expect AliasChanged (which includes RefreshMain) when changing alias
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.AliasChanged);
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task Can_Update_Variation(bool variesByCulture, bool variesBySegment)
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.VariesByCulture = variesByCulture;
        createModel.VariesBySegment = variesBySegment;

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.VariesByCulture = !variesByCulture;
        updateModel.VariesBySegment = !variesBySegment;

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.That(contentType, Is.Not.Null);

        Assert.That(contentType.VariesByCulture(), Is.EqualTo(!variesByCulture));
        Assert.That(contentType.VariesBySegment(), Is.EqualTo(!variesBySegment));

        // expect RefreshMain | VariationChanged when changing variation at content type level
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.VariationChanged);
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task Can_Update_Property_Variation(bool variesByCulture, bool variesBySegment)
    {
        var createModel = ContentTypeCreateModel("Test", "test");

        var container = ContentTypePropertyContainerModel();
        createModel.Containers = [container];

        var propertyTypeModel = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        createModel.Properties = [propertyTypeModel];

        createModel.VariesByCulture = variesByCulture;
        createModel.VariesBySegment = variesBySegment;

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;
        var propertyType = contentType.PropertyTypes.Single();
        Assert.Multiple(() =>
        {
            Assert.That(propertyType.Alias, Is.EqualTo("testProperty"));
            Assert.That(propertyType.VariesByNothing(), Is.True);
        });

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.VariesByCulture = variesByCulture;
        updateModel.VariesBySegment = variesBySegment;

        updateModel.Containers = [container];

        propertyTypeModel = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        propertyTypeModel.VariesByCulture = variesByCulture;
        propertyTypeModel.VariesBySegment = variesBySegment;
        updateModel.Properties = [propertyTypeModel];

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.That(contentType, Is.Not.Null);

        propertyType = contentType.PropertyTypes.Single();
        Assert.Multiple(() =>
        {
            Assert.That(propertyType.Alias, Is.EqualTo("testProperty"));
            Assert.That(propertyType.VariesByCulture(), Is.EqualTo(variesByCulture));
            Assert.That(propertyType.VariesBySegment(), Is.EqualTo(variesBySegment));
        });

        if (variesByCulture || variesBySegment)
        {
            // expect PropertyVariationChanged (which includes RefreshMain) when changing variation at property type level
            AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.PropertyVariationChanged);
        }
        else
        {
            // expect RefreshOther when not property level variation (in effect, no real changes were made here)
            AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
        }
    }

    [Test]
    public async Task Can_Add_Allowed_Types()
    {
        var allowedOne = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Allowed One", "allowedOne"), Constants.Security.SuperUserKey)).Result!;
        var allowedTwo = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Allowed Two", "allowedTwo"), Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedOne.Key, 10, allowedOne.Alias),
        };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedOne.Key, 10, allowedOne.Alias),
            new ContentTypeSort(allowedTwo.Key, 20, allowedTwo.Alias),
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.That(contentType, Is.Not.Null);

        var allowedContentTypes = contentType.AllowedContentTypes?.ToArray();
        Assert.That(allowedContentTypes, Is.Not.Null);
        Assert.That(allowedContentTypes, Has.Length.EqualTo(2));
        Assert.That(allowedContentTypes.Any(c => c.Key == allowedOne.Key && c.SortOrder == 0 && c.Alias == allowedOne.Alias), Is.True);
        Assert.That(allowedContentTypes.Any(c => c.Key == allowedTwo.Key && c.SortOrder == 1 && c.Alias == allowedTwo.Alias), Is.True);

        // expect RefreshOther when changing allowed types
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [Test]
    public async Task Can_Remove_Allowed_Types()
    {
        var allowedOne = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Allowed One", "allowedOne"), Constants.Security.SuperUserKey)).Result!;
        var allowedTwo = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Allowed Two", "allowedTwo"), Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedOne.Key, 10, allowedOne.Alias),
            new ContentTypeSort(allowedTwo.Key, 20, allowedTwo.Alias),
        };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.AllowedContentTypes = Array.Empty<ContentTypeSort>();

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.That(contentType, Is.Not.Null);

        var allowedContentTypes = contentType.AllowedContentTypes?.ToArray();
        Assert.That(allowedContentTypes, Is.Not.Null);
        Assert.That(allowedContentTypes, Is.Empty);

        // expect RefreshOther when changing allowed types
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [Test]
    public async Task Can_Rearrange_Allowed_Types()
    {
        var allowedOne = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Allowed One", "allowedOne"), Constants.Security.SuperUserKey)).Result!;
        var allowedTwo = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Allowed Two", "allowedTwo"), Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedOne.Key, 0, allowedOne.Alias),
            new ContentTypeSort(allowedTwo.Key, 1, allowedTwo.Alias),
        };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedOne.Key, 1, allowedOne.Alias),
            new ContentTypeSort(allowedTwo.Key, 0, allowedTwo.Alias),
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.That(contentType, Is.Not.Null);

        var allowedContentTypes = contentType.AllowedContentTypes?.ToArray();
        Assert.That(allowedContentTypes, Is.Not.Null);
        Assert.That(allowedContentTypes, Has.Length.EqualTo(2));
        Assert.That(allowedContentTypes.Any(c => c.Key == allowedOne.Key && c.SortOrder == 1 && c.Alias == allowedOne.Alias), Is.True);
        Assert.That(allowedContentTypes.Any(c => c.Key == allowedTwo.Key && c.SortOrder == 0 && c.Alias == allowedTwo.Alias), Is.True);

        // expect RefreshOther when only rearranging sort order (UnsortedSequenceEqual means AllowedContentTypes is not dirty)
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [Test]
    public async Task Can_Add_Self_To_Allowed_Types()
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;
        var id = contentType.Id;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(contentType.Key, 0, contentType.Alias)
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.That(contentType, Is.Not.Null);

        var allowedContentTypes = contentType.AllowedContentTypes?.ToArray();
        Assert.That(allowedContentTypes, Is.Not.Null);
        Assert.That(allowedContentTypes, Has.Length.EqualTo(1));
        Assert.That(allowedContentTypes.Any(c => c.Key == contentType.Key && c.SortOrder == 0 && c.Alias == contentType.Alias), Is.True);

        // expect RefreshOther when changing allowed types
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Add_Properties(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container = ContentTypePropertyContainerModel();
        createModel.Containers = new[] { container };

        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { propertyType };

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        updateModel.Containers = new[] { container };
        var newPropertyType = ContentTypePropertyTypeModel("Test Property 2", "testProperty2", containerKey: container.Key);
        newPropertyType.SortOrder = 0;
        propertyType.SortOrder = 1;
        updateModel.Properties = new[] { propertyType, newPropertyType };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(1));
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(2));
        Assert.That(contentType.PropertyGroups.First().PropertyTypes!, Has.Count.EqualTo(2));

        var allPropertyTypes = contentType.PropertyTypes.OrderBy(p => p.SortOrder).ToArray();
        Assert.That(allPropertyTypes.First().Alias, Is.EqualTo("testProperty2"));
        Assert.That(allPropertyTypes.Last().Alias, Is.EqualTo("testProperty"));

        var propertyTypesInContainer = contentType.PropertyGroups.First().PropertyTypes!.OrderBy(p => p.SortOrder).ToArray();
        Assert.That(propertyTypesInContainer.First().Alias, Is.EqualTo("testProperty2"));
        Assert.That(propertyTypesInContainer.Last().Alias, Is.EqualTo("testProperty"));

        Assert.That(contentType.NoGroupPropertyTypes, Is.Empty);

        // expect PropertyAdded (which includes RefreshOther) when adding properties
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.PropertyAdded);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Remove_Properties(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container = ContentTypePropertyContainerModel();
        createModel.Containers = new[] { container };

        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { propertyType };

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        updateModel.Containers = new[] { container };
        updateModel.Properties = Array.Empty<ContentTypePropertyTypeModel>();

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.PropertyGroups, Is.Empty);
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(0));

        Assert.That(contentType.NoGroupPropertyTypes.Count(), Is.EqualTo(0));

        // expect PropertyRemoved (which includes RefreshMain) when removing properties
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.PropertyRemoved);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Remove_Single_Property_From_Container(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container = ContentTypePropertyContainerModel();
        createModel.Containers = [container];

        createModel.Properties =
        [
            ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key),
            ContentTypePropertyTypeModel("Test Property 2", "testProperty2", containerKey: container.Key),
        ];

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(1));
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(2));

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        updateModel.Containers = [container];
        updateModel.Properties =
        [
            ContentTypePropertyTypeModel("Test Property 2", "testProperty2", containerKey: container.Key),
        ];

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(1));
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(1));
        Assert.That(contentType.PropertyTypes.Single().Alias, Is.EqualTo("testProperty2"));

        Assert.That(contentType.NoGroupPropertyTypes.Count(), Is.EqualTo(0));

        // expect PropertyRemoved (which includes RefreshMain) when removing properties
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.PropertyRemoved);
    }

    [Test]
    public async Task Can_Remove_Properties_Without_Container()
    {
        // Create a content type with a property that has no container (no tab/group).
        var createModel = ContentTypeCreateModel("Test", "test", isElement: true);
        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty");
        createModel.Properties = new[] { propertyType };

        var contentTypeCreateAttempt = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(contentTypeCreateAttempt.Success, Is.True);
        Assert.That(contentTypeCreateAttempt.Result, Is.Not.Null);

        // Verify the property was created without a container (should be in NoGroupPropertyTypes).
        var contentType = contentTypeCreateAttempt.Result;
        Assert.That(contentType.NoGroupPropertyTypes.Count(), Is.EqualTo(1));
        Assert.That(contentType.NoGroupPropertyTypes.Single().Alias, Is.EqualTo("testProperty"));
        Assert.That(contentType.PropertyGroups, Is.Empty);

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        // Update the content type removing the property.
        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: true);
        updateModel.Properties = Array.Empty<ContentTypePropertyTypeModel>();

        var contentTypeUpdateAttempt = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(contentTypeUpdateAttempt.Success, Is.True);
        Assert.That(contentTypeUpdateAttempt.Result, Is.Not.Null);

        // Ensure it's actually persisted - retrieve from database to verify deletion.
        contentType = await ContentTypeService.GetAsync(contentTypeUpdateAttempt.Result.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.IsElement, Is.True);
        Assert.That(contentType.PropertyGroups, Is.Empty);
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(0));
        Assert.That(contentType.NoGroupPropertyTypes.Count(), Is.EqualTo(0));

        // expect PropertyRemoved (which includes RefreshMain) when removing properties
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.PropertyRemoved);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Edit_Properties(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty");
        propertyType.Description = "The description";
        createModel.Properties = new[] { propertyType };

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;
        var originalPropertyTypeKey = contentType.PropertyTypes.First().Key;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        propertyType = ContentTypePropertyTypeModel("Test Property 2", "testProperty", key: originalPropertyTypeKey);
        propertyType.Description = "The updated description";
        updateModel.Properties = new[] { propertyType };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.PropertyGroups, Is.Empty);
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(1));

        var property = contentType.PropertyTypes.First();
        Assert.That(property.Name, Is.EqualTo("Test Property 2"));
        Assert.That(property.Alias, Is.EqualTo("testProperty"));
        Assert.That(property.Description, Is.EqualTo("The updated description"));
        Assert.That(property.Key, Is.EqualTo(originalPropertyTypeKey));

        Assert.That(contentType.NoGroupPropertyTypes.Count(), Is.EqualTo(1));

        // expect RefreshOther when changing basic property info (not alias and not variance)
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    public enum PropertyMoveOperation
    {
        ToEarlier,
        ToLater,
    }

    [TestCase(GroupContainerType, PropertyMoveOperation.ToEarlier, false)]
    [TestCase(TabContainerType, PropertyMoveOperation.ToEarlier, false)]
    [TestCase(GroupContainerType, PropertyMoveOperation.ToLater, false)]
    [TestCase(TabContainerType, PropertyMoveOperation.ToLater, false)]
    [TestCase(GroupContainerType, PropertyMoveOperation.ToEarlier, true)]
    [TestCase(TabContainerType, PropertyMoveOperation.ToEarlier, true)]
    [TestCase(GroupContainerType, PropertyMoveOperation.ToLater, true)]
    [TestCase(TabContainerType, PropertyMoveOperation.ToLater, true)]
    public async Task Can_Move_Properties_To_Another_Container(string containerType, PropertyMoveOperation propertyMoveOperation, bool isElement)
    {
        var container1 = ContentTypePropertyContainerModel($"{containerType} 1", containerType);
        var container2 = ContentTypePropertyContainerModel($"{containerType} 2", containerType);
        var propertyType1 = ContentTypePropertyTypeModel("Test Property 1", "testProperty1", containerKey: container1.Key);
        var propertyType2 = ContentTypePropertyTypeModel("Test Property 2", "testProperty2", containerKey: container1.Key);
        var propertyType3 = ContentTypePropertyTypeModel("Test Property 3", "testProperty3", containerKey: container2.Key);
        var propertyType4 = ContentTypePropertyTypeModel("Test Property 4", "testProperty4", containerKey: container2.Key);
        List<ContentTypePropertyTypeModel> properties = [propertyType1, propertyType2, propertyType3, propertyType4];
        var createModel = ContentTypeCreateModel(
            "Test Content Type",
            containers: [container1, container2],
            propertyTypes: properties,
            isElement: isElement);

        var createAttempt = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(createAttempt.Success, Is.True);
            Assert.That(createAttempt.Status, Is.EqualTo(ContentTypeOperationStatus.Success));
            Assert.That(createAttempt.Result, Is.Not.Null);

            Assert.That(createAttempt.Result.PropertyTypes.Count(), Is.EqualTo(4));
            Assert.That(createAttempt.Result.PropertyGroups, Has.Count.EqualTo(2));
            var createdContainer1 = createAttempt.Result.PropertyGroups.SingleOrDefault(x => x.Key == container1.Key);
            Assert.That(createdContainer1, Is.Not.Null);
            Assert.That(createdContainer1.PropertyTypes, Is.Not.Null);
            Assert.That(createdContainer1.PropertyTypes, Has.Count.EqualTo(2));
            Assert.That(createdContainer1.PropertyTypes[0].Name, Is.EqualTo("Test Property 1"));
            Assert.That(createdContainer1.PropertyTypes[1].Name, Is.EqualTo("Test Property 2"));
            var createdContainer2 = createAttempt.Result.PropertyGroups.SingleOrDefault(x => x.Key == container2.Key);
            Assert.That(createdContainer2, Is.Not.Null);
            Assert.That(createdContainer2.PropertyTypes, Is.Not.Null);
            Assert.That(createdContainer2.PropertyTypes?.Count, Is.EqualTo(2));
            Assert.That(createdContainer2.PropertyTypes[0].Name, Is.EqualTo("Test Property 3"));
            Assert.That(createdContainer2.PropertyTypes[1].Name, Is.EqualTo("Test Property 4"));
        });

        Content content = ContentBuilder.CreateBasicContent(createAttempt.Result!);
        foreach (var property in properties)
        {
            content.Properties[property.Alias]!.SetValue(property.Name);
        }

        ContentService.Save(content);

        if (propertyMoveOperation == PropertyMoveOperation.ToEarlier)
        {
            propertyType3.ContainerKey = container1.Key;
        }
        else
        {
            propertyType1.ContainerKey = container2.Key;
        }

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel(
            "Test Content Type",
            isElement: isElement,
            containers: [container1, container2],
            propertyTypes: [propertyType1, propertyType2, propertyType3, propertyType4]);
        var updateAttempt = await ContentTypeEditingService.UpdateAsync(createAttempt.Result, updateModel, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(updateAttempt.Success, Is.True);
            Assert.That(updateAttempt.Status, Is.EqualTo(ContentTypeOperationStatus.Success));
            Assert.That(updateAttempt.Result!.PropertyTypes.Count(), Is.EqualTo(4));
            Assert.That(updateAttempt.Result.PropertyGroups, Has.Count.EqualTo(2));

            var updatedContainer1 = updateAttempt.Result.PropertyGroups.SingleOrDefault(x => x.Key == container1.Key);
            Assert.That(updatedContainer1?.PropertyTypes, Is.Not.Null);
            var updatedContainer2 = updateAttempt.Result.PropertyGroups.SingleOrDefault(x => x.Key == container2.Key);
            Assert.That(updatedContainer2?.PropertyTypes, Is.Not.Null);
            if (propertyMoveOperation == PropertyMoveOperation.ToEarlier)
            {
                Assert.That(updatedContainer1.PropertyTypes, Has.Count.EqualTo(3));
                Assert.That(updatedContainer1.PropertyTypes[0].Name, Is.EqualTo("Test Property 1"));
                Assert.That(updatedContainer1.PropertyTypes[1].Name, Is.EqualTo("Test Property 2"));
                Assert.That(updatedContainer1.PropertyTypes[2].Name, Is.EqualTo("Test Property 3"));
                Assert.That(updatedContainer2.PropertyTypes, Has.Count.EqualTo(1));
                Assert.That(updatedContainer2.PropertyTypes[0].Name, Is.EqualTo("Test Property 4"));
            }
            else
            {
                Assert.That(updatedContainer1.PropertyTypes, Has.Count.EqualTo(1));
                Assert.That(updatedContainer1.PropertyTypes[0].Name, Is.EqualTo("Test Property 2"));
                Assert.That(updatedContainer2.PropertyTypes, Has.Count.EqualTo(3));
                Assert.That(updatedContainer2.PropertyTypes[0].Name, Is.EqualTo("Test Property 1"));
                Assert.That(updatedContainer2.PropertyTypes[1].Name, Is.EqualTo("Test Property 3"));
                Assert.That(updatedContainer2.PropertyTypes[2].Name, Is.EqualTo("Test Property 4"));
            }

            Assert.That(updateAttempt.Result.NoGroupPropertyTypes.Count(), Is.EqualTo(0));

            var updatedContent = ContentService.GetById(content.Id);
            foreach (var property in properties)
            {
                Assert.That(updatedContent?.Properties[property.Alias]?.GetValue(), Is.EqualTo(property.Name));
            }
        });

        // expect RefreshOther when changing moving properties around internally on the content type
        AssertContentTypeRefreshPayload(refreshedPayloads, createAttempt.Result.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Rearrange_Containers(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container1 = ContentTypePropertyContainerModel("One");
        container1.SortOrder = 0;
        var container2 = ContentTypePropertyContainerModel("Two");
        container2.SortOrder = 1;
        createModel.Containers = new[] { container1, container2 };

        var propertyType1 = ContentTypePropertyTypeModel("Test Property 1", "testProperty1", containerKey: container1.Key);
        var propertyType2 = ContentTypePropertyTypeModel("Test Property 2", "testProperty2", containerKey: container2.Key);
        createModel.Properties = new[] { propertyType1, propertyType2 };

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        container2.SortOrder = 0;
        container1.SortOrder = 1;
        updateModel.Containers = new[] { container1, container2 };
        updateModel.Properties = new[] { propertyType1, propertyType2 };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(2));

        var sortedPropertyGroups = contentType.PropertyGroups.OrderBy(g => g.SortOrder).ToArray();
        Assert.That(sortedPropertyGroups.First().PropertyTypes!.Single().Alias, Is.EqualTo("testProperty2"));
        Assert.That(sortedPropertyGroups.Last().PropertyTypes!.Single().Alias, Is.EqualTo("testProperty1"));

        // expect RefreshOther when only rearranging container sort order (PropertyGroups is not dirty for reordering)
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Make_Properties_Orphaned(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container1 = ContentTypePropertyContainerModel("One");
        var container2 = ContentTypePropertyContainerModel("Two");
        createModel.Containers = [container1, container2];

        var propertyType1 = ContentTypePropertyTypeModel("Test Property 1", "testProperty1", containerKey: container1.Key);
        var propertyType2 = ContentTypePropertyTypeModel("Test Property 2", "testProperty2", containerKey: container2.Key);
        createModel.Properties = [propertyType1, propertyType2];

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        updateModel.Containers = [container1];
        propertyType2.ContainerKey = null;
        updateModel.Properties = [propertyType1, propertyType2];

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(1));
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(2));
        Assert.That(contentType.PropertyGroups.First().PropertyTypes!, Has.Count.EqualTo(1));
        Assert.That(contentType.NoGroupPropertyTypes.Count(), Is.EqualTo(1));

        Assert.That(contentType.PropertyGroups.First().PropertyTypes!.Single().Alias, Is.EqualTo("testProperty1"));
        Assert.That(contentType.NoGroupPropertyTypes.Single().Alias, Is.EqualTo("testProperty2"));

        // expect RefreshOther when modifying container structure
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [Test]
    public async Task Can_Add_Compositions()
    {
        var propertyType1 = ContentTypePropertyTypeModel("Test Property 1", "testProperty1");
        var propertyType2 = ContentTypePropertyTypeModel("Test Property 2", "testProperty2");

        var compositionCreateModel = ContentTypeCreateModel("Composition", "composition");
        compositionCreateModel.Properties = new[] { propertyType1 };
        var compositionContentType = (await ContentTypeEditingService.CreateAsync(compositionCreateModel, Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Properties = new[] { propertyType2 };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Properties = new[] { propertyType2 };
        updateModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = CompositionType.Composition }
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.ContentTypeComposition.Count(), Is.EqualTo(1));
        Assert.That(contentType.ContentTypeComposition.Single().Key, Is.EqualTo(compositionContentType.Key));
        var propertyTypeAliases = contentType.CompositionPropertyTypes.Select(c => c.Alias).ToArray();
        Assert.That(propertyTypeAliases, Has.Length.EqualTo(2));
        Assert.That(propertyTypeAliases.Contains("testProperty1"), Is.True);
        Assert.That(propertyTypeAliases.Contains("testProperty2"), Is.True);

        // expect CompositionAdded (which includes RefreshOther) when adding compositions
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.CompositionAdded);
    }

    [Test]
    public async Task Can_Reapply_Compositions()
    {
        var propertyType1 = ContentTypePropertyTypeModel("Test Property 1", "testProperty1");
        var propertyType2 = ContentTypePropertyTypeModel("Test Property 2", "testProperty2");

        var compositionCreateModel = ContentTypeCreateModel("Composition", "composition");
        compositionCreateModel.Properties = new[] { propertyType1 };
        var compositionContentType = (await ContentTypeEditingService.CreateAsync(compositionCreateModel, Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Properties = new[] { propertyType2 };
        createModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = CompositionType.Composition }
        };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Properties = new[] { propertyType2 };
        updateModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = CompositionType.Composition }
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.ContentTypeComposition.Count(), Is.EqualTo(1));
        Assert.That(contentType.ContentTypeComposition.Single().Key, Is.EqualTo(compositionContentType.Key));
        var propertyTypeAliases = contentType.CompositionPropertyTypes.Select(c => c.Alias).ToArray();
        Assert.That(propertyTypeAliases, Has.Length.EqualTo(2));
        Assert.That(propertyTypeAliases.Contains("testProperty1"), Is.True);
        Assert.That(propertyTypeAliases.Contains("testProperty2"), Is.True);

        // expect RefreshOther when re-applying the same compositions (in principle, nothing changes)
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [Test]
    public async Task Can_Remove_Compositions()
    {
        var propertyType1 = ContentTypePropertyTypeModel("Test Property 1", "testProperty1");
        var propertyType2 = ContentTypePropertyTypeModel("Test Property 2", "testProperty2");

        var compositionCreateModel = ContentTypeCreateModel("Composition", "composition");
        compositionCreateModel.Properties = new[] { propertyType1 };
        var compositionContentType = (await ContentTypeEditingService.CreateAsync(compositionCreateModel, Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Properties = new[] { propertyType2 };
        createModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = CompositionType.Composition }
        };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Properties = new[] { propertyType2 };
        updateModel.Compositions = Array.Empty<Composition>();

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.ContentTypeComposition, Is.Empty);
        Assert.That(contentType.CompositionPropertyTypes.Count(), Is.EqualTo(1));
        Assert.That(contentType.CompositionPropertyTypes.Single().Alias, Is.EqualTo("testProperty2"));

        // expect CompositionRemoved (which includes RefreshMain) when removing compositions
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.CompositionRemoved);
    }

    [Test]
    public async Task Can_Reapply_Inheritance()
    {
        var parentContentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Parent"), Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }
            });

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;
        var originalPath = contentType.Path;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }
            });

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.ContentTypeComposition.Count(), Is.EqualTo(1));
        Assert.That(contentType.ParentId, Is.EqualTo(parentContentType.Id));
        Assert.That(contentType.Path, Is.EqualTo(originalPath));
        Assert.That(contentType.Path, Is.EqualTo($"-1,{parentContentType.Id},{contentType.Id}"));

        // expect RefreshOther when re-applying inheritance (in principle, nothing changes)
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [Test]
    public async Task Can_Update_History_Cleanup()
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Cleanup = new ContentTypeCleanup
        {
            PreventCleanup = true, KeepAllVersionsNewerThanDays = 123, KeepLatestVersionPerDayForDays = 456
        };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test updated", "test");
        updateModel.Cleanup = new ContentTypeCleanup
        {
            PreventCleanup = false, KeepAllVersionsNewerThanDays = 234, KeepLatestVersionPerDayForDays = 567
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.That(contentType, Is.Not.Null);

        Assert.That(contentType.HistoryCleanup, Is.Not.Null);
        Assert.That(contentType.HistoryCleanup.PreventCleanup, Is.False);
        Assert.That(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays, Is.EqualTo(234));
        Assert.That(contentType.HistoryCleanup.KeepLatestVersionPerDayForDays, Is.EqualTo(567));

        // expect RefreshOther when changing name.
        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [Test]
    public async Task Can_Reapply_Compositions_For_Content_Type_With_Children()
    {
        var compositionContentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Composition"), Constants.Security.SuperUserKey)).Result!;
        var parentContentType = (await ContentTypeEditingService.CreateAsync(
                ContentTypeCreateModel(
                    "Parent",
                    compositions: [new Composition { CompositionType = CompositionType.Composition, Key = compositionContentType.Key }]),
                Constants.Security.SuperUserKey)).Result!;
        var childContentType = (await ContentTypeEditingService.CreateAsync(
                ContentTypeCreateModel(
                    "Child",
                    compositions: [new Composition { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }]),
                Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel(
            "Parent Updated",
            alias: parentContentType.Alias,
            compositions: [new() { CompositionType = CompositionType.Composition, Key = compositionContentType.Key }]);

        var result = await ContentTypeEditingService.UpdateAsync(parentContentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        parentContentType = await ContentTypeService.GetAsync(parentContentType.Key);

        Assert.That(parentContentType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(parentContentType.Name, Is.EqualTo("Parent Updated"));
            Assert.That(parentContentType.ContentTypeComposition.Count(), Is.EqualTo(1));
            Assert.That(parentContentType.ContentTypeComposition.Single().Key, Is.EqualTo(compositionContentType.Key));
        });

        childContentType = await ContentTypeService.GetAsync(childContentType.Key);

        Assert.That(childContentType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(childContentType.Name, Is.EqualTo("Child"));
            Assert.That(childContentType.ContentTypeComposition.Count(), Is.EqualTo(1));
            Assert.That(childContentType.ContentTypeComposition.Single().Key, Is.EqualTo(parentContentType.Key));
        });

        // expect RefreshOther when re-applying compositions with a name change
        AssertContentTypeRefreshPayload(refreshedPayloads, parentContentType.Id, ContentTypeChangeTypes.RefreshOther);
    }

    [Test]
    public async Task Can_Remove_Compositions_For_Content_Type_With_Children()
    {
        var compositionContentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Composition"), Constants.Security.SuperUserKey)).Result!;
        var parentContentType = (await ContentTypeEditingService.CreateAsync(
                ContentTypeCreateModel(
                    "Parent",
                    compositions: [new Composition { CompositionType = CompositionType.Composition, Key = compositionContentType.Key }]),
                Constants.Security.SuperUserKey)).Result!;
        var childContentType = (await ContentTypeEditingService.CreateAsync(
                ContentTypeCreateModel(
                    "Child",
                    compositions: [new Composition { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }]),
                Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Parent Updated", alias: parentContentType.Alias, compositions: []);

        var result = await ContentTypeEditingService.UpdateAsync(parentContentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        parentContentType = await ContentTypeService.GetAsync(parentContentType.Key);

        Assert.That(parentContentType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(parentContentType.Name, Is.EqualTo("Parent Updated"));
            Assert.That(parentContentType.ContentTypeComposition, Is.Empty);
        });

        childContentType = await ContentTypeService.GetAsync(childContentType.Key);

        Assert.That(childContentType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(childContentType.Name, Is.EqualTo("Child"));
            Assert.That(childContentType.ContentTypeComposition.Count(), Is.EqualTo(1));
            Assert.That(childContentType.ContentTypeComposition.Single().Key, Is.EqualTo(parentContentType.Key));
        });

        // expect CompositionRemoved for the parent and RefreshMain for the child (affected through composition propagation)
        Assert.That(refreshedPayloads, Is.Not.Null);
        Assert.That(refreshedPayloads, Has.Length.EqualTo(2));
        AssertContentTypeRefreshPayload([refreshedPayloads.First()], parentContentType.Id, ContentTypeChangeTypes.CompositionRemoved);
        AssertContentTypeRefreshPayload([refreshedPayloads.Last()], childContentType.Id, ContentTypeChangeTypes.RefreshMain);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Cannot_Move_Properties_To_Non_Existing_Containers(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container = ContentTypePropertyContainerModel("One");
        createModel.Containers = new[] { container };

        var property = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { property };

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        property.ContainerKey = Guid.NewGuid();
        updateModel.Containers = new[] { container };
        updateModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.MissingContainer));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Cannot_Move_Containers_To_Non_Existing_Containers(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container = ContentTypePropertyContainerModel("One");
        createModel.Containers = new[] { container };

        var property = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { property };

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        container.ParentKey = Guid.NewGuid();
        updateModel.Containers = new[] { container };
        updateModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.MissingContainer));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Add_Self_As_Composition()
    {
        var property = ContentTypePropertyTypeModel("Test Property", "testProperty");
        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Properties = new[] { property };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Properties = new[] { property };
        updateModel.Compositions = new[]
        {
            new Composition { Key = contentType.Key, CompositionType = CompositionType.Composition }
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidComposition));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Change_Inheritance()
    {
        var parentContentType1 = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Parent1"), Constants.Security.SuperUserKey)).Result!;
        var parentContentType2 = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Parent2"), Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType1.Key }
            });

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType2.Key }
            });

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidInheritance));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Add_Inheritance()
    {
        var parentContentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Parent"), Constants.Security.SuperUserKey)).Result!;
        var contentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Child"), Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }
            });

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidInheritance));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Add_Multiple_Inheritance()
    {
        var parentContentType1 = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Parent1"), Constants.Security.SuperUserKey)).Result!;
        var parentContentType2 = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Parent2"), Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType1.Key }
            });

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType1.Key },
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType2.Key }
            });

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidInheritance));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Add_Self_As_Inheritance()
    {
        var createModel = ContentTypeCreateModel("Test", "test");

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Compositions = new Composition[]
        {
            new() { CompositionType = CompositionType.Inheritance, Key = contentType.Key }
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidInheritance));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Add_Inheritance_When_Created_In_A_Folder()
    {
        EntityContainer container = ContentTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), "Test folder").Result!.Entity;

        var parentContentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Parent"), Constants.Security.SuperUserKey)).Result!;
        var contentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Child", containerKey: container.Key), Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }
            });

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidInheritance));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [TestCase(CompositionType.Composition, CompositionType.Inheritance)]
    [TestCase(CompositionType.Inheritance, CompositionType.Composition)]
    public async Task Cannot_Change_Composition_To_Inheritance(CompositionType from, CompositionType to)
    {
        var compositionCreateModel = ContentTypeCreateModel("Composition", "composition");
        var compositionContentType = (await ContentTypeEditingService.CreateAsync(compositionCreateModel, Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = from }
        };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = to }
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidInheritance));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [TestCase("something")]
    [TestCase("tab")]
    [TestCase("group")]
    public async Task Cannot_Update_Container_Types_To_Unknown_Types(string containerType)
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        var container = ContentTypePropertyContainerModel("One");
        createModel.Containers = new[] { container };

        var property = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { property };

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        container.Type = containerType;
        updateModel.Containers = new[] { container };
        updateModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidContainerType));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }


    [Test]
    public async Task Cannot_Add_Compositions_For_Content_Type_With_Children()
    {
        var compositionContentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Composition"), Constants.Security.SuperUserKey)).Result!;
        var parentContentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Parent"), Constants.Security.SuperUserKey)).Result!;
        var childContentType = (await ContentTypeEditingService.CreateAsync(
                ContentTypeCreateModel(
                    "Child",
                    compositions: [new Composition { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }]),
                Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel(
            "Parent Updated",
            compositions: [new() { CompositionType = CompositionType.Composition, Key = compositionContentType.Key }]);

        var result = await ContentTypeEditingService.UpdateAsync(parentContentType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);

        // Ensure nothing was persisted
        parentContentType = await ContentTypeService.GetAsync(parentContentType.Key);

        Assert.That(parentContentType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(parentContentType.Name, Is.EqualTo("Parent"));
            Assert.That(parentContentType.ContentTypeComposition.Count(), Is.EqualTo(0));
        });

        childContentType = await ContentTypeService.GetAsync(childContentType.Key);

        Assert.That(childContentType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(childContentType.Name, Is.EqualTo("Child"));
            Assert.That(childContentType.ContentTypeComposition.Count(), Is.EqualTo(1));
            Assert.That(childContentType.ContentTypeComposition.Single().Key, Is.EqualTo(parentContentType.Key));
        });

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Add_Composition_With_Conflicting_Property_Type_Alias()
    {
        var targetContentTypePropertyType = ContentTypePropertyTypeModel("Test Property", "testProperty");
        var targetContentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel(
                "Target",
                propertyTypes: [targetContentTypePropertyType]),
            Constants.Security.SuperUserKey)).Result!;

        var compositionContentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel(
                "Composition",
                propertyTypes: [ContentTypePropertyTypeModel("Same Test Property Alias", "testProperty")]),
            Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel(
            "Target",
            propertyTypes: [targetContentTypePropertyType],
            compositions: [new() { CompositionType = CompositionType.Composition, Key = compositionContentType.Key }]);

        var result = await ContentTypeEditingService.UpdateAsync(targetContentType, updateModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.DuplicatePropertyTypeAlias));
        });

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Can_Add_Composition_With_Conflicting_Property_Type_Alias_When_The_Property_Type_Is_Removed_From_Target()
    {
        var targetContentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel(
                "Target",
                propertyTypes: [ContentTypePropertyTypeModel("Test Property", "testProperty")]),
            Constants.Security.SuperUserKey)).Result!;

        var compositionContentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel(
                "Composition",
                propertyTypes: [ContentTypePropertyTypeModel("Same Test Property Alias", "testProperty")]),
            Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel(
            "Target",
            propertyTypes: [],
            compositions: [new() { CompositionType = CompositionType.Composition, Key = compositionContentType.Key }]);

        var result = await ContentTypeEditingService.UpdateAsync(targetContentType, updateModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.Success));
        });

        // Verify the composition was added and the property from the composition is accessible.
        var updatedContentType = result.Result!;
        Assert.Multiple(() =>
        {
            Assert.That(updatedContentType.ContentTypeComposition.Count(), Is.EqualTo(1));
            Assert.That(updatedContentType.ContentTypeComposition.Single().Key, Is.EqualTo(compositionContentType.Key));

            // The property should come from the composition, not be a local property
            Assert.That(updatedContentType.PropertyTypes.Count(), Is.EqualTo(0));

            // The property from the composition should be accessible via CompositionPropertyTypes
            Assert.That(updatedContentType.CompositionPropertyTypes.Count(), Is.EqualTo(1));
            var compositionProperty = updatedContentType.CompositionPropertyTypes.Single();
            Assert.That(compositionProperty.Alias, Is.EqualTo("testProperty"));
            Assert.That(compositionProperty.Name, Is.EqualTo("Same Test Property Alias"));
        });

        // expect PropertyRemoved (which includes RefreshMain), because a property was removed to "make room" for the
        // new composition - AND CompositionAdded, because the composition was also added in the same operation
        AssertContentTypeRefreshPayload(refreshedPayloads, targetContentType.Id, ContentTypeChangeTypes.PropertyRemoved | ContentTypeChangeTypes.CompositionAdded);
    }

    private static void AssertContentTypeRefreshPayload(ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads, int expectedContentTypeId, ContentTypeChangeTypes expectedChangeTypes)
    {
        Assert.That(refreshedPayloads, Is.Not.Null);
        Assert.That(refreshedPayloads, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            var payload = refreshedPayloads.First();
            Assert.That(payload.Id, Is.EqualTo(expectedContentTypeId));
            Assert.That(payload.ChangeTypes, Is.EqualTo(expectedChangeTypes));
            Assert.That(payload.ItemType, Is.EqualTo(nameof(IContentType)));
        });
    }

    [Test]
    public async Task Cannot_Update_Element_Type_With_Segment_Variation()
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: true);
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: true);
        updateModel.VariesBySegment = true;

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidSegmentVariationForElementType));
    }

    [Test]
    public async Task Cannot_Switch_To_Element_Type_With_Existing_Segment_Variation()
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.VariesBySegment = true;
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: true);
        updateModel.VariesBySegment = true;

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidSegmentVariationForElementType));
    }

    [Test]
    public async Task Cannot_Switch_Document_To_Element_When_Content_Exists()
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.AllowedAsRoot = true;
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var content = ContentService.Create("Test Content", Constants.System.Root, contentType.Alias);
        var saveResult = ContentService.Save(content);
        Assert.That(saveResult.Success, Is.True);

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: true);
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidElementFlagDocumentHasContent));
    }

    [Test]
    public async Task Cannot_Switch_Element_To_Document_When_Element_Instances_Exist()
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: true);
        createModel.AllowedInLibrary = true;
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var element = new Element("Test Element", contentType);
        var saveResult = ElementService.Save(element);
        Assert.That(saveResult.Success, Is.True);

        var updateModel = ContentTypeUpdateModel("Test", "test");
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidElementFlagElementHasContent));
    }

    [Test]
    public async Task Cannot_Switch_Element_To_Document_When_Used_In_Block_Structure()
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: true);
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var dataType = DataTypeBuilder.CreateSimpleElementDataType(
            IOHelper,
            Constants.PropertyEditors.Aliases.BlockList,
            contentType.Key,
            elementSettingKey: null);
        var dataTypeResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.That(dataTypeResult.Success, Is.True);

        var updateModel = ContentTypeUpdateModel("Test", "test");
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidElementFlagElementIsUsedInPropertyEditorConfiguration));
    }
}
