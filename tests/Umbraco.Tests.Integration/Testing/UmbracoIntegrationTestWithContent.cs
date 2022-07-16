// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Testing;

public abstract class UmbracoIntegrationTestWithContent : UmbracoIntegrationTest
{
    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IFileService FileService => GetRequiredService<IFileService>();

    protected ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    protected Content Trashed { get; private set; }

    protected Content Subpage2 { get; private set; }
    protected Content Subpage3 { get; private set; }

    protected Content Subpage { get; private set; }

    protected Content Textpage { get; private set; }

    protected ContentType ContentType { get; private set; }

    [SetUp]
    public void Setup() => CreateTestData();

    public virtual void CreateTestData()
    {
        // NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        // Create and Save ContentType "umbTextpage" -> 1051 (template), 1052 (content type)
        ContentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        ContentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
        ContentTypeService.Save(ContentType);

        // Create and Save Content "Homepage" based on "umbTextpage" -> 1053
        Textpage = ContentBuilder.CreateSimpleContent(ContentType);
        Textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
        ContentService.Save(Textpage, 0);

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> 1054
        Subpage = ContentBuilder.CreateSimpleContent(ContentType, "Text Page 1", Textpage.Id);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.Now.AddMinutes(-5), null);
        ContentService.Save(Subpage, 0, contentSchedule);

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> 1055
        Subpage2 = ContentBuilder.CreateSimpleContent(ContentType, "Text Page 2", Textpage.Id);
        ContentService.Save(Subpage2, 0);


        Subpage3 = ContentBuilder.CreateSimpleContent(ContentType, "Text Page 3", Textpage.Id);
        ContentService.Save(Subpage3, 0);

        // Create and Save Content "Text Page Deleted" based on "umbTextpage" -> 1056
        Trashed = ContentBuilder.CreateSimpleContent(ContentType, "Text Page Deleted", -20);
        Trashed.Trashed = true;
        ContentService.Save(Trashed, 0);
    }
}
