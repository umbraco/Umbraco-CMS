using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;

/// <summary>
/// The collection of registered <see cref="IPropertyValueHandler"/> implementations.
/// </summary>
public sealed class PropertyValueHandlerCollection : BuilderCollectionBase<IPropertyValueHandler>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyValueHandlerCollection"/> class.
    /// </summary>
    /// <param name="items">A factory producing the registered property value handlers.</param>
    public PropertyValueHandlerCollection(Func<IEnumerable<IPropertyValueHandler>> items)
        : base(items)
    {
    }
}
