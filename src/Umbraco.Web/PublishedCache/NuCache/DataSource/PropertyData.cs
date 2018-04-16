using Newtonsoft.Json;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class PropertyData
    {
        [JsonProperty("lang")]
        public int? LanguageId { get; set; }

        [JsonProperty("seg")]
        public string Segment { get; set; }

        [JsonProperty("val")]
        public object Value { get; set; }
    }
}