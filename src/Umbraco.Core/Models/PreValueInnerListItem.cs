using Newtonsoft.Json;

namespace Umbraco.Core.Models
{
    public class PreValueInnerListItem
    {
        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }
}
