// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Testing.SeedProfiles;

/// <summary>
///     Seed profile that performs the Umbraco unattended install and creates
///     standard test content matching <c>UmbracoIntegrationTestWithContent.CreateTestData()</c>.
///     <para>
///         Using this as a <see cref="DatabaseSeedProfileAttribute"/> allows fixtures to
///         restore a snapshot of the installed state with pre-built content instead of
///         running the full install and content creation every time.
///     </para>
/// </summary>
public class InstalledSchemaWithContentSeed : ITestDatabaseSeedProfile
{
    public static readonly Guid ContentTypeKey = new("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
    public static readonly Guid TextpageKey = new("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
    public static readonly Guid SubPageKey = new("07EABF4A-5C62-4662-9F2A-15BBB488BCA5");
    public static readonly Guid SubPage2Key = new("0EED78FC-A6A8-4587-AB18-D3AFE212B1C4");
    public static readonly Guid SubPage3Key = new("29BBB8CF-E69B-4A21-9363-02ED5B6637C4");
    public static readonly Guid TrashedKey = new("EAE9EE57-FFE4-4841-8586-1B636C43A3D4");

    /// <inheritdoc />
    public string SeedKey => "__installed_schema_with_content__";

    /// <inheritdoc />
    public async Task SeedAsync(IServiceProvider services)
    {
        // Determine runtime level so the system knows we're in "Install" state
        var state = services.GetRequiredService<IRuntimeState>();
        state.DetermineRuntimeLevel();

        // Run the unattended install — creates default user, data types, etc.
        services.GetRequiredService<IEventAggregator>()
            .Publish(new UnattendedInstallNotification());

        // Note: we do NOT register the OpenIddict backoffice application here.
        // That is only needed for ManagementApi tests (web application).
        // Service tests use a plain host without API/auth infrastructure.

        // Create the same test data as UmbracoIntegrationTestWithContent.CreateTestData()
        var fileService = services.GetRequiredService<IFileService>();
        var contentTypeService = services.GetRequiredService<IContentTypeService>();
        var contentService = services.GetRequiredService<IContentService>();

        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        fileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        contentType.Key = ContentTypeKey;
        contentTypeService.Save(contentType);

        var textpage = ContentBuilder.CreateSimpleContent(contentType, "Textpage");
        textpage.Key = TextpageKey;
        contentService.Save(textpage, -1);

        var subpage = ContentBuilder.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
        subpage.Key = SubPageKey;
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(-5), null);
        contentService.Save(subpage, -1, contentSchedule);

        var subpage2 = ContentBuilder.CreateSimpleContent(contentType, "Text Page 2", textpage.Id);
        subpage2.Key = SubPage2Key;
        contentService.Save(subpage2, -1);

        var subpage3 = ContentBuilder.CreateSimpleContent(contentType, "Text Page 3", textpage.Id);
        subpage3.Key = SubPage3Key;
        contentService.Save(subpage3, -1);

        var trashed = ContentBuilder.CreateSimpleContent(contentType, "Text Page Deleted", -20);
        trashed.Trashed = true;
        trashed.Key = TrashedKey;
        contentService.Save(trashed, -1);
    }
}
