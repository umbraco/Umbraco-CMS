using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the ValueList editor configuration.
    /// </summary>
    class ValueListConfiguration
    {
        [JsonProperty("items")]
        public List<ValueListItem> Items { get; set; } = new List<ValueListItem>();

        public class ValueListItem
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }
        }
    }
}