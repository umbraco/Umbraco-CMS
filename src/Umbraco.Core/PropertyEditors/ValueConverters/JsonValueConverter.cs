using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The default converter for all property editors that expose a JSON value type.
    /// </summary>
    /// <seealso cref="Umbraco.Core.PropertyEditors.PropertyValueConverterBase" />
    /// <remarks>
    /// Since this is a default converter, it will be ignored if another converter is found (other default converters with a JSON value type will need to shadow this one).
    /// </remarks>
    [DefaultPropertyValueConverter]
    public class JsonValueConverter : PropertyValueConverterBase
    {
        private readonly PropertyEditorCollection _propertyEditors;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonValueConverter" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public JsonValueConverter(PropertyEditorCollection propertyEditors)
        {
            _propertyEditors = propertyEditors;
        }

        /// <inheritdoc />
        public override bool IsConverter(IPublishedPropertyType propertyType) =>
            _propertyEditors.TryGet(propertyType.EditorAlias, out var editor) &&
            editor.GetValueEditor().ValueType.InvariantEquals(ValueTypes.Json);

        /// <inheritdoc />
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(JToken);

        /// <inheritdoc />
        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        /// <inheritdoc />
        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            if (sourceString.DetectIsJson())
            {
                try
                {
                    return JsonConvert.DeserializeObject(sourceString);
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<JsonValueConverter, string>(ex, "Could not parse the string '{JsonString}' to a JSON object", sourceString);
                }
            }

            // It's not JSON, so just return the string
            return sourceString;
        }
    }
}
