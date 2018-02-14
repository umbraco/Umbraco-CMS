using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents an editor for editing values.
    /// </summary>
    public interface IValueEditor
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
    }

    // fixme
    public interface IPropertyValueEditor : IValueEditor
    {
        // fixme services should be injected!
        object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue);
        object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService);
        IEnumerable<XElement> ConvertDbToXml(Property property, IDataTypeService dataTypeService, ILocalizationService localizationService, bool published);
        XNode ConvertDbToXml(PropertyType propertyType, object value, IDataTypeService dataTypeService);
        string ConvertDbToString(PropertyType propertyType, object value, IDataTypeService dataTypeService);

        List<IValueValidator> Validators { get; }

        bool IsReadOnly { get; }
        bool HideLabel { get; }

        // fixme what are these?
        ManifestValidator RequiredValidator { get; }
        ManifestValidator RegexValidator { get; }
    }
}
