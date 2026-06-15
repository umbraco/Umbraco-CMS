using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_At_Root(bool allowedAtRoot)
        => await Test_Can_Create_At_Root(allowedAtRoot, allowedAtRoot);

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToAllowTextPageAtRoot))]
    public async Task Can_Create_At_Root_With_Content_Type_Filter() =>

        // Verifies that when allowed at root, the content can be created if not filtered out by a content type filter.
        await Test_Can_Create_At_Root(true, true);

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToDisallowTextPageAtRoot))]
    public async Task Cannot_Create_At_Root_With_Content_Type_Filter() =>

        // Verifies that when allowed at root, the content cannot be created if filtered out by a content type filter.
        await Test_Can_Create_At_Root(true, false);

    public static void ConfigureContentTypeFilterToAllowTextPageAtRoot(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterForAllowedTextPageAtRoot>();

    public static void ConfigureContentTypeFilterToDisallowTextPageAtRoot(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterForDisallowedTextPageAtRoot>();

    private class ContentTypeFilterForAllowedTextPageAtRoot : ContentTypeFilterForTextPageAtRoot
    {
        public ContentTypeFilterForAllowedTextPageAtRoot()
            : base(true)
        {
        }
    }

    private class ContentTypeFilterForDisallowedTextPageAtRoot : ContentTypeFilterForTextPageAtRoot
    {
        public ContentTypeFilterForDisallowedTextPageAtRoot()
            : base(false)
        {
        }
    }

    private abstract class ContentTypeFilterForTextPageAtRoot : IContentTypeFilter
    {
        private readonly bool _allowed;

        protected ContentTypeFilterForTextPageAtRoot(bool allowed) => _allowed = allowed;

        public Task<IEnumerable<TItem>> FilterAllowedAtRootAsync<TItem>(IEnumerable<TItem> contentTypes)
            where TItem : IContentTypeComposition
            => Task.FromResult(contentTypes.Where(x => (_allowed && x.Alias == "textPage") || (!_allowed && x.Alias != "textPage")));
    }

    private async Task Test_Can_Create_At_Root(bool allowedAtRoot, bool expectSuccess)
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = allowedAtRoot;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "bodyText", Value = "The body text"  }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        if (expectSuccess)
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            VerifyCreate(result.Result.Content);

            // re-get and re-test
            VerifyCreate(await ContentEditingService.GetAsync(result.Result.Content!.Key));

            void VerifyCreate(IContent? createdContent)
            {
                Assert.That(createdContent, Is.Not.Null);
                Assert.That(createdContent.Key, Is.Not.EqualTo(Guid.Empty));
                Assert.That(createdContent.HasIdentity, Is.True);
                Assert.That(createdContent.Name, Is.EqualTo("Test Create"));
                Assert.That(createdContent.GetValue<string>("title"), Is.EqualTo("The title value"));
                AssertBodyTextEquals("The body text", createdContent);
            }
        }
        else
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotAllowed));
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.Result.Content, Is.Null);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_As_Child(bool allowedAsChild)
        => await Test_Can_Create_As_Child(allowedAsChild, allowedAsChild);

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToAllowTextPageAsChild))]
    public async Task Can_Create_As_Child_With_Content_Type_Filter() =>

        // Verifies that when allowed as a child, the content can be created if not filtered out by a content type filter.
        await Test_Can_Create_As_Child(true, true);

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToDisallowTextPageAsChild))]
    public async Task Cannot_Create_As_Child_With_Content_Type_Filter() =>

        // Verifies that when allowed as a child, the content cannot be created if filtered out by a content type filter.
        await Test_Can_Create_As_Child(true, false);

    public static void ConfigureContentTypeFilterToAllowTextPageAsChild(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterForAllowedTextPageAsChild>();

    public static void ConfigureContentTypeFilterToDisallowTextPageAsChild(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterForDisallowedTextPageAsChild>();

    private class ContentTypeFilterForAllowedTextPageAsChild : ContentTypeFilterForTextPageAsChild
    {
        public ContentTypeFilterForAllowedTextPageAsChild()
            : base(true)
        {
        }
    }

    private class ContentTypeFilterForDisallowedTextPageAsChild : ContentTypeFilterForTextPageAsChild
    {
        public ContentTypeFilterForDisallowedTextPageAsChild()
            : base(false)
        {
        }
    }

    private abstract class ContentTypeFilterForTextPageAsChild : IContentTypeFilter
    {
        private readonly bool _allowed;

        protected ContentTypeFilterForTextPageAsChild(bool allowed) => _allowed = allowed;

        public Task<IEnumerable<ContentTypeSort>> FilterAllowedChildrenAsync(IEnumerable<ContentTypeSort> contentTypes, Guid parentContentTypeKey, Guid? parentContentKey)
            => Task.FromResult(contentTypes.Where(x => (_allowed && x.Alias == "textPage") || (!_allowed && x.Alias != "textPage")));
    }

    private async Task Test_Can_Create_As_Child(bool allowedAsChild, bool expectSuccess)
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var childContentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        childContentType.AllowedAsRoot = false;
        await ContentTypeService.CreateAsync(childContentType, Constants.Security.SuperUserKey);

        var rootContentType = ContentTypeBuilder.CreateBasicContentType();
        rootContentType.AllowedAsRoot = true;
        if (allowedAsChild)
        {
            rootContentType.AllowedContentTypes = new[]
            {
                new ContentTypeSort(childContentType.Key, 1, childContentType.Alias)
            };
        }
        await ContentTypeService.CreateAsync(rootContentType, Constants.Security.SuperUserKey);

        var rootKey = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = rootContentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants =
                [
                    new VariantModel { Name = "Root" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!.Key;

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = childContentType.Key,
            TemplateKey = template.Key,
            ParentKey = rootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create Child" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The child title value"  },
                new PropertyValueModel { Alias = "bodyText", Value = "The child body text"  }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        if (expectSuccess)
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

            var createdContent = result.Result.Content;
            Assert.That(createdContent, Is.Not.Null);
            Assert.That(createdContent.Key, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdContent.HasIdentity, Is.True);
            Assert.That(createdContent.Name, Is.EqualTo("Test Create Child"));
            Assert.That(createdContent.GetValue<string>("title"), Is.EqualTo("The child title value"));
            AssertBodyTextEquals("The child body text", createdContent);
        }
        else
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotAllowed));
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.Result.Content, Is.Null);
        }
    }

    [Test]
    public async Task Can_Create_Without_Template()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value"  }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content!.HasIdentity, Is.True);
        Assert.That(result.Result.Content!.GetValue<string>("title"), Is.EqualTo("The title value"));
    }

    [Test]
    public async Task Can_Create_Without_Properties()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content!.HasIdentity, Is.True);
        Assert.That(result.Result.Content!.GetValue<string>("title"), Is.EqualTo(null));
        Assert.That(result.Result.Content!.GetValue<string>("bodyText"), Is.EqualTo(null));
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
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var titleValue = addValidProperties ? "The title value" : null;
        var keywordsValue = addValidProperties ? "12345" : "This is not a number";
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = titleValue  },
                new PropertyValueModel { Alias = "keywords", Value = keywordsValue  }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // success is expected regardless of property level validation - the validation error status is communicated in the attempt status (see below)
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(addValidProperties ? ContentEditingOperationStatus.Success : ContentEditingOperationStatus.PropertyValidationError));
        Assert.That(result.Result, Is.Not.Null);

        if (addValidProperties is false)
        {
            Assert.That(result.Result.ValidationResult.ValidationErrors.Count(), Is.EqualTo(2));
            Assert.That(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "title" && v.ErrorMessages.Length == 1), Is.Not.Null);
            Assert.That(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "keywords" && v.ErrorMessages.Length == 1), Is.Not.Null);
        }

        // NOTE: content creation must be successful, even if the mandatory property is missing (publishing however should not!)
        Assert.That(result.Result.Content!.HasIdentity, Is.True);
        Assert.That(result.Result.Content!.GetValue<string>("title"), Is.EqualTo(titleValue));
        Assert.That(result.Result.Content!.GetValue<string>("keywords"), Is.EqualTo(keywordsValue));
    }

    [Test]
    public async Task Cannot_Create_With_Non_Existing_Parent()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Guid.NewGuid(),
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ParentNotFound));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_Without_Content_Type()
    {
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = Guid.NewGuid(),
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ContentTypeNotFound));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_With_Invalid_Template()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

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
                new VariantModel { Name = "Test Create" }
            ],
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.TemplateNotAllowed));
        Assert.That(result.Result.Content, Is.Not.Null);
        Assert.That(result.Result.Content.HasIdentity, Is.False);
    }

    [Test]
    public async Task Cannot_Create_With_Non_Existing_Template()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = Guid.NewGuid(),
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.TemplateNotFound));
        Assert.That(result.Result.Content, Is.Not.Null);
        Assert.That(result.Result.Content.HasIdentity, Is.False);
    }

    [Test]
    public async Task Cannot_Create_With_Non_Existing_Properties()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value"  },
                new PropertyValueModel { Alias = "no_such_property", Value = "No such property value"  }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.PropertyTypeNotFound));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_Invariant_Content_Without_Name()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value"  }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_With_Culture_Variant_Property_Value_For_Invariant_Content()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel
                {
                    Alias = "title",
                    Value = "The title value",
                    Culture = "en-US"
                }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.PropertyTypeCultureVarianceMismatch));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_With_Segment_Variant_Property_Value_For_Invariant_Content()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel
                {
                    Alias = "title",
                    Value = "The title value",
                    Segment = "segment"
                }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.PropertyTypeSegmentVarianceMismatch));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
    }

    [Test]
    public async Task Can_Create_Culture_Variant()
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
                new VariantModel { Culture = "en-US", Name = "The English Name" },
                new VariantModel { Culture = "da-DK", Name = "The Danish Name" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result.Content, Is.Not.Null);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ContentEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IContent? createdContent)
        {
            Assert.That(createdContent, Is.Not.Null);
            Assert.That(createdContent.GetCultureName("en-US"), Is.EqualTo("The English Name"));
            Assert.That(createdContent.GetCultureName("da-DK"), Is.EqualTo("The Danish Name"));
            Assert.That(createdContent.GetValue<string>("invariantTitle"), Is.EqualTo("The Invariant Title"));
            Assert.That(createdContent.GetValue<string>("variantTitle", "en-US"), Is.EqualTo("The English Title"));
            Assert.That(createdContent.GetValue<string>("variantTitle", "da-DK"), Is.EqualTo("The Danish Title"));
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
            Properties =
            [
                new () { Alias = "invariantTitle", Value = "The Invariant Title" },
                new () { Alias = "variantTitle", Value = "The Default Title" },
                new () { Alias = "variantTitle", Value = "The Seg-1 Title", Segment = "seg-1" },
                new () { Alias = "variantTitle", Value = "The Seg-2 Title", Segment = "seg-2" }
            ],
            Variants =
            [
                new () { Name = "The Name" },
                new () { Segment = "seg-1", Name = "The Name" },
                new () { Segment = "seg-2", Name = "The Name" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result.Content, Is.Not.Null);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ContentEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IContent? createdContent)
        {
            Assert.That(createdContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(createdContent.Name, Is.EqualTo("The Name"));
                Assert.That(createdContent.GetValue<string>("invariantTitle"), Is.EqualTo("The Invariant Title"));
                Assert.That(createdContent.GetValue<string>("variantTitle", segment: null), Is.EqualTo("The Default Title"));
                Assert.That(createdContent.GetValue<string>("variantTitle", segment: "seg-1"), Is.EqualTo("The Seg-1 Title"));
                Assert.That(createdContent.GetValue<string>("variantTitle", segment: "seg-2"), Is.EqualTo("The Seg-2 Title"));
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
            Properties =
            [
                new () { Alias = "invariantTitle", Value = "The Invariant Title" },
                new () { Alias = "variantTitle", Value = "The Default Title in English", Culture = "en-US" },
                new () { Alias = "variantTitle", Value = "The Seg-1 Title in English", Culture = "en-US", Segment = "seg-1" },
                new () { Alias = "variantTitle", Value = "The Seg-2 Title in English", Culture = "en-US", Segment = "seg-2" },
                new () { Alias = "variantTitle", Value = "The Default Title in Danish", Culture = "da-DK" },
                new () { Alias = "variantTitle", Value = "The Seg-1 Title in Danish", Culture = "da-DK", Segment = "seg-1" },
                new () { Alias = "variantTitle", Value = "The Seg-2 Title in Danish", Culture = "da-DK", Segment = "seg-2" }
            ],
            Variants =
            [
                new () { Name = "The English Name", Culture = "en-US" },
                new () { Name = "The English Name", Culture = "en-US", Segment = "seg-1" },
                new () { Name = "The English Name", Culture = "en-US", Segment = "seg-2" },
                new () { Name = "The Danish Name", Culture = "da-DK" },
                new () { Name = "The Danish Name", Culture = "da-DK", Segment = "seg-1" },
                new () { Name = "The Danish Name", Culture = "da-DK", Segment = "seg-2" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result.Content, Is.Not.Null);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ContentEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IContent? createdContent)
        {
            Assert.That(createdContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(createdContent.GetCultureName("en-US"), Is.EqualTo("The English Name"));
                Assert.That(createdContent.GetCultureName("da-DK"), Is.EqualTo("The Danish Name"));
                Assert.That(createdContent.GetValue<string>("invariantTitle"), Is.EqualTo("The Invariant Title"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: null), Is.EqualTo("The Default Title in English"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"), Is.EqualTo("The Seg-1 Title in English"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"), Is.EqualTo("The Seg-2 Title in English"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: null), Is.EqualTo("The Default Title in Danish"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"), Is.EqualTo("The Seg-1 Title in Danish"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"), Is.EqualTo("The Seg-2 Title in Danish"));
            });
        }
    }

    [Test]
    public async Task Can_Create_Culture_And_Segment_Variant_With_Segment_Only_Variant_Property()
    {
        var contentType = await CreateVariantContentType(ContentVariation.CultureAndSegment);
        var propertyType = contentType.PropertyTypes.First(pt => pt.Alias == "invariantTitle");
        propertyType.Alias = "segmentVariantTitle";
        propertyType.Variations = ContentVariation.Segment;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new () { Alias = "segmentVariantTitle", Value = "The Default Segment Variant Title", Segment = null },
                new () { Alias = "segmentVariantTitle", Value = "The Seg-1 Segment Variant Title", Segment = "seg-1" },
                new () { Alias = "segmentVariantTitle", Value = "The Seg-2 Segment Variant Title", Segment = "seg-2" },
                new () { Alias = "variantTitle", Value = "The Default Title in English", Culture = "en-US" },
                new () { Alias = "variantTitle", Value = "The Seg-1 Title in English", Culture = "en-US", Segment = "seg-1" },
                new () { Alias = "variantTitle", Value = "The Seg-2 Title in English", Culture = "en-US", Segment = "seg-2" },
                new () { Alias = "variantTitle", Value = "The Default Title in Danish", Culture = "da-DK" },
                new () { Alias = "variantTitle", Value = "The Seg-1 Title in Danish", Culture = "da-DK", Segment = "seg-1" },
                new () { Alias = "variantTitle", Value = "The Seg-2 Title in Danish", Culture = "da-DK", Segment = "seg-2" }
            ],
            Variants =
            [
                new () { Name = "The English Name", Culture = "en-US" },
                new () { Name = "The English Name", Culture = "en-US", Segment = "seg-1" },
                new () { Name = "The English Name", Culture = "en-US", Segment = "seg-2" },
                new () { Name = "The Danish Name", Culture = "da-DK" },
                new () { Name = "The Danish Name", Culture = "da-DK", Segment = "seg-1" },
                new () { Name = "The Danish Name", Culture = "da-DK", Segment = "seg-2" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result.Content, Is.Not.Null);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ContentEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IContent? createdContent)
        {
            Assert.That(createdContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(createdContent.GetCultureName("en-US"), Is.EqualTo("The English Name"));
                Assert.That(createdContent.GetCultureName("da-DK"), Is.EqualTo("The Danish Name"));
                Assert.That(createdContent.GetValue<string>("segmentVariantTitle"), Is.EqualTo("The Default Segment Variant Title"));
                Assert.That(createdContent.GetValue<string>("segmentVariantTitle", segment: "seg-1"), Is.EqualTo("The Seg-1 Segment Variant Title"));
                Assert.That(createdContent.GetValue<string>("segmentVariantTitle", segment: "seg-2"), Is.EqualTo("The Seg-2 Segment Variant Title"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: null), Is.EqualTo("The Default Title in English"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"), Is.EqualTo("The Seg-1 Title in English"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"), Is.EqualTo("The Seg-2 Title in English"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: null), Is.EqualTo("The Default Title in Danish"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"), Is.EqualTo("The Seg-1 Title in Danish"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"), Is.EqualTo("The Seg-2 Title in Danish"));
            });
        }
    }

    [Test]
    public async Task Can_Create_Culture_And_Segment_Variant_With_Culture_Only_Variant_Property()
    {
        var contentType = await CreateVariantContentType(ContentVariation.CultureAndSegment);
        var propertyType = contentType.PropertyTypes.First(pt => pt.Alias == "invariantTitle");
        propertyType.Alias = "cultureVariantTitle";
        propertyType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new () { Alias = "cultureVariantTitle", Value = "The English Culture Variant Title", Culture = "en-US" },
                new () { Alias = "cultureVariantTitle", Value = "The Danish Culture Variant Title", Culture = "da-DK" },
                new () { Alias = "variantTitle", Value = "The Default Title in English", Culture = "en-US" },
                new () { Alias = "variantTitle", Value = "The Seg-1 Title in English", Culture = "en-US", Segment = "seg-1" },
                new () { Alias = "variantTitle", Value = "The Seg-2 Title in English", Culture = "en-US", Segment = "seg-2" },
                new () { Alias = "variantTitle", Value = "The Default Title in Danish", Culture = "da-DK" },
                new () { Alias = "variantTitle", Value = "The Seg-1 Title in Danish", Culture = "da-DK", Segment = "seg-1" },
                new () { Alias = "variantTitle", Value = "The Seg-2 Title in Danish", Culture = "da-DK", Segment = "seg-2" }
            ],
            Variants =
            [
                new () { Name = "The English Name", Culture = "en-US" },
                new () { Name = "The English Name", Culture = "en-US", Segment = "seg-1" },
                new () { Name = "The English Name", Culture = "en-US", Segment = "seg-2" },
                new () { Name = "The Danish Name", Culture = "da-DK" },
                new () { Name = "The Danish Name", Culture = "da-DK", Segment = "seg-1" },
                new () { Name = "The Danish Name", Culture = "da-DK", Segment = "seg-2" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result.Content, Is.Not.Null);
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ContentEditingService.GetAsync(result.Result.Content.Key));

        void VerifyCreate(IContent? createdContent)
        {
            Assert.That(createdContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(createdContent.GetCultureName("en-US"), Is.EqualTo("The English Name"));
                Assert.That(createdContent.GetCultureName("da-DK"), Is.EqualTo("The Danish Name"));
                Assert.That(createdContent.GetValue<string>("cultureVariantTitle", culture: "en-US"), Is.EqualTo("The English Culture Variant Title"));
                Assert.That(createdContent.GetValue<string>("cultureVariantTitle", culture: "da-DK"), Is.EqualTo("The Danish Culture Variant Title"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: null), Is.EqualTo("The Default Title in English"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"), Is.EqualTo("The Seg-1 Title in English"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"), Is.EqualTo("The Seg-2 Title in English"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: null), Is.EqualTo("The Default Title in Danish"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"), Is.EqualTo("The Seg-1 Title in Danish"));
                Assert.That(createdContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"), Is.EqualTo("The Seg-2 Title in Danish"));
            });
        }
    }

    [Test]
    public async Task Can_Create_With_Explicit_Key()
    {
        var contentType = ContentTypeBuilder.CreateContentMetaContentType();
        contentType.AllowedTemplates = null;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var key = Guid.NewGuid();
        var createModel = new ContentCreateModel
        {
            Key = key,
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new () { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Result.Content, Is.Not.Null);
        Assert.That(result.Result.Content.HasIdentity, Is.True);
        Assert.That(result.Result.Content.Key, Is.EqualTo(key));
        Assert.That(result.Result.Content.GetValue<string>("title"), Is.EqualTo("The title value"));

        var content = await ContentEditingService.GetAsync(key);
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Id, Is.EqualTo(result.Result.Content.Id));
    }

    [Test]
    public async Task Cannot_Create_With_Invariant_Property_Value_For_Variant_Content()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new () { Name = "Test Create", Culture = "en-US" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Variant Title" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.PropertyTypeCultureVarianceMismatch));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_With_Segment_Variant_Property_Value_For_Culture_Variant_Content()
    {
        var contentType = await CreateVariantContentType(ContentVariation.Culture);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new () { Alias = "invariantTitle", Value = "The Invariant Title" },
                new () { Alias = "variantTitle", Value = "The Variant Title", Culture = "en-US", Segment = "segment" }
            ],
            Variants =
            [
                new () { Name = "The name", Culture = "en-US", Segment = "segment" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.PropertyTypeSegmentVarianceMismatch));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
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
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var rootKey = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants =
                [
                    new () { Name = "Root" }
                ]
            },
            Constants.Security.SuperUserKey)).Result.Content!.Key;

        await ContentEditingService.MoveToRecycleBinAsync(rootKey, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = rootKey,
                Variants =
                [
                    new () { Name = "Child" }
                ]
            },
            Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.InTrash));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Is.Null);
    }

    [Test]
    public async Task Cannot_Create_Culture_Variant_With_Incorrect_Culture_Casing()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The English Title", Culture = "en-us" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Danish Title", Culture = "da-dk" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-us", Name = "The English Name" },
                new VariantModel { Culture = "da-dk", Name = "The Danish Name" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.InvalidCulture));
    }

    [Test]
    public async Task Cannot_Create_Segment_Variant_Without_Default_Segment()
    {
        var contentType = await CreateVariantContentType(ContentVariation.Segment);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Seg-1 Title", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Seg-2 Title", Segment = "seg-2" }
            ],
            Variants =
            [
                new () { Segment = "seg-1", Name = "The Name" },
                new () { Segment = "seg-2", Name = "The Name" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ContentTypeSegmentVarianceMismatch));
    }

    private void AssertBodyTextEquals(string expected, IContent content)
    {
        var bodyTextValue = content.GetValue<string>("bodyText");
        Assert.That(
            RichTextPropertyEditorHelper.TryParseRichTextEditorValue(
                bodyTextValue,
                JsonSerializer,
                Mock.Of<ILogger>(),
                out RichTextEditorValue? richTextEditorValue), Is.True);
        Assert.That(richTextEditorValue!.Markup, Is.EqualTo(expected));
    }
}
