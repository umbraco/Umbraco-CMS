using System;
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

        private static readonly object Locker = new object();

        internal static IIndexCriteria ToIndexCriteria(this IndexSet set, IDataService svc,
            IEnumerable<StaticField> indexFieldPolicies)
        {
            if (set.IndexUserFields.Count == 0)
            {
                lock (Locker)
                {
                    //we need to add all user fields to the collection if it is empty (this is the default if none are specified)
                    var userFields = svc.ContentService.GetAllUserPropertyNames();
                    foreach (var u in userFields)
                    {
                        var field = new IndexField() {Name = u};
                        var policy = indexFieldPolicies.FirstOrDefault(x => x.Name == u);
                        if (policy != null)
                        {
                            field.Type = policy.Type;
                            field.EnableSorting = policy.EnableSorting;
                        }
                        set.IndexUserFields.Add(field);
                    }
                }
            }

            if (set.IndexAttributeFields.Count == 0)
            {
                lock (Locker)
                {
                    //we need to add all system fields to the collection if it is empty (this is the default if none are specified)
                    var sysFields = svc.ContentService.GetAllSystemPropertyNames();
                    foreach (var s in sysFields)
                    {
                        var field = new IndexField() { Name = s };
                        var policy = indexFieldPolicies.FirstOrDefault(x => x.Name == s);
                        if (policy != null)
                        {
                            field.Type = policy.Type;
                            field.EnableSorting = policy.EnableSorting;
                        }
                        set.IndexAttributeFields.Add(field);
                    }
                }
            }

            return new IndexCriteria(
                set.IndexAttributeFields.Cast<IIndexField>().ToArray(),
                set.IndexUserFields.Cast<IIndexField>().ToArray(),
                set.IncludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                set.ExcludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                set.IndexParentId);
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
            return set.ToIndexCriteria(svc, Enumerable.Empty<StaticField>());
        }      
      
    }
}
