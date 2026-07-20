using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Represents a collection of <see cref="IEmbedProvider"/> instances.
/// </summary>
public class EmbedProvidersCollection : BuilderCollectionBase<IEmbedProvider>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EmbedProvidersCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection of embed providers.</param>
    public EmbedProvidersCollection(Func<IEnumerable<IEmbedProvider>> items)
        : base(items)
    {
    }
}
