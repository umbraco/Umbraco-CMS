// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Testing;

public abstract class UmbracoIntegrationTestWithContentEditing : UmbracoIntegrationTest
{
    protected IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    protected ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private ContentEditingService ContentEditingService => (ContentEditingService)GetRequiredService<IContentEditingService>();

    private ContentPublishingService ContentPublishingService => (ContentPublishingService)GetRequiredService<IContentPublishingService>();

    protected ContentCreateModel Subpage1 { get; private set; }

    protected ContentCreateModel Subpage2 { get; private set; }

    protected ContentCreateModel PublishedTextPage { get; private set; }

    protected ContentCreateModel Textpage { get; private set; }

    protected ContentScheduleCollection ContentSchedule { get; private set; }

    protected CultureAndScheduleModel CultureAndSchedule { get; private set; }

    protected int TextpageId { get; private set; }

    protected int PublishedTextPageId { get; private set; }

    protected int Subpage1Id { get; private set; }

    protected int Subpage2Id { get; private set; }

    protected ContentTypeCreateModel ContentTypeCreateModel { get; private set; }

    protected IContentType ContentType { get; private set; }

    [SetUp]
    public new void Setup() => CreateTestData();

    protected async void CreateTestData()
    {
        // NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        // Create and Save ContentType "umbTextpage" -> 1051 (template), 1052 (content type)
        ContentTypeCreateModel =
            ContentTypeEditingBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateKey: template.Key);
        ContentTypeCreateModel.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
        ContentTypeCreateModel.AllowedAsRoot = true;
        ContentTypeCreateModel.AllowedContentTypes = new[] { new ContentTypeSort((Guid)ContentTypeCreateModel.Key, 0, ContentTypeCreateModel.Alias) };
        var contentTypeResult = await ContentTypeEditingService.CreateAsync(ContentTypeCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(contentTypeResult.Success);
        ContentType = contentTypeResult.Result;

        // Create and Save Content "Homepage" based on "umbTextpage" -> 1053
        Textpage = ContentEditingBuilder.CreateSimpleContent(ContentTypeCreateModel);
        Textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
        var createContentResultTextPage = await ContentEditingService.CreateAsync(Textpage, Constants.Security.SuperUserKey);
        Assert.IsTrue(createContentResultTextPage.Success);

        if (!Textpage.Key.HasValue)
        {
            throw new InvalidOperationException("The content page key is null.");
        }

        if (createContentResultTextPage.Result.Content != null)
        {
            TextpageId = createContentResultTextPage.Result.Content.Id;
        }

        // Sets the culture and schedule for the content, in this case, we are publishing immediately for all cultures
        ContentSchedule = new ContentScheduleCollection();
        CultureAndSchedule = new CultureAndScheduleModel
        {
            CulturesToPublishImmediately = new HashSet<string> { "*" }, Schedules = ContentSchedule,
        };

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> 1054
        PublishedTextPage = ContentEditingBuilder.CreateSimpleContent(ContentTypeCreateModel, "Published Page", Textpage.Key);
        var createContentResultSubPage = await ContentEditingService.CreateAsync(PublishedTextPage, Constants.Security.SuperUserKey);
        Assert.IsTrue(createContentResultSubPage.Success);

        if (!PublishedTextPage.Key.HasValue)
        {
            throw new InvalidOperationException("The content page key is null.");
        }

        if (createContentResultSubPage.Result.Content != null)
        {
            PublishedTextPageId = createContentResultSubPage.Result.Content.Id;
        }

        await ContentPublishingService.PublishAsync(PublishedTextPage.Key.Value, CultureAndSchedule, Constants.Security.SuperUserKey);

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> 1055
        Subpage1 = ContentEditingBuilder.CreateSimpleContent(ContentTypeCreateModel, "Text Page 1", Textpage.Key);
        var createContentResultSubPage1 = await ContentEditingService.CreateAsync(Subpage1, Constants.Security.SuperUserKey);
        Assert.IsTrue(createContentResultSubPage1.Success);
        if (!Subpage1.Key.HasValue)
        {
            throw new InvalidOperationException("The content page key is null.");
        }

        if (createContentResultSubPage1.Result.Content != null)
        {
            Subpage1Id = createContentResultSubPage1.Result.Content.Id;
        }

        Subpage2 = ContentEditingBuilder.CreateSimpleContent(ContentTypeCreateModel, "Text Page 2", Textpage.Key);
        var createContentResultSubPage2 = await ContentEditingService.CreateAsync(Subpage2, Constants.Security.SuperUserKey);
        Assert.IsTrue(createContentResultSubPage2.Success);
        if (!Subpage2.Key.HasValue)
        {
            throw new InvalidOperationException("The content page key is null.");
        }

        if (createContentResultSubPage2.Result.Content != null)
        {
            Subpage2Id = createContentResultSubPage2.Result.Content.Id;
        }
    }
}
