using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Api.Management.Services.Flags;

/// <summary>
/// Builds an ordered collection of <see cref="IFlagProvider"/>.
/// </summary>
public class FlagProviderCollectionBuilder : OrderedCollectionBuilderBase<FlagProviderCollectionBuilder, FlagProviderCollection, IFlagProvider>
{
    /// <inheritdoc/>
    protected override FlagProviderCollectionBuilder This => this;
}
