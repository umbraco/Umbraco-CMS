using System.Runtime.Serialization;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Manifest;

public interface IPackageManifest
{
    /// <summary>
    ///     Gets the source path of the manifest.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Gets the full absolute file path of the manifest,
    ///         using system directory separators.
    ///     </para>
    /// </remarks>
    string Source { get; set; }

    /// <summary>
    ///     Gets or sets the scripts listed in the manifest.
    /// </summary>
    [DataMember(Name = "javascript")]
    string[] Scripts { get; set; }

    /// <summary>
    ///     Gets or sets the stylesheets listed in the manifest.
    /// </summary>
    [DataMember(Name = "css")]
    string[] Stylesheets { get; set; }

    /// <summary>
    ///     Gets or sets the property editors listed in the manifest.
    /// </summary>
    [DataMember(Name = "propertyEditors")]
    IDataEditor[] PropertyEditors { get; set; }

    /// <summary>
    ///     Gets or sets the parameter editors listed in the manifest.
    /// </summary>
    [DataMember(Name = "parameterEditors")]
    IDataEditor[] ParameterEditors { get; set; }

    /// <summary>
    ///     Gets or sets the grid editors listed in the manifest.
    /// </summary>
    [DataMember(Name = "gridEditors")]
    GridEditor[] GridEditors { get; set; }

    /// <summary>
    ///     Gets or sets the content apps listed in the manifest.
    /// </summary>
    [DataMember(Name = "contentApps")]
    ManifestContentAppDefinition[] ContentApps { get; set; }

    /// <summary>
    ///     Gets or sets the dashboards listed in the manifest.
    /// </summary>
    [DataMember(Name = "dashboards")]
    ManifestDashboard[] Dashboards { get; set; }

    /// <summary>
    ///     Gets or sets the sections listed in the manifest.
    /// </summary>
    [DataMember(Name = "sections")]
    ManifestSection[] Sections { get; set; }
}
