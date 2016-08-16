using System.Collections.Generic;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Represents an injected collection, ie an immutable enumeration of items.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public interface IInjectCollection<out TItem>
    {
        /// <summary>
        /// Gets the items in the collection.
        /// </summary>
        IEnumerable<TItem> Items { get; }
    }
}
