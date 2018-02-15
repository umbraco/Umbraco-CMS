using System.Collections.Generic;
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
        /// Gets the validators to use to validate the edited value.
        /// </summary>
        List<IValueValidator> Validators { get; }

        // fixme what are these?
        ManifestValidator RequiredValidator { get; }
        ManifestValidator RegexValidator { get; }

        // fixme services should be injected!
        // fixme document
        object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue);
        object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService);
        IEnumerable<XElement> ConvertDbToXml(Property property, IDataTypeService dataTypeService, ILocalizationService localizationService, bool published);
        XNode ConvertDbToXml(PropertyType propertyType, object value, IDataTypeService dataTypeService);
        string ConvertDbToString(PropertyType propertyType, object value, IDataTypeService dataTypeService);
    }
}
