using System;
using Newtonsoft.Json;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// Represents the culture variation information on a content item
    /// </summary>
    internal class CultureVariation
    {
        [JsonProperty("nm")]
        public string Name { get; set; }

        [JsonProperty("us")]
        public string UrlSegment { get; set; }

        [JsonProperty("dt")]
        public DateTime Date { get; set; }

        [JsonProperty("isd")]
        public bool IsDraft { get; set; }

        //Legacy properties used to deserialize existing nucache db entries
        [JsonProperty("name")]
        private string LegacyName { set { Name = value; } }

        [JsonProperty("urlSegment")]
        private string LegacyUrlSegment { set { UrlSegment = value; } }

        [JsonProperty("date")]
        private DateTime LegacyDate { set { Date = value; } }

        [JsonProperty("isDraft")]
        private bool LegacyIsDraft { set { IsDraft = value; } }
    }
}
