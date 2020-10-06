using System;
using Newtonsoft.Json;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Represents the content of a package manifest.
    /// </summary>
    public class PackageManifest
    {
        /// <summary>
        /// Gets the source path of the manifest.
        /// </summary>
        /// <remarks>
        /// <para>Gets the full absolute file path of the manifest,
        /// using system directory separators.</para>
        /// </remarks>
        [JsonIgnore]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the scripts listed in the manifest.
        /// </summary>
        [JsonProperty("javascript")]
        public string[] Scripts { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the stylesheets listed in the manifest.
        /// </summary>
        [JsonProperty("css")]
        public string[] Stylesheets { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the property editors listed in the manifest.
        /// </summary>
        [JsonProperty("propertyEditors")]
        public IDataEditor[] PropertyEditors { get; set; } = Array.Empty<IDataEditor>();

        /// <summary>
        /// Gets or sets the parameter editors listed in the manifest.
        /// </summary>
        [JsonProperty("parameterEditors")]
        public IDataEditor[] ParameterEditors { get; set; } = Array.Empty<IDataEditor>();

        /// <summary>
        /// Gets or sets the grid editors listed in the manifest.
        /// </summary>
        [JsonProperty("gridEditors")]
        public GridEditor[] GridEditors { get; set; } = Array.Empty<GridEditor>();

        /// <summary>
        /// Gets or sets the content apps listed in the manifest.
        /// </summary>
        [JsonProperty("contentApps")]
        public ManifestContentAppDefinition[] ContentApps { get; set; } = Array.Empty<ManifestContentAppDefinition>();

        [JsonProperty("headerApps")]
        public ManifestHeaderAppDefinition[] HeaderApps { get; set; } = Array.Empty<ManifestHeaderAppDefinition>();

        /// <summary>
        /// Gets or sets the dashboards listed in the manifest.
        /// </summary>
        [JsonProperty("dashboards")]
        public ManifestDashboard[] Dashboards { get; set; } = Array.Empty<ManifestDashboard>();

        /// <summary>
        /// Gets or sets the sections listed in the manifest.
        /// </summary>
        [JsonProperty("sections")]
        public ManifestSection[] Sections { get; set; } = Array.Empty<ManifestSection>();
    }
}
