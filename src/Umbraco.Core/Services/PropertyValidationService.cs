using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides property validation functionality for content, media, and member properties.
/// </summary>
public class PropertyValidationService : IPropertyValidationService
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ILocalizedTextService _textService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IValueEditorCache _valueEditorCache;
    private readonly ICultureDictionary _cultureDictionary;
    private readonly ILanguageService _languageService;
    private readonly ContentSettings _contentSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyValidationService" /> class.
    /// </summary>
    /// <param name="propertyEditors">The collection of property editors.</param>
    /// <param name="dataTypeService">The data type service for retrieving data types.</param>
    /// <param name="textService">The localized text service for retrieving validation messages.</param>
    /// <param name="valueEditorCache">The value editor cache for caching value editors.</param>
    /// <param name="cultureDictionary">The culture dictionary for translating validation messages.</param>
    /// <param name="languageService">The language service for language operations.</param>
    /// <param name="contentSettings">The content settings options.</param>
    /// <param name="idKeyMap">The cached id-to-key map used to resolve int data type IDs to GUID keys.</param>
    public PropertyValidationService(
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        ILocalizedTextService textService,
        IValueEditorCache valueEditorCache,
        ICultureDictionary cultureDictionary,
        ILanguageService languageService,
        IOptions<ContentSettings> contentSettings,
        IIdKeyMap idKeyMap)
    {
        _propertyEditors = propertyEditors;
        _dataTypeService = dataTypeService;
        _textService = textService;
        _valueEditorCache = valueEditorCache;
        _cultureDictionary = cultureDictionary;
        _languageService = languageService;
        _contentSettings = contentSettings.Value;
        _idKeyMap = idKeyMap;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyValidationService" /> class.
    /// </summary>
    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public PropertyValidationService(
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        ILocalizedTextService textService,
        IValueEditorCache valueEditorCache,
        ICultureDictionary cultureDictionary,
        ILanguageService languageService,
        IOptions<ContentSettings> contentSettings)
        : this(
            propertyEditors,
            dataTypeService,
            textService,
            valueEditorCache,
            cultureDictionary,
            languageService,
            contentSettings,
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>())
    {
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

        IDataType? dataType = propertyType.GetDataType(_dataTypeService, _idKeyMap);
        if (dataType == null)
        {
            throw new InvalidOperationException("No data type found by id " + propertyType.DataTypeId);
        }

        IDataEditor? dataEditor = GetDataEditor(propertyType);
        if (dataEditor is null)
        {
            return [];
        }

        // only validate culture invariant properties if
        // - AllowEditInvariantFromNonDefault is true, or
        // - the default language is being validated, or
        // - the underlying data editor supports partial property value merging (e.g. block level variance)
        var defaultCulture = _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();
        if (propertyType.VariesByCulture() is false
            && _contentSettings.AllowEditInvariantFromNonDefault is false
            && validationContext.CulturesBeingValidated.InvariantContains(defaultCulture) is false
            && dataEditor.CanMergePartialPropertyValues(propertyType) is false)
        {
            return [];
        }

        var isRequired = ShouldValidateAsRequired(propertyType, validationContext);
        return ValidatePropertyValue(dataEditor, dataType, postedValue, isRequired, propertyType.ValidationRegExp, propertyType.MandatoryMessage, propertyType.ValidationRegExpMessage, validationContext);
    }

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

    /// <inheritdoc />
    public bool IsPropertyDataValid(IPublishableContentBase content, out IProperty[] invalidProperties, CultureImpact? impact)
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
#pragma warning disable CS0618 // Type or member is obsolete - IsPropertyValid() will be retained as internal after the obsoletion period.
                return !(propertyTypeVaries || IsPropertyValid(x, PropertyValidationContext.Empty()));
#pragma warning restore CS0618 // Type or member is obsolete
            }

            // impacts all = validate property, all cultures (incl. invariant)
            if (impact.ImpactsAllCultures)
            {
#pragma warning disable CS0618 // Type or member is obsolete - IsPropertyValid() will be retained as internal after the obsoletion period.
                return !IsPropertyValid(x, PropertyValidationContext.CultureAndSegment("*", null));
#pragma warning restore CS0618 // Type or member is obsolete
            }

            // impacts explicit culture = validate variant property, explicit culture
            if (propertyTypeVaries)
            {
#pragma warning disable CS0618 // Type or member is obsolete - IsPropertyValid() will be retained as internal after the obsoletion period.
                return !IsPropertyValid(x, PropertyValidationContext.CultureAndSegment(impact.Culture, null));
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (impact.ImpactsExplicitCulture && GetDataEditor(x.PropertyType)?.CanMergePartialPropertyValues(x.PropertyType) is true)
            {
#pragma warning disable CS0618 // Type or member is obsolete - IsPropertyValid() will be retained as internal after the obsoletion period.
                return !IsPropertyValid(x, new PropertyValidationContext
                {
                    Culture = null,
                    Segment = null,
                    CulturesBeingValidated = [impact.Culture!],
                    SegmentsBeingValidated = []
                });
#pragma warning restore CS0618 // Type or member is obsolete
            }

            // and, for explicit culture, we may also have to validate invariant property, invariant culture
            // if either
            // - it is impacted (default culture), or
            // - there is no published version of the content - maybe non-default culture, but no published version
            var alsoInvariant = impact.ImpactsAlsoInvariantProperties || !content.Published;
#pragma warning disable CS0618 // Type or member is obsolete - IsPropertyValid() will be retained as internal after the obsoletion period.
            return alsoInvariant && !IsPropertyValid(x, PropertyValidationContext.Empty());
#pragma warning restore CS0618 // Type or member is obsolete
        }).ToArray();

        return invalidProperties.Length == 0;
    }

    /// <inheritdoc />
    // TODO (V20): Make this internal rather than removing it entirely in V20, so the
    // referencing unit tests can continue to exercise this code.
    // Also remove the obsolete code warning suppressions added around the various
    // callers to this method (including the unit tests).
    [Obsolete("Property level validation is not going to be supported moving forward. Please use content level validation with IsPropertyDataValid instead. Scheduled for removal in Umbraco 20.")]
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

        // if the property varies by segment, make explicitly sure we validate mandatory against
        // the non-segmented value (or null, if not present)
        if (property.PropertyType.VariesBySegment())
        {
            IPropertyValue? defaultSegmentValue = property
                .Values
                .FirstOrDefault(x => (culture == "*" || x.Culture.InvariantEquals(culture)) && x.Segment == null);
            if (!IsValidPropertyValue(
                    property,
                    defaultSegmentValue?.EditedValue,
                    new PropertyValidationContext
                    {
                        Culture = culture,
                        Segment = null,
                        CulturesBeingValidated = validationContext.CulturesBeingValidated,
                        SegmentsBeingValidated = validationContext.SegmentsBeingValidated,
                    }))
            {
                return false;
            }
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

        var configuration = propertyType.GetDataType(_dataTypeService, _idKeyMap)?.ConfigurationObject;
        IDataValueEditor valueEditor = editor.GetValueEditor(configuration);

        var isRequired = ShouldValidateAsRequired(propertyType, validationContext);
        return !valueEditor.Validate(value, isRequired, propertyType.ValidationRegExp, validationContext).Any();
    }

    /// <summary>
    ///     Determines whether mandatory validation applies within the given validation context.
    /// </summary>
    /// <remarks>
    ///     Values for non-default segments are optional overrides of the default segment value, so a mandatory
    ///     property type is only enforced as required when validating the default (null) segment.
    /// </remarks>
    private static bool ShouldValidateAsRequired(IPropertyType propertyType, PropertyValidationContext validationContext)
        => propertyType.Mandatory && validationContext.Segment.IsNullOrWhiteSpace();

    private IDataEditor? GetDataEditor(IPropertyType propertyType)
        => _propertyEditors[propertyType.PropertyEditorAlias];
}
