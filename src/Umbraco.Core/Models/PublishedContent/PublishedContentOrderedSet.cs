using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents an ordered set of <see cref="IPublishedContent"/>.
    /// </summary>
    /// <typeparam name="T">The type of content.</typeparam>
    public class PublishedContentOrderedSet<T> : PublishedContentSet<T>, IOrderedEnumerable<T>
        where T : class, IPublishedContent
    {
// ReSharper disable ParameterTypeCanBeEnumerable.Local
        internal PublishedContentOrderedSet(IOrderedEnumerable<T> content)
// ReSharper restore ParameterTypeCanBeEnumerable.Local
            : base(content)
        { }

        // note: because we implement IOrderedEnumerable, we don't need to implement the ThenBy nor
        // ThenByDescending methods here, only CreateOrderedEnumerable and that does it.

        #region IOrderedEnumerable<T>

        public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return new PublishedContentOrderedSet<T>(((IOrderedEnumerable<T>)Source).CreateOrderedEnumerable(keySelector, comparer, descending));
        }

        #endregion
    }
}
