using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Base class for content validation services that validates properties against content types.
/// </summary>
/// <typeparam name="TContentType">The type of content type composition.</typeparam>
internal abstract class ContentValidationServiceBase<TContentType>
    where TContentType : IContentTypeComposition
{
    private readonly ILanguageService _languageService;
    private readonly IPropertyValidationService _propertyValidationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentValidationServiceBase{TContentType}"/> class.
    /// </summary>
    /// <param name="propertyValidationService">The property validation service.</param>
    /// <param name="languageService">The language service.</param>
    protected ContentValidationServiceBase(
        IPropertyValidationService propertyValidationService,
        ILanguageService languageService)
    {
        _propertyValidationService = propertyValidationService;
        _languageService = languageService;
    }

    /// <summary>
    /// Handles the validation of properties against the content type.
    /// </summary>
    /// <param name="contentEditingModelBase">The content editing model to validate.</param>
    /// <param name="contentType">The content type to validate against.</param>
    /// <param name="culturesToValidate">Optional cultures to restrict validation to.</param>
    /// <returns>The validation result containing any validation errors.</returns>
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


        foreach (IPropertyType propertyType in invariantPropertyTypes)
        {
            var validationContext = new PropertyValidationContext
            {
                Culture = null, Segment = null, CulturesBeingValidated = cultures
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
                    Culture = culture, Segment = null, CulturesBeingValidated = cultures
                };

                PropertyValueModel? propertyValueModel = contentEditingModelBase
                    .Properties
                    .FirstOrDefault(propertyValue => propertyValue.Alias == propertyType.Alias && propertyValue.Culture.InvariantEquals(culture) && propertyValue.Segment is null);
                validationErrors.AddRange(ValidateProperty(propertyType, propertyValueModel, validationContext));
            }
        }

        foreach (IPropertyType propertyType in segmentVariantPropertyTypes)
        {
            PropertyValueModel[] propertyValuesToValidate = contentEditingModelBase
                .Properties
                .Where(propertyValue => propertyValue.Alias == propertyType.Alias && propertyValue.Culture is null)
                .ToArray();
            var segmentsToValidate = propertyValuesToValidate.Select(pv => pv.Segment).Union([null]).Distinct().ToArray();

            foreach (var segment in segmentsToValidate)
            {
                var validationContext = new PropertyValidationContext
                {
                    Culture = null, Segment = segment, CulturesBeingValidated = cultures
                };

                PropertyValueModel? propertyValueModel = propertyValuesToValidate.FirstOrDefault(pv => pv.Segment.InvariantEquals(segment));
                validationErrors.AddRange(ValidateProperty(propertyType, propertyValueModel, validationContext));
            }
        }

        if (cultureAndSegmentVariantPropertyTypes.Length > 0)
        {
            foreach (IPropertyType propertyType in cultureAndSegmentVariantPropertyTypes)
            {
                foreach (var culture in cultures)
                {
                    PropertyValueModel[] propertyValuesToValidate = contentEditingModelBase
                        .Properties
                        .Where(propertyValue => propertyValue.Alias == propertyType.Alias && propertyValue.Culture.InvariantEquals(culture))
                        .ToArray();
                    var segmentsToValidate = propertyValuesToValidate.Select(pv => pv.Segment).Union([null]).Distinct().ToArray();

                    foreach (var segment in segmentsToValidate)
                    {
                        var validationContext = new PropertyValidationContext
                        {
                            Culture = culture,
                            Segment = segment,
                            CulturesBeingValidated = cultures,
                        };

                        PropertyValueModel? propertyValueModel = propertyValuesToValidate.FirstOrDefault(pv => pv.Segment.InvariantEquals(segment));
                        validationErrors.AddRange(ValidateProperty(propertyType, propertyValueModel, validationContext));
                    }
                }
            }
        }

        return new ContentValidationResult { ValidationErrors = validationErrors };
    }

    /// <summary>
    /// Validates that all cultures specified in the content editing model are valid.
    /// </summary>
    /// <param name="contentEditingModelBase">The content editing model to validate.</param>
    /// <returns><c>true</c> if all cultures are valid; otherwise, <c>false</c>.</returns>
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
