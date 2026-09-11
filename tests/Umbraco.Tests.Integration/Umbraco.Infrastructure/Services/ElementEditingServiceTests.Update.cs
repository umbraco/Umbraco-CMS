using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [Test]
    public async Task Can_Update_Invariant()
    {
        var element = await CreateInvariantElement();

        var updateModel = new ElementUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
                new PropertyValueModel { Alias = "text", Value = "The updated text" }
            ],
        };

        var result = await ElementEditingService.UpdateAsync(element.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdate(IElement? updatedElement)
        {
            Assert.That(updatedElement, Is.Not.Null);
            Assert.That(updatedElement.Name, Is.EqualTo("Updated Name"));
            Assert.That(updatedElement.GetValue<string>("title"), Is.EqualTo("The updated title"));
            Assert.That(updatedElement.GetValue<string>("text"), Is.EqualTo("The updated text"));
        }
    }

    [Test]
    public async Task Can_Update_Culture_Variant()
    {
        var element = await CreateCultureVariantElement();

        var updateModel = new ElementUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish title", Culture = "da-DK" },
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" },
                new VariantModel { Culture = "da-DK", Name = "Updated Danish Name" }
            ],
        };

        var result = await ElementEditingService.UpdateAsync(element.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdate(IElement? updatedElement)
        {
            Assert.That(updatedElement, Is.Not.Null);
            Assert.That(updatedElement.GetCultureName("en-US"), Is.EqualTo("Updated English Name"));
            Assert.That(updatedElement.GetCultureName("da-DK"), Is.EqualTo("Updated Danish Name"));
            Assert.That(updatedElement.GetValue<string>("invariantTitle"), Is.EqualTo("The updated invariant title"));
            Assert.That(updatedElement.GetValue<string>("variantTitle", "en-US"), Is.EqualTo("The updated English title"));
            Assert.That(updatedElement.GetValue<string>("variantTitle", "da-DK"), Is.EqualTo("The updated Danish title"));
        }
    }

    [Test]
    public async Task Can_Update_Segment_Variant()
    {
        var element = await CreateSegmentVariantElement();

        var updateModel = new ElementUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated default title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-1 title", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-2 title", Segment = "seg-2" }
            ],
            Variants =
            [
                new VariantModel { Name = "The Updated Name" },
                new VariantModel { Segment = "seg-1", Name = "The Updated Name" },
                new VariantModel { Segment = "seg-2", Name = "The Updated Name" }
            ],
        };

        var result = await ElementEditingService.UpdateAsync(element.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdate(IElement? updatedElement)
        {
            Assert.That(updatedElement, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(updatedElement.Name, Is.EqualTo("The Updated Name"));
                Assert.That(updatedElement.GetValue<string>("invariantTitle"), Is.EqualTo("The updated invariant title"));
                Assert.That(updatedElement.GetValue<string>("variantTitle", segment: null), Is.EqualTo("The updated default title"));
                Assert.That(updatedElement.GetValue<string>("variantTitle", segment: "seg-1"), Is.EqualTo("The updated seg-1 title"));
                Assert.That(updatedElement.GetValue<string>("variantTitle", segment: "seg-2"), Is.EqualTo("The updated seg-2 title"));
            });
        }
    }

    [Test]
    public async Task Can_Update_Element_After_ContentType_Disallowed_From_Library()
    {
        var elementType = await CreateInvariantElementType();
        var element = await CreateInvariantElement(contentTypeKey: elementType.Key);

        // Disallow the content type from the library after the element has been created
        elementType.AllowedInLibrary = false;
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);

        var updateModel = new ElementUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
                new PropertyValueModel { Alias = "text", Value = "The updated text" }
            ],
        };

        var result = await ElementEditingService.UpdateAsync(element.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        var updatedElement = result.Result.Content;
        Assert.That(updatedElement, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updatedElement.Name, Is.EqualTo("Updated Name"));
            Assert.That(updatedElement.GetValue<string>("title"), Is.EqualTo("The updated title"));
            Assert.That(updatedElement.GetValue<string>("text"), Is.EqualTo("The updated text"));
        });
    }

    [Test]
    public async Task Can_Update_Culture_And_Segment_Variant()
    {
        var element = await CreateCultureAndSegmentVariantElement();

        var updateModel = new ElementUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated default title in English", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-1 title in English", Culture = "en-US", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-2 title in English", Culture = "en-US", Segment = "seg-2" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated default title in Danish", Culture = "da-DK" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-1 title in Danish", Culture = "da-DK", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-2 title in Danish", Culture = "da-DK", Segment = "seg-2" }
            ],
            Variants =
            [
                new VariantModel { Name = "The Updated English Name", Culture = "en-US" },
                new VariantModel { Name = "The Updated English Name", Culture = "en-US", Segment = "seg-1" },
                new VariantModel { Name = "The Updated English Name", Culture = "en-US", Segment = "seg-2" },
                new VariantModel { Name = "The Updated Danish Name", Culture = "da-DK" },
                new VariantModel { Name = "The Updated Danish Name", Culture = "da-DK", Segment = "seg-1" },
                new VariantModel { Name = "The Updated Danish Name", Culture = "da-DK", Segment = "seg-2" }
            ],
        };

        var result = await ElementEditingService.UpdateAsync(element.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdate(IElement? updatedElement)
        {
            Assert.That(updatedElement, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(updatedElement.GetCultureName("en-US"), Is.EqualTo("The Updated English Name"));
                Assert.That(updatedElement.GetCultureName("da-DK"), Is.EqualTo("The Updated Danish Name"));

                Assert.That(updatedElement.GetValue<string>("invariantTitle"), Is.EqualTo("The updated invariant title"));
                Assert.That(updatedElement.GetValue<string>("variantTitle", culture: "en-US", segment: null), Is.EqualTo("The updated default title in English"));
                Assert.That(updatedElement.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"), Is.EqualTo("The updated seg-1 title in English"));
                Assert.That(updatedElement.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"), Is.EqualTo("The updated seg-2 title in English"));
                Assert.That(updatedElement.GetValue<string>("variantTitle", culture: "da-DK", segment: null), Is.EqualTo("The updated default title in Danish"));
                Assert.That(updatedElement.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"), Is.EqualTo("The updated seg-1 title in Danish"));
                Assert.That(updatedElement.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"), Is.EqualTo("The updated seg-2 title in Danish"));
            });
        }
    }
}
