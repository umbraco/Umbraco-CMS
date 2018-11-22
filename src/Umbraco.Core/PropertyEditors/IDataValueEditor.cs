using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents an editor for editing data values.
    /// </summary>
    /// <remarks>This is the base interface for parameter and property value editors.</remarks>
    public interface IDataValueEditor
    {
        /// <summary>
        /// Gets the editor view.
        /// </summary>
        string View { get; }

        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        /// <remarks>The value has to be a valid <see cref="ValueTypes"/> value.</remarks>
        string ValueType { get; set; }

        /// <summary>
        /// Gets a value indicating whether the edited value is read-only.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating whether to display the associated label.
        /// </summary>
        bool HideLabel { get; }

        /// <summary>
        /// Validates a property value.
        /// </summary>
        /// <param name="value">The property value.</param>
        /// <param name="required">A value indicating whether the property value is required.</param>
        /// <param name="format">A specific format (regex) that the property value must respect.</param>
        IEnumerable<ValidationResult> Validate(object value, bool required, string format);

        /// <summary>
        /// Gets the validators to use to validate the edited value.
        /// </summary>
        /// <remarks>
        /// <para>Use this property to add validators, not to validate. Use <see cref="Validate"/> instead.</para>
        /// fixme replace with AddValidator? WithValidator?
        /// </remarks>
        List<IValueValidator> Validators { get; }

        /// <summary>
        /// Converts a value posted by the editor to a property value.
        /// </summary>
        object FromEditor(ContentPropertyData editorValue, object currentValue);

        // fixme - editing - services should be injected

        /// <summary>
        /// Converts a property value to a value for the editor.
        /// </summary>
        object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null);

        // fixme - editing - document or remove these
        // why property vs propertyType? services should be injected! etc...
        IEnumerable<XElement> ConvertDbToXml(Property property, IDataTypeService dataTypeService, ILocalizationService localizationService, bool published);
        XNode ConvertDbToXml(PropertyType propertyType, object value, IDataTypeService dataTypeService);
        string ConvertDbToString(PropertyType propertyType, object value, IDataTypeService dataTypeService);
    }
}
