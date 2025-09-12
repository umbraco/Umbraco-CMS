using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Api.Management.Services.Flags;

/// <summary>
/// Defines an ordered collection of <see cref="IFlagProvider"/>.
/// </summary>
public class FlagProviderCollection : BuilderCollectionBase<IFlagProvider>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlagProviderCollection"/> class.
    /// </summary>
    /// <param name="items">The collection items.</param>
    public FlagProviderCollection(Func<IEnumerable<IFlagProvider>> items)
        : base(items)
    {
    }
}
