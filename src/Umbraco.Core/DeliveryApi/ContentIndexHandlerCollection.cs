using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A collection of <see cref="IContentIndexHandler"/> implementations for the Delivery API.
/// </summary>
public sealed class ContentIndexHandlerCollection : BuilderCollectionBase<IContentIndexHandler>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentIndexHandlerCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the content index handlers.</param>
    public ContentIndexHandlerCollection(Func<IEnumerable<IContentIndexHandler>> items)
        : base(items)
    {
    }
}
