using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class ContentTypeEditingServiceTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Create_With_All_Basic_Settings(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        createModel.Description = "This is the Test description";
        createModel.Icon = "icon icon-something";
        createModel.AllowedAsRoot = true;

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.IsNotNull(contentType);

        Assert.AreEqual(isElement, contentType.IsElement);
        Assert.AreEqual("test", contentType.Alias);
        Assert.AreEqual("Test", contentType.Name);
        Assert.AreEqual(result.Result.Id, contentType.Id);
        Assert.AreEqual(result.Result.Key, contentType.Key);
        Assert.AreEqual("This is the Test description", contentType.Description);
        Assert.AreEqual("icon icon-something", contentType.Icon);
        Assert.IsTrue(contentType.AllowedAsRoot);
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task Can_Create_With_Variation(bool variesByCulture, bool variesBySegment)
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.VariesByCulture = variesByCulture;
        createModel.VariesBySegment = variesBySegment;

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.IsNotNull(contentType);

        Assert.AreEqual(variesByCulture, contentType.VariesByCulture());
        Assert.AreEqual(variesBySegment, contentType.VariesBySegment());
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_In_A_Folder(bool isElement)
    {
        var containerResult = ContentTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), "Test folder");
        Assert.IsTrue(containerResult.Success);
        var container = containerResult.Result?.Entity;
        Assert.IsNotNull(container);

        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement, containerKey: container.Key);
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted in the folder
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.IsNotNull(contentType);
        Assert.AreEqual(container.Id, contentType.ParentId);
        Assert.AreEqual(isElement, contentType.IsElement);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Create_With_Properties_In_A_Container(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);
        var container = ContentTypePropertyContainerModel();
        createModel.Containers = new[] { container };

        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { propertyType };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(isElement, contentType.IsElement);
        Assert.AreEqual(1, contentType.PropertyGroups.Count);
        Assert.AreEqual(1, contentType.PropertyTypes.Count());
        Assert.AreEqual(1, contentType.PropertyGroups.First().PropertyTypes!.Count);
        Assert.AreEqual("testProperty", contentType.PropertyTypes.First().Alias);
        Assert.AreEqual("testProperty", contentType.PropertyGroups.First().PropertyTypes!.First().Alias);
        Assert.IsEmpty(contentType.NoGroupPropertyTypes);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Create_With_Orphaned_Properties(bool isElement)
    {
        var createModel = ContentTypeCreateModel("Test", "test", isElement: isElement);

        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty");
        createModel.Properties = new[] { propertyType };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(isElement, contentType.IsElement);
        Assert.IsEmpty(contentType.PropertyGroups);
        Assert.AreEqual(1, contentType.PropertyTypes.Count());
        Assert.AreEqual("testProperty", contentType.PropertyTypes.First().Alias);
        Assert.AreEqual(1, contentType.NoGroupPropertyTypes.Count());
        Assert.AreEqual("testProperty", contentType.NoGroupPropertyTypes.First().Alias);
    }

    [Test]
    public async Task Can_Specify_Key()
    {
        var key = new Guid("33C326F6-CB5E-43D6-9730-E946AA5F9C7B");
        var createModel = ContentTypeCreateModel(key: key);

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(contentType);
            Assert.AreEqual(key, contentType.Key);
        });
    }

    [Test]
    public async Task Can_Specify_PropertyType_Key()
    {
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
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(contentType);
            var propertyType = contentType.PropertyGroups.FirstOrDefault()?.PropertyTypes?.FirstOrDefault();
            Assert.IsNotNull(propertyType);
            Assert.AreEqual(propertyTypeKey, propertyType.Key);
        });
    }

    [Test]
    public async Task Can_Assign_Allowed_Types()
    {
        var allowedOne = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Allowed One", "allowedOne"), Constants.Security.SuperUserKey)).Result;
        var allowedTwo = (await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel("Allowed Two", "allowedTwo"), Constants.Security.SuperUserKey)).Result;
        Assert.IsNotNull(allowedOne);
        Assert.IsNotNull(allowedTwo);

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedOne.Key, 10, allowedOne.Alias),
            new ContentTypeSort(allowedTwo.Key, 20, allowedTwo.Alias),
        };
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        var contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(contentType);

        var allowedContentTypes = contentType.AllowedContentTypes?.ToArray();
        Assert.IsNotNull(allowedContentTypes);
        Assert.AreEqual(2, allowedContentTypes.Length);
        Assert.IsTrue(allowedContentTypes.Any(c => c.Key == allowedOne.Key && c.SortOrder == 0 && c.Alias == allowedOne.Alias));
        Assert.IsTrue(allowedContentTypes.Any(c => c.Key == allowedTwo.Key && c.SortOrder == 1 && c.Alias == allowedTwo.Alias));
    }

    [Test]
    public async Task Can_Assign_History_Cleanup()
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Cleanup = new ContentTypeCleanup
        {
            PreventCleanup = true, KeepAllVersionsNewerThanDays = 123, KeepLatestVersionPerDayForDays = 456
        };
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        var contentType = await ContentTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(contentType);
        Assert.IsNotNull(contentType.HistoryCleanup);
        Assert.IsTrue(contentType.HistoryCleanup.PreventCleanup);
        Assert.AreEqual(123, contentType.HistoryCleanup.KeepAllVersionsNewerThanDays);
        Assert.AreEqual(456, contentType.HistoryCleanup.KeepLatestVersionPerDayForDays);
    }

    [Test]
    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, false)]
    // Wondering where the last case is? Look at the test below.
    public async Task Can_Create_Composite(bool compositionIsElement, bool contentTypeIsElement)
    {
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
        Assert.IsTrue(compositionResult.Success);
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
        Assert.IsTrue(result.Success);
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, contentType.ContentTypeComposition.Count());
            Assert.AreEqual(compositionType.Key, contentType.ContentTypeComposition.First().Key);
            Assert.AreEqual(1, compositionType.CompositionPropertyGroups.Count());
            Assert.AreEqual(container.Key, compositionType.CompositionPropertyGroups.First().Key);
            Assert.AreEqual(1, compositionType.CompositionPropertyTypes.Count());
            Assert.AreEqual(compositionProperty.Key, compositionType.CompositionPropertyTypes.First().Key);
        });
    }

    [Test]
    public async Task Can_Create_Property_Container_Structure_Matching_Composition_Container_Structure()
    {
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
        Assert.IsTrue(compositionResult.Success);
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
        Assert.IsTrue(result.Success);

        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);
        Assert.AreEqual(2, contentType.PropertyGroups.Count);
        var contentTypeTab = contentType.PropertyGroups.First(g => g.Name == "Composition Tab");
        Assert.AreEqual(tab.Key, contentTypeTab.Key);
        Assert.AreEqual(PropertyGroupType.Tab, contentTypeTab.Type);
        var contentTypeGroup = contentType.PropertyGroups.First(g => g.Name == "Composition Group");
        Assert.AreEqual(group.Key, contentTypeGroup.Key);
        Assert.AreEqual(PropertyGroupType.Group, contentTypeGroup.Type);
        var propertyTypeKeys = contentType.CompositionPropertyTypes.Select(t => t.Key).ToArray();
        Assert.AreEqual(2, propertyTypeKeys.Length);
        Assert.IsTrue(propertyTypeKeys.Contains(compositionProperty.Key));
        Assert.IsTrue(propertyTypeKeys.Contains(property.Key));
        Assert.IsTrue(contentTypeGroup.PropertyTypes?.Contains("myProperty"));
        Assert.IsFalse(contentTypeGroup.PropertyTypes?.Contains("compositionProperty"));
    }

    [Test]
    public async Task Property_Container_Aliases_Are_CamelCased_Names()
    {
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
        Assert.IsTrue(result.Success);
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.AreEqual(3, contentType.PropertyGroups.Count);
        Assert.AreEqual("myTab", contentType.PropertyGroups.First(g => g.Name == "My Tab").Alias);
        Assert.AreEqual("myTab/myGroup", contentType.PropertyGroups.First(g => g.Name == "My Group").Alias);
        Assert.AreEqual("anotherGroup", contentType.PropertyGroups.First(g => g.Name == "AnotherGroup").Alias);
    }

    [Test]
    public async Task Element_Types_Must_Not_Be_Composed_By_non_element_type()
    {
        // This is a pretty interesting one, since it actually seems to be broken in the old backoffice,
        // since the client will always send the isElement flag as false to the GetAvailableCompositeContentTypes endpoint
        // Even if it's an element type, however if we look at the comment in GetAvailableCompositeContentTypes
        // We see that element types are not suppose to be allowed to be composed by non-element types.
        // Since this breaks models builder evidently.
        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            isElement: false);

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

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
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentTypeOperationStatus.InvalidComposition, result.Status);
            Assert.IsNull(result.Result);
        });
    }

    [Test]
    public async Task ContentType_Containing_Composition_Cannot_Be_Used_As_Composition()
    {
        var compositionBase = ContentTypeCreateModel("CompositionBase");

        var baseResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(baseResult.Success);

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
        Assert.IsTrue(compositionResult.Success);

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
            Assert.IsFalse(invalidAttempt.Success);
            Assert.AreEqual(ContentTypeOperationStatus.InvalidComposition, invalidAttempt.Status);
            Assert.IsNull(invalidAttempt.Result);
        });
    }

    [Test]
    public async Task Can_Create_Child()
    {

        var parentProperty = ContentTypePropertyTypeModel("Parent Property", "parentProperty");
        var parentModel = ContentTypeCreateModel(
            "Parent",
            propertyTypes: new[] { parentProperty });

        var parentResult = await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(parentResult.Success);

        var childProperty = ContentTypePropertyTypeModel("Child Property", "childProperty");
        var parentKey = parentResult.Result!.Key;
        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = parentKey,
            },
        };

        var childModel = ContentTypeCreateModel(
            "Child",
            propertyTypes: new[] { childProperty },
            compositions: composition);

        var result = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);

        Assert.Multiple(() =>
        {
            var contentType = result.Result!;
            Assert.AreEqual(parentResult.Result.Id, contentType.ParentId);
            Assert.AreEqual(1, contentType.ContentTypeComposition.Count());
            Assert.AreEqual(parentResult.Result.Key, contentType.ContentTypeComposition.FirstOrDefault()?.Key);
            Assert.AreEqual(2, contentType.CompositionPropertyTypes.Count());
            Assert.IsTrue(contentType.CompositionPropertyTypes.Any(x => x.Alias == parentProperty.Alias));
            Assert.IsTrue(contentType.CompositionPropertyTypes.Any(x => x.Alias == childProperty.Alias));
        });
    }

    // Unlike compositions, it is allowed to inherit on multiple levels
    [Test]
    public async Task Can_Create_Grandchild()
    {
        var rootProperty = ContentTypePropertyTypeModel("Root property");
        ContentTypeCreateModel rootModel = ContentTypeCreateModel(
            "Root",
            propertyTypes: new[] { rootProperty });

        var rootResult = await ContentTypeEditingService.CreateAsync(rootModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(rootResult.Success);

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
        Assert.IsTrue(childResult.Success);

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
        Assert.IsTrue(grandchildResult.Success);

        var root = rootResult.Result!;
        var child = childResult.Result!;
        IContentType grandchild = grandchildResult.Result!;
        Assert.Multiple(() =>
        {
            // Write asserts for this test
            Assert.AreEqual(-1, root.ParentId);
            Assert.AreEqual(root.Id, child.ParentId);
            Assert.AreEqual(child.Id, grandchild.ParentId);

            // We only have the immediate parent as a composition
            Assert.AreEqual(1, grandchild.ContentTypeComposition.Count());
            Assert.AreEqual(child.Key, grandchild.ContentTypeComposition.FirstOrDefault()?.Key);

            // But all the property types are there since we crawl up the chain in CompositionPropertyTypes
            Assert.AreEqual(3, grandchild.CompositionPropertyTypes.Count());
            Assert.IsTrue(grandchild.CompositionPropertyTypes.Any(x => x.Alias == rootProperty.Alias));
            Assert.IsTrue(grandchild.CompositionPropertyTypes.Any(x => x.Alias == childProperty.Alias));
            Assert.IsTrue(grandchild.CompositionPropertyTypes.Any(x => x.Alias == grandchildProperty.Alias));
        });
    }

    [Test]
    public async Task Cannot_Be_Both_Parent_And_Composition()
    {
        var compositionBase = ContentTypeCreateModel("CompositionBase");

        var baseResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(baseResult.Success);

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
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentTypeOperationStatus.InvalidInheritance, result.Status);
        });
    }

    [Test]
    public async Task Cannot_Have_Multiple_Inheritance()
    {
        var parentModel1 = ContentTypeCreateModel("Parent1");
        var parentModel2 = ContentTypeCreateModel("Parent2");

        var parentKey1 = (await ContentTypeEditingService.CreateAsync(parentModel1, Constants.Security.SuperUserKey)).Result?.Key;
        Assert.IsTrue(parentKey1.HasValue);
        var parentKey2 = (await ContentTypeEditingService.CreateAsync(parentModel2, Constants.Security.SuperUserKey)).Result?.Key;
        Assert.IsTrue(parentKey2.HasValue);

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

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidInheritance, result.Status);
    }

    [Test]
    public async Task Cannot_Specify_Duplicate_PropertyType_Alias_From_Compositions()
    {
        var propertyTypeAlias = "testproperty";
        var compositionPropertyType = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias);
        var compositionBase = ContentTypeCreateModel(
            "CompositionBase",
            propertyTypes: new[] { compositionPropertyType });

        var compositionBaseResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionBaseResult.Success);

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
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentTypeOperationStatus.DuplicatePropertyTypeAlias, result.Status);
        });
    }

    [Test]
    public async Task Cannot_Specify_Non_Existent_DocType_As_Composition()
    {
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
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentTypeOperationStatus.InvalidComposition, result.Status);
        });
    }

    [Test]
    public async Task Cannot_Mix_Inheritance_And_ParentKey()
    {
        var parentModel = ContentTypeCreateModel("Parent");
        var parentKey = (await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey)).Result?.Key;
        Assert.IsTrue(parentKey.HasValue);

        var containerResult = ContentTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), "Test folder");
        Assert.IsTrue(containerResult.Success);
        var container = containerResult.Result?.Entity;
        Assert.IsNotNull(container);

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

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidParent, result.Status);
    }

    [Test]
    public async Task Cannot_Use_As_ParentKey()
    {
        var parentModel = ContentTypeCreateModel("Parent");
        var parentKey = (await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey)).Result?.Key;
        Assert.IsTrue(parentKey.HasValue);

        var childModel = ContentTypeCreateModel(
            "Child",
            containerKey: parentKey.Value);

        var result = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidParent, result.Status);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("   ")]
    [TestCase(".")]
    [TestCase("-")]
    [TestCase("!\"#¤%&/()=)?`")]
    public async Task Cannot_Use_Invalid_PropertyType_Alias(string propertyTypeAlias)
    {
        var propertyType = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias);
        var createModel = ContentTypeCreateModel("Test", propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidPropertyTypeAlias, result.Status);
    }

    [TestCase("testProperty", "testProperty")]
    [TestCase("testProperty", "TestProperty")]
    [TestCase("testProperty", "TESTPROPERTY")]
    [TestCase("testProperty", "testproperty")]
    public async Task Cannot_Use_Duplicate_PropertyType_Alias(string propertyTypeAlias1, string propertyTypeAlias2)
    {
        var propertyType1 = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias1);
        var propertyType2 = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias2);
        var createModel = ContentTypeCreateModel("Test", propertyTypes: new[] { propertyType1, propertyType2 });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.DuplicatePropertyTypeAlias, result.Status);
    }

    [TestCase("testAlias", "testAlias")]
    [TestCase("testAlias", "testalias")]
    [TestCase("testAlias", "TESTALIAS")]
    [TestCase("testAlias", "testAlias")]
    [TestCase("testalias", "testAlias")]
    [TestCase("TESTALIAS", "testAlias")]
    public async Task Cannot_Use_Alias_As_PropertyType_Alias(string contentTypeAlias, string propertyTypeAlias)
    {
        var propertyType = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias);
        var createModel = ContentTypeCreateModel("Test", contentTypeAlias, propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.PropertyTypeAliasCannotEqualContentTypeAlias, result.Status);
    }

    [Test]
    public async Task Cannot_Use_Non_Existing_DataType_For_PropertyType()
    {
        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty", dataTypeKey: Guid.NewGuid());
        var createModel = ContentTypeCreateModel("Test", "test", propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.DataTypeNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Use_Empty_Alias_For_PropertyType()
    {
        var propertyType = ContentTypePropertyTypeModel("Test Property", string.Empty);
        var createModel = ContentTypeCreateModel("Test", "test", propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidPropertyTypeAlias, result.Status);
    }

    [Test]
    public async Task Cannot_Use_Empty_Name_For_PropertyType_Container()
    {
        var container = ContentTypePropertyContainerModel(string.Empty);
        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        var createModel = ContentTypeCreateModel("Test", "test", propertyTypes: new[] { propertyType });
        createModel.Containers = new[] { container };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidContainerName, result.Status);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("   ")]
    [TestCase(".")]
    [TestCase("-")]
    [TestCase("!\"#¤%&/()=)?`")]
    [TestCaseSource(nameof(DifferentCapitalizedAlias), new object[] { "System"})]
    public async Task Cannot_Use_Invalid_Alias(string contentTypeAlias)
    {
        var createModel = ContentTypeCreateModel("Test", contentTypeAlias);

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidAlias, result.Status);
    }

    [Test]
    public async Task Cannot_Use_Existing_Alias()
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        createModel = ContentTypeCreateModel("Test 2", "test");
        result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.DuplicateAlias, result.Status);
    }

    [Test]
    public async Task Cannot_Add_Container_From_Composition()
    {
        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        // Let's add a property to ensure that it passes through
        var compositionContainer = ContentTypePropertyContainerModel("Composition Tab");
        compositionBase.Containers = new[] { compositionContainer };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionContainer.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.DuplicateContainer, result.Status);
    }

    [Test]
    public async Task Cannot_Duplicate_Container_Key_From_Composition()
    {
        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        var compositionContainer = ContentTypePropertyContainerModel("Composition Tab");
        compositionBase.Containers = new[] { compositionContainer };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionContainer.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.DuplicateContainer, result.Status);
    }

    [Test]
    public async Task Cannot_Have_Duplicate_Container_Key()
    {
        // Create doc type using the composition
        var createModel = ContentTypeCreateModel("Test", "test");

        // this is invalid; cannot reuse the container key
        var containerKey = Guid.NewGuid();
        var container1 = ContentTypePropertyContainerModel("My Group 1", key: containerKey);
        var container2 = ContentTypePropertyContainerModel("My Group 2", key: containerKey);
        createModel.Containers = new[] { container1, container2 };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.DuplicateContainer, result.Status);
    }

    [Test]
    public async Task Cannot_Add_Property_To_Missing_Container()
    {
        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        var compositionContainer = ContentTypePropertyContainerModel("Composition Tab");
        compositionBase.Containers = new[] { compositionContainer };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionContainer.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.MissingContainer, result.Status);
    }

    [Test]
    public async Task Cannot_Add_Property_Container_To_Missing_Container()
    {
        // Create doc type using the composition
        var createModel = ContentTypeCreateModel();

        var group = ContentTypePropertyContainerModel("My Group", type: GroupContainerType);
        // this is invalid; a container cannot have a non-existing parent container key
        group.ParentKey = Guid.NewGuid();
        createModel.Containers = new[] { group };

        var property = ContentTypePropertyTypeModel("My Property", "myProperty", containerKey: group.Key);
        createModel.Properties = new[] { property };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.MissingContainer, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Property_In_Composition_Container()
    {
        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        // Let's add a property to ensure that it passes through
        var compositionContainer = ContentTypePropertyContainerModel("Composition Tab");
        compositionBase.Containers = new[] { compositionContainer };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionContainer.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.MissingContainer, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Property_Container_In_Composition_Container()
    {
        var compositionBase = ContentTypeCreateModel(
            "Composition Base",
            "compositionBase");

        // Let's add a property to ensure that it passes through
        var compositionContainer = ContentTypePropertyContainerModel("Composition Tab");
        compositionBase.Containers = new[] { compositionContainer };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: compositionContainer.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.MissingContainer, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Composite_With_MediaType()
    {
        var compositionBase = MediaTypeCreateModel("Composition Base");

        // Let's add a property to ensure that it passes through
        var container = MediaTypePropertyContainerModel();
        compositionBase.Containers = new[] { container };

        var compositionProperty = MediaTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: container.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await MediaTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

        // Create doc type using the composition
        var createModel = ContentTypeCreateModel(
            compositions: new[]
            {
                new Composition { CompositionType = CompositionType.Composition, Key = compositionType.Key, },
            });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidComposition, result.Status);
    }

    [TestCase("something")]
    [TestCase("tab")]
    [TestCase("group")]
    public async Task Cannot_Create_Container_With_Unknown_Type(string containerType)
    {
        var createModel = ContentTypeCreateModel("Test", "test");
        var container = ContentTypePropertyContainerModel(name: containerType, type: containerType);
        createModel.Containers = new[] { container };

        var propertyType = ContentTypePropertyTypeModel("Test Property", "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { propertyType };

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidContainerType, result.Status);
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public async Task Cannot_Have_Element_Type_Mismatched_Inheritance(bool parentIsElement, bool childIsElement)
    {
        var parentModel = ContentTypeCreateModel("Parent1", isElement: parentIsElement);

        var parentKey = (await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey)).Result?.Key;
        Assert.IsTrue(parentKey.HasValue);

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

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidElementFlagComparedToParent, result.Status);
    }
}
