using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A collection of <see cref="ISelectorHandler"/> implementations for the Delivery API.
/// </summary>
public sealed class SelectorHandlerCollection : BuilderCollectionBase<ISelectorHandler>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SelectorHandlerCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the selector handlers.</param>
    public SelectorHandlerCollection(Func<IEnumerable<ISelectorHandler>> items)
        : base(items)
    {
    }
}
