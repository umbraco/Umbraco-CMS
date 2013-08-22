using System;
using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Comparer that takes into account the duplicate index of a node name
    /// This is needed as a normal alphabetic sort would go Page (1), Page (10), Page (2) etc.
    /// </summary>
    internal class SimilarNodeNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x.LastIndexOf('(') != -1 && x.LastIndexOf(')') == x.Length - 1 && y.LastIndexOf(')') == y.Length - 1)
            {
                if (x.ToLower().Substring(0, x.LastIndexOf('(')) == y.ToLower().Substring(0, y.LastIndexOf('(')))
                {
                    int xDuplicateIndex = ExtractDuplicateIndex(x);
                    int yDuplicateIndex = ExtractDuplicateIndex(y);

                    if (xDuplicateIndex != 0 && yDuplicateIndex != 0)
                    {
                        return xDuplicateIndex.CompareTo(yDuplicateIndex);
                    }
                }
            }
            return String.Compare(x.ToLower(), y.ToLower(), StringComparison.Ordinal);
        }

        private int ExtractDuplicateIndex(string text)
        {
            int index = 0;

            if (text.LastIndexOf('(') != -1 && text.LastIndexOf('(') < text.Length - 2)
            {
                int startPos = text.LastIndexOf('(') + 1;
                int length = text.Length - 1 - startPos;

                int.TryParse(text.Substring(startPos, length), out index);
            }

            return index;
        }
    }
}