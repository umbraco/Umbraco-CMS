using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Web.UI.Controllers;

/// <summary>
/// Debug controller to set up test data simulating a pre-17.3 site with many documents,
/// for testing the parameter batching fix in DocumentUrlRepository.Save.
///
/// Usage: GET /debug/setup-document-url-test
///
/// After running, the umbracoDocumentUrl table contains pre-17.3 style rows (one per
/// document × language × draft/published). A rebuild triggered by the migration or by
/// clearing the rebuild key will attempt to delete all stale rows at once, exceeding
/// SQL Server's 2100 parameter limit if the fix is not applied.
/// </summary>
public class DebugDocumentUrlController : Controller
{
    private static readonly string[] CultureCodes =
        ["en-US", "da-DK", "de-DE"];

    private static readonly string[] DomainPrefixes =
        ["/en", "/da", "/de"];

    private readonly ILanguageService _languageService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IContentService _contentService;
    private readonly IDomainService _domainService;
    private readonly ITemplateService _templateService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IScopeAccessor _scopeAccessor;
    private readonly IScopeProvider _scopeProvider;

    public DebugDocumentUrlController(
        ILanguageService languageService,
        IContentTypeService contentTypeService,
        IContentService contentService,
        IDomainService domainService,
        ITemplateService templateService,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper,
        IScopeAccessor scopeAccessor,
        IScopeProvider scopeProvider)
    {
        _languageService = languageService;
        _contentTypeService = contentTypeService;
        _contentService = contentService;
        _domainService = domainService;
        _templateService = templateService;
        _dataTypeService = dataTypeService;
        _shortStringHelper = shortStringHelper;
        _scopeAccessor = scopeAccessor;
        _scopeProvider = scopeProvider;
    }

