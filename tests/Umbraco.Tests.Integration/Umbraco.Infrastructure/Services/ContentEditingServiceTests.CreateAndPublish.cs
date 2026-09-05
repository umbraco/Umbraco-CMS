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
    public async Task Can_CreateAndPublish_Invariant_Content()
    {
        var contentType = await CreateInvariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
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

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        VerifyCreateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyCreateAndPublish(await ContentEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreateAndPublish(IContent? content)
        {
            Assert.That(content, Is.Not.Null);
            Assert.That(content.HasIdentity, Is.True);
            Assert.That(content.Published, Is.True);
            Assert.That(content.Name, Is.EqualTo("Test Create And Publish"));
            Assert.That(content.GetValue<string>("title", published: true), Is.EqualTo("The title"));
            Assert.That(content.GetValue<string>("text", published: true), Is.EqualTo("The text"));
        }
    }

    [Test]
    public async Task Can_CreateAndPublish_Culture_Variant_All_Cultures()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
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

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, ["en-US", "da-DK"], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        VerifyCreateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyCreateAndPublish(await ContentEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreateAndPublish(IContent? content)
        {
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Published, Is.True);
            Assert.That(content.IsCulturePublished("en-US"), Is.True);
            Assert.That(content.IsCulturePublished("da-DK"), Is.True);
            Assert.That(content.GetCultureName("en-US"), Is.EqualTo("English Name"));
            Assert.That(content.GetCultureName("da-DK"), Is.EqualTo("Danish Name"));
            Assert.That(content.GetValue<string>("invariantTitle"), Is.EqualTo("The Invariant Title"));
            Assert.That(content.GetValue<string>("variantTitle", "en-US", published: true), Is.EqualTo("The English Title"));
            Assert.That(content.GetValue<string>("variantTitle", "da-DK", published: true), Is.EqualTo("The Danish Title"));
        }
    }

    [Test]
    public async Task Can_CreateAndPublish_Culture_Variant_Single_Culture()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
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

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, ["en-US"], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        VerifyCreateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyCreateAndPublish(await ContentEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreateAndPublish(IContent? content)
        {
            Assert.That(content, Is.Not.Null);
            Assert.That(content.IsCulturePublished("en-US"), Is.True);
            Assert.That(content.IsCulturePublished("da-DK"), Is.False);

            // both values should still be saved
            Assert.That(content.GetValue<string>("variantTitle", "en-US", published: true), Is.EqualTo("The English Title"));
            Assert.That(content.GetValue<string>("variantTitle", "da-DK"), Is.EqualTo("The Danish Title"));
        }
    }

    [Test]
    public async Task Can_CreateAndPublish_With_Template()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "With Template" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title" },
                new PropertyValueModel { Alias = "bodyText", Value = "The body" }
            ],
        };

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var content = result.Result.Content!;
        Assert.That(content.Published, Is.True);
        Assert.That(content.TemplateId, Is.EqualTo(template.Id));
    }

    [Test]
    public async Task Can_CreateAndPublish_With_Explicit_Key()
    {
        var contentType = await CreateInvariantContentType();
        var explicitKey = Guid.NewGuid();

        var createModel = new ContentCreateModel
        {
            Key = explicitKey,
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Explicit Key" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title" }
            ],
        };

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var content = result.Result.Content!;
        Assert.That(content.Published, Is.True);
        Assert.That(content.Key, Is.EqualTo(explicitKey));
    }

    [Test]
    public async Task Cannot_CreateAndPublish_Without_Content_Type()
    {
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = Guid.NewGuid(),
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test" }
            ],
        };

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ContentTypeNotFound));
    }

    [Test]
    public async Task Cannot_CreateAndPublish_With_Non_Existing_Parent()
    {
        var contentType = await CreateInvariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Guid.NewGuid(),
            Variants =
            [
                new VariantModel { Name = "Test" }
            ],
        };

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ParentNotFound));
    }

    [Test]
    public async Task Cannot_CreateAndPublish_With_Non_Existing_Template()
    {
        var contentType = await CreateInvariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = Guid.NewGuid(),
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test" }
            ],
        };

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.TemplateNotFound));
    }

    [Test]
    public async Task Cannot_CreateAndPublish_With_Disallowed_Template()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        // content type without allowed templates
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test" }
            ],
        };

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.TemplateNotAllowed));
    }

    [Test]
    public async Task Cannot_CreateAndPublish_Invariant_Without_Name()
    {
        var contentType = await CreateInvariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title" }
            ],
        };

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch));
    }

    [Test]
    public async Task Cannot_CreateAndPublish_With_Invalid_Culture()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
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

        var result = await ContentEditingService.CreateAndPublishAsync(createModel, [], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.InvalidCulture));
    }
}
