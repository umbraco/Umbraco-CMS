using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    public class SliderPropertyEditorConfiguration
    {
        [JsonProperty("enableRange")]
        public bool EnableRange { get; set; }
    }
}
