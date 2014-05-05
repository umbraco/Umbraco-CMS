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
        internal static IIndexCriteria ToIndexCriteria(this IndexSet set, IDataService svc,
            IEnumerable<StaticField> indexFieldPolicies)
        {

            var attributeFields = set.IndexAttributeFields.Cast<IIndexField>().ToArray();
            var userFields = set.IndexUserFields.Cast<IIndexField>().ToArray();
            var includeNodeTypes = set.IncludeNodeTypes.ToList().Select(x => x.Name).ToArray();
            var excludeNodeTypes = set.ExcludeNodeTypes.ToList().Select(x => x.Name).ToArray();
            var parentId = set.IndexParentId;

            //if there are no user fields defined, we'll populate them from the data source (include them all)
            if (set.IndexUserFields.Count == 0)
            {
                //we need to add all user fields to the collection if it is empty (this is the default if none are specified)
                var userProps = svc.ContentService.GetAllUserPropertyNames();
                var fields = new List<IIndexField>();
                foreach (var u in userProps)
                {
                    var field = new IndexField() { Name = u };
                    var policy = indexFieldPolicies.FirstOrDefault(x => x.Name == u);
                    if (policy != null)
                    {
                        field.Type = policy.Type;
                        field.EnableSorting = policy.EnableSorting;
                    }
                    fields.Add(field);
                }
                userFields = fields.ToArray();
            }

            //if there are no attribute fields defined, we'll populate them from the data source (include them all)
            if (set.IndexAttributeFields.Count == 0)
            {
                //we need to add all system fields to the collection if it is empty (this is the default if none are specified)
                var sysProps = svc.ContentService.GetAllSystemPropertyNames();
                var fields = new List<IIndexField>();
                foreach (var s in sysProps)
                {
                    var field = new IndexField() { Name = s };
                    var policy = indexFieldPolicies.FirstOrDefault(x => x.Name == s);
                    if (policy != null)
                    {
                        field.Type = policy.Type;
                        field.EnableSorting = policy.EnableSorting;
                    }
                    fields.Add(field);
                }
                attributeFields = fields.ToArray();
            }


            return new IndexCriteria(
                attributeFields,
                userFields,
                includeNodeTypes,
                excludeNodeTypes,
                parentId);
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
