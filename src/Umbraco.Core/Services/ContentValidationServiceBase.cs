using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
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
        TContentType contentType)
    {
        var validationErrors = new List<PropertyValidationError>();

        IPropertyType[] contentTypePropertyTypes = contentType.CompositionPropertyTypes.ToArray();
        IPropertyType[] invariantPropertyTypes = contentTypePropertyTypes
            .Where(propertyType => propertyType.VariesByNothing())
            .ToArray();
        IPropertyType[] variantPropertyTypes = contentTypePropertyTypes.Except(invariantPropertyTypes).ToArray();

        foreach (IPropertyType propertyType in invariantPropertyTypes)
        {
            validationErrors.AddRange(ValidateProperty(contentEditingModelBase, propertyType, null, null));
        }

        if (variantPropertyTypes.Any() is false)
        {
            return new ContentValidationResult { ValidationErrors = validationErrors };
        }

        var cultures = await GetCultureCodes();
        // we don't have any managed segments, so we have to make do with the ones passed in the model
        var segments = contentEditingModelBase.Variants.DistinctBy(variant => variant.Segment).Select(variant => variant.Segment).ToArray();

        foreach (IPropertyType propertyType in variantPropertyTypes)
        {
            foreach (var culture in cultures)
            {
                foreach (var segment in segments)
                {
                    validationErrors.AddRange(ValidateProperty(contentEditingModelBase, propertyType, culture, segment));
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

    private IEnumerable<PropertyValidationError> ValidateProperty(ContentEditingModelBase contentEditingModelBase, IPropertyType propertyType, string? culture, string? segment)
    {
        IEnumerable<PropertyValueModel>? properties = culture is null && segment is null
            ? contentEditingModelBase.InvariantProperties
            : contentEditingModelBase
                .Variants
                .FirstOrDefault(variant => string.Equals(variant.Culture, culture, StringComparison.InvariantCultureIgnoreCase) && string.Equals(segment, variant.Segment, StringComparison.InvariantCultureIgnoreCase))?
                .Properties;

        PropertyValueModel? propertyValueModel = properties?.FirstOrDefault(p => p.Alias == propertyType.Alias);

        ValidationResult[] validationResults = _propertyValidationService
                .ValidatePropertyValue(propertyType, propertyValueModel?.Value)
                .ToArray();

        if (validationResults.Any() is false)
        {
            return Enumerable.Empty<PropertyValidationError>();
        }

        PropertyValidationError[] validationErrors = validationResults
            .SelectMany(validationResult => ExtractPropertyValidationResultJsonPath(validationResult, propertyType.Alias, culture, segment))
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
                    Culture = culture,
                    Segment = segment
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
