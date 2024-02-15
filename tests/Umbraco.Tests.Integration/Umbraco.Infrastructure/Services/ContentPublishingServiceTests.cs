using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public partial class ContentPublishingServiceTests : UmbracoIntegrationTestWithContent
{
    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private static readonly ISet<string> _allCultures = new HashSet<string>(){ "*" };

    private static  CultureAndScheduleModel MakeModel(ISet<string> cultures) => new CultureAndScheduleModel()
    {
        CulturesToPublishImmediately = cultures,
        Schedules = new ContentScheduleCollection()
    };

    private static  CultureAndScheduleModel MakeModel(ContentScheduleCollection schedules) => new CultureAndScheduleModel()
    {
        CulturesToPublishImmediately = new HashSet<string>(),
        Schedules = schedules
    };

    [SetUp]
    public void SetupTest()
    {
        ContentNotificationHandler.PublishingContent = null;
        ContentNotificationHandler.UnpublishingContent = null;
    }

    private async Task<Content> CreateInvalidContent(IContent? parent = null)
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        // create a new content type and allow the default content type as child
        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = true;
        contentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias) };
        ContentTypeService.Save(contentType);

        // allow the new content type as child to the default content type
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(contentType.Key, 0, contentType.Alias) };
        ContentTypeService.Save(ContentType);

        var content = ContentBuilder.CreateSimpleContent(contentType, "Invalid Content", parent?.Id ?? Constants.System.Root);
        content.SetValue("title", string.Empty);
        content.SetValue("bodyText", string.Empty);
        content.SetValue("author", string.Empty);
        ContentService.Save(content);

        return content;
    }

    private async Task<(ILanguage LangEn, ILanguage LangDa, IContentType contentType)> SetupVariantTest(bool englishIsMandatoryLanguage = false)
    {
        var langEn = (await LanguageService.GetAsync("en-US"))!;
        if (englishIsMandatoryLanguage)
        {
            langEn.IsMandatory = true;
            await LanguageService.UpdateAsync(langEn, Constants.Security.SuperUserKey);
        }

        var langDa = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var key = Guid.NewGuid();
        var contentType = new ContentTypeBuilder()
            .WithAlias("variantContent")
            .WithName("Variant Content")
            .WithKey(key)
            .WithContentVariation(ContentVariation.Culture)
            .AddAllowedContentType()
            .WithKey(key)
            .WithAlias("variantContent")
            .Done()
            .AddPropertyGroup()
            .WithAlias("content")
            .WithName("Content")
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithVariations(ContentVariation.Culture)
            .WithMandatory(true)
            .Done()
            .Done()
            .Build();

        contentType.AllowedAsRoot = true;
        await ContentTypeService.SaveAsync(contentType, Constants.Security.SuperUserKey);

        return (langEn, langDa, contentType);
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder
            .AddNotificationHandler<ContentPublishingNotification, ContentNotificationHandler>()
            .AddNotificationHandler<ContentUnpublishingNotification, ContentNotificationHandler>();

    private void VerifyIsPublished(Guid key) => Assert.IsTrue(ContentService.GetById(key)!.Published);

    private void VerifyIsNotPublished(Guid key) => Assert.IsFalse(ContentService.GetById(key)!.Published);

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private class ContentNotificationHandler : INotificationHandler<ContentPublishingNotification>, INotificationHandler<ContentUnpublishingNotification>
    {
        public static Action<ContentPublishingNotification>? PublishingContent { get; set; }

        public static Action<ContentUnpublishingNotification>? UnpublishingContent { get; set; }

        public void Handle(ContentPublishingNotification notification) => PublishingContent?.Invoke(notification);

        public void Handle(ContentUnpublishingNotification notification) => UnpublishingContent?.Invoke(notification);
    }
}
