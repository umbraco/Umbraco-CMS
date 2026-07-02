using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [Test]
    public async Task Can_CreateAndPublish_Invariant_Element()
    {
        var elementType = await CreateInvariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = null,
            Variants =
            [
                new VariantModel { Name = "Test Create And Publish" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title" },
                new PropertyValueModel { Alias = "text", Value = "The text" }
            ],
        };

        var result = await ElementEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyCreateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyCreateAndPublish(await ElementEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreateAndPublish(IElement? element)
        {
            Assert.IsNotNull(element);
            Assert.IsTrue(element.HasIdentity);
            Assert.IsTrue(element.Published);
            Assert.AreEqual("Test Create And Publish", element.Name);
            Assert.AreEqual("The title", element.GetValue<string>("title", published: true));
            Assert.AreEqual("The text", element.GetValue<string>("text", published: true));
        }
    }

    [Test]
    public async Task Can_CreateAndPublish_Culture_Variant_All_Cultures()
    {
        var elementType = await CreateVariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = null,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The English Title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Danish Title", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "English Name" },
                new VariantModel { Culture = "da-DK", Name = "Danish Name" }
            ],
        };

        var result = await ElementEditingService.CreateAndPublishAsync(createModel, ["en-US", "da-DK"], Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyCreateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyCreateAndPublish(await ElementEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreateAndPublish(IElement? element)
        {
            Assert.IsNotNull(element);
            Assert.IsTrue(element.Published);
            Assert.IsTrue(element.IsCulturePublished("en-US"));
            Assert.IsTrue(element.IsCulturePublished("da-DK"));
            Assert.AreEqual("English Name", element.GetCultureName("en-US"));
            Assert.AreEqual("Danish Name", element.GetCultureName("da-DK"));
            Assert.AreEqual("The Invariant Title", element.GetValue<string>("invariantTitle"));
            Assert.AreEqual("The English Title", element.GetValue<string>("variantTitle", "en-US", published: true));
            Assert.AreEqual("The Danish Title", element.GetValue<string>("variantTitle", "da-DK", published: true));
        }
    }

    [Test]
    public async Task Can_CreateAndPublish_Culture_Variant_Single_Culture()
    {
        var elementType = await CreateVariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = null,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The English Title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Danish Title", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "English Name" },
                new VariantModel { Culture = "da-DK", Name = "Danish Name" }
            ],
        };

        var result = await ElementEditingService.CreateAndPublishAsync(createModel, ["en-US"], Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyCreateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyCreateAndPublish(await ElementEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreateAndPublish(IElement? element)
        {
            Assert.IsNotNull(element);
            Assert.IsTrue(element.IsCulturePublished("en-US"));
            Assert.IsFalse(element.IsCulturePublished("da-DK"));

            // both values should still be saved
            Assert.AreEqual("The English Title", element.GetValue<string>("variantTitle", "en-US", published: true));
            Assert.AreEqual("The Danish Title", element.GetValue<string>("variantTitle", "da-DK"));
        }
    }

    [Test]
    public async Task Cannot_CreateAndPublish_Without_Content_Type()
    {
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = Guid.NewGuid(),
            ParentKey = null,
            Variants =
            [
                new VariantModel { Name = "Test" }
            ],
        };

        var result = await ElementEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ContentTypeNotFound, result.Status);
    }
}
