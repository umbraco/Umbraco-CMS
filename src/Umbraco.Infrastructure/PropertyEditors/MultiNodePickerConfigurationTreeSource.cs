using Newtonsoft.Json;
using Umbraco.Core;

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
        public Udi StartNodeId {get;set;}
    }
}
