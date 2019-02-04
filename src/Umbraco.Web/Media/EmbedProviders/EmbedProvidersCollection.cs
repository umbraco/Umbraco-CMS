using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class EmbedProvidersCollection : BuilderCollectionBase<IEmbedProvider>
    {
        public EmbedProvidersCollection(IEnumerable<IEmbedProvider> items)
            : base(items)
        { }
    }
}
