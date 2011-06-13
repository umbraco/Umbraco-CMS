using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace umbraco.MacroEngines
{
    public class Grouping<K, T> : IGrouping<K, T>
    {
        public K Key { get; set; }
        public IEnumerable<T> Elements;

        public IEnumerator<T> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }

}
