using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Media.EmbedProviders
{
    public class EmbedProvidersCollection : BuilderCollectionBase<IEmbedProvider>
    {
        public EmbedProvidersCollection(IEnumerable<IEmbedProvider> items)
            : base(items)
        { }
    }
}
