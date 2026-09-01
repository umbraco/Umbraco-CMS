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
        Assert.That(content.Published, Is.False);

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
        Assert.That(result.Success, Is.True);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdateAndPublish(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.That(updatedContent.Published, Is.True);
            Assert.That(updatedContent.Name, Is.EqualTo("Updated Name"));
            Assert.That(updatedContent.GetValue<string>("title", published: true), Is.EqualTo("The updated title"));
            Assert.That(updatedContent.GetValue<string>("text", published: true), Is.EqualTo("The updated text"));
        }
    }

    [Test]
    public async Task Can_UpdateAndPublish_Culture_Variant()
    {
        var content = await CreateCultureVariantContent();
        Assert.That(content.Published, Is.False);

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
        Assert.That(result.Success, Is.True);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdateAndPublish(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.That(updatedContent.Published, Is.True);
            Assert.That(updatedContent.IsCulturePublished("en-US"), Is.True);
            Assert.That(updatedContent.IsCulturePublished("da-DK"), Is.True);
            Assert.That(updatedContent.GetPublishName("en-US"), Is.EqualTo("Updated English Name"));
            Assert.That(updatedContent.GetPublishName("da-DK"), Is.EqualTo("Updated Danish Name"));
            Assert.That(updatedContent.GetValue<string>("invariantTitle", published: true), Is.EqualTo("Updated invariant"));
            Assert.That(updatedContent.GetValue<string>("variantTitle", "en-US", published: true), Is.EqualTo("Updated English"));
            Assert.That(updatedContent.GetValue<string>("variantTitle", "da-DK", published: true), Is.EqualTo("Updated Danish"));
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
        Assert.That(result.Success, Is.True);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdateAndPublish(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.That(updatedContent.IsCulturePublished("en-US"), Is.True);
            Assert.That(updatedContent.IsCulturePublished("da-DK"), Is.False);

            // both values should still be saved
            Assert.That(updatedContent.GetValue<string>("variantTitle", "en-US", published: true), Is.EqualTo("Updated English"));
            Assert.That(updatedContent.GetValue<string>("variantTitle", "da-DK", published: true), Is.Not.EqualTo("Updated Danish"));
            Assert.That(updatedContent.GetValue<string>("variantTitle", "da-DK", published: false), Is.EqualTo("Updated Danish"));
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
        Assert.That(createResult.Success, Is.True);
        var content = createResult.Result.Content!;
        Assert.That(content.Published, Is.True);

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
        Assert.That(result.Success, Is.True);

        var updatedContent = await ContentEditingService.GetAsync(content.Key);
        Assert.That(updatedContent, Is.Not.Null);
        Assert.That(updatedContent.Published, Is.True);
        Assert.That(updatedContent.Name, Is.EqualTo("Republished Name"));
        Assert.That(updatedContent.GetValue<string>("title", published: true), Is.EqualTo("Republished title"));
        Assert.That(updatedContent.GetValue<string>("text", published: true), Is.EqualTo("Republished text"));
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
        Assert.That(createResult.Success, Is.True);
        var content = createResult.Result.Content!;
        Assert.That(content.TemplateId, Is.EqualTo(template.Id));

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
        Assert.That(result.Success, Is.True);

        var updatedContent = await ContentEditingService.GetAsync(content.Key);
        Assert.That(updatedContent, Is.Not.Null);
        Assert.That(updatedContent.Published, Is.True);
        Assert.That(updatedContent.TemplateId, Is.EqualTo(template2.Id));
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotFound));
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.InvalidCulture));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result.Content!.Published, Is.True);

        // re-get and verify the label property was not changed
        var updatedContent = await ContentEditingService.GetAsync(content.Key);
        Assert.That(updatedContent, Is.Not.Null);
        Assert.That(updatedContent.GetValue<string>("label"), Is.EqualTo(labelValue));
        Assert.That(updatedContent.GetValue<string>("title"), Is.EqualTo("Updated title"));
    }
}
