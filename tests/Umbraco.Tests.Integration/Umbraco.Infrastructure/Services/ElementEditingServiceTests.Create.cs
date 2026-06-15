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

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ElementEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreate(IElement? createdElement)
        {
            Assert.That(createdElement, Is.Not.Null);
            Assert.That(createdElement.Key, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdElement.HasIdentity, Is.True);
            Assert.That(createdElement.Name, Is.EqualTo("Test Create"));
            Assert.That(createdElement.GetValue<string>("title"), Is.EqualTo("The title value"));
            Assert.That(createdElement.GetValue<string>("text"), Is.EqualTo("The text value"));
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
        Assert.That(result.Success, Is.True);

        var element = await ElementEditingService.GetAsync(elementKey);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.ParentId, Is.EqualTo(container.Id));

        var children = GetFolderChildren(containerKey);
        Assert.That(children, Has.Length.EqualTo(1));
        Assert.That(children[0].Key, Is.EqualTo(elementKey));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content!.HasIdentity, Is.True);
        Assert.That(result.Result.Content!.GetValue<string>("title"), Is.EqualTo(null));
        Assert.That(result.Result.Content!.GetValue<string>("text"), Is.EqualTo(null));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(addValidProperties ? ContentEditingOperationStatus.Success : ContentEditingOperationStatus.PropertyValidationError));
        Assert.That(result.Result, Is.Not.Null);

        if (addValidProperties is false)
        {
            Assert.That(result.Result.ValidationResult.ValidationErrors.Count(), Is.EqualTo(2));
            Assert.That(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "title" && v.ErrorMessages.Length == 1), Is.Not.Null);
            Assert.That(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "text" && v.ErrorMessages.Length == 1), Is.Not.Null);
        }

        // NOTE: creation must be successful, even if the mandatory property is missing (publishing however should not!)
        Assert.That(result.Result.Content!.HasIdentity, Is.True);
        Assert.That(result.Result.Content!.GetValue<string>("title"), Is.EqualTo(titleValue));
        Assert.That(result.Result.Content!.GetValue<string>("text"), Is.EqualTo(textValue));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result.Content, Is.Not.Null);
        Assert.That(result.Result.Content.HasIdentity, Is.True);
        Assert.That(result.Result.Content.Key, Is.EqualTo(key));

        var element = await ElementEditingService.GetAsync(key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Id, Is.EqualTo(result.Result.Content.Id));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result.Content, Is.Not.Null);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ElementEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IElement? createdElement)
        {
            Assert.That(createdElement, Is.Not.Null);
            Assert.That(createdElement.GetCultureName("en-US"), Is.EqualTo("The English Name"));
            Assert.That(createdElement.GetCultureName("da-DK"), Is.EqualTo("The Danish Name"));
            Assert.That(createdElement.GetValue<string>("invariantTitle"), Is.EqualTo("The Invariant Title"));
            Assert.That(createdElement.GetValue<string>("variantTitle", "en-US"), Is.EqualTo("The English Title"));
            Assert.That(createdElement.GetValue<string>("variantTitle", "da-DK"), Is.EqualTo("The Danish Title"));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result.Content, Is.Not.Null);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ElementEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IElement? createdElement)
        {
            Assert.That(createdElement, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(createdElement.Name, Is.EqualTo("The Name"));
                Assert.That(createdElement.GetValue<string>("invariantTitle"), Is.EqualTo("The Invariant Title"));
                Assert.That(createdElement.GetValue<string>("variantTitle", segment: null), Is.EqualTo("The Default Title"));
                Assert.That(createdElement.GetValue<string>("variantTitle", segment: "seg-1"), Is.EqualTo("The Seg-1 Title"));
                Assert.That(createdElement.GetValue<string>("variantTitle", segment: "seg-2"), Is.EqualTo("The Seg-2 Title"));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result.Content, Is.Not.Null);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ElementEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IElement? createdElement)
        {
            Assert.That(createdElement, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(createdElement.GetCultureName("en-US"), Is.EqualTo("The English Name"));
                Assert.That(createdElement.GetCultureName("da-DK"), Is.EqualTo("The Danish Name"));
                Assert.That(createdElement.GetValue<string>("invariantTitle"), Is.EqualTo("The Invariant Title"));
                Assert.That(createdElement.GetValue<string>("variantTitle", culture: "en-US", segment: null), Is.EqualTo("The Default Title in English"));
                Assert.That(createdElement.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"), Is.EqualTo("The Seg-1 Title in English"));
                Assert.That(createdElement.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"), Is.EqualTo("The Seg-2 Title in English"));
                Assert.That(createdElement.GetValue<string>("variantTitle", culture: "da-DK", segment: null), Is.EqualTo("The Default Title in Danish"));
                Assert.That(createdElement.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"), Is.EqualTo("The Seg-1 Title in Danish"));
                Assert.That(createdElement.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"), Is.EqualTo("The Seg-2 Title in Danish"));
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ContentTypeNotFound));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.PropertyTypeNotFound));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.PropertyTypeNotFound));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_Element_Based_On_NonElement_ContentType()
    {
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        Assert.That(contentType.IsElement, Is.False);
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotAllowed));
    }

    [Test]
    public async Task Cannot_Create_Element_Based_On_ContentType_Not_Allowed_In_Library()
    {
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.IsElement = true;
        contentType.AllowedInLibrary = false;
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotAllowed));
    }
}
