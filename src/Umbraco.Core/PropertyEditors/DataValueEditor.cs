using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a value editor.
/// </summary>
[DataContract]
public class DataValueEditor : IDataValueEditor
{
    private readonly IJsonSerializer? _jsonSerializer;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataValueEditor" /> class.
    /// </summary>
    public DataValueEditor(
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IJsonSerializer? jsonSerializer) // for tests, and manifest
    {
        _localizedTextService = localizedTextService;
        _shortStringHelper = shortStringHelper;
        _jsonSerializer = jsonSerializer;
        ValueType = ValueTypes.String;
        Validators = new List<IValueValidator>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataValueEditor" /> class.
    /// </summary>
    public DataValueEditor(
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        DataEditorAttribute attribute)
    {
        if (attribute == null)
        {
            throw new ArgumentNullException(nameof(attribute));
        }

        _localizedTextService = localizedTextService;
        _shortStringHelper = shortStringHelper;
        _jsonSerializer = jsonSerializer;

        var view = attribute.View;
        if (string.IsNullOrWhiteSpace(view))
        {
            throw new ArgumentException("The attribute does not specify a view.", nameof(attribute));
        }

        if (view.StartsWith("~/"))
        {
            view = ioHelper.ResolveRelativeOrVirtualUrl(view);
        }

        View = view;
        ValueType = attribute.ValueType;
        HideLabel = attribute.HideLabel;
    }

    /// <summary>
    ///     Gets or sets the value editor configuration.
    /// </summary>
    public virtual object? Configuration { get; set; }

    public bool SupportsReadOnly { get; set; }

    /// <summary>
    ///     Gets the validator used to validate the special property type -level "required".
    /// </summary>
    public virtual IValueRequiredValidator RequiredValidator => new RequiredValidator(_localizedTextService);

    /// <summary>
    ///     Gets the validator used to validate the special property type -level "format".
    /// </summary>
    public virtual IValueFormatValidator FormatValidator => new RegexValidator(_localizedTextService);

    /// <summary>
    ///     Gets or sets the editor view.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The view can be three things: (1) the full virtual path, or (2) the relative path to the current Umbraco
    ///         folder, or (3) a view name which maps to views/propertyeditors/{view}/{view}.html.
    ///     </para>
    /// </remarks>
    [Required]
    [DataMember(Name = "view")]
    public string? View { get; set; }

    /// <summary>
    ///     The value type which reflects how it is validated and stored in the database
    /// </summary>
    [DataMember(Name = "valueType")]
    public string ValueType { get; set; }

    /// <summary>
    ///     A collection of validators for the pre value editor
    /// </summary>
    [DataMember(Name = "validation")]
    public List<IValueValidator> Validators { get; private set; } = new();

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(object? value, bool required, string? format)
    {
        List<ValidationResult>? results = null;
        var r = Validators.SelectMany(v => v.Validate(value, ValueType, Configuration)).ToList();
        if (r.Any())
        {
            results = r;
        }

        // mandatory and regex validators cannot be part of valueEditor.Validators because they
        // depend on values that are not part of the configuration, .Mandatory and .ValidationRegEx,
        // so they have to be explicitly invoked here.
        if (required)
        {
            r = RequiredValidator.ValidateRequired(value, ValueType).ToList();
            if (r.Any())
            {
                if (results == null)
                {
                    results = r;
                }
                else
                {
                    results.AddRange(r);
                }
            }
        }

        var stringValue = value?.ToString();
        if (!string.IsNullOrWhiteSpace(format) && !string.IsNullOrWhiteSpace(stringValue))
        {
            r = FormatValidator.ValidateFormat(value, ValueType, format).ToList();
            if (r.Any())
            {
                if (results == null)
                {
                    results = r;
                }
                else
                {
                    results.AddRange(r);
                }
            }
        }

        return results ?? Enumerable.Empty<ValidationResult>();
    }

    /// <summary>
    ///     If this is true than the editor will be displayed full width without a label
    /// </summary>
    [DataMember(Name = "hideLabel")]
    public bool HideLabel { get; set; }

    /// <summary>
    ///     Set this to true if the property editor is for display purposes only
    /// </summary>
    public virtual bool IsReadOnly => false;

    /// <summary>
    ///     A method to deserialize the string value that has been saved in the content editor to an object to be stored in the
    ///     database.
    /// </summary>
    /// <param name="editorValue">The value returned by the editor.</param>
    /// <param name="currentValue">
    ///     The current value that has been persisted to the database for this editor. This value may be
    ///     useful for how the value then get's deserialized again to be re-persisted. In most cases it will probably not be
    ///     used.
    /// </param>
    /// <returns>The value that gets persisted to the database.</returns>
    /// <remarks>
    ///     By default this will attempt to automatically convert the string value to the value type supplied by ValueType.
    ///     If overridden then the object returned must match the type supplied in the ValueType, otherwise persisting the
    ///     value to the DB will fail when it tries to validate the value type.
    /// </remarks>
    public virtual object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {
        Attempt<object?> result = TryConvertValueToCrlType(editorValue.Value);
        if (result.Success == false)
        {
            StaticApplicationLogging.Logger.LogWarning(
                "The value {EditorValue} cannot be converted to the type {StorageTypeValue}", editorValue.Value, ValueTypes.ToStorageType(ValueType));
            return null;
        }

        return result.Result;
    }

    /// <summary>
    ///     A method used to format the database value to a value that can be used by the editor.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="segment">The segment.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentOutOfRangeException">ValueType was out of range.</exception>
    /// <remarks>
    ///     The object returned will automatically be serialized into JSON notation. For most property editors
    ///     the value returned is probably just a string, but in some cases a JSON structure will be returned.
    /// </remarks>
    public virtual object? ToEditor(IProperty property, string? culture = null, string? segment = null)
    {
        var value = property.GetValue(culture, segment);
        if (value == null)
        {
            return string.Empty;
        }

        switch (ValueTypes.ToStorageType(ValueType))
        {
            case ValueStorageType.Ntext:
            case ValueStorageType.Nvarchar:
                // If it is a string type, we will attempt to see if it is JSON stored data, if it is we'll try to convert
                // to a real JSON object so we can pass the true JSON object directly to Angular!
                var stringValue = value as string ?? value.ToString();
                if (stringValue!.DetectIsJson())
                {
                    try
                    {
                        dynamic? json = _jsonSerializer?.Deserialize<dynamic>(stringValue!);
                        return json;
                    }
                    catch
                    {
                        // Swallow this exception, we thought it was JSON but it really isn't so continue returning a string
                    }
                }

                return stringValue;

            case ValueStorageType.Integer:
            case ValueStorageType.Decimal:
                // Decimals need to be formatted with invariant culture (dots, not commas)
                // Anything else falls back to ToString()
                Attempt<decimal> decimalValue = value.TryConvertTo<decimal>();

                return decimalValue.Success
                    ? decimalValue.Result.ToString(NumberFormatInfo.InvariantInfo)
                    : value.ToString();

            case ValueStorageType.Date:
                Attempt<DateTime?> dateValue = value.TryConvertTo<DateTime?>();
                if (dateValue.Success == false || dateValue.Result == null)
                {
                    return string.Empty;
                }

                // Dates will be formatted as yyyy-MM-dd HH:mm:ss
                return dateValue.Result.Value.ToIsoString();

            default:
                throw new ArgumentOutOfRangeException("ValueType was out of range.");
        }
    }

    // TODO: the methods below should be replaced by proper property value convert ToXPath usage!

    /// <summary>
    ///     Converts a property to Xml fragments.
    /// </summary>
    public IEnumerable<XElement> ConvertDbToXml(IProperty property, bool published)
    {
        published &= property.PropertyType.SupportsPublishing;

        var nodeName = property.PropertyType.Alias.ToSafeAlias(_shortStringHelper);

        foreach (IPropertyValue pvalue in property.Values)
        {
            var value = published ? pvalue.PublishedValue : pvalue.EditedValue;
            if (value == null || (value is string stringValue && string.IsNullOrWhiteSpace(stringValue)))
            {
                continue;
            }

            var xElement = new XElement(nodeName);
            if (pvalue.Culture != null)
            {
                xElement.Add(new XAttribute("lang", pvalue.Culture));
            }

            if (pvalue.Segment != null)
            {
                xElement.Add(new XAttribute("segment", pvalue.Segment));
            }

            XNode xValue = ConvertDbToXml(property.PropertyType, value);
            xElement.Add(xValue);

            yield return xElement;
        }
    }

    /// <summary>
    ///     Converts a property value to an Xml fragment.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, this returns the value of ConvertDbToString but ensures that if the db value type is
    ///         NVarchar or NText, the value is returned as a CDATA fragment - else it's a Text fragment.
    ///     </para>
    ///     <para>Returns an XText or XCData instance which must be wrapped in a element.</para>
    ///     <para>If the value is empty we will not return as CDATA since that will just take up more space in the file.</para>
    /// </remarks>
    public XNode ConvertDbToXml(IPropertyType propertyType, object? value)
    {
        // check for null or empty value, we don't want to return CDATA if that is the case
        if (value == null || value.ToString().IsNullOrWhiteSpace())
        {
            return new XText(ConvertDbToString(propertyType, value));
        }

        switch (ValueTypes.ToStorageType(ValueType))
        {
            case ValueStorageType.Date:
            case ValueStorageType.Integer:
            case ValueStorageType.Decimal:
                return new XText(ConvertDbToString(propertyType, value));
            case ValueStorageType.Nvarchar:
            case ValueStorageType.Ntext:
                // put text in cdata
                return new XCData(ConvertDbToString(propertyType, value));
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    ///     Converts a property value to a string.
    /// </summary>
    public virtual string ConvertDbToString(IPropertyType propertyType, object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        switch (ValueTypes.ToStorageType(ValueType))
        {
            case ValueStorageType.Nvarchar:
            case ValueStorageType.Ntext:
                return value.ToXmlString<string>();
            case ValueStorageType.Integer:
            case ValueStorageType.Decimal:
                return value.ToXmlString(value.GetType());
            case ValueStorageType.Date:
                // treat dates differently, output the format as xml format
                Attempt<DateTime?> date = value.TryConvertTo<DateTime?>();
                if (date.Success == false || date.Result == null)
                {
                    return string.Empty;
                }

                return date.Result.ToXmlString<DateTime>();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    ///     Used to try to convert the string value to the correct CLR type based on the <see cref="ValueType" /> specified for
    ///     this value editor.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    ///     The result of the conversion attempt.
    /// </returns>
    /// <exception cref="System.ArgumentOutOfRangeException">ValueType was out of range.</exception>
    internal Attempt<object?> TryConvertValueToCrlType(object? value)
    {
        // Ensure empty string and JSON values are converted to null
        if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
        {
            value = null;
        }
        else if (value is not string && ValueType.InvariantEquals(ValueTypes.Json))
        {
            // Only serialize value when it's not already a string
            var jsonValue = _jsonSerializer?.Serialize(value);
            if (jsonValue?.DetectIsEmptyJson() ?? false)
            {
                value = null;
            }
            else
            {
                value = jsonValue;
            }
        }

        // Convert the string to a known type
        Type valueType;
        switch (ValueTypes.ToStorageType(ValueType))
        {
            case ValueStorageType.Ntext:
            case ValueStorageType.Nvarchar:
                valueType = typeof(string);
                break;

            case ValueStorageType.Integer:
                // Ensure these are nullable so we can return a null if required
                // NOTE: This is allowing type of 'long' because I think JSON.NEt will deserialize a numerical value as long instead of int
                // Even though our DB will not support this (will get truncated), we'll at least parse to this
                valueType = typeof(long?);

                // If parsing is successful, we need to return as an int, we're only dealing with long's here because of JSON.NET,
                // we actually don't support long values and if we return a long value, it will get set as a 'long' on the Property.Value (object) and then
                // when we compare the values for dirty tracking we'll be comparing an int -> long and they will not match.
                Attempt<object?> result = value.TryConvertTo(valueType);

                return result.Success && result.Result != null
                    ? Attempt<object?>.Succeed((int)(long)result.Result)
                    : result;

            case ValueStorageType.Decimal:
                // Ensure these are nullable so we can return a null if required
                valueType = typeof(decimal?);
                break;

            case ValueStorageType.Date:
                // Ensure these are nullable so we can return a null if required
                valueType = typeof(DateTime?);
                break;

            default:
                throw new ArgumentOutOfRangeException("ValueType was out of range.");
        }

        return value.TryConvertTo(valueType);
    }
}
