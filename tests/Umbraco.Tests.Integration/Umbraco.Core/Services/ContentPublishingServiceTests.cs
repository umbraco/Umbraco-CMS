using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
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
public partial class ContentPublishingServiceTests : UmbracoIntegrationTestWithContent
{
    private const string UnknownCulture = "ke-Ke";

    private readonly DateTime _schedulePublishDate = DateTime.UtcNow.AddDays(1).TruncateTo(DateTimeExtensions.DateTruncate.Second);
    private readonly DateTime _scheduleUnPublishDate = DateTime.UtcNow.AddDays(2).TruncateTo(DateTimeExtensions.DateTruncate.Second);

    [SetUp]
    public new void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private async Task<(ILanguage LangEn, ILanguage LangDa, ILanguage LangBe, IContentType contentType)> SetupVariantDoctypeAsync()
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

    private async Task<IContent> CreateVariantContentAsync(ILanguage langEn, ILanguage langDa, ILanguage langBe, IContentType contentType, Guid? parentKey = null)
    {
        var documentKey = Guid.NewGuid();

        var createModel = new ContentCreateModel
        {
            Key = documentKey,
            ContentTypeKey = contentType.Key,
            ParentKey = parentKey,
            Variants =
            [
                new VariantModel
                {
                    Name = langEn.CultureName,
                    Culture = langEn.IsoCode,
                    Properties = Enumerable.Empty<PropertyValueModel>(),
                },
                new VariantModel
                {
                    Name = langDa.CultureName,
                    Culture = langDa.IsoCode,
                    Properties = Enumerable.Empty<PropertyValueModel>(),
                },
                new VariantModel
                {
                    Name = langBe.CultureName,
                    Culture = langBe.IsoCode,
                    Properties = Enumerable.Empty<PropertyValueModel>(),
                }
            ]
        };

        var createAttempt = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (createAttempt.Success is false)
        {
            throw new Exception("Something unexpected went wrong setting up the test data");
        }

        return createAttempt.Result.Content!;
    }

    private async Task<IContentType> SetupInvariantDoctypeAsync()
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

    private async Task<IContent> CreateInvariantContentAsync(IContentType contentType, Guid? parentKey = null)
    {
        var documentKey = Guid.NewGuid();

        var createModel = new ContentCreateModel
        {
            Key = documentKey,
            ContentTypeKey = contentType.Key,
            InvariantName = "Test",
            ParentKey = parentKey,
        };

        var createAttempt = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (createAttempt.Success is false)
        {
            throw new Exception($"Something unexpected went wrong setting up the test data. Status: {createAttempt.Status}");
        }

        return createAttempt.Result.Content!;
    }

    private async Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>>
        SchedulePublishAndUnPublishForAllCulturesAsync(
            IContent setupData,
            (ILanguage LangEn, ILanguage LangDa, ILanguage LangBe, IContentType contentType) setupInfo)
        => await ContentPublishingService.PublishAsync(
            setupData.Key,
            [
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule =
                        new ContentScheduleModel
                        {
                            PublishDate = _schedulePublishDate, UnpublishDate = _scheduleUnPublishDate,
                        },
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule =
                        new ContentScheduleModel
                        {
                            PublishDate = _schedulePublishDate, UnpublishDate = _scheduleUnPublishDate,
                        },
                },
                new()
                {
                    Culture = setupInfo.LangBe.IsoCode,
                    Schedule = new ContentScheduleModel
                    {
                        PublishDate = _schedulePublishDate, UnpublishDate = _scheduleUnPublishDate,
                    },
                },
            ],
            Constants.Security.SuperUserKey);

    private async Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>>
        SchedulePublishAndUnPublishInvariantAsync(
            IContent setupData)
        => await ContentPublishingService.PublishAsync(
            setupData.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule =
                        new ContentScheduleModel
                        {
                            PublishDate = _schedulePublishDate, UnpublishDate = _scheduleUnPublishDate,
                        },
                },
            ],
            Constants.Security.SuperUserKey);
}
