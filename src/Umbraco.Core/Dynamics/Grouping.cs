using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Dynamic;

namespace Umbraco.Core.Dynamics
{
    internal class Grouping<K, T> : IGrouping<K, T> where T : DynamicObject
    {
        public K Key { get; set; }
        public IEnumerable<T> Elements;

        public IEnumerator<T> GetEnumerator()
        {
            var temp = new DynamicDocumentList(Elements.Cast<DynamicDocument>());
            return (IEnumerator<T>)temp.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public DynamicDocumentList OrderBy(string ordering)
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
                return new DynamicDocumentList(Elements.OrderBy(item =>
                {
                    object key = null;
                    (item as DynamicObject).TryGetMember(new DynamicQueryableGetMemberBinder(ordering, false), out key);
                    return key;
                }).Cast<DynamicDocument>());
            }
            else
            {
                return new DynamicDocumentList(Elements.OrderByDescending(item =>
                {
                    object key = null;
                    (item as DynamicObject).TryGetMember(new DynamicQueryableGetMemberBinder(ordering, false), out key);
                    return key;
                }).Cast<DynamicDocument>());
            }
        }
    }

}
