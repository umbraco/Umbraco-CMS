using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
///     Represents an ordered collection of <see cref="IMachineIdentityProvider" /> instances.
/// </summary>
public class MachineIdentityProviderCollection : BuilderCollectionBase<IMachineIdentityProvider>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MachineIdentityProviderCollection" /> class.
    /// </summary>
    /// <param name="items">A factory function that returns the machine identity providers.</param>
    public MachineIdentityProviderCollection(Func<IEnumerable<IMachineIdentityProvider>> items)
        : base(items)
    {
    }
}
