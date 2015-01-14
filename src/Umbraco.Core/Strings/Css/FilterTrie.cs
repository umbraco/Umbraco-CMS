using System.Collections.Generic;

namespace Umbraco.Core.Strings.Css
{
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