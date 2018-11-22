using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Dynamic;
using Umbraco.Core.Dynamics;
using Umbraco.Web.Models;

namespace Umbraco.Web.Dynamics
{
    internal class Grouping<K, T> : IGrouping<K, T> where T : DynamicObject
    {
        public K Key { get; set; }
        public IEnumerable<T> Elements { get; set; }

        public IEnumerator<T> GetEnumerator()
        {
            var temp = new DynamicPublishedContentList(Elements.Cast<DynamicPublishedContent>());
            return (IEnumerator<T>)temp.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public DynamicPublishedContentList OrderBy(string ordering)
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
                return new DynamicPublishedContentList(Elements.OrderBy(item =>
                {
                    object key = null;
                    (item as DynamicObject).TryGetMember(new DynamicQueryableGetMemberBinder(ordering, false), out key);
                    return key;
                }).Cast<DynamicPublishedContent>());
            }
            else
            {
                return new DynamicPublishedContentList(Elements.OrderByDescending(item =>
                {
                    object key = null;
                    (item as DynamicObject).TryGetMember(new DynamicQueryableGetMemberBinder(ordering, false), out key);
                    return key;
                }).Cast<DynamicPublishedContent>());
            }
        }
    }

}
