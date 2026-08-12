using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;

/// <summary>
/// The collection of registered <see cref="IPropertyValueHandler"/> implementations.
/// </summary>
internal sealed class PropertyValueHandlerCollection : BuilderCollectionBase<IPropertyValueHandler>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyValueHandlerCollection"/> class.
    /// </summary>
    public PropertyValueHandlerCollection(Func<IEnumerable<IPropertyValueHandler>> items)
        : base(items)
    {
    }
}
