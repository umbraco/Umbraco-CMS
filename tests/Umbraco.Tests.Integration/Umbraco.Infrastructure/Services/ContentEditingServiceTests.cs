using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public partial class ContentEditingServiceTests : UmbracoIntegrationTestWithContent
{
    [SetUp]
    public void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

    protected override void CustomTestSetup(IUmbracoBuilder builder) =>
        builder.AddNotificationHandler<ContentCopiedNotification, RelateOnCopyNotificationHandler>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentType CreateInvariantContentType(params ITemplate[] templates)
    {
        var contentTypeBuilder = new ContentTypeBuilder()
            .WithAlias("invariantTest")
            .WithName("Invariant Test")
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .AddPropertyType()
            .WithAlias("text")
            .WithName("Text")
            .WithVariations(ContentVariation.Nothing)
            .Done();

        foreach (var template in templates)
        {
            contentTypeBuilder
                .AddAllowedTemplate()
                .WithId(template.Id)
                .WithAlias(template.Alias)
                .WithName(template.Name ?? template.Alias)
                .Done();
        }

        if (templates.Any())
        {
            contentTypeBuilder.WithDefaultTemplateId(templates.First().Id);
        }

        var contentType = contentTypeBuilder.Build();
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        return contentType;
    }

    private async Task<IContentType> CreateVariantContentType()
    {
        var language = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("cultureVariationTest")
            .WithName("Culture Variation Test")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias("variantTitle")
            .WithName("Variant Title")
            .WithVariations(ContentVariation.Culture)
            .Done()
            .AddPropertyType()
            .WithAlias("invariantTitle")
            .WithName("Invariant Title")
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .Build();
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);
        return contentType;
    }

    private async Task<IContent> CreateInvariantContent(params ITemplate[] templates)
    {
        var contentType = CreateInvariantContentType(templates);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Initial Name",
            TemplateKey = templates.FirstOrDefault()?.Key,
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The initial title" },
                new PropertyValueModel { Alias = "text", Value = "The initial text" }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result!;
    }

    private async Task<IContent> CreateVariantContent()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "invariantTitle", Value = "The initial invariant title" }
            },
            Variants = new []
            {
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "Initial English Name",
                    Properties = new []
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The initial English title" }
                    }
                },
                new VariantModel
                {
                    Culture = "da-DK",
                    Name = "Initial Danish Name",
                    Properties = new []
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The initial Danish title" }
                    }
                }
            }
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result!;
    }

    private async Task<IContentType> CreateTextPageContentTypeAsync()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        return contentType;
    }

    private async Task<(IContent root, IContent child)> CreateRootAndChildAsync(IContentType contentType, string rootName = "The Root", string childName = "The Child")
    {
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = rootName
        };

        var root = (await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        contentType.AllowedContentTypes = new List<ContentTypeSort>
        {
            new (contentType.Key, 1, contentType.Alias)
        };
        ContentTypeService.Save(contentType);

        createModel.ParentKey = root.Key;
        createModel.InvariantName = childName;

        var child = (await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;
        Assert.AreEqual(root.Id, child.ParentId);

        return (root, child);
    }
 }
