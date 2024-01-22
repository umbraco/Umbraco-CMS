using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal abstract class ContentValidationServiceBase<TContentType>
    where TContentType : IContentTypeComposition
{
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly ILanguageService _languageService;
    private readonly ILogger<ContentValidationServiceBase<TContentType>> _logger;

    protected ContentValidationServiceBase(
        PropertyEditorCollection propertyEditorCollection,
        ILanguageService languageService,
        ILogger<ContentValidationServiceBase<TContentType>> logger)
    {
        _propertyEditorCollection = propertyEditorCollection;
        _languageService = languageService;
        _logger = logger;
    }

    protected async Task<Attempt<IList<PropertyValidationError>, ContentEditingOperationStatus>> HandlePropertiesValidationAsync(
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
            return ValidationResult();
        }

        var cultures = (await _languageService.GetAllAsync()).Select(language => language.IsoCode).ToArray();
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

        return ValidationResult();

        Attempt<IList<PropertyValidationError>, ContentEditingOperationStatus> ValidationResult()
            => validationErrors.Any() is false
                ? Attempt.SucceedWithStatus<IList<PropertyValidationError>, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, validationErrors)
                : Attempt.FailWithStatus<IList<PropertyValidationError>, ContentEditingOperationStatus>(ContentEditingOperationStatus.PropertyValidationError, validationErrors);
    }

    private IEnumerable<PropertyValidationError> ValidateProperty(ContentEditingModelBase contentEditingModelBase, IPropertyType propertyType, string? culture, string? segment)
    {
        if (_propertyEditorCollection.TryGet(propertyType.PropertyEditorAlias, out IDataEditor? dataEditor) is false)
        {
            _logger.LogWarning("Unable to validate property - no data editor found for property editor: {PropertyEditorAlias}", propertyType.PropertyEditorAlias);
            return Enumerable.Empty<PropertyValidationError>();
        }

        IEnumerable<PropertyValueModel>? properties = culture is null && segment is null
            ? contentEditingModelBase.InvariantProperties
            : contentEditingModelBase
                .Variants
                .FirstOrDefault(variant => variant.Culture == culture && variant.Segment == segment)?
                .Properties;

        PropertyValueModel? propertyValueModel = properties?.FirstOrDefault(p => p.Alias == propertyType.Alias);

        ValidationResult[] validationResults = dataEditor
            .GetValueEditor()
            .Validate(
                propertyValueModel?.Value,
                propertyType.Mandatory,
                propertyType.ValidationRegExp)
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
