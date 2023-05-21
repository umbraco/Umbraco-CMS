using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     A package manifest made up of all combined manifests
/// </summary>
public class CompositePackageManifest
{
    public CompositePackageManifest(
        IReadOnlyList<IDataEditor> propertyEditors,
        IReadOnlyList<IDataEditor> parameterEditors,
        IReadOnlyList<GridEditor> gridEditors,
        IReadOnlyList<ManifestContentAppDefinition> contentApps,
        IReadOnlyList<ManifestDashboard> dashboards,
        IReadOnlyList<ManifestSection> sections,
        IReadOnlyDictionary<BundleOptions, IReadOnlyList<ManifestAssets>> scripts,
        IReadOnlyDictionary<BundleOptions, IReadOnlyList<ManifestAssets>> stylesheets)
    {
        PropertyEditors = propertyEditors ?? throw new ArgumentNullException(nameof(propertyEditors));
        ParameterEditors = parameterEditors ?? throw new ArgumentNullException(nameof(parameterEditors));
        GridEditors = gridEditors ?? throw new ArgumentNullException(nameof(gridEditors));
        ContentApps = contentApps ?? throw new ArgumentNullException(nameof(contentApps));
        Dashboards = dashboards ?? throw new ArgumentNullException(nameof(dashboards));
        Sections = sections ?? throw new ArgumentNullException(nameof(sections));
        Scripts = scripts ?? throw new ArgumentNullException(nameof(scripts));
        Stylesheets = stylesheets ?? throw new ArgumentNullException(nameof(stylesheets));
    }

    /// <summary>
    ///     Gets or sets the property editors listed in the manifest.
    /// </summary>
    public IReadOnlyList<IDataEditor> PropertyEditors { get; }

    /// <summary>
    ///     Gets or sets the parameter editors listed in the manifest.
    /// </summary>
    public IReadOnlyList<IDataEditor> ParameterEditors { get; }

    /// <summary>
    ///     Gets or sets the grid editors listed in the manifest.
    /// </summary>
    public IReadOnlyList<GridEditor> GridEditors { get; }

    /// <summary>
    ///     Gets or sets the content apps listed in the manifest.
    /// </summary>
    public IReadOnlyList<ManifestContentAppDefinition> ContentApps { get; }

    /// <summary>
    ///     Gets or sets the dashboards listed in the manifest.
    /// </summary>
    public IReadOnlyList<ManifestDashboard> Dashboards { get; }

    /// <summary>
    ///     Gets or sets the sections listed in the manifest.
    /// </summary>
    public IReadOnlyCollection<ManifestSection> Sections { get; }

    public IReadOnlyDictionary<BundleOptions, IReadOnlyList<ManifestAssets>> Scripts { get; }

    public IReadOnlyDictionary<BundleOptions, IReadOnlyList<ManifestAssets>> Stylesheets { get; }
}
