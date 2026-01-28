using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [Test]
    public async Task Can_Create_At_Root()
    {
        var elementType = await CreateInvariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "text", Value = "The text value" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ElementEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreate(IElement? createdElement)
        {
            Assert.IsNotNull(createdElement);
            Assert.AreNotEqual(Guid.Empty, createdElement.Key);
            Assert.IsTrue(createdElement.HasIdentity);
            Assert.AreEqual("Test Create", createdElement.Name);
            Assert.AreEqual("The title value", createdElement.GetValue<string>("title"));
            Assert.AreEqual("The text value", createdElement.GetValue<string>("text"));
        }
    }

    [Test]
    public async Task Can_Create_In_A_Folder()
    {
        var elementType = await CreateInvariantElementType();

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var elementKey = Guid.NewGuid();
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = containerKey,
            Key = elementKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "text", Value = "The text value" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var element = await ElementEditingService.GetAsync(elementKey);
        Assert.NotNull(element);
        Assert.AreEqual(container.Id, element.ParentId);

        var children = GetFolderChildren(containerKey);
        Assert.AreEqual(1, children.Length);
        Assert.AreEqual(elementKey, children[0].Key);
    }

    [Test]
    public async Task Can_Create_Without_Properties()
    {
        var elementType = await CreateInvariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsTrue(result.Result.Content!.HasIdentity);
        Assert.AreEqual(null, result.Result.Content!.GetValue<string>("title"));
        Assert.AreEqual(null, result.Result.Content!.GetValue<string>("text"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_With_Property_Validation(bool addValidProperties)
    {
        var elementType = await CreateInvariantElementType();
        elementType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        elementType.PropertyTypes.First(pt => pt.Alias == "text").ValidationRegExp = "^\\d*$";
        elementType.AllowedAsRoot = true;
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);

        var titleValue = addValidProperties ? "The title value" : null;
        var textValue = addValidProperties ? "12345" : "This is not a number";
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = titleValue },
                new PropertyValueModel { Alias = "text", Value = textValue }
            },
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // success is expected regardless of property level validation - the validation error status is communicated in the attempt status (see below)
        Assert.IsTrue(result.Success);
        Assert.AreEqual(addValidProperties ? ContentEditingOperationStatus.Success : ContentEditingOperationStatus.PropertyValidationError, result.Status);
        Assert.IsNotNull(result.Result);

        if (addValidProperties is false)
        {
            Assert.AreEqual(2, result.Result.ValidationResult.ValidationErrors.Count());
            Assert.IsNotNull(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "title" && v.ErrorMessages.Length == 1));
            Assert.IsNotNull(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "text" && v.ErrorMessages.Length == 1));
        }

        // NOTE: creation must be successful, even if the mandatory property is missing (publishing however should not!)
        Assert.IsTrue(result.Result.Content!.HasIdentity);
        Assert.AreEqual(titleValue, result.Result.Content!.GetValue<string>("title"));
        Assert.AreEqual(textValue, result.Result.Content!.GetValue<string>("text"));
    }

    [Test]
    public async Task Can_Create_With_Explicit_Key()
    {
        var elementType = await CreateInvariantElementType();

        var key = Guid.NewGuid();
        var createModel = new ElementCreateModel
        {
            Key = key,
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result.Content);
        Assert.IsTrue(result.Result.Content.HasIdentity);
        Assert.AreEqual(key, result.Result.Content.Key);

        var element = await ElementEditingService.GetAsync(key);
        Assert.IsNotNull(element);
        Assert.AreEqual(result.Result.Content.Id, element.Id);
    }

    [Test]
    public async Task Can_Create_Culture_Variant()
    {
        var elementType = await CreateVariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The English Title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Danish Title", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "The English Name" },
                new VariantModel { Culture = "da-DK", Name = "The Danish Name" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result.Content);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ElementEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IElement? createdElement)
        {
            Assert.IsNotNull(createdElement);
            Assert.AreEqual("The English Name", createdElement.GetCultureName("en-US"));
            Assert.AreEqual("The Danish Name", createdElement.GetCultureName("da-DK"));
            Assert.AreEqual("The Invariant Title", createdElement.GetValue<string>("invariantTitle"));
            Assert.AreEqual("The English Title", createdElement.GetValue<string>("variantTitle", "en-US"));
            Assert.AreEqual("The Danish Title", createdElement.GetValue<string>("variantTitle", "da-DK"));
        }
    }

    [Test]
    public async Task Can_Create_Segment_Variant()
    {
        var elementType = await CreateVariantElementType(ContentVariation.Segment);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Default Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Seg-1 Title", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Seg-2 Title", Segment = "seg-2" }
            ],
            Variants =
            [
                new VariantModel { Name = "The Name" },
                new VariantModel { Segment = "seg-1", Name = "The Name" },
                new VariantModel { Segment = "seg-2", Name = "The Name" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result.Content);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ElementEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IElement? createdElement)
        {
            Assert.IsNotNull(createdElement);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The Name", createdElement.Name);
                Assert.AreEqual("The Invariant Title", createdElement.GetValue<string>("invariantTitle"));
                Assert.AreEqual("The Default Title", createdElement.GetValue<string>("variantTitle", segment: null));
                Assert.AreEqual("The Seg-1 Title", createdElement.GetValue<string>("variantTitle", segment: "seg-1"));
                Assert.AreEqual("The Seg-2 Title", createdElement.GetValue<string>("variantTitle", segment: "seg-2"));
            });
        }
    }

    [Test]
    public async Task Can_Create_Culture_And_Segment_Variant()
    {
        var elementType = await CreateVariantElementType(ContentVariation.CultureAndSegment);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Default Title in English", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Seg-1 Title in English", Culture = "en-US", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Seg-2 Title in English", Culture = "en-US", Segment = "seg-2" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Default Title in Danish", Culture = "da-DK" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Seg-1 Title in Danish", Culture = "da-DK", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Seg-2 Title in Danish", Culture = "da-DK", Segment = "seg-2" }
            ],
            Variants =
            [
                new VariantModel { Name = "The English Name", Culture = "en-US" },
                new VariantModel { Name = "The English Name", Culture = "en-US", Segment = "seg-1" },
                new VariantModel { Name = "The English Name", Culture = "en-US", Segment = "seg-2" },
                new VariantModel { Name = "The Danish Name", Culture = "da-DK" },
                new VariantModel { Name = "The Danish Name", Culture = "da-DK", Segment = "seg-1" },
                new VariantModel { Name = "The Danish Name", Culture = "da-DK", Segment = "seg-2" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result.Content);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ElementEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IElement? createdElement)
        {
            Assert.IsNotNull(createdElement);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The English Name", createdElement.GetCultureName("en-US"));
                Assert.AreEqual("The Danish Name", createdElement.GetCultureName("da-DK"));
                Assert.AreEqual("The Invariant Title", createdElement.GetValue<string>("invariantTitle"));
                Assert.AreEqual("The Default Title in English", createdElement.GetValue<string>("variantTitle", culture: "en-US", segment: null));
                Assert.AreEqual("The Seg-1 Title in English", createdElement.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"));
                Assert.AreEqual("The Seg-2 Title in English", createdElement.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"));
                Assert.AreEqual("The Default Title in Danish", createdElement.GetValue<string>("variantTitle", culture: "da-DK", segment: null));
                Assert.AreEqual("The Seg-1 Title in Danish", createdElement.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"));
                Assert.AreEqual("The Seg-2 Title in Danish", createdElement.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"));
            });
        }
    }

    [Test]
    public async Task Cannot_Create_Without_Element_Type()
    {
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = Guid.NewGuid(),
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ContentTypeNotFound, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
    }

    [Test]
    public async Task Cannot_Create_With_Non_Existing_Properties()
    {
        var elementType = await CreateInvariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "no_such_property", Value = "No such property value" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyTypeNotFound, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
    }

    [Test]
    public async Task Cannot_Create_Invariant_Element_Without_Name()
    {
        var elementType = await CreateInvariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
    }

    [TestCase(ContentVariation.Culture)]
    [TestCase(ContentVariation.Segment)]
    public async Task Cannot_Create_With_Variant_Property_Value_For_Invariant_Content(ContentVariation contentVariation)
    {
        var elementType = await CreateInvariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel
                {
                    Alias = "title",
                    Value = "The title value",
                },
                new PropertyValueModel
                {
                    Alias = "bodyText",
                    Value = "The body text value",
                    Culture = contentVariation is ContentVariation.Culture ? "en-US" : null,
                    Segment = contentVariation is ContentVariation.Segment ? "segment" : null,
                }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyTypeNotFound, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
    }

    [Test]
    [Ignore("We will get around to fixing this as part of the general Elements clean-up task.", Until = "2026-03-31")]
    // TODO ELEMENTS: make ContentEditingServiceBase element aware so it can guard against this test case
    // TODO ELEMENTS: create a similar test for content creation based on element types
    public async Task Cannot_Create_Element_Based_On_NonElement_ContentType()
    {
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        Assert.IsFalse(contentType.IsElement);
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "bodyText", Value = "The body text" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
    }
}
