using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Integration tests for granular <see cref="ContentTypeChangeTypes"/> flags emitted
///     by content type editing operations.
/// </summary>
internal sealed partial class ContentTypeEditingServiceTests
{
    [Test]
    public async Task Change_Alias_Emits_AliasChanged()
    {
        var contentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel("Test", "test"), Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "testRenamed");
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(1, refreshedPayloads!.Length);
        var payload = refreshedPayloads.First();
        Assert.Multiple(() =>
        {
            Assert.IsTrue(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.AliasChanged), "Expected AliasChanged flag");
            Assert.IsTrue(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "AliasChanged should include RefreshMain");
        });
    }

    [Test]
    public async Task Change_Property_Alias_Via_EditingService_Emits_PropertyRemoved()
    {
        // NOTE: The editing service looks up existing properties by alias, so changing a property alias
        // results in the old property being removed and a new one being created. This triggers
        // PropertyRemoved rather than PropertyAliasChanged. The PropertyAliasChanged flag would fire
        // when a property type's alias is changed in-place via the lower-level ContentTypeService.
        var container = ContentTypePropertyContainerModel();
        var propertyType = ContentTypePropertyTypeModel("Title", "title", containerKey: container.Key);
        var contentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel("Test", "test", propertyTypes: [propertyType], containers: [container]),
            Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        // Update the property with a different alias but same key — the editing service treats this
        // as removing the old property and adding a new one
        var updatedPropertyType = ContentTypePropertyTypeModel("Title", "titleRenamed", key: propertyType.Key, containerKey: container.Key);
        var updateModel = ContentTypeUpdateModel("Test", "test", propertyTypes: [updatedPropertyType], containers: [container]);
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(1, refreshedPayloads!.Length);
        var payload = refreshedPayloads.First();
        Assert.Multiple(() =>
        {
            Assert.IsTrue(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.PropertyRemoved), "Expected PropertyRemoved flag (old property removed)");
            Assert.IsTrue(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "PropertyRemoved should include RefreshMain");
        });
    }

    [Test]
    public async Task Change_Property_Alias_InPlace_Emits_PropertyAliasChanged()
    {
        // Test via the lower-level ContentTypeService where the property alias is changed in-place
        var container = ContentTypePropertyContainerModel();
        var propertyType = ContentTypePropertyTypeModel("Title", "title", containerKey: container.Key);
        var contentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel("Test", "test", propertyTypes: [propertyType], containers: [container]),
            Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        // Modify the alias in-place on the existing property type object
        var existingProperty = contentType.PropertyTypes.Single();
        existingProperty.Alias = "titleRenamed";
        ContentTypeService.Save(contentType);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(1, refreshedPayloads!.Length);
        var payload = refreshedPayloads.First();
        Assert.Multiple(() =>
        {
            Assert.IsTrue(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.PropertyAliasChanged), "Expected PropertyAliasChanged flag");
            Assert.IsTrue(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "PropertyAliasChanged should include RefreshMain");
        });
    }

    [Test]
    public async Task Remove_Property_Emits_PropertyRemoved()
    {
        var container = ContentTypePropertyContainerModel();
        var propertyType = ContentTypePropertyTypeModel("Title", "title", containerKey: container.Key);
        var contentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel("Test", "test", propertyTypes: [propertyType], containers: [container]),
            Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test", propertyTypes: [], containers: [container]);
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(1, refreshedPayloads!.Length);
        var payload = refreshedPayloads.First();
        Assert.Multiple(() =>
        {
            Assert.IsTrue(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.PropertyRemoved), "Expected PropertyRemoved flag");
            Assert.IsTrue(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "PropertyRemoved should include RefreshMain");
            Assert.IsFalse(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.AliasChanged), "Should not have AliasChanged");
        });
    }

    [Test]
    public async Task Remove_Composition_Emits_CompositionRemoved()
    {
        var compositionProperty = ContentTypePropertyTypeModel("Comp Property", "compProperty");
        var compositionType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel("Composition", "composition", propertyTypes: [compositionProperty]),
            Constants.Security.SuperUserKey)).Result!;

        var ownProperty = ContentTypePropertyTypeModel("Own Property", "ownProperty");
        var contentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel(
                "Test",
                "test",
                propertyTypes: [ownProperty],
                compositions: [new Composition { Key = compositionType.Key, CompositionType = CompositionType.Composition }]),
            Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test", propertyTypes: [ownProperty], compositions: []);
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(1, refreshedPayloads!.Length);
        var payload = refreshedPayloads.First();
        Assert.Multiple(() =>
        {
            Assert.IsTrue(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.CompositionRemoved), "Expected CompositionRemoved flag");
            Assert.IsTrue(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "CompositionRemoved should include RefreshMain");
            Assert.IsFalse(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.PropertyRemoved), "Should not have PropertyRemoved");
        });
    }

    [Test]
    public async Task Change_Property_Variation_Emits_PropertyVariationChanged()
    {
        var container = ContentTypePropertyContainerModel();
        var propertyType = ContentTypePropertyTypeModel("Title", "title", containerKey: container.Key);

        var createModel = ContentTypeCreateModel("Test", "test", propertyTypes: [propertyType], containers: [container]);
        createModel.VariesByCulture = true;
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        // Change the property to vary by culture
        var updatedPropertyType = ContentTypePropertyTypeModel("Title", "title", containerKey: container.Key);
        updatedPropertyType.VariesByCulture = true;
        var updateModel = ContentTypeUpdateModel("Test", "test", propertyTypes: [updatedPropertyType], containers: [container]);
        updateModel.VariesByCulture = true;
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(1, refreshedPayloads!.Length);
        var payload = refreshedPayloads.First();
        Assert.Multiple(() =>
        {
            Assert.IsTrue(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.PropertyVariationChanged), "Expected PropertyVariationChanged flag");
            Assert.IsTrue(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "PropertyVariationChanged should include RefreshMain");
            Assert.IsFalse(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.VariationChanged), "Should not have VariationChanged (content type variation did not change)");
        });
    }

    [Test]
    public async Task Change_ContentType_Variation_Emits_VariationChanged_And_RefreshMain()
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.VariesByCulture = false;
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.VariesByCulture = true;
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(1, refreshedPayloads!.Length);
        var payload = refreshedPayloads.First();
        Assert.Multiple(() =>
        {
            Assert.IsTrue(payload.ChangeTypes.HasType(ContentTypeChangeTypes.VariationChanged), "Expected VariationChanged flag");
            Assert.IsTrue(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "Should include RefreshMain");
        });
    }

    [Test]
    public async Task Multiple_Structural_Changes_Emit_Combined_Flags()
    {
        var container = ContentTypePropertyContainerModel();
        var propertyType = ContentTypePropertyTypeModel("Title", "title", containerKey: container.Key);
        var contentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel("Test", "test", propertyTypes: [propertyType], containers: [container]),
            Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        // Change the alias AND remove the property in the same update
        var updateModel = ContentTypeUpdateModel("Test", "testRenamed", propertyTypes: [], containers: [container]);
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(1, refreshedPayloads!.Length);
        var payload = refreshedPayloads.First();
        Assert.Multiple(() =>
        {
            Assert.IsTrue(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.AliasChanged), "Expected AliasChanged flag");
            Assert.IsTrue(payload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.PropertyRemoved), "Expected PropertyRemoved flag");
            Assert.IsTrue(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "Should include RefreshMain");
        });
    }

    [Test]
    public async Task Non_Structural_Change_Emits_RefreshOther_Without_RefreshMain()
    {
        var contentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel("Test", "test"), Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        // Change only basic metadata (name, description, icon)
        var updateModel = ContentTypeUpdateModel("Test Updated", "test");
        updateModel.Description = "Updated description";
        updateModel.Icon = "icon icon-updated";
        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(1, refreshedPayloads!.Length);
        var payload = refreshedPayloads.First();
        Assert.Multiple(() =>
        {
            Assert.IsTrue(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshOther), "Should include RefreshOther");
            Assert.IsFalse(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "Should NOT include RefreshMain");
        });
    }

    [Test]
    public async Task Create_Emits_Create_Flag_Only()
    {
        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var result = await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel("Test", "test"), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(1, refreshedPayloads!.Length);
        var payload = refreshedPayloads.First();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentTypeChangeTypes.Create, payload.ChangeTypes);
            Assert.IsFalse(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "Create should not include RefreshMain");
            Assert.IsFalse(payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshOther), "Create should not include RefreshOther");
        });
    }

    [Test]
    public async Task Remove_Composition_From_Parent_Propagates_RefreshMain_To_Child()
    {
        var compositionProperty = ContentTypePropertyTypeModel("Comp Property", "compProperty");
        var compositionType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel("Composition", "composition", propertyTypes: [compositionProperty]),
            Constants.Security.SuperUserKey)).Result!;

        var parentType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel(
                "Parent",
                "parent",
                compositions: [new Composition { Key = compositionType.Key, CompositionType = CompositionType.Composition }]),
            Constants.Security.SuperUserKey)).Result!;

        var childType = (await ContentTypeEditingService.CreateAsync(
            ContentTypeCreateModel(
                "Child",
                "child",
                compositions: [new Composition { Key = parentType.Key, CompositionType = CompositionType.Inheritance }]),
            Constants.Security.SuperUserKey)).Result!;

        ContentTypeCacheRefresher.JsonPayload[]? refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        // Remove the composition from the parent
        var updateModel = ContentTypeUpdateModel("Parent", "parent", compositions: []);
        var result = await ContentTypeEditingService.UpdateAsync(parentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(refreshedPayloads);
        Assert.AreEqual(2, refreshedPayloads!.Length);

        // Parent should have CompositionRemoved
        var parentPayload = refreshedPayloads.Single(p => p.Id == parentType.Id);
        Assert.IsTrue(parentPayload.ChangeTypes.HasTypesAll(ContentTypeChangeTypes.CompositionRemoved), "Parent should have CompositionRemoved");

        // Child should have RefreshMain (propagated from parent's structural change)
        var childPayload = refreshedPayloads.Single(p => p.Id == childType.Id);
        Assert.IsTrue(childPayload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain), "Child should have RefreshMain (propagated)");
    }
}
