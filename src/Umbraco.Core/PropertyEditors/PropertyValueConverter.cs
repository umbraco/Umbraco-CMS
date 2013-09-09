using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Used to convert property values from various sources to various destination formats for use with caching
    /// </summary>
    public abstract class PropertyValueConverter
    {
        /// <summary>
        /// Returns the alias of the PropertyEditor that this converter is for
        /// </summary>
        public abstract string AssociatedPropertyEditorAlias { get; }

        public abstract Attempt<object> ConvertSourceToObject(object valueToConvert, PublishedPropertyDefinition propertyDefinition, bool isPreviewing);
    }
}