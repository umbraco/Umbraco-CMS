using System;
using System.Collections.Generic;

namespace umbraco.ActionHandlers
{
    /// <summary>
    /// Comparer that takes into account the duplicate index of a node name
    /// This is needed as a normal alphabetic sort would go Page (1), Page (10), Page (2) etc.
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in future versions")]
    public class SimilarNodeNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x.LastIndexOf(')') == x.Length - 1 && y.LastIndexOf(')') == y.Length - 1)
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
            return x.ToLower().CompareTo(y.ToLower());                   
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