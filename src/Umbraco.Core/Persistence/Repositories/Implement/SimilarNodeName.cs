using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class SimilarNodeName
    {
        private int _numPos = -2;

        public int Id { get; set; }
        public string Name { get; set; }

        // cached - reused
        public int NumPos
        {
            get
            {
                if (_numPos != -2) return _numPos;

                var name = Name;

                // cater nodes with no name.
                if (string.IsNullOrWhiteSpace(name))
                    return _numPos;

                if (name[name.Length - 1] != ')')
                    return _numPos = -1;

                var pos = name.LastIndexOf('(');
                if (pos < 2 || pos == name.Length - 2) // < 2 and not < 0, because we want at least "x ("
                    return _numPos = -1;

                return _numPos = pos;
            }
        }

        // not cached - used only once
        public int NumVal
        {
            get
            {
                if (NumPos < 0)
                    throw new InvalidOperationException();
                int num;
                if (int.TryParse(Name.Substring(NumPos + 1, Name.Length - 2 - NumPos), out num))
                    return num;
                return 0;
            }
        }

        // compare without allocating, nor parsing integers
        internal class Comparer : IComparer<SimilarNodeName>
        {
            public int Compare(SimilarNodeName x, SimilarNodeName y)
            {
                if (x == null) throw new ArgumentNullException("x");
                if (y == null) throw new ArgumentNullException("y");

                var xpos = x.NumPos;
                var ypos = y.NumPos;

                var xname = x.Name;
                var yname = y.Name;

                if (xpos < 0 || ypos < 0 || xpos != ypos)
                    return string.Compare(xname, yname, StringComparison.Ordinal);

                // compare the part before (number)
                var n = string.Compare(xname, 0, yname, 0, xpos, StringComparison.Ordinal);
                if (n != 0)
                    return n;

                // compare (number) lengths
                var diff = xname.Length - yname.Length;
                if (diff != 0) return diff < 0 ? -1 : +1;

                // actually compare (number)
                var i = xpos;
                while (i < xname.Length - 1)
                {
                    if (xname[i] != yname[i])
                        return xname[i] < yname[i] ? -1 : +1;
                    i++;
                }
                return 0;
            }
        }

        // gets a unique name
        public static string GetUniqueName(IEnumerable<SimilarNodeName> names, int nodeId, string nodeName)
        {
            var uniqueNumber = 1;
            var uniqueing = false;
            foreach (var name in names.OrderBy(x => x, new Comparer()))
            {
                // ignore self
                if (nodeId != 0 && name.Id == nodeId) continue;

                if (uniqueing)
                {
                    if (name.NumPos > 0 && name.Name.InvariantStartsWith(nodeName) && name.NumVal == uniqueNumber)
                        uniqueNumber++;
                    else
                        break;
                }
                else if (name.Name.InvariantEquals(nodeName))
                {
                    uniqueing = true;
                }
            }

            return uniqueing || string.IsNullOrWhiteSpace(nodeName)
                ? string.Concat(nodeName, " (", uniqueNumber.ToString(), ")")
                : nodeName;
        }
    }
}
