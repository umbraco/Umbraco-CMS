using Newtonsoft.Json;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// The content item 1:M data that is serialized to JSON
    /// </summary>
    internal class ContentSerializedData
    {
        [JsonProperty("properties")]
        public Dictionary<string, PropertyData[]> PropertyData { get; set; }

        [JsonProperty("cultureData")]
        public Dictionary<string, CultureVariation> CultureData { get; set; }
    }
}
