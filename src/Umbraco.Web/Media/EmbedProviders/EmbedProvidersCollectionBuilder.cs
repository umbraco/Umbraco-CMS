using Umbraco.Core.Composing;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class EmbedProvidersCollectionBuilder : OrderedCollectionBuilderBase<EmbedProvidersCollectionBuilder, EmbedProvidersCollection, IEmbedProvider>
    {
        protected override EmbedProvidersCollectionBuilder This => this;
    }
}
