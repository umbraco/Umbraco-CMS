// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
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
    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IFileService FileService => GetRequiredService<IFileService>();

    protected ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private ContentEditingService ContentEditingService =>
        (ContentEditingService)GetRequiredService<IContentEditingService>();

    private ContentPublishingService ContentPublishingService =>
        (ContentPublishingService)GetRequiredService<IContentPublishingService>();

    protected ContentCreateModel Trashed { get; private set; }

    protected ContentCreateModel Subpage2 { get; private set; }
    protected ContentCreateModel Subpage3 { get; private set; }

    protected ContentCreateModel Subpage { get; private set; }

    protected ContentCreateModel Textpage { get; private set; }

    protected ContentType ContentType { get; private set; }

    [SetUp]
    public void Setup() => CreateTestData();

    private async void CreateTestData()
    {
        // NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        FileService.SaveTemplate(template);

        // Create and Save ContentType "umbTextpage" -> 1051 (template), 1052 (content type)
        ContentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        ContentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);

        // Create and Save Content "Homepage" based on "umbTextpage" -> 1053
        Textpage = ContentEditingBuilder.CreateSimpleContent(ContentType);
        Textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
        await ContentEditingService.CreateAsync(Textpage, Constants.Security.SuperUserKey);

        if (!Textpage.Key.HasValue)
        {
            throw new InvalidOperationException("The content page key is null.");
        }

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> 1054
        Subpage = ContentEditingBuilder.CreateSimpleContent(ContentType, "Text Page 1", Textpage.Key);
        ContentScheduleCollection contentSchedule =
            ContentScheduleCollection.CreateWithEntry(DateTime.Now.AddMinutes(-5), null);
        CultureAndScheduleModel cultureAndSchedule = new CultureAndScheduleModel
        {
            CulturesToPublishImmediately = Subpage.Variants.Select(x => x.Culture).ToHashSet(),
            Schedules = contentSchedule
        };
        await ContentEditingService.CreateAsync(Subpage, Constants.Security.SuperUserKey);
        if (!Subpage.Key.HasValue)
        {
            throw new InvalidOperationException("The content page key is null.");
        }

        await ContentPublishingService.PublishAsync(Subpage.Key.Value, cultureAndSchedule,
            Constants.Security.SuperUserKey);

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> 1055
        Subpage2 = ContentEditingBuilder.CreateSimpleContent(ContentType, "Text Page 2", Textpage.Key);
        await ContentEditingService.CreateAsync(Subpage2, Constants.Security.SuperUserKey);

        if (!Subpage2.Key.HasValue)
        {
            throw new InvalidOperationException("The content page key is null.");
        }

        Subpage3 = ContentEditingBuilder.CreateSimpleContent(ContentType, "Text Page 3", Textpage.Key);
        await ContentEditingService.CreateAsync(Subpage3, Constants.Security.SuperUserKey);

        if (!Subpage3.Key.HasValue)
        {
            throw new InvalidOperationException("The content page key is null.");
        }
        // // Create and Save Content "Text Page Deleted" based on "umbTextpage" -> 1056
        // Trashed = ContentEditingBuilder.CreateSimpleContent(ContentType, "Text Page Deleted", -20);
        // Trashed.Trashed = true;
        // ContentEditingService.CreateAsync(Trashed, -1);
    }
}
