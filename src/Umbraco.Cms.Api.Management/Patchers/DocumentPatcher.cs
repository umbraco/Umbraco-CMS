using System.Text.Json;
using System.Text.RegularExpressions;
using Umbraco.Cms.Api.Management.OperationStatus;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors.JsonPath;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Patchers;

/// <summary>
/// Applies JSON Patch operations with JSONPath to documents, converting them to update models.
/// </summary>
public class DocumentPatcher
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILanguageService _languageService;
    private readonly JsonPathEvaluator _jsonPathEvaluator;
    private readonly JsonPathCultureExtractor _cultureExtractor;

    public DocumentPatcher(
        IContentEditingService contentEditingService,
        IContentTypeService contentTypeService,
        ILanguageService languageService)
    {
        _contentEditingService = contentEditingService;
        _contentTypeService = contentTypeService;
        _languageService = languageService;
        _jsonPathEvaluator = new JsonPathEvaluator();
        _cultureExtractor = new JsonPathCultureExtractor();
    }

    /// <summary>
    /// Applies PATCH operations to a document and returns an update model.
    /// Validates operations and returns appropriate error status if validation fails.
    /// </summary>
    /// <param name="documentKey">The document key.</param>
    /// <param name="patchModel">The patch model containing operations and affected cultures/segments.</param>
    /// <param name="userKey">The user performing the operation.</param>
    /// <returns>An attempt containing the update model or an error status.</returns>
    public async Task<Attempt<ContentUpdateModel, ContentPatchingOperationStatus>> ApplyPatchAsync(
        Guid documentKey,
        ContentPatchModel patchModel,
        Guid userKey)
    {
        // Validate operation structure
        foreach (PatchOperationModel operation in patchModel.Operations)
        {
            if (!_jsonPathEvaluator.IsValidExpression(operation.Path))
            {
                return Attempt.FailWithStatus(ContentPatchingOperationStatus.InvalidOperation, default(ContentUpdateModel)!);
            }

            // Validate that replace/add operations have a value
            if ((operation.Op == PatchOperationType.Replace || operation.Op == PatchOperationType.Add) &&
                operation.Value is null)
            {
                return Attempt.FailWithStatus(ContentPatchingOperationStatus.InvalidOperation, default(ContentUpdateModel)!);
            }
        }

        // Load the content
        IContent? content = await _contentEditingService.GetAsync(documentKey);
        if (content is null)
        {
            return Attempt.FailWithStatus(ContentPatchingOperationStatus.NotFound, default(ContentUpdateModel)!);
        }

        // Validate cultures (cultures are already extracted in the patch model)
        if (patchModel.AffectedCultures.Any())
        {
            IEnumerable<ILanguage> allLanguages = await _languageService.GetAllAsync();
            var availableCultures = allLanguages.Select(l => l.IsoCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var invalidCultures = patchModel.AffectedCultures.Where(c => !availableCultures.Contains(c)).ToArray();
            if (invalidCultures.Any())
            {
                return Attempt.FailWithStatus(ContentPatchingOperationStatus.InvalidCulture, default(ContentUpdateModel)!);
            }
        }

        // Get content type
        IContentType? contentType = _contentTypeService.Get(content.ContentTypeId);
        if (contentType is null)
        {
            return Attempt.FailWithStatus(ContentPatchingOperationStatus.ContentTypeNotFound, default(ContentUpdateModel)!);
        }

        // Apply operations to build update model
        var variants = new Dictionary<(string? Culture, string? Segment), VariantModel>();
        var properties = new List<PropertyValueModel>();
        Guid? templateKey = null;

        foreach (PatchOperationModel operation in patchModel.Operations)
        {
            // Parse the JSONPath to determine what to update
            if (operation.Path.Contains("$.variants"))
            {
                // Variant operation (name update)
                var culture = _cultureExtractor.ExtractCultures(operation.Path).FirstOrDefault();
                var segment = _cultureExtractor.ExtractSegments(operation.Path).FirstOrDefault();

                if (operation.Op != PatchOperationType.Replace && operation.Op != PatchOperationType.Add)
                {
                    continue;
                }

                var key = (culture, segment);
                if (!variants.ContainsKey(key))
                {
                    variants[key] = new VariantModel
                    {
                        Culture = culture,
                        Segment = segment,
                        Name = string.Empty
                    };
                }

                if (operation.Path.Contains(".name"))
                {
                    variants[key].Name = operation.Value?.ToString() ?? string.Empty;
                }
            }
            else if (operation.Path.Contains("$.values"))
            {
                // Property value operation
                var culture = _cultureExtractor.ExtractCultures(operation.Path).FirstOrDefault();
                var segment = _cultureExtractor.ExtractSegments(operation.Path).FirstOrDefault();

                // Extract property alias from the path (simplified parsing)
                // Path format: $.values[?(@.alias == 'propertyAlias' && ...)].value
                Match aliasMatch = Regex.Match(
                    operation.Path,
                    @"@\.alias\s*==\s*['""]([^'""]+)['""]");

                if (!aliasMatch.Success)
                {
                    return Attempt.FailWithStatus(ContentPatchingOperationStatus.InvalidOperation, default(ContentUpdateModel)!);
                }

                var propertyAlias = aliasMatch.Groups[1].Value;

                // Validate property exists on the content type
                IPropertyType? propertyType = contentType.CompositionPropertyTypes.FirstOrDefault(pt => pt.Alias == propertyAlias);
                if (propertyType is null)
                {
                    return Attempt.FailWithStatus(ContentPatchingOperationStatus.PropertyTypeNotFound, default(ContentUpdateModel)!);
                }

                // Ensure variant exists for this culture/segment combination
                // This is required for segment variation support
                var key = (culture, segment);
                if (!variants.ContainsKey(key))
                {
                    // Get the current name from the content
                    var currentName = content.GetCultureName(culture) ?? string.Empty;

                    variants[key] = new VariantModel
                    {
                        Culture = culture,
                        Segment = segment,
                        Name = currentName
                    };
                }

                // If content type varies by segment and we're updating a specific segment,
                // ensure we also have a default (null segment) variant to satisfy validation
                if (segment != null && contentType.VariesBySegment())
                {
                    var defaultKey = (culture, (string?)null);
                    if (!variants.ContainsKey(defaultKey))
                    {
                        var currentName = content.GetCultureName(culture) ?? string.Empty;

                        variants[defaultKey] = new VariantModel
                        {
                            Culture = culture,
                            Segment = null,
                            Name = currentName
                        };
                    }
                }

                if (operation.Op == PatchOperationType.Replace || operation.Op == PatchOperationType.Add)
                {
                    properties.Add(new PropertyValueModel
                    {
                        Alias = propertyAlias,
                        Value = operation.Value,
                        Culture = culture,
                        Segment = segment,
                    });
                }
                else if (operation.Op == PatchOperationType.Remove)
                {
                    properties.Add(new PropertyValueModel
                    {
                        Alias = propertyAlias,
                        Value = null,
                        Culture = culture,
                        Segment = segment,
                    });
                }
            }
            else if (operation.Path.Contains("$.template"))
            {
                // Template operation
                if (operation.Op == PatchOperationType.Replace || operation.Op == PatchOperationType.Add)
                {
                    if (operation.Value is JsonElement jsonElement && jsonElement.TryGetProperty("id", out JsonElement idElement))
                    {
                        templateKey = idElement.GetGuid();
                    }
                    else if (operation.Value is Guid guid)
                    {
                        templateKey = guid;
                    }
                }
                else if (operation.Op == PatchOperationType.Remove)
                {
                    templateKey = null;
                }
            }
        }

        var updateModel = new ContentUpdateModel
        {
            Variants = variants.Values.ToArray(),
            Properties = properties.ToArray(),
            TemplateKey = templateKey,
        };

        return Attempt.SucceedWithStatus(ContentPatchingOperationStatus.Success, updateModel);
    }
}