    [HttpGet("/debug/setup-document-url-test")]
    public async Task<IActionResult> SetupTestData()
    {
        var sw = Stopwatch.StartNew();
        var log = new List<string>();

        // Step 1: Create 10 languages.
        var languageIds = new List<int>();
        foreach (var cultureCode in CultureCodes)
        {
            ILanguage? existing = await _languageService.GetAsync(cultureCode);
            if (existing is not null)
            {
                languageIds.Add(existing.Id);
                log.Add($"Language {cultureCode} already exists (id={existing.Id}).");
                continue;
            }

            var language = new Language(cultureCode, cultureCode);
            Attempt<ILanguage, LanguageOperationStatus> result = await _languageService.CreateAsync(language, Constants.Security.SuperUserKey);
            if (result.Success)
            {
                languageIds.Add(result.Result!.Id);
                log.Add($"Created language {cultureCode} (id={result.Result.Id}).");
            }
            else
            {
                log.Add($"Failed to create language {cultureCode}: {result.Status}.");
            }
        }

        // Step 2: Create a template that renders the Message property and the current culture.
        const string templateAlias = "debugBulkPage";
        ITemplate? template = await _templateService.GetAsync(templateAlias);
        if (template is null)
        {
            var templateContent = """
                @inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
                @{
                    Layout = null;
                }
                <!DOCTYPE html>
                <html>
                <head><title>@Model!.Name</title></head>
                <body>
                    <h1>@Model!.Name</h1>
                    <p>Message: @Model!.Value("message")</p>
                    <p>Culture: @System.Globalization.CultureInfo.CurrentCulture.Name</p>
                </body>
                </html>
                """;

            Attempt<ITemplate, TemplateOperationStatus> templateResult = await _templateService.CreateAsync(
                "Debug Bulk Page", templateAlias, templateContent, Constants.Security.SuperUserKey);

            if (templateResult.Success)
            {
                template = templateResult.Result;
                log.Add($"Created template '{templateAlias}' (id={template!.Id}).");
            }
            else
            {
                log.Add($"Failed to create template: {templateResult.Status}.");
                return Content(string.Join("\n", log), "text/plain");
            }
        }
        else
        {
            log.Add($"Template '{templateAlias}' already exists (id={template.Id}).");
        }

        // Step 3: Create an invariant content type with a "Message" text string property.
        const string contentTypeAlias = "debugBulkPage";
        IContentType? contentType = _contentTypeService.Get(contentTypeAlias);
        if (contentType is null)
        {
            // Get the Textstring data type.
            IDataType? textstringDataType = await _dataTypeService.GetAsync(Constants.DataTypes.Guids.TextstringGuid);
            if (textstringDataType is null)
            {
                log.Add("ERROR: Textstring data type not found.");
                return Content(string.Join("\n", log), "text/plain");
            }

            contentType = new ContentType(_shortStringHelper, Constants.System.Root)
            {
                Alias = contentTypeAlias,
                Name = "Debug Bulk Page",
                Icon = "icon-document",
                AllowedAsRoot = true,
                AllowedTemplates = new[] { template },
                DefaultTemplateId = template.Id,
            };

            // Add a "Message" property.
            var propertyType = new PropertyType(_shortStringHelper, textstringDataType, "message")
            {
                Name = "Message",
            };
            contentType.AddPropertyGroup("content", "Content");
            contentType.AddPropertyType(propertyType, "content", "Content");

            await _contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

            // Allow under itself (need the Id from creation).
            contentType.AllowedContentTypes = new[]
            {
                new ContentTypeSort(contentType.Key, 0, contentType.Alias),
            };
            await _contentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

            log.Add($"Created content type '{contentTypeAlias}' with Message property and template (id={contentType.Id}).");
        }
        else
        {
            log.Add($"Content type '{contentTypeAlias}' already exists (id={contentType.Id}).");
        }

        // Step 4: Create 10 root nodes, each with a domain and language.
        var rootNodes = new List<IContent>();
        for (var i = 0; i < CultureCodes.Length; i++)
        {
            var rootName = $"Root {CultureCodes[i]}";
            var root = new Content(rootName, Constants.System.Root, contentType)
            {
                TemplateId = template.Id
            };
            root.SetValue("message", $"Message for {rootName}");
            _contentService.Save(root);
            rootNodes.Add(root);

            Attempt<DomainUpdateResult, DomainOperationStatus> domainResult = await _domainService.UpdateDomainsAsync(
                root.Key,
                new DomainsUpdateModel
                {
                    Domains = [new DomainModel { DomainName = DomainPrefixes[i], IsoCode = CultureCodes[i] }],
                });

            log.Add($"Created root '{rootName}' (key={root.Key}) with domain '{DomainPrefixes[i]}' / {CultureCodes[i]} (success={domainResult.Success}).");
        }

        // Step 5: Create 100 documents under each root node, each with a message.
        const int childrenPerRoot = 3;
        foreach (IContent root in rootNodes)
        {
            var batchSw = Stopwatch.StartNew();
            for (var i = 0; i < childrenPerRoot; i++)
            {
                var child = new Content($"Page {i}", root.Id, contentType);
                child.TemplateId = template.Id;
                child.SetValue("message", $"Message for {child.Name} under {root.Name}");
                _contentService.Save(child);
            }

            batchSw.Stop();
            log.Add($"Created {childrenPerRoot} children under '{root.Name}' in {batchSw.Elapsed.TotalSeconds:F1}s.");
        }

        // Step 6: Publish all root nodes with all descendants.
        foreach (IContent root in rootNodes)
        {
            var freshRoot = _contentService.GetById(root.Key);
            if (freshRoot is null)
            {
                log.Add($"WARNING: Could not re-fetch root '{root.Name}' for publishing.");
                continue;
            }

            var batchSw = Stopwatch.StartNew();
            var results = _contentService.PublishBranch(freshRoot, PublishBranchFilter.IncludeUnpublished, ["*"]).ToList();
            batchSw.Stop();

            var succeeded = results.Count(r => r.Success);
            var failed = results.Count(r => !r.Success);
            log.Add($"Published branch for '{freshRoot.Name}': {succeeded} succeeded, {failed} failed in {batchSw.Elapsed.TotalSeconds:F1}s.");

            // Log failure reasons to help diagnose PublishBranch issues.
            foreach (var failure in results.Where(r => !r.Success).Take(5))
            {
                log.Add($"  FAIL: {failure.Result} for '{failure.Content?.Name}' (id={failure.Content?.Id})");
            }

            if (failed > 5)
            {
                log.Add($"  ... and {failed - 5} more failures.");
            }
        }

        // Step 7: Rewrite umbracoDocumentUrl to simulate pre-17.3 state.
        // TEMPORARILY COMMENTED OUT — uncomment to test the migration/rebuild bug.
        /*
        using (IScope scope = _scopeProvider.CreateScope())
        {
            IUmbracoDatabase database = _scopeAccessor.AmbientScope!.Database;

            // Clear existing URL rows.
            var deletedCount = database.Execute("DELETE FROM umbracoDocumentUrl");
            log.Add($"Deleted {deletedCount} existing umbracoDocumentUrl rows.");

            // Get all document keys and names for realistic URL segments.
            List<DocumentInfo> documents = database.Fetch<DocumentInfo>(
                $"SELECT n.uniqueId AS [Key], n.text AS Name FROM umbracoNode n WHERE n.nodeObjectType = @0 AND n.trashed = @1",
                Constants.ObjectTypes.Document,
                false);

            log.Add($"Found {documents.Count} non-trashed documents to create URL rows for.");

            // Insert pre-17.3 style rows: one per (document × language × draft/published).
            var insertCount = 0;
            foreach (DocumentInfo doc in documents)
            {
                // Generate a realistic URL segment from the document name.
                var urlSegment = doc.Name?.ToLowerInvariant().Replace(" ", "-") ?? "unnamed";

                foreach (var languageId in languageIds)
                {
                    foreach (var isDraft in new[] { true, false })
                    {
                        database.Execute(
                            "INSERT INTO umbracoDocumentUrl (uniqueId, languageId, isDraft, urlSegment, isPrimary) VALUES (@0, @1, @2, @3, @4)",
                            doc.Key,
                            languageId,
                            isDraft,
                            urlSegment,
                            true);
                        insertCount++;
                    }
                }
            }

            scope.Complete();

            // Verify we exceed the SQL Server parameter limit.
            var sqlServerLimit = 2100;
            log.Add($"Inserted {insertCount:N0} pre-17.3 style umbracoDocumentUrl rows.");
            log.Add($"  ({documents.Count} docs × {languageIds.Count} languages × 2 draft/published)");
            log.Add($"  SQL Server parameter limit: {sqlServerLimit}");
            log.Add(insertCount > sqlServerLimit
                ? $"  ✓ Row count ({insertCount:N0}) exceeds limit — will reproduce the bug without the fix."
                : $"  ✗ Row count ({insertCount:N0}) does not exceed limit — need more documents.");
        }
        */

        sw.Stop();

        return Content(string.Join("\n", log), "text/plain");
    }

    /// <summary>
    /// Simple DTO for fetching document key and name from the node table.
    /// </summary>
    private class DocumentInfo
    {
        public Guid Key { get; set; }

        public string? Name { get; set; }
    }
}
