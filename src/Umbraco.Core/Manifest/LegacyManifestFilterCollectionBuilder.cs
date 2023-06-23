using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Manifest;

public class LegacyManifestFilterCollectionBuilder : OrderedCollectionBuilderBase<LegacyManifestFilterCollectionBuilder,
    LegacyManifestFilterCollection, ILegacyManifestFilter>
{
    protected override LegacyManifestFilterCollectionBuilder This => this;

    // do NOT cache this, it's only used once
    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient;
}
