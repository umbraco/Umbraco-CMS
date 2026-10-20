using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
///     Builds a <see cref="MachineIdentityProviderCollection" /> by registering <see cref="IMachineIdentityProvider" />
///     implementations.
/// </summary>
public class MachineIdentityProviderCollectionBuilder
    : OrderedCollectionBuilderBase<MachineIdentityProviderCollectionBuilder, MachineIdentityProviderCollection, IMachineIdentityProvider>
{
    /// <inheritdoc />
    protected override MachineIdentityProviderCollectionBuilder This => this;
}
