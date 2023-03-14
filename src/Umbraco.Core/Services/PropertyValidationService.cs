using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class PropertyValidationService : IPropertyValidationService
{
    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ILocalizedTextService _textService;
    private readonly IValueEditorCache _valueEditorCache;

    public PropertyValidationService(
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        ILocalizedTextService textService,
        IValueEditorCache valueEditorCache)
    {
        _propertyEditors = propertyEditors;
        _dataTypeService = dataTypeService;
        _textService = textService;
        _valueEditorCache = valueEditorCache;
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> ValidatePropertyValue(
        IPropertyType propertyType,
        object? postedValue)
    {
        if (propertyType is null)
        {
            throw new ArgumentNullException(nameof(propertyType));
        }

        IDataType? dataType = _dataTypeService.GetDataType(propertyType.DataTypeId);
        if (dataType == null)
        {
            throw new InvalidOperationException("No data type found by id " + propertyType.DataTypeId);
        }

        IDataEditor? editor = _propertyEditors[propertyType.PropertyEditorAlias];
        if (editor == null)
        {
            throw new InvalidOperationException("No property editor found by alias " +
                                                propertyType.PropertyEditorAlias);
        }

        return ValidatePropertyValue(editor, dataType, postedValue, propertyType.Mandatory, propertyType.ValidationRegExp, propertyType.MandatoryMessage, propertyType.ValidationRegExpMessage);
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> ValidatePropertyValue(
        IDataEditor editor,
        IDataType dataType,
        object? postedValue,
        bool isRequired,
        string? validationRegExp,
        string? isRequiredMessage,
        string? validationRegExpMessage)
    {
        // Retrieve default messages used for required and regex validatation.  We'll replace these
        // if set with custom ones if they've been provided for a given property.
        var requiredDefaultMessages = new[]
        {
            _textService.Localize("validation", "invalidNull"), _textService.Localize("validation", "invalidEmpty"),
        };
        var formatDefaultMessages = new[] { _textService.Localize("validation", "invalidPattern") };

        IDataValueEditor valueEditor = _valueEditorCache.GetValueEditor(editor, dataType);
        foreach (ValidationResult validationResult in valueEditor.Validate(postedValue, isRequired, validationRegExp))
        {
            // If we've got custom error messages, we'll replace the default ones that will have been applied in the call to Validate().
            if (isRequired && !string.IsNullOrWhiteSpace(isRequiredMessage) &&
                requiredDefaultMessages.Contains(validationResult.ErrorMessage, StringComparer.OrdinalIgnoreCase))
            {
                validationResult.ErrorMessage = isRequiredMessage;
            }

            if (!string.IsNullOrWhiteSpace(validationRegExp) && !string.IsNullOrWhiteSpace(validationRegExpMessage) &&
                formatDefaultMessages.Contains(validationResult.ErrorMessage, StringComparer.OrdinalIgnoreCase))
            {
                validationResult.ErrorMessage = validationRegExpMessage;
            }

            yield return validationResult;
        }
    }

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
                return !(propertyTypeVaries || IsPropertyValid(x, null));
            }

            // impacts all = validate property, all cultures (incl. invariant)
            if (impact.ImpactsAllCultures)
            {
                return !IsPropertyValid(x);
            }

            // impacts explicit culture = validate variant property, explicit culture
            if (propertyTypeVaries)
            {
                return !IsPropertyValid(x, impact.Culture);
            }

            // and, for explicit culture, we may also have to validate invariant property, invariant culture
            // if either
            // - it is impacted (default culture), or
            // - there is no published version of the content - maybe non-default culture, but no published version
            var alsoInvariant = impact.ImpactsAlsoInvariantProperties || !content.Published;
            return alsoInvariant && !IsPropertyValid(x, null);
        }).ToArray();

        return invalidProperties.Length == 0;
    }

    /// <inheritdoc />
    public bool IsPropertyValid(IProperty property, string? culture = "*", string? segment = "*")
    {
        // NOTE - the pvalue and vvalues logic in here is borrowed directly from the Property.Values setter so if you are wondering what that's all about, look there.
        // The underlying Property._pvalue and Property._vvalues are not exposed but we can re-create these values ourselves which is what it's doing.
        culture = culture?.NullOrWhiteSpaceAsNull();
        segment = segment?.NullOrWhiteSpaceAsNull();

        IPropertyValue? pvalue = null;

        // if validating invariant/neutral, and it is supported, validate
        // (including ensuring that the value exists, if mandatory)
        if ((culture == null || culture == "*") && (segment == null || segment == "*") &&
            property.PropertyType.SupportsVariation(null, null))
        {
            // validate pvalue (which is the invariant value)
            pvalue = property.Values.FirstOrDefault(x => x.Culture == null && x.Segment == null);
            if (!IsValidPropertyValue(property, pvalue?.EditedValue))
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
            return culture == "*" || IsValidPropertyValue(property, null);
        }

        // else validate vvalues (but don't revalidate pvalue)
        var pvalues = property.Values.Where(x =>
                x != pvalue && // don't revalidate pvalue
                property.PropertyType.SupportsVariation(x.Culture, x.Segment, true) && // the value variation is ok
                    (culture == "*" || x.Culture.InvariantEquals(culture)) && // the culture matches
                    (segment == "*" || x.Segment.InvariantEquals(segment))) // the segment matches
            .ToList();

        return pvalues.Count == 0 || pvalues.All(x => IsValidPropertyValue(property, x.EditedValue));
    }

    /// <summary>
    ///     Boolean indicating whether the passed in value is valid
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    /// <returns>True is property value is valid, otherwise false</returns>
    private bool IsValidPropertyValue(IProperty property, object? value) =>
        IsPropertyValueValid(property.PropertyType, value);

    /// <summary>
    ///     Determines whether a value is valid for this property type.
    /// </summary>
    private bool IsPropertyValueValid(IPropertyType propertyType, object? value)
    {
        IDataEditor? editor = _propertyEditors[propertyType.PropertyEditorAlias];
        if (editor == null)
        {
            // nothing much we can do validation wise if the property editor has been removed.
            // the property will be displayed as a label, so flagging it as invalid would be pointless.
            return true;
        }

        var configuration = _dataTypeService.GetDataType(propertyType.DataTypeId)?.Configuration;
        IDataValueEditor valueEditor = editor.GetValueEditor(configuration);
        return !valueEditor.Validate(value, propertyType.Mandatory, propertyType.ValidationRegExp).Any();
    }
}
