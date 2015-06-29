using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Css
{
    /// <summary>
    /// A generic node for building a Trie
    /// </summary>
    /// <typeparam name="TKey">the Type used for the node path</typeparam>
    /// <typeparam name="TValue">the Type used for the node value</typeparam>
    /// <remarks>
    /// http://en.wikipedia.org/wiki/Trie
    /// </remarks>
    internal class TrieNode<TKey, TValue> : ITrieNode<TKey, TValue>
    {
        #region Fields

        private readonly IDictionary<TKey, ITrieNode<TKey, TValue>> Children;
        private TValue value = default(TValue);

        #endregion Fields

        #region Init

        /// <summary>
        /// Ctor
        /// </summary>
        public TrieNode()
            : this(-1)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="capacity"></param>
        public TrieNode(int capacity)
        {
            if (capacity < 1)
            {
                this.Children = new Dictionary<TKey, ITrieNode<TKey, TValue>>();
            }
            else
            {
                this.Children = new Dictionary<TKey, ITrieNode<TKey, TValue>>(capacity);
            }
        }

        #endregion Init

        #region Properties

        public ITrieNode<TKey, TValue> this[TKey key]
        {
            get
            {
                if (!this.Children.ContainsKey(key))
                {
                    return null;
                }
                return this.Children[key];
            }

            // added "internal" to get around change in C# 3.0 modifiers
            // this worked fine in C# 2.0 but they fixed that bug
            // http://blogs.msdn.com/ericlippert/archive/2008/03/28/why-can-t-i-access-a-protected-member-from-a-derived-class-part-two-why-can-i.aspx
            protected internal set { this.Children[key] = value; }
        }

        public TValue Value
        {
            get { return this.value; }

            // added "internal" to get around change in C# 3.0 modifiers
            // this worked fine in C# 2.0 but they fixed that bug
            // http://blogs.msdn.com/ericlippert/archive/2008/03/28/why-can-t-i-access-a-protected-member-from-a-derived-class-part-two-why-can-i.aspx
            protected internal set
            {
                if (!EqualityComparer<TValue>.Default.Equals(this.value, default(TValue)))
                {
                    throw new InvalidOperationException("Trie path collision: the value for TrieNode<" + value.GetType().Name + "> has already been assigned.");
                }
                this.value = value;
            }
        }

        public bool HasValue
        {
            get
            {
                return !EqualityComparer<TValue>.Default.Equals(this.value, default(TValue));
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines if child exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(TKey key)
        {
            return this.Children.ContainsKey(key);
        }

        #endregion Methods
    }

    internal interface ITrieNode<TKey, TValue>
    {
        #region Properties

        ITrieNode<TKey, TValue> this[TKey key]
        {
            get;
        }

        TValue Value
        {
            get;
        }

        bool HasValue
        {
            get;
        }

        #endregion Methods

        #region Methods

        bool Contains(TKey key);

        #endregion Methods
    }
}