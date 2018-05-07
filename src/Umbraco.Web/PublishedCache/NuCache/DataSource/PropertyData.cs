using Newtonsoft.Json;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class PropertyData
    {
        [JsonProperty("culture")]
        public string Culture { get; set; }

        [JsonProperty("seg")]
        public string Segment { get; set; }

        [JsonProperty("val")]
        public object Value { get; set; }
    }
}
