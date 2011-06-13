using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace umbraco.MacroEngines
{
    public class DynamicGrouping : IEnumerable
    {
        public IEnumerable<Grouping<object, DynamicNode>> Inner;

        public DynamicGrouping OrderBy(string expression)
        {
            return this;
        }

        public DynamicGrouping(DynamicNodeList list, string groupBy)
        {
            Inner =
              list
              .Items
              .GroupBy(item =>
              {
                  object result = null;
                  item.TryGetMember(new DynamicQueryableGetMemberBinder(groupBy, false), out result);
                  return result;
              })
              .Select(item => new Grouping<object, DynamicNode>()
              {
                  Key = item.Key,
                  Elements = item.Select(inner => inner)
              });
        }
        public DynamicGrouping(IEnumerable<Grouping<object, DynamicNode>> source)
        {
            this.Inner = source;
        }

        public IEnumerator GetEnumerator()
        {
            return Inner.GetEnumerator();
        }
    }
}
