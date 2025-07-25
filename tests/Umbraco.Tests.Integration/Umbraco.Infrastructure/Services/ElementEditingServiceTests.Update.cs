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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdate(IElement? updatedElement)
        {
            Assert.IsNotNull(updatedElement);
            Assert.AreEqual("Updated Name", updatedElement.Name);
            Assert.AreEqual("The updated title", updatedElement.GetValue<string>("title"));
            Assert.AreEqual("The updated text", updatedElement.GetValue<string>("text"));
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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdate(IElement? updatedElement)
        {
            Assert.IsNotNull(updatedElement);
            Assert.AreEqual("Updated English Name", updatedElement.GetCultureName("en-US"));
            Assert.AreEqual("Updated Danish Name", updatedElement.GetCultureName("da-DK"));
            Assert.AreEqual("The updated invariant title", updatedElement.GetValue<string>("invariantTitle"));
            Assert.AreEqual("The updated English title", updatedElement.GetValue<string>("variantTitle", "en-US"));
            Assert.AreEqual("The updated Danish title", updatedElement.GetValue<string>("variantTitle", "da-DK"));
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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdate(IElement? updatedElement)
        {
            Assert.IsNotNull(updatedElement);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The Updated Name", updatedElement.Name);
                Assert.AreEqual("The updated invariant title", updatedElement.GetValue<string>("invariantTitle"));
                Assert.AreEqual("The updated default title", updatedElement.GetValue<string>("variantTitle", segment: null));
                Assert.AreEqual("The updated seg-1 title", updatedElement.GetValue<string>("variantTitle", segment: "seg-1"));
                Assert.AreEqual("The updated seg-2 title", updatedElement.GetValue<string>("variantTitle", segment: "seg-2"));
            });
        }
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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdate(IElement? updatedElement)
        {
            Assert.IsNotNull(updatedElement);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The Updated English Name", updatedElement.GetCultureName("en-US"));
                Assert.AreEqual("The Updated Danish Name", updatedElement.GetCultureName("da-DK"));

                Assert.AreEqual("The updated invariant title", updatedElement.GetValue<string>("invariantTitle"));
                Assert.AreEqual("The updated default title in English", updatedElement.GetValue<string>("variantTitle", culture: "en-US", segment: null));
                Assert.AreEqual("The updated seg-1 title in English", updatedElement.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"));
                Assert.AreEqual("The updated seg-2 title in English", updatedElement.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"));
                Assert.AreEqual("The updated default title in Danish", updatedElement.GetValue<string>("variantTitle", culture: "da-DK", segment: null));
                Assert.AreEqual("The updated seg-1 title in Danish", updatedElement.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"));
                Assert.AreEqual("The updated seg-2 title in Danish", updatedElement.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"));
            });
        }
    }
}
