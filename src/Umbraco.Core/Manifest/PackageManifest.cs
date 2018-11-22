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
        /// The json array used to initialize the application with the CSS dependencies required
        /// </summary>
        public JArray StylesheetInitialize { get; set; }

        /// <summary>
        /// The json array of property editors
        /// </summary>
        public JArray PropertyEditors { get; set; }

        /// <summary>
        /// The json array of parameter editors
        /// </summary>
        public JArray ParameterEditors { get; set; }

        /// <summary>
        /// The json array of grid editors
        /// </summary>
        public JArray GridEditors { get; set; }
    }
}