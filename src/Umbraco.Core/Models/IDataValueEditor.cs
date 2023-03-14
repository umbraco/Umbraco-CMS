using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents an editor for editing data values.
/// </summary>
/// <remarks>This is the base interface for parameter and property value editors.</remarks>
public interface IDataValueEditor
{
    /// <summary>
    ///     Gets the editor view.
    /// </summary>
    string? View { get; }

    /// <summary>
    ///     Gets the type of the value.
    /// </summary>
    /// <remarks>The value has to be a valid <see cref="ValueTypes" /> value.</remarks>
    string ValueType { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the edited value is read-only.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    ///     Gets a value indicating whether to display the associated label.
    /// </summary>
    bool HideLabel { get; }

    /// <summary>
    /// Gets a value indicating whether the IDataValueEditor supports readonly mode
    /// </summary>
    bool SupportsReadOnly => false;


    /// <summary>
    ///     Gets the validators to use to validate the edited value.
    /// </summary>
    /// <remarks>
    ///     <para>Use this property to add validators, not to validate. Use <see cref="Validate" /> instead.</para>
    ///     TODO: replace with AddValidator? WithValidator?
    /// </remarks>
    List<IValueValidator> Validators { get; }

    /// <summary>
    ///     Validates a property value.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="required">A value indicating whether the property value is required.</param>
    /// <param name="format">A specific format (regex) that the property value must respect.</param>
    IEnumerable<ValidationResult> Validate(object? value, bool required, string? format);

    /// <summary>
    ///     Converts a value posted by the editor to a property value.
    /// </summary>
    object? FromEditor(ContentPropertyData editorValue, object? currentValue);

    /// <summary>
    ///     Converts a property value to a value for the editor.
    /// </summary>
    object? ToEditor(IProperty property, string? culture = null, string? segment = null);

    // TODO: / deal with this when unplugging the xml cache
    // why property vs propertyType? services should be injected! etc...

    /// <summary>
    ///     Used for serializing an <see cref="IContent" /> item for packaging
    /// </summary>
    /// <param name="property"></param>
    /// <param name="published"></param>
    /// <returns></returns>
    IEnumerable<XElement> ConvertDbToXml(IProperty property, bool published);

    /// <summary>
    ///     Used for serializing an <see cref="IContent" /> item for packaging
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    XNode ConvertDbToXml(IPropertyType propertyType, object value);

    string ConvertDbToString(IPropertyType propertyType, object? value);
}
