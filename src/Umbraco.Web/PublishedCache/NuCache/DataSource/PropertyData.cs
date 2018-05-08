﻿using System;
using Newtonsoft.Json;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class PropertyData
    {
        private string _culture;
        private string _segment;

        [JsonProperty("culture")]
        public string Culture
        {
            get => _culture;
            set => _culture = value ?? throw new ArgumentNullException(nameof(value));
        }

        [JsonProperty("seg")]
        public string Segment
        {
            get => _segment;
            set => _segment = value ?? throw new ArgumentNullException(nameof(value));
        }

        [JsonProperty("val")]
        public object Value { get; set; }
    }
}
