using System.Collections.Generic;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Provides a base class for injected collections.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class InjectCollectionBase<TItem> : IInjectCollection<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InjectCollectionBase{TItem}"/> with items.
        /// </summary>
        /// <param name="items">The items.</param>
        protected InjectCollectionBase(IEnumerable<TItem> items)
        {
            Items = items;
        }

        /// <inheritdoc />
        public IEnumerable<TItem> Items { get; }
    }
}