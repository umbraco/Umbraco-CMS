using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine;
using Examine.LuceneEngine;
using UmbracoExamine.DataServices;
using Examine.LuceneEngine.Config;

namespace UmbracoExamine.Config
{
    /// <summary>
    /// Extension methods for IndexSet
    /// </summary>
    public static class IndexSetExtensions
    {
        internal static IIndexCriteria ToIndexCriteria(this IndexSet set,
            IDataService svc,
            StaticFieldCollection indexFieldPolicies,
            IEnumerable<IndexField> additionalUserFields = null)
        {
            return new LazyIndexCriteria(set, svc, indexFieldPolicies, additionalUserFields);
        }

        /// <summary>
        /// Convert the indexset to indexerdata.
        /// This detects if there are no user/system fields specified and if not, uses the data service to look them 
        /// up and update the in memory IndexSet.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="svc"></param>
        /// <returns></returns>
        public static IIndexCriteria ToIndexCriteria(this IndexSet set, IDataService svc)
        {
            return set.ToIndexCriteria(svc, new StaticFieldCollection());
        }      
      
    }
}
