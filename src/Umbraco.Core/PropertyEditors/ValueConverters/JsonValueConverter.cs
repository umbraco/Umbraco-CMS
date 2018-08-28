using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The default converter for all property editors that expose a JSON value type
    /// </summary>
    /// <remarks>
    /// Since this is a default (umbraco) converter it will be ignored if another converter found conflicts with this one.
    /// </remarks>
    [DefaultPropertyValueConverter]
    public class JsonValueConverter : PropertyValueConverterBase
    {
        private readonly PropertyEditorCollection _propertyEditors;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonValueConverter"/> class.
        /// </summary>
        public JsonValueConverter(PropertyEditorCollection propertyEditors)
        {
            _propertyEditors = propertyEditors;
        }

        /// <summary>
        /// It is a converter for any value type that is "JSON"
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return _propertyEditors.TryGet(propertyType.EditorAlias, out var editor)
                   && editor.GetValueEditor().ValueType.InvariantEquals(ValueTypes.Json);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (JToken);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            if (sourceString.DetectIsJson())
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject(sourceString);
                    return obj;
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<JsonValueConverter>(ex, "Could not parse the string '{JsonString}' to a json object", sourceString);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }

        //TODO: Now to convert that to XPath!
    }
}
