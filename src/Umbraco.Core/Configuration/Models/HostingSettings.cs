using System.Text.Json.Serialization;

namespace Umbraco.Core.Configuration.Models
{
    public class HostingSettings
    {
        /// <summary>
        /// Gets the configuration for the location of temporary files.
        /// </summary>
        [JsonPropertyName("LocalTempStorage")]
        public LocalTempStorage LocalTempStorageLocation { get; set; } = LocalTempStorage.Default;

        public string ApplicationVirtualPath => null;

        /// <summary>
        ///     Gets a value indicating whether umbraco is running in [debug mode].
        /// </summary>
        /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
        [JsonPropertyName("Debug")]
        public bool DebugMode { get; set; } = false;
    }
}
