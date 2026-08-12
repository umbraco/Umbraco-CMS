using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [Test]
    public async Task Can_UpdateAndPublish_Invariant()
    {
        var content = await CreateInvariantContent();
        Assert.IsFalse(content.Published);

        var updateModel = new ContentUpdateModel
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

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, [], Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdateAndPublish(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.IsTrue(updatedContent.Published);
            Assert.AreEqual("Updated Name", updatedContent.Name);
            Assert.AreEqual("The updated title", updatedContent.GetValue<string>("title", published: true));
            Assert.AreEqual("The updated text", updatedContent.GetValue<string>("text", published: true));
        }
    }

    [Test]
    public async Task Can_UpdateAndPublish_Culture_Variant()
    {
        var content = await CreateCultureVariantContent();
        Assert.IsFalse(content.Published);

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "Updated invariant" },
                new PropertyValueModel { Alias = "variantTitle", Value = "Updated English", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "Updated Danish", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" },
                new VariantModel { Culture = "da-DK", Name = "Updated Danish Name" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, ["en-US", "da-DK"], Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdateAndPublish(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.IsTrue(updatedContent.Published);
            Assert.IsTrue(updatedContent.IsCulturePublished("en-US"));
            Assert.IsTrue(updatedContent.IsCulturePublished("da-DK"));
            Assert.AreEqual("Updated English Name", updatedContent.GetPublishName("en-US"));
            Assert.AreEqual("Updated Danish Name", updatedContent.GetPublishName("da-DK"));
            Assert.AreEqual("Updated invariant", updatedContent.GetValue<string>("invariantTitle", published: true));
            Assert.AreEqual("Updated English", updatedContent.GetValue<string>("variantTitle", "en-US", published: true));
            Assert.AreEqual("Updated Danish", updatedContent.GetValue<string>("variantTitle", "da-DK", published: true));
        }
    }

    [Test]
    public async Task Can_UpdateAndPublish_Single_Culture()
    {
        var content = await CreateCultureVariantContent();

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "Updated invariant" },
                new PropertyValueModel { Alias = "variantTitle", Value = "Updated English", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "Updated Danish", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" },
                new VariantModel { Culture = "da-DK", Name = "Updated Danish Name" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, ["en-US"], Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdateAndPublish(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.IsTrue(updatedContent.IsCulturePublished("en-US"));
            Assert.IsFalse(updatedContent.IsCulturePublished("da-DK"));

            // both values should still be saved
            Assert.AreEqual("Updated English", updatedContent.GetValue<string>("variantTitle", "en-US", published: true));
            Assert.AreNotEqual("Updated Danish", updatedContent.GetValue<string>("variantTitle", "da-DK", published: true));
            Assert.AreEqual("Updated Danish", updatedContent.GetValue<string>("variantTitle", "da-DK", published: false));
        }
    }

    [Test]
    public async Task Can_UpdateAndPublish_Already_Published_Content()
    {
        var contentType = await CreateInvariantContentType();

        // create and publish initially
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Original Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Original title" },
                new PropertyValueModel { Alias = "text", Value = "Original text" }
            ],
        };

        var createResult = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);
        var content = createResult.Result.Content!;
        Assert.IsTrue(content.Published);

        // now update and republish
        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Republished Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Republished title" },
                new PropertyValueModel { Alias = "text", Value = "Republished text" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, [], Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var updatedContent = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updatedContent);
        Assert.IsTrue(updatedContent.Published);
        Assert.AreEqual("Republished Name", updatedContent.Name);
        Assert.AreEqual("Republished title", updatedContent.GetValue<string>("title", published: true));
        Assert.AreEqual("Republished text", updatedContent.GetValue<string>("text", published: true));
    }

    [Test]
    public async Task Can_UpdateAndPublish_Template()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var template2 = TemplateBuilder.CreateTextPageTemplate("altTemplate");
        await TemplateService.CreateAsync(template2, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedTemplates = new[] { template, template2 };
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Template Test" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Title" },
                new PropertyValueModel { Alias = "bodyText", Value = "Body" }
            ],
        };

        var createResult = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);
        var content = createResult.Result.Content!;
        Assert.AreEqual(template.Id, content.TemplateId);

        // update with different template
        var updateModel = new ContentUpdateModel
        {
            TemplateKey = template2.Key,
            Variants =
            [
                new VariantModel { Name = "Template Test Updated" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Updated Title" },
                new PropertyValueModel { Alias = "bodyText", Value = "Updated Body" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, [], Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var updatedContent = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updatedContent);
        Assert.IsTrue(updatedContent.Published);
        Assert.AreEqual(template2.Id, updatedContent.TemplateId);
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_Non_Existing_Content()
    {
        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Test" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(Guid.NewGuid(), updateModel, [], Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_With_Invalid_Culture()
    {
        var content = await CreateCultureVariantContent();

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "Invariant" },
                new PropertyValueModel { Alias = "variantTitle", Value = "English", Culture = "en-us" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-us", Name = "English" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, ["en-us"], Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.InvalidCulture, result.Status);
    }

    [Test]
    public async Task Can_UpdateAndPublish_Readonly_Property_Is_Preserved()
    {
        var content = await CreateInvariantContent();
        var labelValue = content.GetValue<string>("label");

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Updated title" },
                new PropertyValueModel { Alias = "text", Value = "Updated text" },
                new PropertyValueModel { Alias = "label", Value = "Trying to change label" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, [], Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Result.Content!.Published);

        // re-get and verify the label property was not changed
        var updatedContent = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updatedContent);
        Assert.AreEqual(labelValue, updatedContent.GetValue<string>("label"));
        Assert.AreEqual("Updated title", updatedContent.GetValue<string>("title"));
    }
}
