﻿using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// The content model stored in the content cache database table serialized as JSON
    /// </summary>
    [DataContract] // NOTE: Use DataContract annotations here to control how MessagePack serializes/deserializes the data to use INT keys
    public class ContentCacheDataModel
    {
        // TODO: We don't want to allocate empty arrays
        //dont serialize empty properties
        [DataMember(Order = 0)]
        [JsonProperty("pd")]
        [JsonConverter(typeof(AutoInterningStringKeyCaseInsensitiveDictionaryConverter<PropertyData[]>))]
        public Dictionary<string, PropertyData[]> PropertyData { get; set; }

        [DataMember(Order = 1)]
        [JsonProperty("cd")]
        [JsonConverter(typeof(AutoInterningStringKeyCaseInsensitiveDictionaryConverter<CultureVariation>))]
        public Dictionary<string, CultureVariation> CultureData { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty("us")]
        public string UrlSegment { get; set; }

        //Legacy properties used to deserialize existing nucache db entries
        [IgnoreDataMember]
        [JsonProperty("properties")]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<PropertyData[]>))]
        private Dictionary<string, PropertyData[]> LegacyPropertyData { set { PropertyData = value; } }

        [IgnoreDataMember]
        [JsonProperty("cultureData")]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<CultureVariation>))]
        private Dictionary<string, CultureVariation> LegacyCultureData { set { CultureData = value; } }

        [IgnoreDataMember]
        [JsonProperty("urlSegment")]
        private string LegacyUrlSegment { set { UrlSegment = value; } }
    }
}
