using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents a set of <see cref="IPublishedContent"/>.
    /// </summary>
    /// <typeparam name="T">The type of content.</typeparam>
    /// <remarks>
    /// <para>A <c>ContentSet{T}</c> is created from an <c>IEnumerable{T}</c> using the <c>ToContentSet</c>
    /// extension method.</para>
    /// <para>The content set source is enumerated only once. Same as what you get
    /// when you call ToList on an IEnumerable. Only, ToList enumerates its source when
    /// created, whereas a content set enumerates its source only when the content set itself
    /// is enumerated.</para>
    /// </remarks>
    public class PublishedContentSet<T> : IEnumerable<T>
        where T : class, IPublishedContent
    {
        // used by <c>ToContentSet</c> extension method to initialize a new set from an IEnumerable.
        internal PublishedContentSet(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            Source = source;
        }

        #region Source

        protected readonly IEnumerable<T> Source;
        
        #endregion

        #region Enumerated

        // cache the enumeration so we don't enumerate more than once. Same as what you get
        // when you call ToList on an IEnumerable. Only, ToList enumerates its source when
        // created, whereas a content set enumerates its source only when the content set itself
        // is enumerated.

        // cache the wrapped items so if we reset the enumeration, we do not re-wrap everything (only new items).

        private T[] _enumerated;
        private readonly Dictionary<T, IPublishedContentExtended> _xContent = new Dictionary<T, IPublishedContentExtended>();

        // wrap an item, ie create the actual clone for this set
        private T MapContentAsT(T t)
        {
            return MapContent(t) as T;
        }

        internal IPublishedContentExtended MapContent(T t)
        {
            IPublishedContentExtended extend;
            if (_xContent.TryGetValue(t, out extend)) return extend;

            extend = PublishedContentExtended.Extend(t, this);
            var asT = extend as T;
            if (asT == null)
                throw new InvalidOperationException(string.Format("Failed extend a published content of type {0}."
                                                                  + "Got {1} when expecting {2}.", t.GetType().FullName, extend.GetType().FullName, typeof(T).FullName));
            _xContent[t] = extend;
            return extend;
        }

        private T[] Enumerated
        {
            get
            {
                // enumerate the source and cache the result
                // tell clones about their index within the set (for perfs purposes)
                var index = 0;
                return _enumerated ?? (_enumerated = Source.Select(t =>
                    {
                        var extend = MapContent(t);
                        extend.SetIndex(index++);
                        return extend as T;
                    }).ToArray());
            }
        }

        // indicates that the source has changed
        // so the set can clear its inner caches
        // should only be used by DynamicPublishedContentList
        internal void SourceChanged()
        {
            // reset the cached enumeration so it's enumerated again
            if (_enumerated == null) return;
            _enumerated = null;

            foreach (var item in _xContent.Values)
                item.ClearIndex();

            var removed = _xContent.Keys.Except(Source);
            foreach (var content in removed)
            {
                _xContent[content].ClearContentSet();
                _xContent.Remove(content);
            }
        }

        /// <summary>
        /// Gets the number of items in the set.
        /// </summary>
        /// <returns>The number of items in the set.</returns>
        /// <remarks>Will cause the set to be enumerated if it hasn't been already.</remarks>
        public virtual int Count
        {
            get { return Enumerated.Length; }
        }
        #endregion

        #region IEnumerable<T>

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Enumerated).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Wrap methods returning T

        public T ElementAt(int index)
        {
            return MapContentAsT(Source.ElementAt(index));
        }

        public T ElementAtOrDefault(int index)
        {
            var element = Source.ElementAtOrDefault(index);
            return element == null ? null : MapContentAsT(element);
        }

        public T First()
        {
            return MapContentAsT(Source.First());
        }

        public T First(Func<T, bool> predicate)
        {
            return MapContentAsT(Source.First(predicate));
        }

        public T FirstOrDefault()
        {
            var first = Source.FirstOrDefault();
            return first == null ? null : MapContentAsT(first);
        }

        public T FirstOrDefault(Func<T, bool> predicate)
        {
            var first = Source.FirstOrDefault(predicate);
            return first == null ? null : MapContentAsT(first);
        }

        public T Last()
        {
            return MapContentAsT(Source.Last());
        }

        public T Last(Func<T, bool> predicate)
        {
            return MapContentAsT(Source.Last(predicate));
        }

        public T LastOrDefault()
        {
            var last = Source.LastOrDefault();
            return last == null ? null : MapContentAsT(last);
        }

        public T LastOrDefault(Func<T, bool> predicate)
        {
            var last = Source.LastOrDefault(predicate);
            return last == null ? null : MapContentAsT(last);
        }

        public T Single()
        {
            return MapContentAsT(Source.Single());
        }

        public T Single(Func<T, bool> predicate)
        {
            return MapContentAsT(Source.Single(predicate));
        }

        public T SingleOrDefault()
        {
            var single = Source.SingleOrDefault();
            return single == null ? null : MapContentAsT(single);
        }

        public T SingleOrDefault(Func<T, bool> predicate)
        {
            var single = Source.SingleOrDefault(predicate);
            return single == null ? null : MapContentAsT(single);
        }

        #endregion

        #region Wrap methods returning IOrderedEnumerable<T>

        public PublishedContentOrderedSet<T> OrderBy<TKey>(Func<T, TKey> keySelector)
        {
            return new PublishedContentOrderedSet<T>(Source.OrderBy(keySelector));
        }

        public PublishedContentOrderedSet<T> OrderBy<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            return new PublishedContentOrderedSet<T>(Source.OrderBy(keySelector, comparer));
        }

        public PublishedContentOrderedSet<T> OrderByDescending<TKey>(Func<T, TKey> keySelector)
        {
            return new PublishedContentOrderedSet<T>(Source.OrderByDescending(keySelector));
        }

        public PublishedContentOrderedSet<T> OrderByDescending<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            return new PublishedContentOrderedSet<T>(Source.OrderByDescending(keySelector, comparer));
        }

        #endregion
    }
}
