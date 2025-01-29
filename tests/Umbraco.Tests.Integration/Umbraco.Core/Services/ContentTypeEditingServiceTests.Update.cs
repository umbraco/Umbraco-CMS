using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ContentTypeEditingServiceTests
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

        var updateModel = ContentTypeUpdateModel("Test updated", "testUpdated", isElement: isElement);
        updateModel.Description = "This is the Test description updated";
        updateModel.Icon = "icon icon-something-updated";
        updateModel.AllowedAsRoot = false;

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.IsNotNull(contentType);

        Assert.AreEqual(isElement, contentType.IsElement);
        Assert.AreEqual("testUpdated", contentType.Alias);
        Assert.AreEqual("Test updated", contentType.Name);
        Assert.AreEqual(result.Result.Id, contentType.Id);
        Assert.AreEqual(result.Result.Key, contentType.Key);
        Assert.AreEqual("This is the Test description updated", contentType.Description);
        Assert.AreEqual("icon icon-something-updated", contentType.Icon);
        Assert.IsFalse(contentType.AllowedAsRoot);
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

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.VariesByCulture = !variesByCulture;
        updateModel.VariesBySegment = !variesBySegment;

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.IsNotNull(contentType);

        Assert.AreEqual(!variesByCulture, contentType.VariesByCulture());
        Assert.AreEqual(!variesBySegment, contentType.VariesBySegment());
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

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedOne.Key, 10, allowedOne.Alias),
            new ContentTypeSort(allowedTwo.Key, 20, allowedTwo.Alias),
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(contentType);

        var allowedContentTypes = contentType.AllowedContentTypes?.ToArray();
        Assert.IsNotNull(allowedContentTypes);
        Assert.AreEqual(2, allowedContentTypes.Length);
        Assert.IsTrue(allowedContentTypes.Any(c => c.Key == allowedOne.Key && c.SortOrder == 0 && c.Alias == allowedOne.Alias));
        Assert.IsTrue(allowedContentTypes.Any(c => c.Key == allowedTwo.Key && c.SortOrder == 1 && c.Alias == allowedTwo.Alias));
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

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.AllowedContentTypes = Array.Empty<ContentTypeSort>();

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(contentType);

        var allowedContentTypes = contentType.AllowedContentTypes?.ToArray();
        Assert.IsNotNull(allowedContentTypes);
        Assert.AreEqual(0, allowedContentTypes.Length);
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

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedOne.Key, 1, allowedOne.Alias),
            new ContentTypeSort(allowedTwo.Key, 0, allowedTwo.Alias),
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(contentType);

        var allowedContentTypes = contentType.AllowedContentTypes?.ToArray();
        Assert.IsNotNull(allowedContentTypes);
        Assert.AreEqual(2, allowedContentTypes.Length);
        Assert.IsTrue(allowedContentTypes.Any(c => c.Key == allowedOne.Key && c.SortOrder == 1 && c.Alias == allowedOne.Alias));
        Assert.IsTrue(allowedContentTypes.Any(c => c.Key == allowedTwo.Key && c.SortOrder == 0 && c.Alias == allowedTwo.Alias));
    }

    [Test]
    public async Task Can_Add_Self_To_Allowed_Types()
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;
        var id = contentType.Id;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(contentType.Key, 0, contentType.Alias)
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(contentType);

        var allowedContentTypes = contentType.AllowedContentTypes?.ToArray();
        Assert.IsNotNull(allowedContentTypes);
        Assert.AreEqual(1, allowedContentTypes.Length);
        Assert.IsTrue(allowedContentTypes.Any(c => c.Key == contentType.Key && c.SortOrder == 0 && c.Alias == contentType.Alias));
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

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        updateModel.Containers = new[] { container };
        var newPropertyType = ContentTypePropertyTypeModel("Test Property 2", "testProperty2", containerKey: container.Key);
        newPropertyType.SortOrder = 0;
        propertyType.SortOrder = 1;
        updateModel.Properties = new[] { propertyType, newPropertyType };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(isElement, contentType.IsElement);
        Assert.AreEqual(1, contentType.PropertyGroups.Count);
        Assert.AreEqual(2, contentType.PropertyTypes.Count());
        Assert.AreEqual(2, contentType.PropertyGroups.First().PropertyTypes!.Count);

        var allPropertyTypes = contentType.PropertyTypes.OrderBy(p => p.SortOrder).ToArray();
        Assert.AreEqual("testProperty2", allPropertyTypes.First().Alias);
        Assert.AreEqual("testProperty", allPropertyTypes.Last().Alias);

        var propertyTypesInContainer = contentType.PropertyGroups.First().PropertyTypes!.OrderBy(p => p.SortOrder).ToArray();
        Assert.AreEqual("testProperty2", propertyTypesInContainer.First().Alias);
        Assert.AreEqual("testProperty", propertyTypesInContainer.Last().Alias);

        Assert.IsEmpty(contentType.NoGroupPropertyTypes);
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

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        updateModel.Containers = new[] { container };
        updateModel.Properties = Array.Empty<ContentTypePropertyTypeModel>();

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(isElement, contentType.IsElement);
        Assert.AreEqual(0, contentType.PropertyGroups.Count);
        Assert.AreEqual(0, contentType.PropertyTypes.Count());

        Assert.AreEqual(0, contentType.NoGroupPropertyTypes.Count());
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

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        propertyType = ContentTypePropertyTypeModel("Test Property 2", "testProperty", key: originalPropertyTypeKey);
        propertyType.Description = "The updated description";
        updateModel.Properties = new[] { propertyType };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(isElement, contentType.IsElement);
        Assert.AreEqual(0, contentType.PropertyGroups.Count);
        Assert.AreEqual(1, contentType.PropertyTypes.Count());

        var property = contentType.PropertyTypes.First();
        Assert.AreEqual("Test Property 2", property.Name);
        Assert.AreEqual("testProperty", property.Alias);
        Assert.AreEqual("The updated description", property.Description);
        Assert.AreEqual(originalPropertyTypeKey, property.Key);

        Assert.AreEqual(1, contentType.NoGroupPropertyTypes.Count());
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Move_Properties_To_Another_Container(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container1 = ContentTypePropertyContainerModel("One");
        createModel.Containers = new[] { container1 };

        var propertyType1 = ContentTypePropertyTypeModel("Test Property 1", "testProperty1", containerKey: container1.Key);
        var propertyType2 = ContentTypePropertyTypeModel("Test Property 2", "testProperty2", containerKey: container1.Key);
        createModel.Properties = new[] { propertyType1, propertyType2 };

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        var container2 = ContentTypePropertyContainerModel("Two");
        container2.SortOrder = 0;
        container1.SortOrder = 1;
        updateModel.Containers = new[] { container1, container2 };
        propertyType2.ContainerKey = container2.Key;
        updateModel.Properties = new[] { propertyType1, propertyType2 };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(isElement, contentType.IsElement);
        Assert.AreEqual(2, contentType.PropertyGroups.Count);
        Assert.AreEqual(2, contentType.PropertyTypes.Count());
        Assert.AreEqual(1, contentType.PropertyGroups.First().PropertyTypes!.Count);
        Assert.AreEqual(1, contentType.PropertyGroups.Last().PropertyTypes!.Count);
        Assert.IsEmpty(contentType.NoGroupPropertyTypes);

        var sortedPropertyGroups = contentType.PropertyGroups.OrderBy(g => g.SortOrder).ToArray();
        Assert.AreEqual("testProperty2", sortedPropertyGroups.First().PropertyTypes!.Single().Alias);
        Assert.AreEqual("testProperty1", sortedPropertyGroups.Last().PropertyTypes!.Single().Alias);
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

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        container2.SortOrder = 0;
        container1.SortOrder = 1;
        updateModel.Containers = new[] { container1, container2 };
        updateModel.Properties = new[] { propertyType1, propertyType2 };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(isElement, contentType.IsElement);
        Assert.AreEqual(2, contentType.PropertyGroups.Count);

        var sortedPropertyGroups = contentType.PropertyGroups.OrderBy(g => g.SortOrder).ToArray();
        Assert.AreEqual("testProperty2", sortedPropertyGroups.First().PropertyTypes!.Single().Alias);
        Assert.AreEqual("testProperty1", sortedPropertyGroups.Last().PropertyTypes!.Single().Alias);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Make_Properties_Orphaned(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container1 = ContentTypePropertyContainerModel("One");
        var container2 = ContentTypePropertyContainerModel("Two");
        createModel.Containers = new[] { container1, container2 };

        var propertyType1 = ContentTypePropertyTypeModel("Test Property 1", "testProperty1", containerKey: container1.Key);
        var propertyType2 = ContentTypePropertyTypeModel("Test Property 2", "testProperty2", containerKey: container2.Key);
        createModel.Properties = new[] { propertyType1, propertyType2 };

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        updateModel.Containers = new[] { container1 };
        propertyType2.ContainerKey = null;
        updateModel.Properties = new[] { propertyType1, propertyType2 };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(isElement, contentType.IsElement);
        Assert.AreEqual(1, contentType.PropertyGroups.Count);
        Assert.AreEqual(2, contentType.PropertyTypes.Count());
        Assert.AreEqual(1, contentType.PropertyGroups.First().PropertyTypes!.Count);
        Assert.AreEqual(1, contentType.NoGroupPropertyTypes.Count());

        Assert.AreEqual("testProperty1", contentType.PropertyGroups.First().PropertyTypes!.Single().Alias);
        Assert.AreEqual("testProperty2", contentType.NoGroupPropertyTypes.Single().Alias);
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

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Properties = new[] { propertyType2 };
        updateModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = CompositionType.Composition }
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(1, contentType.ContentTypeComposition.Count());
        Assert.AreEqual(compositionContentType.Key, contentType.ContentTypeComposition.Single().Key);
        var propertyTypeAliases = contentType.CompositionPropertyTypes.Select(c => c.Alias).ToArray();
        Assert.AreEqual(2, propertyTypeAliases.Length);
        Assert.IsTrue(propertyTypeAliases.Contains("testProperty1"));
        Assert.IsTrue(propertyTypeAliases.Contains("testProperty2"));
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

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Properties = new[] { propertyType2 };
        updateModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = CompositionType.Composition }
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(1, contentType.ContentTypeComposition.Count());
        Assert.AreEqual(compositionContentType.Key, contentType.ContentTypeComposition.Single().Key);
        var propertyTypeAliases = contentType.CompositionPropertyTypes.Select(c => c.Alias).ToArray();
        Assert.AreEqual(2, propertyTypeAliases.Length);
        Assert.IsTrue(propertyTypeAliases.Contains("testProperty1"));
        Assert.IsTrue(propertyTypeAliases.Contains("testProperty2"));
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

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Properties = new[] { propertyType2 };
        updateModel.Compositions = Array.Empty<Composition>();

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.IsEmpty(contentType.ContentTypeComposition);
        Assert.AreEqual(1, contentType.CompositionPropertyTypes.Count());
        Assert.AreEqual("testProperty2", contentType.CompositionPropertyTypes.Single().Alias);
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

        var updateModel = ContentTypeUpdateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }
            });

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(1, contentType.ContentTypeComposition.Count());
        Assert.AreEqual(parentContentType.Id, contentType.ParentId);
        Assert.AreEqual(originalPath, contentType.Path);
        Assert.AreEqual($"-1,{parentContentType.Id},{contentType.Id}", contentType.Path);
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

        var updateModel = ContentTypeUpdateModel("Test updated", "testUpdated");
        updateModel.Cleanup = new ContentTypeCleanup
        {
            PreventCleanup = false, KeepAllVersionsNewerThanDays = 234, KeepLatestVersionPerDayForDays = 567
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.IsNotNull(contentType);

        Assert.IsNotNull(contentType.HistoryCleanup);
        Assert.IsFalse(contentType.HistoryCleanup.PreventCleanup);
        Assert.AreEqual(234, contentType.HistoryCleanup.KeepAllVersionsNewerThanDays);
        Assert.AreEqual(567, contentType.HistoryCleanup.KeepLatestVersionPerDayForDays);
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

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        property.ContainerKey = Guid.NewGuid();
        updateModel.Containers = new[] { container };
        updateModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.MissingContainer, result.Status);
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

        var updateModel = ContentTypeUpdateModel("Test", "test", isElement: isElement);
        container.ParentKey = Guid.NewGuid();
        updateModel.Containers = new[] { container };
        updateModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.MissingContainer, result.Status);
    }

    [Test]
    public async Task Cannot_Add_Self_As_Composition()
    {
        var property = ContentTypePropertyTypeModel("Test Property", "testProperty");
        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Properties = new[] { property };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Properties = new[] { property };
        updateModel.Compositions = new[]
        {
            new Composition { Key = contentType.Key, CompositionType = CompositionType.Composition }
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidComposition, result.Status);
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

        var updateModel = ContentTypeUpdateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType2.Key }
            });

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidInheritance, result.Status);
    }

    [Test]
    public async Task Cannot_Add_Inheritance()
    {
        var parentContentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Parent"), Constants.Security.SuperUserKey)).Result!;
        var contentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Child"), Constants.Security.SuperUserKey)).Result!;

        var updateModel = ContentTypeUpdateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }
            });

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidInheritance, result.Status);
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

        var updateModel = ContentTypeUpdateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType1.Key },
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType2.Key }
            });

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidInheritance, result.Status);
    }

    [Test]
    public async Task Cannot_Add_Self_As_Inheritance()
    {
        var createModel = ContentTypeCreateModel("Test", "test");

        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Compositions = new Composition[]
        {
            new() { CompositionType = CompositionType.Inheritance, Key = contentType.Key }
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidInheritance, result.Status);
    }

    [Test]
    public async Task Cannot_Add_Inheritance_When_Created_In_A_Folder()
    {
        EntityContainer container = ContentTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), "Test folder").Result!.Entity;

        var parentContentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Parent"), Constants.Security.SuperUserKey)).Result!;
        var contentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Child", containerKey: container.Key), Constants.Security.SuperUserKey)).Result!;

        var updateModel = ContentTypeUpdateModel(
            "Child",
            compositions: new Composition[]
            {
                new() { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }
            });

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidInheritance, result.Status);
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

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = to }
        };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidInheritance, result.Status);
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

        var updateModel = ContentTypeUpdateModel("Test", "test");
        container.Type = containerType;
        updateModel.Containers = new[] { container };
        updateModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidContainerType, result.Status);
    }
}
