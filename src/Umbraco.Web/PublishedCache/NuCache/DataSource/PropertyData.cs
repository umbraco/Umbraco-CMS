using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class PropertyData
    {
        private string _culture;
        private string _segment;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "c")]
        public string Culture
        {
            get => _culture;
            set => _culture = value ?? throw new ArgumentNullException(nameof(value)); // TODO: or fallback to string.Empty? CANNOT be null
        }

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "s")]
        public string Segment
        {
            get => _segment;
            set => _segment = value ?? throw new ArgumentNullException(nameof(value)); // TODO: or fallback to string.Empty? CANNOT be null
        }

        [JsonProperty("v")]
        public object Value { get; set; }


        //Legacy properties used to deserialize existing nucache db entries
        [JsonProperty("culture")]
        private string LegacyCulture
        {
            set => Culture = value;
        }

        [JsonProperty("seg")]
        private string LegacySegment
        {
            set => Segment = value;
        }

        [JsonProperty("val")]
        private object LegacyValue
        {
            set => Value = value;
        }
    }
}
