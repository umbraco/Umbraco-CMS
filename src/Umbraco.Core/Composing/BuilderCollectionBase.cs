using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides a base class for builder collections.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class BuilderCollectionBase<TItem> : IBuilderCollection<TItem>
    {
        private readonly TItem[] _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderCollectionBase{TItem}"/> with items.
        /// </summary>
        /// <param name="items">The items.</param>
        protected BuilderCollectionBase(IEnumerable<TItem> items)
        {
            _items = items.ToArray();
        }

        /// <inheritdoc />
        public int Count => _items.Length;

        /// <summary>
        /// Gets an enumerator.
        /// </summary>
        public IEnumerator<TItem> GetEnumerator()
        {
            return ((IEnumerable<TItem>) _items).GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
