using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Web.Dynamics
{
	public class DynamicGrouping : IEnumerable<IGrouping<object, IPublishedContent>>, IEnumerable<Grouping<object, DynamicPublishedContent>>
    {
        internal IEnumerable<Grouping<object, DynamicPublishedContent>> Inner;

        public DynamicGrouping OrderBy(string expression)
        {
            return this;
        }

        public DynamicGrouping(DynamicPublishedContentList list, string groupBy)
        {
            Inner =
              list
              .Items
              .Select(node =>
                {
                    string predicate = groupBy;
                    var internalList = new DynamicPublishedContentList(new DynamicPublishedContent[] { node });
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
              .Select(item => new Grouping<object, DynamicPublishedContent>()
              {
                  Key = item.Key,
                  Elements = item.Select(inner => inner.Node)
              });
        }
        internal DynamicGrouping(IEnumerable<Grouping<object, DynamicPublishedContent>> source)
        {
            this.Inner = source;
        }

		IEnumerator<Grouping<object, DynamicPublishedContent>> IEnumerable<Grouping<object, DynamicPublishedContent>>.GetEnumerator()
		{
			return Inner.GetEnumerator();
		}

		IEnumerator<IGrouping<object, IPublishedContent>> IEnumerable<IGrouping<object, IPublishedContent>>.GetEnumerator()
		{
			return Inner.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
        {
            return Inner.GetEnumerator();
        }
    }
}
