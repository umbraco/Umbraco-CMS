using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal abstract class ContentValidationServiceBase<TContentType>
    where TContentType : IContentTypeComposition
{
    private readonly ILanguageService _languageService;
    private readonly IPropertyValidationService _propertyValidationService;
    private readonly ISegmentService _segmentService;

    protected ContentValidationServiceBase(
        IPropertyValidationService propertyValidationService,
        ILanguageService languageService,
        ISegmentService segmentService)
    {
        _propertyValidationService = propertyValidationService;
        _languageService = languageService;
        _segmentService = segmentService;
    }

    protected async Task<ContentValidationResult> HandlePropertiesValidationAsync(
        ContentEditingModelBase contentEditingModelBase,
        TContentType contentType,
        IEnumerable<string?>? culturesToValidate = null)
    {
        var validationErrors = new List<PropertyValidationError>();

        IPropertyType[] contentTypePropertyTypes = contentType.CompositionPropertyTypes.ToArray();
        IPropertyType[] invariantPropertyTypes = contentTypePropertyTypes
            .Where(propertyType => propertyType.Variations == ContentVariation.Nothing)
            .ToArray();
        IPropertyType[] cultureVariantPropertyTypes = contentTypePropertyTypes
            .Where(propertyType => propertyType.Variations == ContentVariation.Culture)
            .ToArray();
        IPropertyType[] segmentVariantPropertyTypes = contentTypePropertyTypes
            .Where(propertyType => propertyType.Variations == ContentVariation.Segment)
            .ToArray();
        IPropertyType[] cultureAndSegmentVariantPropertyTypes = contentTypePropertyTypes
            .Where(propertyType => propertyType.Variations == ContentVariation.CultureAndSegment)
            .ToArray();

        var cultures = culturesToValidate?.WhereNotNull().Except(["*"]).ToArray();
        if (cultures?.Any() is not true)
        {
            cultures = await GetCultureCodes();
        }

        // Fetch defined segments for culture filtering (we only want to validate segments defined for any culture or the specific
        // culture being validated).
        Dictionary<string, Segment> definedSegments = cultureAndSegmentVariantPropertyTypes.Length > 0
            ? await GetDefinedSegments()
            : new(StringComparer.InvariantCultureIgnoreCase);

        // we don't have any managed segments, so we have to make do with the ones passed in the model
        var segments =
            new string?[] { null }
                .Union(contentEditingModelBase.Variants
                    .Where(variant => variant.Culture is null || cultures.Contains(variant.Culture))
                    .DistinctBy(variant => variant.Segment).Select(variant => variant.Segment)
                    .WhereNotNull()
                )
                .ToArray();

        foreach (IPropertyType propertyType in invariantPropertyTypes)
        {
            var validationContext = new PropertyValidationContext
            {
                Culture = null, Segment = null, CulturesBeingValidated = cultures, SegmentsBeingValidated = segments
            };

            PropertyValueModel? propertyValueModel = contentEditingModelBase
                .Properties
                .FirstOrDefault(propertyValue => propertyValue.Alias == propertyType.Alias && propertyValue.Culture is null && propertyValue.Segment is null);
            validationErrors.AddRange(ValidateProperty(propertyType, propertyValueModel, validationContext));
        }

        foreach (IPropertyType propertyType in cultureVariantPropertyTypes)
        {
            foreach (var culture in cultures)
            {
                var validationContext = new PropertyValidationContext
                {
                    Culture = culture, Segment = null, CulturesBeingValidated = cultures, SegmentsBeingValidated = segments
                };

                PropertyValueModel? propertyValueModel = contentEditingModelBase
                    .Properties
                    .FirstOrDefault(propertyValue => propertyValue.Alias == propertyType.Alias && propertyValue.Culture.InvariantEquals(culture) && propertyValue.Segment is null);
                validationErrors.AddRange(ValidateProperty(propertyType, propertyValueModel, validationContext));
            }
        }

        foreach (IPropertyType propertyType in segmentVariantPropertyTypes)
        {
            foreach (var segment in segments)
            {
                var validationContext = new PropertyValidationContext
                {
                    Culture = null, Segment = segment, CulturesBeingValidated = cultures, SegmentsBeingValidated = segments
                };

                PropertyValueModel? propertyValueModel = contentEditingModelBase
                    .Properties
                    .FirstOrDefault(propertyValue => propertyValue.Alias == propertyType.Alias && propertyValue.Culture is null && propertyValue.Segment.InvariantEquals(segment));
                validationErrors.AddRange(ValidateProperty(propertyType, propertyValueModel, validationContext));
            }
        }

        foreach (IPropertyType propertyType in cultureAndSegmentVariantPropertyTypes)
        {
            foreach (var culture in cultures)
            {
                foreach (var segment in segments.DefaultIfEmpty(null))
                {
                    // Skip validation if the segment has cultures defined and the current culture is not included.
                    if (IsSegmentDefinedForCulture(culture, segment, definedSegments) is false)
                    {
                        continue;
                    }

                    var validationContext = new PropertyValidationContext
                    {
                        Culture = culture, Segment = segment, CulturesBeingValidated = cultures, SegmentsBeingValidated = segments
                    };

                    PropertyValueModel? propertyValueModel = contentEditingModelBase
                        .Properties
                        .FirstOrDefault(propertyValue => propertyValue.Alias == propertyType.Alias && propertyValue.Culture.InvariantEquals(culture) && propertyValue.Segment.InvariantEquals(segment));
                    validationErrors.AddRange(ValidateProperty(propertyType, propertyValueModel, validationContext));
                }
            }
        }

        return new ContentValidationResult { ValidationErrors = validationErrors };
    }

    private async Task<Dictionary<string, Segment>> GetDefinedSegments()
    {
        Attempt<PagedModel<Segment>?, SegmentOperationStatus> segmentsResult = await _segmentService.GetPagedSegmentsAsync(0, int.MaxValue);
        return segmentsResult.Success && segmentsResult.Result?.Items is not null
            ? segmentsResult.Result.Items.ToDictionary(s => s.Alias, StringComparer.InvariantCultureIgnoreCase)
            : new(StringComparer.InvariantCultureIgnoreCase);
    }

    private static bool IsSegmentDefinedForCulture(string culture, string? segmentAlias, Dictionary<string, Segment> definedSegments)
    {
        if (string.IsNullOrWhiteSpace(segmentAlias))
        {
            return true;
        }

        if (definedSegments.TryGetValue(segmentAlias, out Segment? segment))
        {
            return segment.Cultures is null || segment.Cultures.Contains(culture, StringComparer.InvariantCultureIgnoreCase);
        }

        return true; // The segment from the incoming model is not defined, so we consider it should be validated for all cultures.
    }

    public async Task<bool> ValidateCulturesAsync(ContentEditingModelBase contentEditingModelBase)
    {
        var cultures = await GetCultureCodes();
        var invalidCultures = contentEditingModelBase
            .Variants
            .Select(variant => variant.Culture)
            .WhereNotNull().Except(cultures).ToArray();

        return invalidCultures.IsCollectionEmpty();
    }

    private async Task<string[]> GetCultureCodes() => (await _languageService.GetAllIsoCodesAsync()).ToArray();

    private IEnumerable<PropertyValidationError> ValidateProperty(IPropertyType propertyType, PropertyValueModel? propertyValueModel, PropertyValidationContext validationContext)
    {
        ValidationResult[] validationResults = _propertyValidationService
                .ValidatePropertyValue(propertyType, propertyValueModel?.Value, validationContext)
                .ToArray();

        if (validationResults.Any() is false)
        {
            return Enumerable.Empty<PropertyValidationError>();
        }

        PropertyValidationError[] validationErrors = validationResults
            .SelectMany(validationResult => ExtractPropertyValidationResultJsonPath(validationResult, propertyType.Alias, validationContext.Culture, validationContext.Segment))
            .ToArray();
        if (validationErrors.Any() is false)
        {
            validationErrors = new[]
            {
                new PropertyValidationError
                {
                    JsonPath = string.Empty,
                    ErrorMessages = validationResults.Select(v => v.ErrorMessage).WhereNotNull().ToArray(),
                    Alias = propertyType.Alias,
                    Culture = validationContext.Culture,
                    Segment = validationContext.Segment
                }
            };
        }

        return validationErrors;
    }

    private IEnumerable<PropertyValidationError> ExtractPropertyValidationResultJsonPath(ValidationResult validationResult, string alias, string? culture, string? segment)
    {
        if (validationResult is not INestedValidationResults nestedValidationResults)
        {
            return Enumerable.Empty<PropertyValidationError>();
        }

        JsonPathValidationError[] results = nestedValidationResults
            .ValidationResults
            .SelectMany(JsonPathValidator.ExtractJsonPathValidationErrors)
            .ToArray();

        return results.Select(item => new PropertyValidationError
            {
                JsonPath = item.JsonPath,
                ErrorMessages = item.ErrorMessages.ToArray(),
                Alias = alias,
                Culture = culture,
                Segment = segment
            }).ToArray();
    }
}
