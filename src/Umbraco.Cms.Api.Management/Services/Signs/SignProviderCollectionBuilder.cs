using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Api.Management.Services.Signs;

/// <summary>
/// Builds an ordered collection of <see cref="ISignProvider"/>.
/// </summary>
public class SignProviderCollectionBuilder : OrderedCollectionBuilderBase<SignProviderCollectionBuilder, SignProviderCollection, ISignProvider>
{
    /// <inheritdoc/>
    protected override SignProviderCollectionBuilder This => this;
}
