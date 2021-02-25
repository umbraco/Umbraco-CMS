using Newtonsoft.Json;
using System.Collections.Generic;
using Umbraco.Core.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// The content item 1:M data that is serialized to JSON
    /// </summary>
    internal class ContentNestedData
    {
        //dont serialize empty properties
        [JsonProperty("pd")]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<PropertyData[]>))]
        public Dictionary<string, PropertyData[]> PropertyData { get; set; }

        [JsonProperty("cd")]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<CultureVariation>))]
        public Dictionary<string, CultureVariation> CultureData { get; set; }

        [JsonProperty("us")]
        public string UrlSegment { get; set; }

        //Legacy properties used to deserialize existing nucache db entries
        [JsonProperty("properties")]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<PropertyData[]>))]
        private Dictionary<string, PropertyData[]> LegacyPropertyData { set { PropertyData = value; } }

        [JsonProperty("cultureData")]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<CultureVariation>))]
        private Dictionary<string, CultureVariation> LegacyCultureData { set { CultureData = value; } }

        [JsonProperty("urlSegment")]
        private string LegacyUrlSegment { set { UrlSegment = value; } }
    }
}
