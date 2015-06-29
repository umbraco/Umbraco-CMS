using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Css
{
    /// <summary>
    /// Defines a character sequence to filter out when reading.
    /// </summary>
    /// <remarks>
    /// If the sequence exists in the read source, it will be read out as if it was never there.
    /// </remarks>
    internal struct ReadFilter
    {
        #region Fields

        public readonly string StartToken;
        public readonly string EndToken;

        #endregion Fields

        #region Init

        public ReadFilter(string start, string end)
        {
            if (String.IsNullOrEmpty(start))
            {
                throw new ArgumentNullException("start");
            }
            if (String.IsNullOrEmpty(end))
            {
                throw new ArgumentNullException("end");
            }

            this.StartToken = start;
            this.EndToken = end;
        }

        #endregion Init
    }

    /// <summary>
    /// Creates a Trie out of ReadFilters
    /// </summary>
    internal class FilterTrie : TrieNode<char, string>
    {
        #region Constants

        private const int DefaultTrieWidth = 1;

        #endregion Constants

        #region Init

        internal FilterTrie(IEnumerable<ReadFilter> filters)
            : base(DefaultTrieWidth)
        {
            // load trie
            foreach (ReadFilter filter in filters)
            {
                TrieNode<char, string> node = this;

                // build out the path for StartToken
                foreach (char ch in filter.StartToken)
                {
                    if (!node.Contains(ch))
                    {
                        node[ch] = new TrieNode<char, string>(DefaultTrieWidth);
                    }

                    node = (TrieNode<char, string>)node[ch];
                }

                // at the end of StartToken path is the EndToken
                node.Value = filter.EndToken;
            }
        }

        #endregion Init
    }
}