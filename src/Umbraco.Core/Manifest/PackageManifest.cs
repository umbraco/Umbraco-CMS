using System.Runtime.Serialization;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Represents the content of a package manifest.
/// </summary>
[DataContract]
public class PackageManifest
{
    private string? _packageName;

    /// <summary>
    ///     An optional package name. If not specified then the directory name is used.
    /// </summary>
    [DataMember(Name = "name")]
    public string? PackageName
    {
        get
        {
            if (!_packageName.IsNullOrWhiteSpace())
            {
                return _packageName;
            }

            if (!Source.IsNullOrWhiteSpace())
            {
                _packageName = Path.GetFileName(Path.GetDirectoryName(Source));
            }

            return _packageName;
        }
        set => _packageName = value;
    }

    [DataMember(Name = "packageView")]
    public string? PackageView { get; set; }

    /// <summary>
    ///     Gets the source path of the manifest.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Gets the full absolute file path of the manifest,
    ///         using system directory separators.
    ///     </para>
    /// </remarks>
    [IgnoreDataMember]
    public string Source { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the version of the package
    /// </summary>
    [DataMember(Name = "version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether telemetry is allowed
    /// </summary>
    [DataMember(Name = "allowPackageTelemetry")]
    public bool AllowPackageTelemetry { get; set; } = true;

    [DataMember(Name = "bundleOptions")]
    public BundleOptions BundleOptions { get; set; }

    /// <summary>
    ///     Gets or sets the scripts listed in the manifest.
    /// </summary>
    [DataMember(Name = "javascript")]
    public string[] Scripts { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Gets or sets the stylesheets listed in the manifest.
    /// </summary>
    [DataMember(Name = "css")]
    public string[] Stylesheets { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Gets or sets the property editors listed in the manifest.
    /// </summary>
    [DataMember(Name = "propertyEditors")]
    public IDataEditor[] PropertyEditors { get; set; } = Array.Empty<IDataEditor>();

    /// <summary>
    ///     Gets or sets the parameter editors listed in the manifest.
    /// </summary>
    [DataMember(Name = "parameterEditors")]
    public IDataEditor[] ParameterEditors { get; set; } = Array.Empty<IDataEditor>();

    /// <summary>
    ///     Gets or sets the grid editors listed in the manifest.
    /// </summary>
    [DataMember(Name = "gridEditors")]
    public GridEditor[] GridEditors { get; set; } = Array.Empty<GridEditor>();

    /// <summary>
    ///     Gets or sets the content apps listed in the manifest.
    /// </summary>
    [DataMember(Name = "contentApps")]
    public ManifestContentAppDefinition[] ContentApps { get; set; } = Array.Empty<ManifestContentAppDefinition>();

    /// <summary>
    ///     Gets or sets the dashboards listed in the manifest.
    /// </summary>
    [DataMember(Name = "dashboards")]
    public ManifestDashboard[] Dashboards { get; set; } = Array.Empty<ManifestDashboard>();

    /// <summary>
    ///     Gets or sets the sections listed in the manifest.
    /// </summary>
    [DataMember(Name = "sections")]
    public ManifestSection[] Sections { get; set; } = Array.Empty<ManifestSection>();
}
