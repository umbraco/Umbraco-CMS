// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SeedProfiles;

/// <summary>
///     Seed profile that creates a text page content type with a template and a published document.
///     Multiple test fixture classes can share this seed — the first one pays the cost,
///     subsequent fixtures restore from a database snapshot.
/// </summary>
public class TextPageDocumentSeed : ITestDatabaseSeedProfile
{
    /// <summary>
    ///     Well-known key for the template created by this seed.
    /// </summary>
    public static readonly Guid TemplateKey = new("aaaaaaaa-0001-0001-0001-000000000001");

    /// <summary>
    ///     Well-known key for the content type created by this seed.
    /// </summary>
    public static readonly Guid ContentTypeKey = new("aaaaaaaa-0002-0002-0002-000000000002");

    /// <summary>
    ///     Well-known key for the published document created by this seed.
    /// </summary>
    public static readonly Guid DocumentKey = new("aaaaaaaa-0003-0003-0003-000000000003");

    /// <inheritdoc />
    public string SeedKey => "TextPageDocument";

    /// <inheritdoc />
    public async Task SeedAsync(IServiceProvider services)
    {
        var templateService = services.GetRequiredService<ITemplateService>();
        var contentTypeService = services.GetRequiredService<IContentTypeService>();
        var contentEditingService = services.GetRequiredService<IContentEditingService>();
        var contentPublishingService = services.GetRequiredService<IContentPublishingService>();

        // Create template
        var template = TemplateBuilder.CreateTextPageTemplate("TextPageSeedTemplate");
        template.Key = TemplateKey;
        await templateService.CreateAsync(template, Constants.Security.SuperUserKey);

        // Create content type
        var contentType = ContentTypeBuilder.CreateTextPageContentType(
            defaultTemplateId: template.Id,
            name: "TextPageSeedContentType",
            alias: "textPageSeedContentType");
        contentType.Key = ContentTypeKey;
        contentType.AllowedAsRoot = true;
        await contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create and publish a document
        var createModel = new ContentCreateModel
        {
            Key = DocumentKey,
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            Variants = new List<VariantModel> { new() { Name = "Seeded Text Page" } },
        };
        await contentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        await contentPublishingService.PublishAsync(DocumentKey, new[] { new CulturePublishScheduleModel { Culture = "*" } }, Constants.Security.SuperUserKey);
    }
}
