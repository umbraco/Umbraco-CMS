using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ContentTypeEditingServiceTests
{
    [Test]
    public async Task Can_Create_Basic_ContentType()
    {
        var name = "Test";
        var alias = "test";

        var createModel = CreateCreateModel(alias: alias, name: name, isElement: true);
        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // Ensure it's actually persisted
        var contentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(contentType);
            Assert.IsTrue(contentType.IsElement);
            Assert.AreEqual(alias, contentType.Alias);
            Assert.AreEqual(name, contentType.Name);
            Assert.AreEqual(result.Result.Id, contentType.Id);
            Assert.AreEqual(result.Result.Key, contentType.Key);
        });
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
            name: "Document Type Using Composition",
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
    public async Task Can_Create_Child_Document_Type()
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
            compositions: composition,
            parentKey: parentKey);

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
            compositions: composition,
            parentKey: rootKey);

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
            compositions: grandchildComposition,
            parentKey: childKey);

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
            },
            parentKey: baseResult.Result.Key);

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentTypeOperationStatus.InvalidInheritance, result.Status);
        });
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
            Assert.AreEqual(ContentTypeOperationStatus.CompositionTypeNotFound, result.Status);
        });
    }
}
