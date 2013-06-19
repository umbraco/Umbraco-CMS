using Newtonsoft.Json.Linq;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Represents a manifest file for packages
    /// </summary>
    internal class PackageManifest
    {
        /// <summary>
        /// The json array used to initialize the application with the JS dependencies required
        /// </summary>
        public JArray JavaScriptInitialize { get; set; }

        /// <summary>
        /// The json array of property editors
        /// </summary>
        public JArray PropertyEditors { get; set; }
    }
}