using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Core.Deploy
{
    public class GridValue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sections")]
        public IEnumerable<Section> Sections { get; set; }

        public class Section
        {
            [JsonProperty("grid")]
            public string Grid { get; set; }

            [JsonProperty("rows")]
            public IEnumerable<Row> Rows { get; set; }
        }

        public class Row
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("areas")]
            public IEnumerable<Area> Areas { get; set; }

            [JsonProperty("styles")]
            public JToken Styles { get; set; }

            [JsonProperty("config")]
            public JToken Config { get; set; }
        }

        public class Area
        {
            [JsonProperty("grid")]
            public string Grid { get; set; }

            [JsonProperty("controls")]
            public IEnumerable<Control> Controls { get; set; }

            [JsonProperty("styles")]
            public JToken Styles { get; set; }

            [JsonProperty("config")]
            public JToken Config { get; set; }
        }

        public class Control
        {
            [JsonProperty("value")]
            public JToken Value { get; set; }

            [JsonProperty("editor")]
            public Editor Editor { get; set; }

            [JsonProperty("styles")]
            public JToken Styles { get; set; }

            [JsonProperty("config")]
            public JToken Config { get; set; }
        }

        public class Editor
        {
            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("view")]
            public string View { get; set; }
        }
    }
}