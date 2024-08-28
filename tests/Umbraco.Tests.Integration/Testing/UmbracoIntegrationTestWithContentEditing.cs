// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Testing;

public abstract class UmbracoIntegrationTestWithContentEditing : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private ContentEditingService ContentEditingService =>
        (ContentEditingService)GetRequiredService<IContentEditingService>();

    private ContentPublishingService ContentPublishingService =>
        (ContentPublishingService)GetRequiredService<IContentPublishingService>();


    protected ContentCreateModel Subpage2 { get; private set; }
    protected ContentCreateModel Subpage3 { get; private set; }

    protected ContentCreateModel Subpage { get; private set; }

    protected ContentCreateModel Textpage { get; private set; }

    protected ContentScheduleCollection ContentSchedule { get; private set; }

    protected CultureAndScheduleModel CultureAndSchedule { get; private set; }

    protected int TextpageId { get; private set; }

    protected int SubpageId { get; private set; }

    protected int Subpage2Id { get; private set; }

    protected int Subpage3Id { get; private set; }

    protected ContentType ContentType { get; private set; }

    [SetUp]
    public new void Setup() => CreateTestData();

    protected async void CreateTestData()
    {
        // NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        // Create and Save ContentType "umbTextpage" -> 1051 (template), 1052 (content type)
        ContentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        ContentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
        ContentType.AllowedAsRoot = true;
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias) };
        var contentTypeResult = await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);
        Assert.IsTrue(contentTypeResult.Success);

        // Create and Save Content "Homepage" based on "umbTextpage" -> 1053
        Textpage = ContentEditingBuilder.CreateSimpleContent(ContentType);
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
        Subpage = ContentEditingBuilder.CreateSimpleContent(ContentType, "Text Page 1", Textpage.Key);
        var createContentResultSubPage = await ContentEditingService.CreateAsync(Subpage, Constants.Security.SuperUserKey);
        Assert.IsTrue(createContentResultSubPage.Success);

        if (!Subpage.Key.HasValue)
        {
            throw new InvalidOperationException("The content page key is null.");
        }

        if (createContentResultSubPage.Result.Content != null)
        {
            SubpageId = createContentResultSubPage.Result.Content.Id;
        }

        await ContentPublishingService.PublishAsync(Subpage.Key.Value, CultureAndSchedule, Constants.Security.SuperUserKey);

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> 1055
        Subpage2 = ContentEditingBuilder.CreateSimpleContent(ContentType, "Text Page 2", Textpage.Key);
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

        Subpage3 = ContentEditingBuilder.CreateSimpleContent(ContentType, "Text Page 3", Textpage.Key);
        var createContentResultSubPage3 = await ContentEditingService.CreateAsync(Subpage3, Constants.Security.SuperUserKey);
        Assert.IsTrue(createContentResultSubPage3.Success);
        if (!Subpage3.Key.HasValue)
        {
            throw new InvalidOperationException("The content page key is null.");
        }

        if (createContentResultSubPage3.Result.Content != null)
        {
            Subpage3Id = createContentResultSubPage3.Result.Content.Id;
        }
    }
}
