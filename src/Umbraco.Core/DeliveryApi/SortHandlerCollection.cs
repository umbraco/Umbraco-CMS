using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A collection of <see cref="ISortHandler"/> implementations for the Delivery API.
/// </summary>
public sealed class SortHandlerCollection : BuilderCollectionBase<ISortHandler>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SortHandlerCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the sort handlers.</param>
    public SortHandlerCollection(Func<IEnumerable<ISortHandler>> items)
        : base(items)
    {
    }
}
