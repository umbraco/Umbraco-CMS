using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// A model representing the value saved for the grid
    /// </summary>
    public class GridValue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sections")]
        public IEnumerable<GridSection> Sections { get; set; }

        public class GridSection
        {
            [JsonProperty("grid")]
            public string Grid { get; set; }

            [JsonProperty("rows")]
            public IEnumerable<GridRow> Rows { get; set; }
        }

        public class GridRow
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("areas")]
            public IEnumerable<GridArea> Areas { get; set; }

            [JsonProperty("styles")]
            public JToken Styles { get; set; }

            [JsonProperty("config")]
            public JToken Config { get; set; }
        }

        public class GridArea
        {
            [JsonProperty("grid")]
            public string Grid { get; set; }

            [JsonProperty("controls")]
            public IEnumerable<GridControl> Controls { get; set; }

            [JsonProperty("styles")]
            public JToken Styles { get; set; }

            [JsonProperty("config")]
            public JToken Config { get; set; }
        }

        public class GridControl
        {
            [JsonProperty("value")]
            public JToken Value { get; set; }

            [JsonProperty("editor")]
            public GridEditor Editor { get; set; }

            [JsonProperty("styles")]
            public JToken Styles { get; set; }

            [JsonProperty("config")]
            public JToken Config { get; set; }
        }

        public class GridEditor
        {
            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("view")]
            public string View { get; set; }
        }
    }
}