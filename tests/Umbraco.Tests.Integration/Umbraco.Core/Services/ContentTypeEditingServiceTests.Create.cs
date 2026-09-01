using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class ContentTypeEditingServiceTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Create_With_All_Basic_Settings(bool isElement)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        createModel.Description = "This is the Test description";
        createModel.Icon = "icon icon-something";
        createModel.AllowedAsRoot = true;

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.That(contentType, Is.Not.Null);

        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.Alias, Is.EqualTo("test"));
        Assert.That(contentType.Name, Is.EqualTo("Test"));
        Assert.That(contentType.Id, Is.EqualTo(result.Result.Id));
        Assert.That(contentType.Key, Is.EqualTo(result.Result.Key));
        Assert.That(contentType.Description, Is.EqualTo("This is the Test description"));
        Assert.That(contentType.Icon, Is.EqualTo("icon icon-something"));
        Assert.That(contentType.AllowedAsRoot, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task Can_Create_With_Variation(bool variesByCulture, bool variesBySegment)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.VariesByCulture = variesByCulture;
        createModel.VariesBySegment = variesBySegment;

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.That(contentType, Is.Not.Null);

        Assert.That(contentType.VariesByCulture(), Is.EqualTo(variesByCulture));
        Assert.That(contentType.VariesBySegment(), Is.EqualTo(variesBySegment));

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_In_A_Folder(bool isElement)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var containerResult = ContentTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), "Test folder");
        Assert.That(containerResult.Success, Is.True);
        var container = containerResult.Result?.Entity;
        Assert.That(container, Is.Not.Null);

        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement, containerKey: container.Key);
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted in the folder
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.ParentId, Is.EqualTo(container.Id));
        Assert.That(contentType.IsElement, Is.EqualTo(isElement));

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Create_With_Properties_In_A_Container(bool isElement)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container = ContentTypePropertyContainerModel();
        createModel.Containers = new[] { container };

        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { propertyType };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(1));
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(1));
        Assert.That(contentType.PropertyGroups.First().PropertyTypes!, Has.Count.EqualTo(1));
        Assert.That(contentType.PropertyTypes.First().Alias, Is.EqualTo("testProperty"));
        Assert.That(contentType.PropertyGroups.First().PropertyTypes!.First().Alias, Is.EqualTo("testProperty"));
        Assert.That(contentType.NoGroupPropertyTypes, Is.Empty);

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Create_With_Orphaned_Properties(bool isElement)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);

        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty");
        createModel.Properties = new[] { propertyType };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.IsElement, Is.EqualTo(isElement));
        Assert.That(contentType.PropertyGroups, Is.Empty);
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(1));
        Assert.That(contentType.PropertyTypes.First().Alias, Is.EqualTo("testProperty"));
        Assert.That(contentType.NoGroupPropertyTypes.Count(), Is.EqualTo(1));
        Assert.That(contentType.NoGroupPropertyTypes.First().Alias, Is.EqualTo("testProperty"));

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [Test]
    public async Task Can_Specify_Key()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var key = new Guid("33C326F6-CB5E-43D6-9730-E946AA5F9C7B");
        var createModel = ContentTypeCreateModel(key: key);

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(contentType, Is.Not.Null);
            Assert.That(contentType.Key, Is.EqualTo(key));
        });

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [Test]
    public async Task Can_Specify_PropertyType_Key()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var propertyTypeKey = new Guid("82DDEBD8-D2CA-423E-B88D-6890F26152B4");

        var propertyTypeContainer = ContentTypePropertyContainerModel();
        var propertyTypeCreateModel = ContentTypePropertyTypeModel(key: propertyTypeKey, containerKey: propertyTypeContainer.Key);

        var createModel = ContentTypeCreateModel(
            propertyTypes: new[] { propertyTypeCreateModel },
            containers: new[] { propertyTypeContainer });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(contentType, Is.Not.Null);
            var propertyType = contentType.PropertyGroups.FirstOrDefault()?.PropertyTypes?.FirstOrDefault();
            Assert.That(propertyType, Is.Not.Null);
            Assert.That(propertyType.Key, Is.EqualTo(propertyTypeKey));
        });

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [Test]
    public async Task Can_Assign_Allowed_Types()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var allowedOne = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Allowed One", "allowedOne"), Constants.Security.SuperUserKey)).Result;
        var allowedTwo = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Allowed Two", "allowedTwo"), Constants.Security.SuperUserKey)).Result;
        Assert.That(allowedOne, Is.Not.Null);
        Assert.That(allowedTwo, Is.Not.Null);

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedOne.Key, 10, allowedOne.Alias),
            new ContentTypeSort(allowedTwo.Key, 20, allowedTwo.Alias),
        };
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        var contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.That(contentType, Is.Not.Null);

        var allowedContentTypes = contentType.AllowedContentTypes?.ToArray();
        Assert.That(allowedContentTypes, Is.Not.Null);
        Assert.That(allowedContentTypes, Has.Length.EqualTo(2));
        Assert.That(allowedContentTypes.Any(c => c.Key == allowedOne.Key && c.SortOrder == 0 && c.Alias == allowedOne.Alias), Is.True);
        Assert.That(allowedContentTypes.Any(c => c.Key == allowedTwo.Key && c.SortOrder == 1 && c.Alias == allowedTwo.Alias), Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [Test]
    public async Task Can_Assign_History_Cleanup()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Cleanup = new ContentTypeCleanup
        {
            PreventCleanup = true, KeepAllVersionsNewerThanDays = 123, KeepLatestVersionPerDayForDays = 456
        };
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        var contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.That(contentType, Is.Not.Null);
        Assert.That(contentType.HistoryCleanup, Is.Not.Null);
        Assert.That(contentType.HistoryCleanup.PreventCleanup, Is.True);
        Assert.That(contentType.HistoryCleanup.KeepAllVersionsNewerThanDays, Is.EqualTo(123));
        Assert.That(contentType.HistoryCleanup.KeepLatestVersionPerDayForDays, Is.EqualTo(456));

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [Test]
    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, false)]
    // Wondering where the last case is? Look at the test below.
    public async Task Can_Create_Composite(bool compositionIsElement, bool contentTypeIsElement)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase",
            isElement: compositionIsElement);

        // Let's add a property to ensure that it passes through
        var container = ContentTypePropertyContainerModel();
        compositionBase.Containers = new[] { container };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: container.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel(
            isElement: contentTypeIsElement,
            compositions: new[]
            {
                new Composition
                {
                CompositionType = CompositionType.Composition,
                Key = compositionType.Key,
                },
            });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.That(contentType.ContentTypeComposition.Count(), Is.EqualTo(1));
            Assert.That(contentType.ContentTypeComposition.First().Key, Is.EqualTo(compositionType.Key));
            Assert.That(compositionType.CompositionPropertyGroups.Count(), Is.EqualTo(1));
            Assert.That(compositionType.CompositionPropertyGroups.First().Key, Is.EqualTo(container.Key));
            Assert.That(compositionType.CompositionPropertyTypes.Count(), Is.EqualTo(1));
            Assert.That(compositionType.CompositionPropertyTypes.First().Key, Is.EqualTo(compositionProperty.Key));
        });

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [Test]
    public async Task Can_Create_Property_Container_Structure_Matching_Composition_Container_Structure()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        // Let's add a property to ensure that it passes through
        var compositionTab = ContentTypePropertyContainerModel("Composition Tab", type: TabContainerType);
        var compositionGroup = ContentTypePropertyContainerModel("Composition Group", type: GroupContainerType);
        compositionGroup.ParentKey = compositionTab.Key;
        compositionBase.Containers = new[] { compositionTab, compositionGroup };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionGroup.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition { CompositionType = CompositionType.Composition, Key = compositionType.Key, },
            });

        var tab = ContentTypePropertyContainerModel("Composition Tab", type: TabContainerType);
        var group = ContentTypePropertyContainerModel("Composition Group", type: GroupContainerType);
        group.ParentKey = tab.Key;
        createModel.Containers = new[] { tab, group };

        var property = ContentTypePropertyTypeModel("My Property", "myProperty", containerKey: group.Key);
        createModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(2));
        var contentTypeTab = contentType.PropertyGroups.First(g => g.Name == "Composition Tab");
        Assert.That(contentTypeTab.Key, Is.EqualTo(tab.Key));
        Assert.That(contentTypeTab.Type, Is.EqualTo(PropertyGroupType.Tab));
        var contentTypeGroup = contentType.PropertyGroups.First(g => g.Name == "Composition Group");
        Assert.That(contentTypeGroup.Key, Is.EqualTo(group.Key));
        Assert.That(contentTypeGroup.Type, Is.EqualTo(PropertyGroupType.Group));
        var propertyTypeKeys = contentType.CompositionPropertyTypes.Select(t => t.Key).ToArray();
        Assert.That(propertyTypeKeys, Has.Length.EqualTo(2));
        Assert.That(propertyTypeKeys.Contains(compositionProperty.Key), Is.True);
        Assert.That(propertyTypeKeys.Contains(property.Key), Is.True);
        Assert.That(contentTypeGroup.PropertyTypes?.Contains("myProperty"), Is.True);
        Assert.That(contentTypeGroup.PropertyTypes?.Contains("compositionProperty"), Is.False);

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [Test]
    public async Task Property_Container_Aliases_Are_CamelCased_Names()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var createModel = ContentTypeCreateModel("Test", "test");
        var tab = ContentTypePropertyContainerModel("My Tab", type: TabContainerType);
        var group1 = ContentTypePropertyContainerModel("My Group", type: GroupContainerType);
        group1.ParentKey = tab.Key;
        var group2 = ContentTypePropertyContainerModel("AnotherGroup", type: GroupContainerType);
        createModel.Containers = new[] { tab, group1, group2 };
        var property = ContentTypePropertyTypeModel("My Property", "myProperty", containerKey: group1.Key);
        // assign some properties to the groups to make sure they're not cleaned out as "empty groups"
        createModel.Properties = new[]
        {
            ContentTypePropertyTypeModel("My Property 1", "myProperty1", containerKey: group1.Key),
            ContentTypePropertyTypeModel("My Property 2", "myProperty2", containerKey: group2.Key)
        };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(3));
        Assert.That(contentType.PropertyGroups.First(g => g.Name == "My Tab").Alias, Is.EqualTo("myTab"));
        Assert.That(contentType.PropertyGroups.First(g => g.Name == "My Group").Alias, Is.EqualTo("myTab/myGroup"));
        Assert.That(contentType.PropertyGroups.First(g => g.Name == "AnotherGroup").Alias, Is.EqualTo("anotherGroup"));

        AssertContentTypeRefreshPayload(refreshedPayloads, contentType.Id, ContentTypeChangeTypes.Create);
    }

    [Test]
    public async Task Element_Types_Must_Not_Be_Composed_By_non_element_type()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        // This is a pretty interesting one, since it actually seems to be broken in the old backoffice,
        // since the client will always send the isElement flag as false to the GetAvailableCompositeContentTypes endpoint
        // Even if it's an element type, however if we look at the comment in GetAvailableCompositeContentTypes
        // We see that element types are not suppose to be allowed to be composed by non-element types.
        // Since this breaks models builder evidently.
        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            isElement: false);

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        AssertContentTypeRefreshPayload(refreshedPayloads, compositionType.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var createModel = ContentTypeCreateModel(
            "Content Type Using Composition",
            compositions: new[]
            {
                new Composition
                {
                    CompositionType = CompositionType.Composition,
                    Key = compositionType.Key,
                },
            },
            isElement: true);

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidComposition));
            Assert.That(result.Result, Is.Null);
        });

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task ContentType_Containing_Composition_Cannot_Be_Used_As_Composition()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionBase = ContentTypeCreateModel("CompositionBase");

        var baseResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(baseResult.Success, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, baseResult.Result!.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var composition = ContentTypeCreateModel(
            "Composition",
            compositions: new[]
            {
                new Composition
                {
                    CompositionType = CompositionType.Composition, Key = baseResult.Result!.Key
                }
            });

        var compositionResult = await ContentTypeEditingService.CreateAsync(composition, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, compositionResult.Result!.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        // This is not allowed because the composition also has a composition (compositionBase).
        var invalidComposition = ContentTypeCreateModel(
            "Invalid",
            compositions: new[]
            {
                new Composition
                {
                    CompositionType = CompositionType.Composition,
                    Key = compositionResult.Result!.Key
                },
            });

        var invalidAttempt = await ContentTypeEditingService.CreateAsync(invalidComposition, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(invalidAttempt.Success, Is.False);
            Assert.That(invalidAttempt.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidComposition));
            Assert.That(invalidAttempt.Result, Is.Null);
        });

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Can_Create_Child()
    {
        var parentProperty = ContentTypePropertyTypeModel("Parent Property", "parentProperty");
        var parentModel = ContentTypeCreateModel(
            "Parent",
            propertyTypes: new[] { parentProperty });

        var parentResult = await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey);
        Assert.That(parentResult.Success, Is.True);

        var childProperty = ContentTypePropertyTypeModel("Child Property", "childProperty");
        var parentKey = parentResult.Result!.Key;
        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = parentKey,
            },
        };

        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var childModel = ContentTypeCreateModel(
            "Child",
            propertyTypes: new[] { childProperty },
            compositions: composition);

        var result = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);

        Assert.Multiple(() =>
        {
            var contentType = result.Result!;
            Assert.That(contentType.ParentId, Is.EqualTo(parentResult.Result.Id));
            Assert.That(contentType.ContentTypeComposition.Count(), Is.EqualTo(1));
            Assert.That(contentType.ContentTypeComposition.FirstOrDefault()?.Key, Is.EqualTo(parentResult.Result.Key));
            Assert.That(contentType.CompositionPropertyTypes.Count(), Is.EqualTo(2));
            Assert.That(contentType.CompositionPropertyTypes.Any(x => x.Alias == parentProperty.Alias), Is.True);
            Assert.That(contentType.CompositionPropertyTypes.Any(x => x.Alias == childProperty.Alias), Is.True);
        });

        AssertContentTypeRefreshPayload(refreshedPayloads, result.Result!.Id, ContentTypeChangeTypes.Create);
    }

    // Unlike compositions, it is allowed to inherit on multiple levels
    [Test]
    public async Task Can_Create_Grandchild()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var rootProperty = ContentTypePropertyTypeModel("Root property");
        ContentTypeCreateModel rootModel = ContentTypeCreateModel(
            "Root",
            propertyTypes: new[] { rootProperty });

        var rootResult = await ContentTypeEditingService.CreateAsync(rootModel, Constants.Security.SuperUserKey);
        Assert.That(rootResult.Success, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, rootResult.Result!.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var childProperty = ContentTypePropertyTypeModel("Child Property", "childProperty");
        var rootKey = rootResult.Result!.Key;
        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = rootKey,
            },
        };

        var childModel = ContentTypeCreateModel(
            "Child",
            propertyTypes: new[] { childProperty },
            compositions: composition);

        var childResult = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);
        Assert.That(childResult.Success, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, childResult.Result!.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var grandchildProperty = ContentTypePropertyTypeModel("Grandchild Property", "grandchildProperty");
        var childKey = childResult.Result!.Key;
        Composition[] grandchildComposition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = childKey,
            },
        };

        var grandchildModel = ContentTypeCreateModel(
            "Grandchild",
            propertyTypes: new[] { grandchildProperty },
            compositions: grandchildComposition);

        var grandchildResult = await ContentTypeEditingService.CreateAsync(grandchildModel, Constants.Security.SuperUserKey);
        Assert.That(grandchildResult.Success, Is.True);

        var root = rootResult.Result!;
        var child = childResult.Result!;
        IContentType grandchild = grandchildResult.Result!;
        Assert.Multiple(() =>
        {
            // Write asserts for this test
            Assert.That(root.ParentId, Is.EqualTo(-1));
            Assert.That(child.ParentId, Is.EqualTo(root.Id));
            Assert.That(grandchild.ParentId, Is.EqualTo(child.Id));

            // We only have the immediate parent as a composition
            Assert.That(grandchild.ContentTypeComposition.Count(), Is.EqualTo(1));
            Assert.That(grandchild.ContentTypeComposition.FirstOrDefault()?.Key, Is.EqualTo(child.Key));

            // But all the property types are there since we crawl up the chain in CompositionPropertyTypes
            Assert.That(grandchild.CompositionPropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(grandchild.CompositionPropertyTypes.Any(x => x.Alias == rootProperty.Alias), Is.True);
            Assert.That(grandchild.CompositionPropertyTypes.Any(x => x.Alias == childProperty.Alias), Is.True);
            Assert.That(grandchild.CompositionPropertyTypes.Any(x => x.Alias == grandchildProperty.Alias), Is.True);
        });

        AssertContentTypeRefreshPayload(refreshedPayloads, grandchild.Id, ContentTypeChangeTypes.Create);
    }

    [Test]
    public async Task Can_Create_Child_To_Content_Type_With_Composition()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionContentType = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Composition"), Constants.Security.SuperUserKey)).Result!;
        var parentContentType = (await ContentTypeEditingService.CreateAsync(
                ContentTypeCreateModel(
                    "Parent",
                    compositions: [new Composition { CompositionType = CompositionType.Composition, Key = compositionContentType.Key }]),
                Constants.Security.SuperUserKey)).Result!;

        AssertContentTypeRefreshPayload(refreshedPayloads, parentContentType.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var result = await ContentTypeEditingService.CreateAsync(
                ContentTypeCreateModel(
                    "Child",
                    compositions: [new Composition { CompositionType = CompositionType.Inheritance, Key = parentContentType.Key }]),
                Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        var childContentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.That(childContentType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(childContentType.Name, Is.EqualTo("Child"));
            Assert.That(childContentType.ContentTypeComposition.Count(), Is.EqualTo(1));
            Assert.That(childContentType.ContentTypeComposition.Single().Key, Is.EqualTo(parentContentType.Key));
        });

        AssertContentTypeRefreshPayload(refreshedPayloads, childContentType.Id, ContentTypeChangeTypes.Create);
    }

    [Test]
    public async Task Cannot_Be_Both_Parent_And_Composition()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionBase = ContentTypeCreateModel("CompositionBase");

        var baseResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(baseResult.Success, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, baseResult.Result!.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition
                {
                    CompositionType = CompositionType.Composition, Key = baseResult.Result!.Key
                },
                new Composition
                {
                    CompositionType = CompositionType.Inheritance, Key = baseResult.Result!.Key
                },
            });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidInheritance));
        });

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Have_Multiple_Inheritance()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var parentModel1 = ContentTypeCreateModel("Parent1");
        var parentModel2 = ContentTypeCreateModel("Parent2");

        var parent1 = (await ContentTypeEditingService.CreateAsync(parentModel1, Constants.Security.SuperUserKey)).Result;
        var parentKey1 = parent1?.Key;
        Assert.That(parentKey1.HasValue, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, parent1.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var parent2 = (await ContentTypeEditingService.CreateAsync(parentModel2, Constants.Security.SuperUserKey)).Result;
        var parentKey2 = parent2?.Key;
        Assert.That(parentKey2.HasValue, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, parent2.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var childProperty = ContentTypePropertyTypeModel("Child Property", "childProperty");
        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = parentKey1.Value,
            },
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = parentKey2.Value,
            },
        };

        var childModel = ContentTypeCreateModel(
            "Child",
            propertyTypes: new[] { childProperty },
            compositions: composition);

        var result = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidInheritance));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Specify_Duplicate_PropertyType_Alias_From_Compositions()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var propertyTypeAlias = "testproperty";
        var compositionPropertyType = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias);
        var compositionBase = ContentTypeCreateModel(
            "CompositionBase",
            propertyTypes: new[] { compositionPropertyType });

        var compositionBaseResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionBaseResult.Success, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, compositionBaseResult.Result!.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition
                {
                    CompositionType = CompositionType.Composition, Key = compositionBaseResult.Result!.Key
                },
            },
            propertyTypes: new[]
            {
                ContentTypePropertyTypeModel("Test Property", propertyTypeAlias)
            });
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.DuplicatePropertyTypeAlias));
        });

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Specify_Non_Existent_DocType_As_Composition()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition
                {
                    CompositionType = CompositionType.Composition, Key = Guid.NewGuid()
                },
            });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidComposition));
        });

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Mix_Inheritance_And_ParentKey()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var parentModel = ContentTypeCreateModel("Parent");
        var parent = (await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey)).Result;
        var parentKey = parent?.Key;
        Assert.That(parentKey.HasValue, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, parent.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var containerResult = ContentTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), "Test folder");
        Assert.That(containerResult.Success, Is.True);
        var container = containerResult.Result?.Entity;
        Assert.That(container, Is.Not.Null);

        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = parentKey.Value,
            }
        };

        var childModel = ContentTypeCreateModel(
            "Child",
            containerKey: container.Key,
            compositions: composition);

        var result = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidParent));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Have_Same_Key_For_Inheritance_And_Parent()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var parentModel = ContentTypeCreateModel("Parent");
        var parent = (await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey)).Result;
        Assert.That(parent, Is.Not.Null);

        AssertContentTypeRefreshPayload(refreshedPayloads, parent.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = parent.Key,
            }
        };

        var childModel = ContentTypeCreateModel(
            "Child",
            containerKey: parent.Key,
            compositions: composition);

        var result = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidParent));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Use_As_ParentKey()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var parentModel = ContentTypeCreateModel("Parent");
        var parent = (await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey)).Result;
        var parentKey = parent?.Key;
        Assert.That(parentKey.HasValue, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, parent.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        var childModel = ContentTypeCreateModel(
            "Child",
            containerKey: parentKey.Value);

        var result = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidParent));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("   ")]
    [TestCase(".")]
    [TestCase("-")]
    [TestCase("!\"#¤%&/()=)?`")]
    public async Task Cannot_Use_Invalid_PropertyType_Alias(string propertyTypeAlias)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var propertyType = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias);
        var createModel = ContentTypeCreateModel("Test", propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidPropertyTypeAlias));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [TestCase("testProperty", "testProperty")]
    [TestCase("testProperty", "TestProperty")]
    [TestCase("testProperty", "TESTPROPERTY")]
    [TestCase("testProperty", "testproperty")]
    public async Task Cannot_Use_Duplicate_PropertyType_Alias(string propertyTypeAlias1, string propertyTypeAlias2)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var propertyType1 = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias1);
        var propertyType2 = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias2);
        var createModel = ContentTypeCreateModel("Test", propertyTypes: new[] { propertyType1, propertyType2 });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.DuplicatePropertyTypeAlias));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [TestCase("testAlias", "testAlias")]
    [TestCase("testAlias", "testalias")]
    [TestCase("testAlias", "TESTALIAS")]
    [TestCase("testAlias", "testAlias")]
    [TestCase("testalias", "testAlias")]
    [TestCase("TESTALIAS", "testAlias")]
    public async Task Cannot_Use_Alias_As_PropertyType_Alias(string contentTypeAlias, string propertyTypeAlias)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var propertyType = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias);
        var createModel = ContentTypeCreateModel("Test", contentTypeAlias, propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.PropertyTypeAliasCannotEqualContentTypeAlias));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Use_Non_Existing_DataType_For_PropertyType()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty", dataTypeKey: Guid.NewGuid());
        var createModel = ContentTypeCreateModel("Test", "test", propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.DataTypeNotFound));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Use_Empty_Alias_For_PropertyType()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var propertyType = ContentTypePropertyTypeModel("Test Property", string.Empty);
        var createModel = ContentTypeCreateModel("Test", "test", propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidPropertyTypeAlias));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Use_Empty_Name_For_PropertyType_Container()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var container = ContentTypePropertyContainerModel(string.Empty);
        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        var createModel = ContentTypeCreateModel("Test", "test", propertyTypes: new[] { propertyType });
        createModel.Containers = new[] { container };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidContainerName));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("   ")]
    [TestCase(".")]
    [TestCase("-")]
    [TestCase("!\"#¤%&/()=)?`")]
    [TestCaseSource(nameof(DifferentCapitalizedAlias), new object[] { "System" })]
    public async Task Cannot_Use_Invalid_Alias(string contentTypeAlias)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var createModel = ContentTypeCreateModel("Test", contentTypeAlias);

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidAlias));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [TestCase("test")] // Matches alias case sensitively.
    [TestCase("Test")] // Matches alias case insensitively.
    public async Task Cannot_Use_Existing_Alias(string newAlias)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var createModel = ContentTypeCreateModel("Test", "test");
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, result.Result!.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        createModel = ContentTypeCreateModel("Test 2", newAlias);
        result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.DuplicateAlias));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Add_Container_From_Composition()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        // Let's add a property to ensure that it passes through
        var compositionContainer = ContentTypePropertyContainerModel("Composition Tab");
        compositionBase.Containers = new[] { compositionContainer };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionContainer.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        AssertContentTypeRefreshPayload(refreshedPayloads, compositionType.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition { CompositionType = CompositionType.Composition, Key = compositionType.Key, },
            });

        // this is invalid; the model should not contain the composition container definitions (they will be resolved by ContentTypeEditingService)
        createModel.Containers = new[] { compositionContainer };
        var property = ContentTypePropertyTypeModel("My Property", "myProperty", containerKey: compositionContainer.Key);
        createModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.DuplicateContainer));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Duplicate_Container_Key_From_Composition()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        var compositionContainer = ContentTypePropertyContainerModel("Composition Tab");
        compositionBase.Containers = new[] { compositionContainer };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionContainer.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        AssertContentTypeRefreshPayload(refreshedPayloads, compositionType.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition { CompositionType = CompositionType.Composition, Key = compositionType.Key, },
            });

        // this is invalid; cannot reuse the container key
        var container = ContentTypePropertyContainerModel("My Group", type: GroupContainerType, key: compositionContainer.Key);
        createModel.Containers = new[] { container };
        var property = ContentTypePropertyTypeModel("My Property", "myProperty", containerKey: container.Key);
        createModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.DuplicateContainer));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Have_Duplicate_Container_Key()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel("Test", "test");

        // this is invalid; cannot reuse the container key
        var containerKey = Guid.NewGuid();
        var container1 = ContentTypePropertyContainerModel("My Group 1", key: containerKey);
        var container2 = ContentTypePropertyContainerModel("My Group 2", key: containerKey);
        createModel.Containers = new[] { container1, container2 };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.DuplicateContainer));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Add_Property_To_Missing_Container()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        var compositionContainer = ContentTypePropertyContainerModel("Composition Tab");
        compositionBase.Containers = new[] { compositionContainer };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionContainer.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        AssertContentTypeRefreshPayload(refreshedPayloads, compositionType.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition { CompositionType = CompositionType.Composition, Key = compositionType.Key, },
            });

        // this is invalid; cannot add properties to non-existing containers
        var property = ContentTypePropertyTypeModel("My Property", "myProperty", containerKey: Guid.NewGuid());
        createModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.MissingContainer));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Add_Property_Container_To_Missing_Container()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel();

        var group = ContentTypePropertyContainerModel("My Group", type: GroupContainerType);
        // this is invalid; a container cannot have a non-existing parent container key
        group.ParentKey = Guid.NewGuid();
        createModel.Containers = new[] { group };

        var property = ContentTypePropertyTypeModel("My Property", "myProperty", containerKey: group.Key);
        createModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.MissingContainer));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_Property_In_Composition_Container()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        // Let's add a property to ensure that it passes through
        var compositionContainer = ContentTypePropertyContainerModel("Composition Tab");
        compositionBase.Containers = new[] { compositionContainer };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionContainer.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        AssertContentTypeRefreshPayload(refreshedPayloads, compositionType.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition { CompositionType = CompositionType.Composition, Key = compositionType.Key, },
            });

        // this is invalid; cannot add a property on a container that belongs to the composition (the container must be duplicated to the content type itself)
        var property = ContentTypePropertyTypeModel("My Property", "myProperty", containerKey: compositionContainer.Key);
        createModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.MissingContainer));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_Property_Container_In_Composition_Container()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        // Let's add a property to ensure that it passes through
        var compositionContainer = ContentTypePropertyContainerModel("Composition Tab");
        compositionBase.Containers = new[] { compositionContainer };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionContainer.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        AssertContentTypeRefreshPayload(refreshedPayloads, compositionType.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition { CompositionType = CompositionType.Composition, Key = compositionType.Key, },
            });

        // this is invalid; cannot create a new container within a parent container that belongs to the composition (the parent container must be duplicated to the content type itself)
        var container = ContentTypePropertyContainerModel("My Group", type: GroupContainerType);
        container.ParentKey = compositionContainer.Key;
        createModel.Containers = new[] { container };
        var property = ContentTypePropertyTypeModel("My Property", "myProperty", containerKey: container.Key);
        createModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.MissingContainer));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_Composite_With_MediaType()
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var compositionBase = MediaTypeCreateModel("Composition Base");

        // Let's add a property to ensure that it passes through
        var container = MediaTypePropertyContainerModel();
        compositionBase.Containers = new[] { container };

        var compositionProperty = MediaTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: container.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await MediaTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition { CompositionType = CompositionType.Composition, Key = compositionType.Key, },
            });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidComposition));

        // no changes should have been notified (media type creation succeeds, but we are listening for content type change notifications)
        Assert.That(refreshedPayloads, Is.Null);
    }

    [TestCase("something")]
    [TestCase("tab")]
    [TestCase("group")]
    public async Task Cannot_Create_Container_With_Unknown_Type(string containerType)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var createModel = ContentTypeCreateModel("Test", "test");
        var container = ContentTypePropertyContainerModel(name: containerType, type: containerType);
        createModel.Containers = new[] { container };

        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { propertyType };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidContainerType));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public async Task Cannot_Have_Element_Type_Mismatched_Inheritance(bool parentIsElement, bool childIsElement)
    {
        ContentTypeCacheRefresher.JsonPayload[] refreshedPayloads = null;
        ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = payloads
            => refreshedPayloads = payloads;

        var parentModel = ContentTypeCreateModel("Parent1", isElement: parentIsElement);

        var parent = (await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey)).Result;
        var parentKey = parent?.Key;
        Assert.That(parentKey.HasValue, Is.True);

        AssertContentTypeRefreshPayload(refreshedPayloads, parent.Id, ContentTypeChangeTypes.Create);
        refreshedPayloads = null;

        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = parentKey.Value,
            }
        };

        var childModel = ContentTypeCreateModel(
            "Child",
            compositions: composition,
            isElement: childIsElement);

        var result = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidElementFlagComparedToParent));

        // no changes should have been notified
        Assert.That(refreshedPayloads, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_Element_Type_With_Segment_Variation()
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: true);
        createModel.VariesBySegment = true;

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidSegmentVariationForElementType));
    }
}
