using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Manifest
{
    /// <summary>
    /// Contains the manifest filters.
    /// </summary>
    public class ManifestFilterCollection : BuilderCollectionBase<IManifestFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestFilterCollection"/> class.
        /// </summary>
        public ManifestFilterCollection(IEnumerable<IManifestFilter> items)
            : base(items)
        { }

        /// <summary>
        /// Filters package manifests.
        /// </summary>
        /// <param name="manifests">The package manifests.</param>
        public void Filter(List<PackageManifest> manifests)
        {
            foreach (var filter in this)
                filter.Filter(manifests);
        }
    }
}
