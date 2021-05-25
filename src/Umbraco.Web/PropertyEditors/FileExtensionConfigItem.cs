using Newtonsoft.Json;

namespace Umbraco.Web.PropertyEditors
{
    public class FileExtensionConfigItem : IFileExtensionConfigItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
