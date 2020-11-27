using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// Represents a single block's data in raw form
    /// </summary>
    public class BlockItemData
    {
        [JsonProperty("contentTypeKey")]
        public Guid ContentTypeKey { get; set; }

        /// <summary>
        /// not serialized, manually set and used during internally
        /// </summary>
        [JsonIgnore]
        public string ContentTypeAlias { get; set; }

        [JsonProperty("udi")]
        [JsonConverter(typeof(UdiJsonConverter))]
        public Udi Udi { get; set; }

        [JsonIgnore]
        public Guid Key => Udi != null ? ((GuidUdi)Udi).Guid : throw new InvalidOperationException("No Udi assigned");

        /// <summary>
        /// The remaining properties will be serialized to a dictionary
        /// </summary>
        /// <remarks>
        /// The JsonExtensionDataAttribute is used to put the non-typed properties into a bucket
        /// http://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm
        /// NestedContent serializes to string, int, whatever eg
        ///   "stringValue":"Some String","numericValue":125,"otherNumeric":null
        /// </remarks>
        [JsonExtensionData]
        public Dictionary<string, object> RawPropertyValues { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Used during deserialization to convert the raw property data into data with a property type context
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, BlockPropertyValue> PropertyValues { get; set; } = new Dictionary<string, BlockPropertyValue>();

        /// <summary>
        /// Used during deserialization to populate the property value/property type of a block item content property
        /// </summary>
        public class BlockPropertyValue
        {
            public BlockPropertyValue(object value, PropertyType propertyType)
            {
                Value = value;
                PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            }

            public object Value { get; }
            public PropertyType PropertyType { get; }
        }
    }
}
