using Umbraco.Core.Composing;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class EmbedProvidersCollectionBuilder : OrderedCollectionBuilderBase<EmbedProvidersCollectionBuilder, EmbedProvidersCollection, IEmbedProvider>
    {
        protected override EmbedProvidersCollectionBuilder This => this;

        public override EmbedProvidersCollection CreateCollection(IFactory factory)
        {
            // No fancy magic here of reading package.manifest or using typeloader
            // Have to explicitly add a OEmbedProvider to this collection
            return base.CreateCollection(factory);
        }        
    }
}
