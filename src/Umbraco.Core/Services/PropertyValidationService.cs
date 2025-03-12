using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class PropertyValidationService : IPropertyValidationService
{
    private readonly IDataTypeService _dataTypeService;
    private readonly ILocalizedTextService _textService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IValueEditorCache _valueEditorCache;
    private readonly ICultureDictionary _cultureDictionary;

    public PropertyValidationService(
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        ILocalizedTextService textService,
        IValueEditorCache valueEditorCache,
        ICultureDictionary cultureDictionary)
    {
        _propertyEditors = propertyEditors;
        _dataTypeService = dataTypeService;
        _textService = textService;
        _valueEditorCache = valueEditorCache;
        _cultureDictionary = cultureDictionary;
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> ValidatePropertyValue(
        IPropertyType propertyType,
        object? postedValue,
        PropertyValidationContext validationContext)
    {
        if (propertyType is null)
        {
            throw new ArgumentNullException(nameof(propertyType));
        }

        IDataType? dataType = GetDataType(propertyType);
        if (dataType == null)
        {
            throw new InvalidOperationException("No data type found by id " + propertyType.DataTypeId);
        }

        IDataEditor? dataEditor = GetDataEditor(propertyType);
        if (dataEditor == null)
        {
            throw new InvalidOperationException("No property editor found by alias " +
                                                propertyType.PropertyEditorAlias);
        }

        return ValidatePropertyValue(dataEditor, dataType, postedValue, propertyType.Mandatory, propertyType.ValidationRegExp, propertyType.MandatoryMessage, propertyType.ValidationRegExpMessage, validationContext);
    }

    [Obsolete("Please use the overload that accepts a PropertyValidationContext. Will be removed in V16.")]
    public IEnumerable<ValidationResult> ValidatePropertyValue(
        IPropertyType propertyType,
        object? postedValue)
        => ValidatePropertyValue(propertyType, postedValue, PropertyValidationContext.Empty());

    /// <inheritdoc />
    public IEnumerable<ValidationResult> ValidatePropertyValue(
        IDataEditor editor,
        IDataType dataType,
        object? postedValue,
        bool isRequired,
        string? validationRegExp,
        string? isRequiredMessage,
        string? validationRegExpMessage,
        PropertyValidationContext validationContext)
    {
        // Retrieve default messages used for required and regex validatation.  We'll replace these
        // if set with custom ones if they've been provided for a given property.
        var requiredDefaultMessages = new[] { Constants.Validation.ErrorMessages.Properties.Missing, Constants.Validation.ErrorMessages.Properties.Empty };
        var formatDefaultMessages = new[] { Constants.Validation.ErrorMessages.Properties.PatternMismatch };

        IDataValueEditor valueEditor = _valueEditorCache.GetValueEditor(editor, dataType);
        foreach (ValidationResult validationResult in valueEditor.Validate(postedValue, isRequired, validationRegExp, validationContext))
        {
            // If we've got custom error messages, we'll replace the default ones that will have been applied in the call to Validate().
            if (isRequired && !string.IsNullOrWhiteSpace(isRequiredMessage) &&
                requiredDefaultMessages.Contains(validationResult.ErrorMessage, StringComparer.OrdinalIgnoreCase))
            {
                validationResult.ErrorMessage = _textService.UmbracoDictionaryTranslate(_cultureDictionary, isRequiredMessage);
            }

            if (!string.IsNullOrWhiteSpace(validationRegExp) && !string.IsNullOrWhiteSpace(validationRegExpMessage) &&
                formatDefaultMessages.Contains(validationResult.ErrorMessage, StringComparer.OrdinalIgnoreCase))
            {
                validationResult.ErrorMessage = _textService.UmbracoDictionaryTranslate(_cultureDictionary, validationRegExpMessage);
            }

            yield return validationResult;
        }
    }

    [Obsolete("Please use the overload that accepts a PropertyValidationContext. Will be removed in V16.")]
    public IEnumerable<ValidationResult> ValidatePropertyValue(
        IDataEditor editor,
        IDataType dataType,
        object? postedValue,
        bool isRequired,
        string? validationRegExp,
        string? isRequiredMessage,
        string? validationRegExpMessage)
        => ValidatePropertyValue(editor, dataType, postedValue, isRequired, validationRegExp, isRequiredMessage, validationRegExpMessage, PropertyValidationContext.Empty());

    /// <inheritdoc />
    public bool IsPropertyDataValid(IContent content, out IProperty[] invalidProperties, CultureImpact? impact)
    {
        // select invalid properties
        invalidProperties = content.Properties.Where(x =>
        {
            var propertyTypeVaries = x.PropertyType.VariesByCulture();

            if (impact is null)
            {
                return false;
            }

            // impacts invariant = validate invariant property, invariant culture
            if (impact.ImpactsOnlyInvariantCulture)
            {
                return !(propertyTypeVaries || IsPropertyValid(x, PropertyValidationContext.Empty()));
            }

            // impacts all = validate property, all cultures (incl. invariant)
            if (impact.ImpactsAllCultures)
            {
                return !IsPropertyValid(x, PropertyValidationContext.CultureAndSegment("*", null));
            }

            // impacts explicit culture = validate variant property, explicit culture
            if (propertyTypeVaries)
            {
                return !IsPropertyValid(x, PropertyValidationContext.CultureAndSegment(impact.Culture, null));
            }

            if (impact.ImpactsExplicitCulture && GetDataEditor(x.PropertyType)?.CanMergePartialPropertyValues(x.PropertyType) is true)
            {
                return !IsPropertyValid(x, new PropertyValidationContext
                {
                    Culture = null,
                    Segment = null,
                    CulturesBeingValidated = [impact.Culture!],
                    SegmentsBeingValidated = []
                });
            }

            // and, for explicit culture, we may also have to validate invariant property, invariant culture
            // if either
            // - it is impacted (default culture), or
            // - there is no published version of the content - maybe non-default culture, but no published version
            var alsoInvariant = impact.ImpactsAlsoInvariantProperties || !content.Published;
            return alsoInvariant && !IsPropertyValid(x, PropertyValidationContext.Empty());
        }).ToArray();

        return invalidProperties.Length == 0;
    }

    [Obsolete("Please use the overload that accepts a PropertyValidationContext. Will be removed in V16.")]
    public bool IsPropertyValid(IProperty property, string culture = "*", string segment = "*")
        => IsPropertyValid(property, PropertyValidationContext.CultureAndSegment(culture, segment));

    /// <inheritdoc />
    public bool IsPropertyValid(IProperty property, PropertyValidationContext validationContext)
    {
        // NOTE - the pvalue and vvalues logic in here is borrowed directly from the Property.Values setter so if you are wondering what that's all about, look there.
        // The underlying Property._pvalue and Property._vvalues are not exposed but we can re-create these values ourselves which is what it's doing.
        validationContext = new PropertyValidationContext
        {
            Culture = validationContext.Culture?.NullOrWhiteSpaceAsNull(),
            Segment = validationContext.Segment?.NullOrWhiteSpaceAsNull(),
            CulturesBeingValidated = validationContext.CulturesBeingValidated,
            SegmentsBeingValidated = validationContext.SegmentsBeingValidated
        };

        var culture = validationContext.Culture;
        var segment = validationContext.Segment;

        IPropertyValue? pvalue = null;

        // if validating invariant/neutral, and it is supported, validate
        // (including ensuring that the value exists, if mandatory)
        if ((culture == null || culture == "*") && (segment == null || segment == "*") &&
            property.PropertyType.SupportsVariation(null, null))
        {
            // validate pvalue (which is the invariant value)
            pvalue = property.Values.FirstOrDefault(x => x.Culture == null && x.Segment == null);
            if (!IsValidPropertyValue(property, pvalue?.EditedValue, validationContext))
            {
                return false;
            }
        }

        // if validating only invariant/neutral, we are good
        if (culture == null && segment == null)
        {
            return true;
        }

        // if nothing else to validate, we are good
        if ((culture == null || culture == "*") && (segment == null || segment == "*") &&
            !property.PropertyType.VariesByCulture())
        {
            return true;
        }

        // for anything else, validate the existing values (including mandatory),
        // but we cannot validate mandatory globally (we don't know the possible cultures and segments)

        // validate vvalues (which are the variant values)

        // if we don't have vvalues (property.Values is empty or only contains pvalue), validate null
        if (property.Values.Count == (pvalue == null ? 0 : 1))
        {
            return culture == "*" || IsValidPropertyValue(property, null, validationContext);
        }

        // else validate vvalues (but don't revalidate pvalue)
        var vvalues = property.Values.Where(x =>
                x != pvalue && // don't revalidate pvalue
                property.PropertyType.SupportsVariation(x.Culture, x.Segment, true) && // the value variation is ok
                    (culture == "*" || x.Culture.InvariantEquals(culture)) && // the culture matches
                    (segment == "*" || x.Segment.InvariantEquals(segment))) // the segment matches
            .ToList();

        // if we do not have any vvalues at this point, validate null (no variant values present)
        if (vvalues.Any() is false)
        {
            return IsValidPropertyValue(property, null, validationContext);
        }

        return vvalues.All(x => IsValidPropertyValue(property, x.EditedValue, validationContext));
    }

    /// <summary>
    ///     Boolean indicating whether the passed in value is valid
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns>True is property value is valid, otherwise false</returns>
    private bool IsValidPropertyValue(IProperty property, object? value, PropertyValidationContext validationContext) =>
        IsPropertyValueValid(property.PropertyType, value, validationContext);

    /// <summary>
    ///     Determines whether a value is valid for this property type.
    /// </summary>
    private bool IsPropertyValueValid(IPropertyType propertyType, object? value, PropertyValidationContext validationContext)
    {
        IDataEditor? editor = GetDataEditor(propertyType);
        if (editor == null)
        {
            // nothing much we can do validation wise if the property editor has been removed.
            // the property will be displayed as a label, so flagging it as invalid would be pointless.
            return true;
        }

        var configuration = GetDataType(propertyType)?.ConfigurationObject;
        IDataValueEditor valueEditor = editor.GetValueEditor(configuration);

        return !valueEditor.Validate(value, propertyType.Mandatory, propertyType.ValidationRegExp, validationContext).Any();
    }

    private IDataType? GetDataType(IPropertyType propertyType)
        => _dataTypeService.GetDataType(propertyType.DataTypeId);

    private IDataEditor? GetDataEditor(IPropertyType propertyType)
        => _propertyEditors[propertyType.PropertyEditorAlias];
}
