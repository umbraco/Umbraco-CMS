using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Core.Models;

// TODO: Make a property value converter for this!

/// <summary>
///     A model representing the value saved for the grid
/// </summary>
public class GridValue
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("sections")]
    public IEnumerable<GridSection> Sections { get; set; } = null!;

    public class GridSection
    {
        [JsonProperty("grid")]
        public string? Grid { get; set; } // TODO: what is this?

        [JsonProperty("rows")]
        public IEnumerable<GridRow> Rows { get; set; } = null!;
    }

    public class GridRow
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("areas")]
        public IEnumerable<GridArea> Areas { get; set; } = null!;

        [JsonProperty("styles")]
        public JToken? Styles { get; set; }

        [JsonProperty("config")]
        public JToken? Config { get; set; }
    }

    public class GridArea
    {
        [JsonProperty("grid")]
        public string? Grid { get; set; } // TODO: what is this?

        [JsonProperty("controls")]
        public IEnumerable<GridControl> Controls { get; set; } = null!;

        [JsonProperty("styles")]
        public JToken? Styles { get; set; }

        [JsonProperty("config")]
        public JToken? Config { get; set; }
    }

    public class GridControl
    {
        [JsonProperty("value")]
        public JToken? Value { get; set; }

        [JsonProperty("editor")]
        public GridEditor Editor { get; set; } = null!;

        [JsonProperty("styles")]
        public JToken? Styles { get; set; }

        [JsonProperty("config")]
        public JToken? Config { get; set; }
    }

    public class GridEditor
    {
        [JsonProperty("alias")]
        public string Alias { get; set; } = null!;

        [JsonProperty("view")]
        public string? View { get; set; }
    }
}
