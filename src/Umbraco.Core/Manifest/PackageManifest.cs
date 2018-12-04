using System;
using Newtonsoft.Json;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Represents the content of a package manifest.
    /// </summary>
    public class PackageManifest
    {
        [JsonProperty("javascript")]
        public string[] Scripts { get; set; } = Array.Empty<string>();

        [JsonProperty("css")]
        public string[] Stylesheets { get; set; }= Array.Empty<string>();

        [JsonProperty("propertyEditors")]
        public IDataEditor[] PropertyEditors { get; set; } = Array.Empty<IDataEditor>();

        [JsonProperty("parameterEditors")]
        public IDataEditor[] ParameterEditors { get; set; } = Array.Empty<IDataEditor>();

        [JsonProperty("gridEditors")]
        public GridEditor[] GridEditors { get; set; } = Array.Empty<GridEditor>();

        [JsonProperty("contentApps")]
        public IContentAppDefinition[] ContentApps { get; set; } = Array.Empty<IContentAppDefinition>();

        [JsonProperty("dashboards")]
        public ManifestDashboardDefinition[] Dashboards { get; set; } = Array.Empty<ManifestDashboardDefinition>();
    }
}
