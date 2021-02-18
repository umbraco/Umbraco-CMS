using System;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Manifest
{
    /// <summary>
    /// Represents the content of a package manifest.
    /// </summary>
    [DataContract]
    public class PackageManifest
    {
        /// <summary>
        /// Gets the source path of the manifest.
        /// </summary>
        /// <remarks>
        /// <para>Gets the full absolute file path of the manifest,
        /// using system directory separators.</para>
        /// </remarks>
        [IgnoreDataMember]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the scripts listed in the manifest.
        /// </summary>
        [DataMember(Name = "javascript")]
        public string[] Scripts { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the stylesheets listed in the manifest.
        /// </summary>
        [DataMember(Name = "css")]
        public string[] Stylesheets { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the property editors listed in the manifest.
        /// </summary>
        [DataMember(Name = "propertyEditors")]
        public IDataEditor[] PropertyEditors { get; set; } = Array.Empty<IDataEditor>();

        /// <summary>
        /// Gets or sets the parameter editors listed in the manifest.
        /// </summary>
        [DataMember(Name = "parameterEditors")]
        public IDataEditor[] ParameterEditors { get; set; } = Array.Empty<IDataEditor>();

        /// <summary>
        /// Gets or sets the grid editors listed in the manifest.
        /// </summary>
        [DataMember(Name = "gridEditors")]
        public GridEditor[] GridEditors { get; set; } = Array.Empty<GridEditor>();

        /// <summary>
        /// Gets or sets the content apps listed in the manifest.
        /// </summary>
        [DataMember(Name = "contentApps")]
        public ManifestContentAppDefinition[] ContentApps { get; set; } = Array.Empty<ManifestContentAppDefinition>();

        /// <summary>
        /// Gets or sets the dashboards listed in the manifest.
        /// </summary>
        [DataMember(Name = "dashboards")]
        public ManifestDashboard[] Dashboards { get; set; } = Array.Empty<ManifestDashboard>();

        /// <summary>
        /// Gets or sets the sections listed in the manifest.
        /// </summary>
        [DataMember(Name = "sections")]
        public ManifestSection[] Sections { get; set; } = Array.Empty<ManifestSection>();
    }
}
