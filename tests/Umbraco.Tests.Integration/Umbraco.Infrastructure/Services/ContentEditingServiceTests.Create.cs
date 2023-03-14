﻿using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Create_At_Root(bool allowedAtRoot)
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = allowedAtRoot;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "bodyText", Value = "The body text" }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        if (allowedAtRoot)
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
            VerifyCreate(result.Result);

            // re-get and re-test
            VerifyCreate(await ContentEditingService.GetAsync(result.Result!.Key));

            void VerifyCreate(IContent? createdContent)
            {
                Assert.IsNotNull(createdContent);
                Assert.AreNotEqual(Guid.Empty, createdContent.Key);
                Assert.IsTrue(createdContent.HasIdentity);
                Assert.AreEqual("Test Create", createdContent.Name);
                Assert.AreEqual("The title value", createdContent.GetValue<string>("title"));
                Assert.AreEqual("The body text", createdContent.GetValue<string>("bodyText"));
            }
        }
        else
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
            Assert.IsNull(result.Result);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Create_As_Child(bool allowedAsChild)
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var childContentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        childContentType.AllowedAsRoot = false;
        ContentTypeService.Save(childContentType);

        var rootContentType = ContentTypeBuilder.CreateBasicContentType();
        rootContentType.AllowedAsRoot = true;
        if (allowedAsChild)
        {
            rootContentType.AllowedContentTypes = new[]
            {
                new ContentTypeSort(new Lazy<int>(() => childContentType.Id), childContentType.Key, 1, childContentType.Alias)
            };
        }
        ContentTypeService.Save(rootContentType);

        var rootKey = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
        {
            ContentTypeKey = rootContentType.Key, InvariantName = "Root", ParentKey = Constants.System.RootKey,
        },
            Constants.Security.SuperUserKey)).Result.Key;

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = childContentType.Key,
            TemplateKey = template.Key,
            ParentKey = rootKey,
            InvariantName = "Test Create Child",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The child title value" },
                new PropertyValueModel { Alias = "bodyText", Value = "The child body text" }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        if (allowedAsChild)
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

            var createdContent = result.Result;
            Assert.NotNull(createdContent);
            Assert.AreNotEqual(Guid.Empty, createdContent.Key);
            Assert.IsTrue(createdContent.HasIdentity);
            Assert.AreEqual("Test Create Child", createdContent.Name);
            Assert.AreEqual("The child title value", createdContent.GetValue<string>("title"));
            Assert.AreEqual("The child body text", createdContent.GetValue<string>("bodyText"));
        }
        else
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
            Assert.IsNull(result.Result);
        }
    }

    [Test]
    public async Task Can_Create_Without_Template()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsTrue(result.Result.HasIdentity);
        Assert.AreEqual("The title value", result.Result.GetValue<string>("title"));
    }

    [Test]
    public async Task Can_Create_Without_Properties()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create"
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsTrue(result.Result.HasIdentity);
        Assert.AreEqual(null, result.Result.GetValue<string>("title"));
        Assert.AreEqual(null, result.Result.GetValue<string>("bodyText"));
    }

    [Test]
    public async Task Cannot_Create_With_Non_Existing_Parent()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Guid.NewGuid(),
            InvariantName = "Test Create"
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ParentNotFound, result.Status);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task Cannot_Create_Without_Content_Type()
    {
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = Guid.NewGuid(),
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create",
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ContentTypeNotFound, result.Status);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task Cannot_Create_With_Invalid_Template()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create"
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.TemplateNotAllowed, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsFalse(result.Result.HasIdentity);
    }

    [Test]
    public async Task Cannot_Create_With_Non_Existing_Template()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = Guid.NewGuid(),
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create"
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.TemplateNotFound, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsFalse(result.Result.HasIdentity);
    }

    [Test]
    public async Task Cannot_Create_With_Non_Existing_Properties()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "no_such_property", Value = "No such property value" },
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyTypeNotFound, result.Status);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task Cannot_Create_Invariant_Content_Without_Name()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = null,
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch, result.Status);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task Cannot_Create_With_Variant_Property_Value_For_Invariant_Content()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            },
            Variants = new []
            {
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "The English Name",
                    Properties = new []
                    {
                        new PropertyValueModel { Alias = "bodyText", Value = "The body text value" }
                    }
                }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch, result.Status);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task Can_Create_Culture_Variant()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" }
            },
            Variants = new[]
            {
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "The English Name",
                    Properties = new[]
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The English Title" }
                    }
                },
                new VariantModel
                {
                    Culture = "da-DK",
                    Name = "The Danish Name",
                    Properties = new[]
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The Danish Title" }
                    }
                }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyCreate(result.Result);

        // re-get and re-test
        VerifyCreate(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyCreate(IContent? createdContent)
        {
            Assert.IsNotNull(createdContent);
            Assert.AreEqual("The English Name", createdContent.GetCultureName("en-US"));
            Assert.AreEqual("The Danish Name", createdContent.GetCultureName("da-DK"));
            Assert.AreEqual("The Invariant Title", createdContent.GetValue<string>("invariantTitle"));
            Assert.AreEqual("The English Title", createdContent.GetValue<string>("variantTitle", "en-US"));
            Assert.AreEqual("The Danish Title", createdContent.GetValue<string>("variantTitle", "da-DK"));
        }
    }

    [Test]
    public async Task Cannot_Create_With_Invariant_Property_Value_For_Variant_Content()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Variant Title" }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyTypeNotFound, result.Status);
        Assert.IsNull(result.Result);
    }
}
