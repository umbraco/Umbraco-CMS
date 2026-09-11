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

        var result = await ElementEditingService.UpdateAndPublishAsync(element.Key, updateModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdateAndPublish(IElement? updatedElement)
        {
            Assert.That(updatedElement, Is.Not.Null);
            Assert.That(updatedElement.Published, Is.True);
            Assert.That(updatedElement.Name, Is.EqualTo("Updated Name"));
            Assert.That(updatedElement.GetValue<string>("title", published: true), Is.EqualTo("The updated title"));
            Assert.That(updatedElement.GetValue<string>("text", published: true), Is.EqualTo("The updated text"));
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

        var result = await ElementEditingService.UpdateAndPublishAsync(element.Key, updateModel, ["en-US"], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        VerifyUpdateAndPublish(await ElementEditingService.GetAsync(element.Key));

        void VerifyUpdateAndPublish(IElement? updatedElement)
        {
            Assert.That(updatedElement, Is.Not.Null);
            Assert.That(updatedElement.IsCulturePublished("en-US"), Is.True);
            Assert.That(updatedElement.IsCulturePublished("da-DK"), Is.False);

            // both cultures should be saved even though only one was published
            Assert.That(updatedElement.GetValue<string>("variantTitle", "en-US", published: true), Is.EqualTo("The updated English title"));
            Assert.That(updatedElement.GetValue<string>("variantTitle", "da-DK"), Is.EqualTo("The updated Danish title"));
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

        var result = await ElementEditingService.UpdateAndPublishAsync(Guid.NewGuid(), updateModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotFound));
    }
}
