using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Dynamic;

namespace umbraco.MacroEngines
{
    public class Grouping<K, T> : IGrouping<K, T> where T : DynamicObject
    {
        public K Key { get; set; }
        public IEnumerable<T> Elements;

        public IEnumerator<T> GetEnumerator()
        {
            DynamicNodeList temp = new DynamicNodeList(Elements.Cast<DynamicNode>());
            return (IEnumerator<T>)temp.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public DynamicNodeList OrderBy(string ordering)
        {
            bool descending = false;
            if (ordering.IndexOf(" descending", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                ordering = ordering.Replace(" descending", "");
                descending = true;
            }
            if (ordering.IndexOf(" desc", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                ordering = ordering.Replace(" desc", "");
                descending = true;
            }

            if (!descending)
            {
                return new DynamicNodeList(Elements.OrderBy(item =>
                {
                    object key = null;
                    (item as DynamicObject).TryGetMember(new DynamicQueryableGetMemberBinder(ordering, false), out key);
                    return key;
                }).Cast<DynamicNode>());
            }
            else
            {
                return new DynamicNodeList(Elements.OrderByDescending(item =>
                {
                    object key = null;
                    (item as DynamicObject).TryGetMember(new DynamicQueryableGetMemberBinder(ordering, false), out key);
                    return key;
                }).Cast<DynamicNode>());
            }
        }
    }

}
