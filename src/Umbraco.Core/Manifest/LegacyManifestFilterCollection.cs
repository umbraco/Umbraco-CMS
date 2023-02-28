using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Contains the manifest filters.
/// </summary>
public class LegacyManifestFilterCollection : BuilderCollectionBase<ILegacyManifestFilter>
{
    public LegacyManifestFilterCollection(Func<IEnumerable<ILegacyManifestFilter>> items)
        : base(items)
    {
    }

    /// <summary>
    ///     Filters package manifests.
    /// </summary>
    /// <param name="manifests">The package manifests.</param>
    public void Filter(List<LegacyPackageManifest> manifests)
    {
        foreach (ILegacyManifestFilter filter in this)
        {
            filter.Filter(manifests);
        }
    }
}
