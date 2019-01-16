using Newtonsoft.Json;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the 'startNode' value for the <see cref="MultiNodePickerConfiguration"/>
    /// </summary>
    [JsonObject]
    public class MultiNodePickerConfigurationTreeSource
    {
        [JsonProperty("type")]
        public string ObjectType {get;set;}

        [JsonProperty("query")]
        public string StartNodeQuery {get;set;}

        [JsonProperty("id")]
        public string StartNodeId {get;set;}
    }
}
