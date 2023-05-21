using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Manifest;

public class ManifestFilterCollectionBuilder : OrderedCollectionBuilderBase<ManifestFilterCollectionBuilder,
    ManifestFilterCollection, IManifestFilter>
{
    protected override ManifestFilterCollectionBuilder This => this;

    // do NOT cache this, it's only used once
    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient;
}
