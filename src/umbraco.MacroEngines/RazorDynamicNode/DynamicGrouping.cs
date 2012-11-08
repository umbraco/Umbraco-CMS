using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace umbraco.MacroEngines
{
	public class DynamicGrouping : IEnumerable<IGrouping<object, DynamicNode>>, IEnumerable<Grouping<object, DynamicNode>>
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
              .Select(node =>
                {
                    string predicate = groupBy;
                    var internalList = new DynamicNodeList(new DynamicNode[] { node });
                    var query = (IQueryable<object>)internalList.Select(predicate, new object[] { });
                    var key = query.FirstOrDefault();
                    return new
                    {
                        Key = key,
                        Node = node
                    };
                })
              .Where(item => item.Key != null)
              .GroupBy(item => item.Key)
              .Select(item => new Grouping<object, DynamicNode>()
              {
                  Key = item.Key,
                  Elements = item.Select(inner => inner.Node)
              });
        }
        public DynamicGrouping(IEnumerable<Grouping<object, DynamicNode>> source)
        {
            this.Inner = source;
        }

		IEnumerator<Grouping<object, DynamicNode>> IEnumerable<Grouping<object, DynamicNode>>.GetEnumerator()
		{
			return Inner.GetEnumerator();
		}

		IEnumerator<IGrouping<object, DynamicNode>> IEnumerable<IGrouping<object, DynamicNode>>.GetEnumerator()
		{
			return Inner.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
        {
            return Inner.GetEnumerator();
        }
    }
}
