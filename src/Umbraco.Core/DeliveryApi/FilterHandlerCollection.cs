using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A collection of <see cref="IFilterHandler"/> implementations for the Delivery API.
/// </summary>
public sealed class FilterHandlerCollection : BuilderCollectionBase<IFilterHandler>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FilterHandlerCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the filter handlers.</param>
    public FilterHandlerCollection(Func<IEnumerable<IFilterHandler>> items)
        : base(items)
    {
    }
}
