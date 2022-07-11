using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

public class EmbedProvidersCollectionBuilder : OrderedCollectionBuilderBase<EmbedProvidersCollectionBuilder, EmbedProvidersCollection, IEmbedProvider>
{
    protected override EmbedProvidersCollectionBuilder This => this;
}
