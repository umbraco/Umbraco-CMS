using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the ValueList editor configuration.
    /// </summary>
    public class ValueListConfiguration
    {
        [ConfigurationField("items", "Configure", "multivalues", Description = "Add, remove or sort values for the list.")]
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
