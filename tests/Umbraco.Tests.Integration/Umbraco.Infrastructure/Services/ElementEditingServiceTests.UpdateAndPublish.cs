using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [Test]
    public async Task Can_UpdateAndPublish_Invariant_Element()
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

        var result = await ElementEditingService.UpdateAndPublishAsync(element.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdateAndPublish(IElement? updatedElement)
        {
            Assert.IsNotNull(updatedElement);
            Assert.IsTrue(updatedElement.Published);
            Assert.AreEqual("Updated Name", updatedElement.Name);
            Assert.AreEqual("The updated title", updatedElement.GetValue<string>("title", published: true));
            Assert.AreEqual("The updated text", updatedElement.GetValue<string>("text", published: true));
        }
    }

    [Test]
    public async Task Can_UpdateAndPublish_Culture_Variant_Single_Culture()
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

        var result = await ElementEditingService.UpdateAndPublishAsync(element.Key, updateModel, new HashSet<string> { "en-US" }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyUpdateAndPublish(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdateAndPublish(IElement? updatedElement)
        {
            Assert.IsNotNull(updatedElement);
            Assert.IsTrue(updatedElement.IsCulturePublished("en-US"));
            Assert.IsFalse(updatedElement.IsCulturePublished("da-DK"));

            // both cultures should be saved even though only one was published
            Assert.AreEqual("The updated English title", updatedElement.GetValue<string>("variantTitle", "en-US", published: true));
            Assert.AreEqual("The updated Danish title", updatedElement.GetValue<string>("variantTitle", "da-DK"));
        }
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_Non_Existing_Element()
    {
        var updateModel = new ElementUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
        };

        var result = await ElementEditingService.UpdateAndPublishAsync(Guid.NewGuid(), updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status.ContentEditingOperationStatus);
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_With_Invalid_Property_Values()
    {
        var element = await CreateCultureVariantElement();

        // "variantTitle" is mandatory, so publishing is rejected while saving is not
        var updateModel = new ElementUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = null, Culture = "en-US" },
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" },
                new VariantModel { Culture = "da-DK", Name = "Updated Danish Name" }
            ],
        };

        var result = await ElementEditingService.UpdateAndPublishAsync(element.Key, updateModel, new HashSet<string> { "en-US" }, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
            Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Status.ContentPublishingOperationStatus);
            Assert.AreEqual(new[] { "variantTitle" }, result.Result.InvalidPropertyAliases.ToArray());
        });

        var updated = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNotNull(updated);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Updated English Name", updated.GetCultureName("en-US"));
            Assert.IsFalse(updated.IsCulturePublished("en-US"));
            Assert.AreEqual("The updated invariant title", updated.GetValue<string>("invariantTitle"));
        });
    }

    [TestCase("*")]
    [TestCase("zz-ZZ")]
    public async Task Cannot_UpdateAndPublish_With_An_Invalid_Culture_To_Publish(string culture)
    {
        var element = await CreateCultureVariantElement();

        var updateModel = new ElementUpdateModel
        {
            Properties = [new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title", Culture = "en-US" }],
            Variants = [new VariantModel { Culture = "en-US", Name = "Updated English Name" }],
        };

        var result = await ElementEditingService.UpdateAndPublishAsync(element.Key, updateModel, new HashSet<string> { culture }, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.InvalidCulture, result.Status.ContentEditingOperationStatus);
            Assert.IsNull(result.Status.ContentPublishingOperationStatus);
        });
    }
}
