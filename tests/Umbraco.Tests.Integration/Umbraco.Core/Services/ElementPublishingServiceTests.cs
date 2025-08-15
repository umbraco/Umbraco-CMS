using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public partial class ElementPublishingServiceTests : UmbracoIntegrationTest
{
    [SetUp]
    public new void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementCacheService ElementCacheService => GetRequiredService<IElementCacheService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private async Task<(ILanguage LangEn, ILanguage LangDa, ILanguage LangBe, IContentType ContentType)> SetupVariantElementTypeAsync()
    {
        var langEn = (await LanguageService.GetAsync("en-US"))!;
        var langDa = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);
        var langBe = new LanguageBuilder()
            .WithCultureInfo("nl-BE")
            .Build();
        await LanguageService.CreateAsync(langBe, Constants.Security.SuperUserKey);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("variantContent")
            .WithName("Variant Content")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSupportsPublishing(true)
                .Done()
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithMandatory(true)
                .WithVariations(ContentVariation.Culture)
                .Done()
            .Build();

        contentType.AllowedAsRoot = true;
        var createAttempt = await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        if (createAttempt.Success is false)
        {
            throw new Exception("Something unexpected went wrong setting up the test data structure");
        }

        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 1, contentType.Alias)];
        var updateAttempt = await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);
        if (updateAttempt.Success is false)
        {
            throw new Exception("Something unexpected went wrong setting up the test data structure");
        }

        return (langEn, langDa, langBe, contentType);
    }

    private async Task<IElement> CreateVariantElementAsync(
        ILanguage langEn,
        ILanguage langDa,
        ILanguage langBe,
        IContentType contentType,
        Guid? parentKey = null,
        string? englishTitleValue = "Test title")
    {
        var documentKey = Guid.NewGuid();

        var createModel = new ElementCreateModel
        {
            Key = documentKey,
            ContentTypeKey = contentType.Key,
            ParentKey = parentKey,
            Properties = [
                new PropertyValueModel
                {
                    Alias = "title",
                    Value = englishTitleValue,
                    Culture = langEn.IsoCode
                },
                new PropertyValueModel
                {
                    Alias = "title",
                    Value = "Test titel",
                    Culture = langDa.IsoCode
                },
                new PropertyValueModel
                {
                    Alias = "title",
                    Value = "Titel van de test",
                    Culture = langBe.IsoCode
                }
            ],
            Variants =
            [
                new VariantModel { Name = langEn.CultureName, Culture = langEn.IsoCode },
                new VariantModel { Name = langDa.CultureName, Culture = langDa.IsoCode },
                new VariantModel { Name = langBe.CultureName, Culture = langBe.IsoCode }
            ],
        };

        var createAttempt = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (createAttempt.Success is false)
        {
            throw new Exception("Something unexpected went wrong setting up the test data");
        }

        return createAttempt.Result.Content!;
    }

    private async Task<IContentType> SetupInvariantElementTypeAsync()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("invariantContent")
            .WithName("Invariant Content")
            .WithAllowAsRoot(true)
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSupportsPublishing(true)
                .Done()
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithMandatory(true)
                .Done()
            .Build();

        var createAttempt = await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        if (createAttempt.Success is false)
        {
            throw new Exception("Something unexpected went wrong setting up the test data structure");
        }

        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 1, contentType.Alias)];
        var updateAttempt = await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);
        if (updateAttempt.Success is false)
        {
            throw new Exception("Something unexpected went wrong setting up the test data structure");
        }

        return contentType;
    }

    private async Task<IElement> CreateInvariantContentAsync(IContentType contentType, Guid? parentKey = null, string? titleValue = "Test title")
    {
        var documentKey = Guid.NewGuid();

        var createModel = new ElementCreateModel
        {
            Key = documentKey,
            ContentTypeKey = contentType.Key,
            Variants = [new () { Name = "Test" }],
            ParentKey = parentKey,
            Properties =
            [
                new PropertyValueModel
                {
                    Alias = "title",
                    Value = titleValue,
                }
            ],
        };

        var createAttempt = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (createAttempt.Success is false)
        {
            throw new Exception($"Something unexpected went wrong setting up the test data. Status: {createAttempt.Status}");
        }

        return createAttempt.Result.Content!;
    }
}
