using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal abstract class ContentValidationServiceBase<TContentType>
    where TContentType : IContentTypeComposition
{
    private readonly ILanguageService _languageService;
    private readonly IPropertyValidationService _propertyValidationService;

    protected ContentValidationServiceBase(
        IPropertyValidationService propertyValidationService,
        ILanguageService languageService)
    {
        _propertyValidationService = propertyValidationService;
        _languageService = languageService;
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

        // We don't have managed segments, so we have to make do with the ones passed in the model.
        var segments =
        new string?[] { null }
            .Union(contentEditingModelBase.Variants
                .Where(variant => variant.Culture is null || cultures.Contains(variant.Culture))
                .DistinctBy(variant => variant.Segment).Select(variant => variant.Segment)
                .WhereNotNull())
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

        if (cultureAndSegmentVariantPropertyTypes.Length > 0)
        {
            // Get a mapping of segments to their associated cultures based on the variants and properties provided in the model.
            // Without managed segments again we need to rely on the model data.
            Dictionary<string, HashSet<string>> segmentCultures = GetPopulatedSegmentCultures(contentEditingModelBase, cultures);

            foreach (IPropertyType propertyType in cultureAndSegmentVariantPropertyTypes)
            {
                foreach (var culture in cultures)
                {
                    foreach (var segment in segments.DefaultIfEmpty(null))
                    {
                        // Skip validation if the segment has cultures defined and the current culture is not included.
                        if (segment is not null &&
                            segmentCultures.TryGetValue(segment, out HashSet<string>? associatedCultures) &&
                            associatedCultures.Contains(culture) is false)
                        {
                            continue;
                        }

                        var validationContext = new PropertyValidationContext
                        {
                            Culture = culture,
                            Segment = segment,
                            CulturesBeingValidated = cultures,
                            SegmentsBeingValidated = segments,
                        };

                        PropertyValueModel? propertyValueModel = contentEditingModelBase
                            .Properties
                            .FirstOrDefault(propertyValue => propertyValue.Alias == propertyType.Alias && propertyValue.Culture.InvariantEquals(culture) && propertyValue.Segment.InvariantEquals(segment));
                        validationErrors.AddRange(ValidateProperty(propertyType, propertyValueModel, validationContext));
                    }
                }
            }
        }

        return new ContentValidationResult { ValidationErrors = validationErrors };
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

    /// <summary>
    /// Gets a dictionary of segments along with the cultures they are associated with.
    /// </summary>
    /// <param name="contentEditingModel">The content editing model.</param>
    /// <param name="cultures">The cultures to consider when finding associated cultures for each segment.</param>
    /// <returns>
    /// A dictionary where the key is a unique segment from <see cref="ContentEditingModelBase.Variants"/> and the value is
    /// the set of cultures that have at least one property defined for that segment in <see cref="ContentEditingModelBase.Properties"/>.
    /// </returns>
    /// <remarks>
    /// Internal to support unit testing.
    /// </remarks>
    internal static Dictionary<string, HashSet<string>> GetPopulatedSegmentCultures(ContentEditingModelBase contentEditingModel, string[] cultures)
    {
        IEnumerable<string> uniqueSegments = contentEditingModel.Variants.Select(variant => variant.Segment).WhereNotNull().Distinct();

        return uniqueSegments.ToDictionary(
            segment => segment,
            segment => contentEditingModel.Properties
                .Where(property => property.Segment.InvariantEquals(segment))
                .Where(property => property.Culture is not null && cultures.Contains(property.Culture))
                .Select(property => property.Culture!)
                .ToHashSet());
    }

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
