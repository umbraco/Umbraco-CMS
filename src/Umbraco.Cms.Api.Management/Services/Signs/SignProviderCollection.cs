using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Api.Management.Services.Signs;

/// <summary>
/// Defines an ordered collection of <see cref="ISignProvider"/>.
/// </summary>
public class SignProviderCollection : BuilderCollectionBase<ISignProvider>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignProviderCollection"/> class.
    /// </summary>
    /// <param name="items">The collection items.</param>
    public SignProviderCollection(Func<IEnumerable<ISignProvider>> items)
        : base(items)
    {
    }
}
