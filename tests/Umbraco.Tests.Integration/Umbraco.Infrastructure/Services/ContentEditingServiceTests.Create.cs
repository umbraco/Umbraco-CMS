using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_At_Root(bool allowedAtRoot)
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
            VerifyCreate(result.Result.Content);

            // re-get and re-test
            VerifyCreate(await ContentEditingService.GetAsync(result.Result.Content!.Key));

            void VerifyCreate(IContent? createdContent)
            {
                Assert.IsNotNull(createdContent);
                Assert.AreNotEqual(Guid.Empty, createdContent.Key);
                Assert.IsTrue(createdContent.HasIdentity);
                Assert.AreEqual("Test Create", createdContent.Name);
                Assert.AreEqual("The title value", createdContent.GetValue<string>("title"));
                AssertBodyTextEquals("The body text", createdContent);
            }
        }
        else
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
            Assert.IsNotNull(result.Result);
            Assert.IsNull(result.Result.Content);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_As_Child(bool allowedAsChild)
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
                new ContentTypeSort(childContentType.Key, 1, childContentType.Alias)
            };
        }
        ContentTypeService.Save(rootContentType);

        var rootKey = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = rootContentType.Key, InvariantName = "Root", ParentKey = Constants.System.RootKey,
            },
            Constants.Security.SuperUserKey)).Result.Content!.Key;

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

            var createdContent = result.Result.Content;
            Assert.NotNull(createdContent);
            Assert.AreNotEqual(Guid.Empty, createdContent.Key);
            Assert.IsTrue(createdContent.HasIdentity);
            Assert.AreEqual("Test Create Child", createdContent.Name);
            Assert.AreEqual("The child title value", createdContent.GetValue<string>("title"));
            AssertBodyTextEquals("The child body text", createdContent);
        }
        else
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
            Assert.IsNotNull(result.Result);
            Assert.IsNull(result.Result.Content);
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
        Assert.IsTrue(result.Result.Content!.HasIdentity);
        Assert.AreEqual("The title value", result.Result.Content!.GetValue<string>("title"));
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
        Assert.IsTrue(result.Result.Content!.HasIdentity);
        Assert.AreEqual(null, result.Result.Content!.GetValue<string>("title"));
        Assert.AreEqual(null, result.Result.Content!.GetValue<string>("bodyText"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_With_Property_Validation(bool addValidProperties)
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        contentType.PropertyTypes.First(pt => pt.Alias == "keywords").ValidationRegExp = "^\\d*$";
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var titleValue = addValidProperties ? "The title value" : null;
        var keywordsValue = addValidProperties ? "12345" : "This is not a number";
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = titleValue },
                new PropertyValueModel { Alias = "keywords", Value = keywordsValue }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // success is expected regardless of property level validation - the validation error status is communicated in the attempt status (see below)
        Assert.IsTrue(result.Success);
        Assert.AreEqual(addValidProperties ? ContentEditingOperationStatus.Success : ContentEditingOperationStatus.PropertyValidationError, result.Status);
        Assert.IsNotNull(result.Result);

        if (addValidProperties is false)
        {
            Assert.AreEqual(2, result.Result.ValidationResult.ValidationErrors.Count());
            Assert.IsNotNull(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "title" && v.ErrorMessages.Length == 1));
            Assert.IsNotNull(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "keywords" && v.ErrorMessages.Length == 1));
        }

        // NOTE: content creation must be successful, even if the mandatory property is missing (publishing however should not!)
        Assert.IsTrue(result.Result.Content!.HasIdentity);
        Assert.AreEqual(titleValue, result.Result.Content!.GetValue<string>("title"));
        Assert.AreEqual(keywordsValue, result.Result.Content!.GetValue<string>("keywords"));
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
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
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
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
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
        Assert.IsNotNull(result.Result.Content);
        Assert.IsFalse(result.Result.Content.HasIdentity);
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
        Assert.IsNotNull(result.Result.Content);
        Assert.IsFalse(result.Result.Content.HasIdentity);
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
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
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
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
    }

    [TestCase(ContentVariation.Culture)]
    [TestCase(ContentVariation.Segment)]
    public async Task Cannot_Create_With_Variant_Property_Value_For_Invariant_Content(ContentVariation contentVariation)
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
                    Culture = contentVariation is ContentVariation.Culture ? "en-US" : null,
                    Segment = contentVariation is ContentVariation.Segment ? "segment" : null,
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
        Assert.AreEqual(ContentEditingOperationStatus.PropertyTypeNotFound, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
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
        Assert.IsNotNull(result.Result.Content);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ContentEditingService.GetAsync(result.Result.Content.Key));

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
    public async Task Can_Create_Segment_Variant()
    {
        var contentType = await CreateVariantContentType(ContentVariation.Segment);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantProperties =
            [
                new () { Alias = "invariantTitle", Value = "The Invariant Title" }
            ],
            Variants =
            [
                new ()
                {
                    Segment = null,
                    Name = "The Name",
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Default Title" }
                    ]
                },
                new ()
                {
                    Segment = "seg-1",
                    Name = "The Name",
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Seg-1 Title" }
                    ]
                },
                new ()
                {
                    Segment = "seg-2",
                    Name = "The Name",
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Seg-2 Title" }
                    ]
                }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result.Content);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ContentEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IContent? createdContent)
        {
            Assert.IsNotNull(createdContent);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The Name", createdContent.Name);
                Assert.AreEqual("The Invariant Title", createdContent.GetValue<string>("invariantTitle"));
                Assert.AreEqual("The Default Title", createdContent.GetValue<string>("variantTitle", segment: null));
                Assert.AreEqual("The Seg-1 Title", createdContent.GetValue<string>("variantTitle", segment: "seg-1"));
                Assert.AreEqual("The Seg-2 Title", createdContent.GetValue<string>("variantTitle", segment: "seg-2"));
            });
        }
    }

    [Test]
    public async Task Can_Create_Culture_And_Segment_Variant()
    {
        var contentType = await CreateVariantContentType(ContentVariation.CultureAndSegment);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantProperties =
            [
                new () { Alias = "invariantTitle", Value = "The Invariant Title" }
            ],
            Variants =
            [
                new ()
                {
                    Name = "The English Name",
                    Culture = "en-US",
                    Segment = null,
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Default Title in English" }
                    ]
                },
                new ()
                {
                    Name = "The English Name",
                    Culture = "en-US",
                    Segment = "seg-1",
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Seg-1 Title in English" }
                    ]
                },
                new ()
                {
                    Name = "The English Name",
                    Culture = "en-US",
                    Segment = "seg-2",
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Seg-2 Title in English" }
                    ]
                },
                new ()
                {
                    Name = "The Danish Name",
                    Culture = "da-DK",
                    Segment = null,
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Default Title in Danish" }
                    ]
                },
                new ()
                {
                    Name = "The Danish Name",
                    Culture = "da-DK",
                    Segment = "seg-1",
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Seg-1 Title in Danish" }
                    ]
                },
                new ()
                {
                    Name = "The Danish Name",
                    Culture = "da-DK",
                    Segment = "seg-2",
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Seg-2 Title in Danish" }
                    ]
                }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result.Content);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ContentEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IContent? createdContent)
        {
            Assert.IsNotNull(createdContent);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The English Name", createdContent.GetCultureName("en-US"));
                Assert.AreEqual("The Danish Name", createdContent.GetCultureName("da-DK"));
                Assert.AreEqual("The Invariant Title", createdContent.GetValue<string>("invariantTitle"));
                Assert.AreEqual("The Default Title in English", createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: null));
                Assert.AreEqual("The Seg-1 Title in English", createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"));
                Assert.AreEqual("The Seg-2 Title in English", createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"));
                Assert.AreEqual("The Default Title in Danish", createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: null));
                Assert.AreEqual("The Seg-1 Title in Danish", createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"));
                Assert.AreEqual("The Seg-2 Title in Danish", createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"));
            });
        }
    }

    [Test]
    public async Task Can_Create_With_Explicit_Key()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var key = Guid.NewGuid();
        var createModel = new ContentCreateModel
        {
            Key = key,
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
        Assert.IsNotNull(result.Result.Content);
        Assert.IsTrue(result.Result.Content.HasIdentity);
        Assert.AreEqual(key, result.Result.Content.Key);
        Assert.AreEqual("The title value", result.Result.Content.GetValue<string>("title"));

        var content = await ContentEditingService.GetAsync(key);
        Assert.IsNotNull(content);
        Assert.AreEqual(result.Result.Content.Id, content.Id);
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
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
    }

    [Test]
    public async Task Cannot_Create_With_Segment_Variant_Property_Value_For_Culture_Variant_Content()
    {
        var contentType = await CreateVariantContentType(ContentVariation.Culture);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantProperties = [
                new () { Alias = "invariantTitle", Value = "The Invariant Title" },
            ],
            Variants = [
                new ()
                {
                    Name = "The name",
                    Culture = "en-US",
                    Segment = "segment",
                    Properties = [
                        new () { Alias = "variantTitle", Value = "The Variant Title" }
                    ]
                }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyTypeNotFound, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
    }

    [Test]
    public async Task Cannot_Create_Under_Trashed_Parent()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.AllowedAsRoot = true;
        contentType.AllowedContentTypes = new[]
        {
            new ContentTypeSort(contentType.Key, 1, contentType.Alias)
        };
        ContentTypeService.Save(contentType);

        var rootKey = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key, InvariantName = "Root", ParentKey = Constants.System.RootKey
            },
            Constants.Security.SuperUserKey)).Result.Content!.Key;

        await ContentEditingService.MoveToRecycleBinAsync(rootKey, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key, InvariantName = "Child", ParentKey = rootKey,
            },
            Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.InTrash, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.IsNull(result.Result.Content);
    }

    [Test]
    public async Task Cannot_Create_Culture_Variant_With_Incorrect_Culture_Casing()
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
                    Culture = "en-us",
                    Name = "The English Name",
                    Properties = new[]
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The English Title" }
                    }
                },
                new VariantModel
                {
                    Culture = "da-dk",
                    Name = "The Danish Name",
                    Properties = new[]
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The Danish Title" }
                    }
                }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.InvalidCulture, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Segment_Variant_Without_Default_Segment()
    {
        var contentType = await CreateVariantContentType(ContentVariation.Segment);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" }
            },
            Variants =
            [
                new ()
                {
                    Segment = "seg-1",
                    Name = "The Name",
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Seg-1 Title" }
                    ]
                },
                new ()
                {
                    Segment = "seg-2",
                    Name = "The Name",
                    Properties =
                    [
                        new () { Alias = "variantTitle", Value = "The Seg-2 Title" }
                    ]
                }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ContentTypeSegmentVarianceMismatch, result.Status);
    }

    private void AssertBodyTextEquals(string expected, IContent content)
    {
        var bodyTextValue = content.GetValue<string>("bodyText");
        Assert.IsTrue(
            RichTextPropertyEditorHelper.TryParseRichTextEditorValue(
                bodyTextValue,
                JsonSerializer,
                Mock.Of<ILogger>(),
                out RichTextEditorValue? richTextEditorValue));
        Assert.AreEqual(expected, richTextEditorValue!.Markup);
    }
}
