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
        [JsonProperty("properties")]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<PropertyData[]>))]
        public Dictionary<string, PropertyData[]> PropertyData { get; set; }

        [JsonProperty("cultureData")]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<CultureVariation>))]
        public Dictionary<string, CultureVariation> CultureData { get; set; }
    }
}
