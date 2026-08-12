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
        Assert.IsTrue(result.Success);
        VerifyCreateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyCreateAndPublish(await ContentEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreateAndPublish(IContent? content)
        {
            Assert.IsNotNull(content);
            Assert.IsTrue(content.HasIdentity);
            Assert.IsTrue(content.Published);
            Assert.AreEqual("Test Create And Publish", content.Name);
            Assert.AreEqual("The title", content.GetValue<string>("title", published: true));
            Assert.AreEqual("The text", content.GetValue<string>("text", published: true));
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
        Assert.IsTrue(result.Success);
        VerifyCreateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyCreateAndPublish(await ContentEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreateAndPublish(IContent? content)
        {
            Assert.IsNotNull(content);
            Assert.IsTrue(content.Published);
            Assert.IsTrue(content.IsCulturePublished("en-US"));
            Assert.IsTrue(content.IsCulturePublished("da-DK"));
            Assert.AreEqual("English Name", content.GetCultureName("en-US"));
            Assert.AreEqual("Danish Name", content.GetCultureName("da-DK"));
            Assert.AreEqual("The Invariant Title", content.GetValue<string>("invariantTitle"));
            Assert.AreEqual("The English Title", content.GetValue<string>("variantTitle", "en-US", published: true));
            Assert.AreEqual("The Danish Title", content.GetValue<string>("variantTitle", "da-DK", published: true));
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
        Assert.IsTrue(result.Success);
        VerifyCreateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyCreateAndPublish(await ContentEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreateAndPublish(IContent? content)
        {
            Assert.IsNotNull(content);
            Assert.IsTrue(content.IsCulturePublished("en-US"));
            Assert.IsFalse(content.IsCulturePublished("da-DK"));

            // both values should still be saved
            Assert.AreEqual("The English Title", content.GetValue<string>("variantTitle", "en-US", published: true));
            Assert.AreEqual("The Danish Title", content.GetValue<string>("variantTitle", "da-DK"));
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
        Assert.IsTrue(result.Success);

        var content = result.Result.Content!;
        Assert.IsTrue(content.Published);
        Assert.AreEqual(template.Id, content.TemplateId);
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
        Assert.IsTrue(result.Success);

        var content = result.Result.Content!;
        Assert.IsTrue(content.Published);
        Assert.AreEqual(explicitKey, content.Key);
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ContentTypeNotFound, result.Status);
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ParentNotFound, result.Status);
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.TemplateNotFound, result.Status);
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.TemplateNotAllowed, result.Status);
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch, result.Status);
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.InvalidCulture, result.Status);
    }
}
