using Newtonsoft.Json;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// Represents the culture variation information on a content item
    /// </summary>
    internal class CultureVariation
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        //TODO: We may want some date stamps here
    }
}
