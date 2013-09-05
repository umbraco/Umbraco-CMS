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

        #region IOrderedEnumerable<T>

        public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return new PublishedContentOrderedSet<T>(((IOrderedEnumerable<T>)Source).CreateOrderedEnumerable(keySelector, comparer, descending));
        }

        #endregion

        // fixme wtf?!
#if IMPLEMENT_LINQ_EXTENSIONS

        // BEWARE!
        // here, Source.Whatever() will invoke the System.Linq.Enumerable extension method
        // and not the extension methods that we may have defined on IEnumerable<T> or
        // IOrderedEnumerable<T>, provided that they are NOT within the scope at compile time.

        #region Wrap methods returning IOrderedEnumerable<T>

        public PublishedContentOrderedSet<T> ThenBy<TKey>(Func<T, TKey> keySelector)
        {
            return new PublishedContentOrderedSet<T>(((IOrderedEnumerable<T>)Source).ThenBy(keySelector));
        }

        public PublishedContentOrderedSet<T> ThenBy<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            return new PublishedContentOrderedSet<T>(((IOrderedEnumerable<T>)Source).ThenBy(keySelector, comparer));
        }

        public PublishedContentOrderedSet<T> ThenByDescending<TKey>(Func<T, TKey> keySelector)
        {
            return new PublishedContentOrderedSet<T>(((IOrderedEnumerable<T>)Source).ThenByDescending(keySelector));
        }

        public PublishedContentOrderedSet<T> ThenByDescending<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            return new PublishedContentOrderedSet<T>(((IOrderedEnumerable<T>)Source).ThenByDescending(keySelector, comparer));
        }

        #endregion

#endif
    }
}
