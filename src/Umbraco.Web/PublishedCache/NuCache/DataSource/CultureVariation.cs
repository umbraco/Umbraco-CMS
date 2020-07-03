using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// Represents the culture variation information on a content item
    /// </summary>
    [DataContract] // NOTE: Use DataContract annotations here to control how MessagePack serializes/deserializes the data to use INT keys
    public class CultureVariation
    {
        [DataMember(Order = 0)]
        [JsonProperty("nm")]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        [JsonProperty("us")]
        public string UrlSegment { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty("dt")]
        public DateTime Date { get; set; }

        [DataMember(Order = 3)]
        [JsonProperty("isd")]
        public bool IsDraft { get; set; }

        //Legacy properties used to deserialize existing nucache db entries
        [DataMember(Order = 4)]
        [JsonProperty("name")]
        private string LegacyName { set { Name = value; } }

        [DataMember(Order = 5)]
        [JsonProperty("urlSegment")]
        private string LegacyUrlSegment { set { UrlSegment = value; } }

        [DataMember(Order = 6)]
        [JsonProperty("date")]
        private DateTime LegacyDate { set { Date = value; } }

        [DataMember(Order = 7)]
        [JsonProperty("isDraft")]
        private bool LegacyIsDraft { set { IsDraft = value; } }
    }
}
