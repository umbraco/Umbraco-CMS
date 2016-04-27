using System.Linq;
using Examine;
using Examine.LuceneEngine.Config;
using Umbraco.Core.Services;

namespace UmbracoExamine
{
    internal static class LegacyExtensions
    {

        private static readonly object Locker = new object();
      
        public static IIndexCriteria ToIndexCriteria(this IndexSet set, IContentTypeService contentTypeService)
        {
            if (set.IndexUserFields.Count == 0)
            {
                lock (Locker)
                {
                    //we need to add all user fields to the collection if it is empty (this is the default if none are specified)
                    var userFields = contentTypeService.GetAllPropertyTypeAliases();
                    foreach (var u in userFields)
                    {
                        set.IndexUserFields.Add(new IndexField() { Name = u });
                    }
                }
            }

            if (set.IndexAttributeFields.Count == 0)
            {
                lock (Locker)
                {
                    //we need to add all system fields to the collection if it is empty (this is the default if none are specified)
                    var sysFields = BaseUmbracoIndexer.IndexFieldPolicies.Select(x => x.Name);
                    foreach (var s in sysFields)
                    {
                        set.IndexAttributeFields.Add(new IndexField() { Name = s });
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

    }
}