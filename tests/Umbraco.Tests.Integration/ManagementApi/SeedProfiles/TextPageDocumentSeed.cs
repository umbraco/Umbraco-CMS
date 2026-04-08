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
/// Seed profile that creates a text page content type with a template and a published document.
/// </summary>
public class TextPageDocumentSeed : ITestDatabaseSeedProfile
{
    /// <summary>
    ///     Well-known key for the template created by this seed.
    /// </summary>
    public static readonly Guid TemplateKey = new("abe050d1-a094-44f1-b49b-fc0b13c53404");

    /// <summary>
    ///     Well-known key for the content type created by this seed.
    /// </summary>
    public static readonly Guid ContentTypeKey = new("a128b76b-6a34-44b3-b149-8988185f5d2e");

    /// <summary>
    ///     Well-known key for the published document created by this seed.
    /// </summary>
    public static readonly Guid DocumentKey = new("27fa2640-d13f-4caf-9f36-9ff1065dad51");

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

        // Create contentType
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
