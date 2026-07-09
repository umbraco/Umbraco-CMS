using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Builds an <see cref="EmbedProvidersCollection"/> by registering <see cref="IEmbedProvider"/> implementations.
/// </summary>
public class EmbedProvidersCollectionBuilder : OrderedCollectionBuilderBase<EmbedProvidersCollectionBuilder, EmbedProvidersCollection, IEmbedProvider>
{
    /// <inheritdoc />
    protected override EmbedProvidersCollectionBuilder This => this;
}
