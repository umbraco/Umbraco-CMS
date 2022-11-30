using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

public class EmbedProvidersCollection : BuilderCollectionBase<IEmbedProvider>
{
    public EmbedProvidersCollection(Func<IEnumerable<IEmbedProvider>> items)
        : base(items)
    {
    }
}
