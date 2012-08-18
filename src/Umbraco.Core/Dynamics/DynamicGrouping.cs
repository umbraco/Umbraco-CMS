using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Umbraco.Core.Dynamics
{
    internal class DynamicGrouping : IEnumerable
    {
        public IEnumerable<Grouping<object, DynamicDocument>> Inner;

        public DynamicGrouping OrderBy(string expression)
        {
            return this;
        }

        public DynamicGrouping(DynamicDocumentList list, string groupBy)
        {
            Inner =
              list
              .Items
              .Select(node =>
                {
                    string predicate = groupBy;
                    var internalList = new DynamicDocumentList(new DynamicDocument[] { node });
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
              .Select(item => new Grouping<object, DynamicDocument>()
              {
                  Key = item.Key,
                  Elements = item.Select(inner => inner.Node)
              });
        }
        public DynamicGrouping(IEnumerable<Grouping<object, DynamicDocument>> source)
        {
            this.Inner = source;
        }

        public IEnumerator GetEnumerator()
        {
            return Inner.GetEnumerator();
        }
    }
}
