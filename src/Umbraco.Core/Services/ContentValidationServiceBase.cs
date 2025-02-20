using System.ComponentModel.DataAnnotations;
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
            .Where(propertyType => propertyType.VariesByNothing())
            .ToArray();
        IPropertyType[] variantPropertyTypes = contentTypePropertyTypes.Except(invariantPropertyTypes).ToArray();

        var cultures = culturesToValidate?.WhereNotNull().Except(["*"]).ToArray();
        if (cultures?.Any() is not true)
        {
            cultures = await GetCultureCodes();
        }

        // we don't have any managed segments, so we have to make do with the ones passed in the model
        var segments = contentEditingModelBase.Variants
            .Where(variant => variant.Culture is null || cultures.Contains(variant.Culture))
            .DistinctBy(variant => variant.Segment).Select(variant => variant.Segment)
            .WhereNotNull()
            .ToArray();

        foreach (IPropertyType propertyType in invariantPropertyTypes)
        {
            var validationContext = new PropertyValidationContext
            {
                Culture = null, Segment = null, CulturesBeingValidated = cultures, SegmentsBeingValidated = segments
            };

            PropertyValueModel? propertyValueModel = contentEditingModelBase.InvariantProperties.FirstOrDefault(propertyValue => propertyValue.Alias == propertyType.Alias);
            validationErrors.AddRange(ValidateProperty(propertyType, propertyValueModel, validationContext));
        }

        if (variantPropertyTypes.Any() is false)
        {
            return new ContentValidationResult { ValidationErrors = validationErrors };
        }

        foreach (IPropertyType propertyType in variantPropertyTypes)
        {
            foreach (var culture in cultures)
            {
                foreach (var segment in segments.DefaultIfEmpty(null))
                {
                    var validationContext = new PropertyValidationContext
                    {
                        Culture = culture, Segment = segment, CulturesBeingValidated = cultures, SegmentsBeingValidated = segments
                    };

                    PropertyValueModel? propertyValueModel = contentEditingModelBase
                        .Variants
                        .FirstOrDefault(variant => string.Equals(variant.Culture, culture, StringComparison.InvariantCultureIgnoreCase) && string.Equals(segment, variant.Segment, StringComparison.InvariantCultureIgnoreCase))?
                        .Properties
                        .FirstOrDefault(propertyValue => propertyValue.Alias == propertyType.Alias);
                    validationErrors.AddRange(ValidateProperty(propertyType, propertyValueModel, validationContext));
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

    private async Task<string[]> GetCultureCodes() => (await _languageService.GetAllAsync()).Select(language => language.IsoCode).ToArray();

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
