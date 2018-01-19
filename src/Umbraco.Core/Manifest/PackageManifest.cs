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
        [JsonProperty("javascript")]
        public string[] Scripts { get; set; } = Array.Empty<string>();

        [JsonProperty("css")]
        public string[] Stylesheets { get; set; }= Array.Empty<string>();

        [JsonProperty("propertyEditors")]
        public PropertyEditor[] PropertyEditors { get; set; } = Array.Empty<PropertyEditor>();

        [JsonProperty("parameterEditors")]
        public ParameterEditor[] ParameterEditors { get; set; } = Array.Empty<ParameterEditor>();

        [JsonProperty("gridEditors")]
        public GridEditor[] GridEditors { get; set; } = Array.Empty<GridEditor>();
    }
}
