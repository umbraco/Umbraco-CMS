using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

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
        public IList<GridSection> Sections { get; set; }

        public class GridSection
        {
            [JsonProperty("grid")]
            public int Grid { get; set; }

            [JsonProperty("allowAll")]
            public bool AllowAll { get; set; }

            [JsonProperty("rows")]
            public IEnumerable<GridRow> Rows { get; set; }
        }

        public class GridRow : GridAttributesBase
        {
            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("areas")]
            public IEnumerable<GridArea> Areas { get; set; }

            [JsonProperty("hasConfig")]
            public bool HasConfig { get; set; }

            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("active")]
            public bool Active { get; set; }
        }

        public class GridArea : GridAttributesBase
        {
            [JsonProperty("grid")]
            public string Grid { get; set; }

            [JsonProperty("allowAll")]
            public bool AllowAll { get; set; }

            [JsonProperty("allowed")]
            public List<string> Allowed { get; set; }

            [JsonProperty("hasConfig")]
            public bool HasConfig { get; set; }

            [JsonProperty("controls")]
            public IEnumerable<GridControl> Controls { get; set; }

            [JsonProperty("active")]
            public bool Active { get; set; }
        }

        public class GridControl : GridAttributesBase
        {
            [JsonProperty("value")]
            public JToken Value { get; set; }

            [JsonProperty("editor")]
            public GridEditor Editor { get; set; }
        }

        public class GridEditor
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("view")]
            public string View { get; set; }

            [JsonProperty("render")]
            public string Render { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; }

            [JsonProperty("config")]
            public GridConfig Config { get; set; }
        }

        public class GridConfig
        {
            [JsonProperty("style")]
            public string Style { get; set; }

            [JsonProperty("markup")]
            public string Markup { get; set; }

            [JsonProperty("allowedDocTypes")]
            public List<string> AllowedDocTypes { get; set; }

            [JsonProperty("nameTemplate")]
            public string NameTemplate { get; set; }

            [JsonProperty("enablePreview")]
            public bool? EnablePreview { get; set; }

            [JsonProperty("viewPath")]
            public string ViewPath { get; set; }

            [JsonProperty("previewViewPath")]
            public string PreviewViewPath { get; set; }

            [JsonProperty("previewCssFilePath")]
            public string PreviewCssFilePath { get; set; }

            [JsonProperty("previewJsFilePath")]
            public string PreviewJsFilePath { get; set; }
        }
    }
}