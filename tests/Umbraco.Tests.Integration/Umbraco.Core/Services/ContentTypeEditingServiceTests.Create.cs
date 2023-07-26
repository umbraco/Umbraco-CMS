﻿using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ContentTypeEditingServiceTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Create_ContentType_With_All_Basic_Settings(bool isElement)
    {
        var createModel = CreateCreateModel("Test", "test", isElement: isElement);
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
    public async Task Can_Create_ContentType_With_Variation(bool variesByCulture, bool variesBySegment)
    {
        var createModel = CreateCreateModel("Test", "test");
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
    public async Task Can_Create_ContentType_In_A_Folder(bool isElement)
    {
        var containerResult = ContentTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), "Test folder");
        Assert.IsTrue(containerResult.Success);
        var container = containerResult.Result?.Entity;
        Assert.IsNotNull(container);

        var createModel = CreateCreateModel("Test", "test", isElement: isElement, parentKey: container.Key);
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
    public async Task Can_Create_ContentType_With_Properties_In_A_Container(bool isElement)
    {
        var createModel = CreateCreateModel("Test", "test", isElement: isElement);
        var container = CreateContainer();
        createModel.Containers = new[] { container };

        var propertyType = CreatePropertyType(name: "Test Property", alias: "testProperty", containerKey: container.Key);
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
    public async Task Can_Create_ContentType_With_Orphaned_Properties(bool isElement)
    {
        var createModel = CreateCreateModel("Test", "test", isElement: isElement);

        var propertyType = CreatePropertyType(name: "Test Property", alias: "testProperty");
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
        var createModel = CreateCreateModel(key: key);

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

        var propertyTypeContainer = CreateContainer();
        var propertyTypeCreateModel = CreatePropertyType(key: propertyTypeKey, containerKey: propertyTypeContainer.Key);

        var createModel = CreateCreateModel(
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
    public async Task Can_Assign_Allowed_Types_To_ContentType()
    {
        var allowedOne = (await ContentTypeEditingService.CreateAsync(CreateCreateModel(name: "Allowed One", alias: "allowedOne"), Constants.Security.SuperUserKey)).Result;
        var allowedTwo = (await ContentTypeEditingService.CreateAsync(CreateCreateModel(name: "Allowed Two", alias: "allowedTwo"), Constants.Security.SuperUserKey)).Result;
        Assert.IsNotNull(allowedOne);
        Assert.IsNotNull(allowedTwo);

        var createModel = CreateCreateModel(name: "Test", alias: "test");
        createModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(new Lazy<int>(() => allowedOne.Id), allowedOne.Key, 10, allowedOne.Alias),
            new ContentTypeSort(new Lazy<int>(() => allowedTwo.Id), allowedTwo.Key, 20, allowedTwo.Alias),
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
    public async Task Can_Assign_History_Cleanup_To_ContentType()
    {
        var createModel = CreateCreateModel(name: "Test", alias: "test");
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
    public async Task Can_Create_Composite_ContentType(bool compositionIsElement, bool contentTypeIsElement)
    {
        var compositionBase = CreateCreateModel(
            alias: "compositionBase",
            name: "Composition Base",
            isElement: compositionIsElement);

        // Let's add a property to ensure that it passes through
        var container = CreateContainer();
        compositionBase.Containers = new[] { container };

        var compositionProperty = CreatePropertyType(name: "Composition Property", alias: "compositionProperty", containerKey: container.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

        // Create doc type using the composition
        var createModel = CreateCreateModel(
            isElement: contentTypeIsElement,
            compositions: new[]
            {
                new Composition
                {
                CompositionType = CompositionType.Composition,
                Key = compositionType.Key,
                },
            }
        );

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
    public async Task Element_Types_Must_Not_Be_Composed_By_non_element_type()
    {
        // This is a pretty interesting one, since it actually seems to be broken in the old backoffice,
        // since the client will always send the isElement flag as false to the GetAvailableCompositeContentTypes endpoint
        // Even if it's an element type, however if we look at the comment in GetAvailableCompositeContentTypes
        // We see that element types are not suppose to be allowed to be composed by non-element types.
        // Since this breaks models builder evidently.
        var compositionBase = CreateCreateModel(
            name: "Composition Base",
            isElement: false);

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

        var createModel = CreateCreateModel(
            name: "Content Type Using Composition",
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
        var compositionBase = CreateCreateModel(name: "CompositionBase");

        var baseResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(baseResult.Success);

        var composition = CreateCreateModel(
            name: "Composition",
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
        var invalidComposition = CreateCreateModel(
            name: "Invalid",
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
    public async Task Can_Create_Child_ContentType()
    {

        var parentProperty = CreatePropertyType("Parent Property", "parentProperty");
        var parentModel = CreateCreateModel(
            name: "Parent",
            propertyTypes: new[] { parentProperty });

        var parentResult = await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(parentResult.Success);

        var childProperty = CreatePropertyType("Child Property", "childProperty");
        var parentKey = parentResult.Result!.Key;
        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = parentKey,
            },
        };

        var childModel = CreateCreateModel(
            name: "Child",
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
    public async Task Can_Create_Grandchild_ContentType()
    {
        var rootProperty = CreatePropertyType("Root property");
        ContentTypeCreateModel rootModel = CreateCreateModel(
            name: "Root",
            propertyTypes: new[] { rootProperty });

        var rootResult = await ContentTypeEditingService.CreateAsync(rootModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(rootResult.Success);

        var childProperty = CreatePropertyType("Child Property", "childProperty");
        var rootKey = rootResult.Result!.Key;
        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = rootKey,
            },
        };

        var childModel = CreateCreateModel(
            name: "Child",
            propertyTypes: new[] { childProperty },
            compositions: composition);

        var childResult = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(childResult.Success);

        var grandchildProperty = CreatePropertyType("Grandchild Property", "grandchildProperty");
        var childKey = childResult.Result!.Key;
        Composition[] grandchildComposition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = childKey,
            },
        };

        var grandchildModel = CreateCreateModel(
            name: "Grandchild",
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
    public async Task ContentType_Cannot_Be_Both_Parent_And_Composition()
    {
        var compositionBase = CreateCreateModel(name: "CompositionBase");

        var baseResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(baseResult.Success);

        var createModel = CreateCreateModel(
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
        var parentModel1 = CreateCreateModel(name: "Parent1");
        var parentModel2 = CreateCreateModel(name: "Parent2");

        var parentKey1 = (await ContentTypeEditingService.CreateAsync(parentModel1, Constants.Security.SuperUserKey)).Result?.Key;
        Assert.IsTrue(parentKey1.HasValue);
        var parentKey2 = (await ContentTypeEditingService.CreateAsync(parentModel2, Constants.Security.SuperUserKey)).Result?.Key;
        Assert.IsTrue(parentKey2.HasValue);

        var childProperty = CreatePropertyType("Child Property", "childProperty");
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

        var childModel = CreateCreateModel(
            name: "Child",
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
        var compositionPropertyType = CreatePropertyType("Test Property", propertyTypeAlias);
        var compositionBase = CreateCreateModel(
            name: "CompositionBase",
            propertyTypes: new[] { compositionPropertyType });

        var compositionBaseResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionBaseResult.Success);

        var createModel = CreateCreateModel(
            compositions: new[]
            {
                new Composition
                {
                    CompositionType = CompositionType.Composition, Key = compositionBaseResult.Result!.Key
                },
            },
            propertyTypes: new[]
            {
                CreatePropertyType("Test Property", propertyTypeAlias)
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
        var createModel = CreateCreateModel(
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
        var parentModel = CreateCreateModel(name: "Parent");
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

        var childModel = CreateCreateModel(
            name: "Child",
            parentKey: container.Key,
            compositions: composition);

        var result = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidParent, result.Status);
    }

    [Test]
    public async Task Cannot_Use_ContentType_As_ParentKey()
    {
        var parentModel = CreateCreateModel(name: "Parent");
        var parentKey = (await ContentTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey)).Result?.Key;
        Assert.IsTrue(parentKey.HasValue);

        var childModel = CreateCreateModel(
            name: "Child",
            parentKey: parentKey.Value);

        var result = await ContentTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidParent, result.Status);
    }

    // test some properties from IPublishedContent
    [TestCase(nameof(IPublishedContent.Id))]
    [TestCase(nameof(IPublishedContent.Name))]
    [TestCase(nameof(IPublishedContent.SortOrder))]
    // test some properties from IPublishedElement
    [TestCase(nameof(IPublishedElement.Properties))]
    [TestCase(nameof(IPublishedElement.ContentType))]
    [TestCase(nameof(IPublishedElement.Key))]
    // test some methods from IPublishedContent
    [TestCase(nameof(IPublishedContent.IsDraft))]
    [TestCase(nameof(IPublishedContent.IsPublished))]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("   ")]
    [TestCase(".")]
    [TestCase("-")]
    [TestCase("!\"#¤%&/()=)?`")]
    public async Task Cannot_Use_Invalid_PropertyType_Alias(string propertyTypeAlias)
    {
        // ensure that property casing is ignored when handling reserved property aliases
        var propertyTypeAliases = new[]
        {
            propertyTypeAlias, propertyTypeAlias.ToLowerInvariant(), propertyTypeAlias.ToUpperInvariant()
        };

        foreach (var alias in propertyTypeAliases)
        {
            var propertyType = CreatePropertyType("Test Property", alias);
            var createModel = CreateCreateModel(name: "Test", propertyTypes: new[] { propertyType });

            var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentTypeOperationStatus.InvalidPropertyTypeAlias, result.Status);
        }
    }

    [TestCase("testProperty", "testProperty")]
    [TestCase("testProperty", "TestProperty")]
    [TestCase("testProperty", "TESTPROPERTY")]
    [TestCase("testProperty", "testproperty")]
    public async Task Cannot_Use_Duplicate_PropertyType_Alias(string propertyTypeAlias1, string propertyTypeAlias2)
    {
        var propertyType1 = CreatePropertyType("Test Property", propertyTypeAlias1);
        var propertyType2 = CreatePropertyType("Test Property", propertyTypeAlias2);
        var createModel = CreateCreateModel(name: "Test", propertyTypes: new[] { propertyType1, propertyType2 });

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
    public async Task Cannot_Use_ContentType_Alias_As_PropertyType_Alias(string contentTypeAlias, string propertyTypeAlias)
    {
        var propertyType = CreatePropertyType("Test Property", propertyTypeAlias);
        var createModel = CreateCreateModel(name: "Test", alias: contentTypeAlias, propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidPropertyTypeAlias, result.Status);
    }

    [Test]
    public async Task Cannot_Use_Non_Existing_DataType_For_PropertyType()
    {
        var propertyType = CreatePropertyType("Test Property", "testProperty", dataTypeKey: Guid.NewGuid());
        var createModel = CreateCreateModel(name: "Test", alias: "test", propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.DataTypeNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Use_Empty_Alias_For_PropertyType()
    {
        var propertyType = CreatePropertyType("Test Property", string.Empty);
        var createModel = CreateCreateModel(name: "Test", alias: "test", propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidPropertyTypeAlias, result.Status);
    }

    [Test]
    public async Task Cannot_Use_Empty_Name_For_PropertyType_Container()
    {
        var container = CreateContainer(name: string.Empty);
        var propertyType = CreatePropertyType("Test Property", "testProperty", containerKey: container.Key);
        var createModel = CreateCreateModel(name: "Test", alias: "test", propertyTypes: new[] { propertyType });
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
    [TestCase("system")]
    [TestCase("System")]
    [TestCase("SYSTEM")]
    public async Task Cannot_Use_Invalid_Alias_For_ContentType(string contentTypeAlias)
    {
        var createModel = CreateCreateModel(name: "Test", alias: contentTypeAlias);

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.InvalidAlias, result.Status);
    }

    [Test]
    public async Task Cannot_Use_Existing_ContentType_Alias_For_ContentType()
    {
        var createModel = CreateCreateModel(name: "Test", alias: "test");
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        createModel = CreateCreateModel(name: "Test 2", alias: "test");
        result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.DuplicateAlias, result.Status);
    }
}
