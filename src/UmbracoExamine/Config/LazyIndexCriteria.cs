using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Config;
using UmbracoExamine.DataServices;

namespace UmbracoExamine.Config
{
    internal class LazyIndexCriteria : IIndexCriteria
    {
        public LazyIndexCriteria(
            IndexSet set,
            IDataService svc,
            StaticFieldCollection indexFieldPolicies,
            IEnumerable<IndexField> additionalUserFields = null)
        {
            if (set == null) throw new ArgumentNullException("set");
            if (indexFieldPolicies == null) throw new ArgumentNullException("indexFieldPolicies");
            if (svc == null) throw new ArgumentNullException("svc");
           
            _lazyCriteria = new Lazy<IIndexCriteria>(() =>
            {
                var attributeFields = set.IndexAttributeFields.Cast<IIndexField>().ToArray();
                var userFields = set.IndexUserFields.Cast<IIndexField>().ToList();
                var includeNodeTypes = set.IncludeNodeTypes.Cast<IIndexField>().Select(x => x.Name).ToArray();
                var excludeNodeTypes = set.ExcludeNodeTypes.Cast<IIndexField>().Select(x => x.Name).ToArray();
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

                        StaticField policy;
                        if (indexFieldPolicies.TryGetValue(u, out policy))
                        {
                            field.Type = policy.Type;
                            field.EnableSorting = policy.EnableSorting;
                        }
                        fields.Add(field);
                    }
                    userFields = fields.ToList();
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

                        StaticField policy;
                        if (indexFieldPolicies.TryGetValue(s, out policy))
                        {
                            field.Type = policy.Type;
                            field.EnableSorting = policy.EnableSorting;
                        }
                        fields.Add(field);
                    }
                    attributeFields = fields.ToArray();
                }

                //merge in the additional user fields if any are defined
                if (additionalUserFields != null)
                {
                    foreach (var field in additionalUserFields)
                    {
                        var f = field; //copy local
                        if (userFields.Any(x => x.Name == f.Name) == false)
                        {
                            userFields.Add(f);
                        }
                    }
                    
                }

                return new IndexCriteria(
                    attributeFields,
                    userFields,
                    includeNodeTypes,
                    excludeNodeTypes,
                    parentId);
            });
        }

        private readonly Lazy<IIndexCriteria> _lazyCriteria;

        public IEnumerable<string> ExcludeNodeTypes
        {
            get { return _lazyCriteria.Value.ExcludeNodeTypes; }
        }

        public IEnumerable<string> IncludeNodeTypes
        {
            get { return _lazyCriteria.Value.IncludeNodeTypes; }
        }

        public int? ParentNodeId
        {
            get { return _lazyCriteria.Value.ParentNodeId; }
        }

        public IEnumerable<IIndexField> StandardFields
        {
            get { return _lazyCriteria.Value.StandardFields; }
        }

        public IEnumerable<IIndexField> UserFields
        {
            get { return _lazyCriteria.Value.UserFields; }
        }
    }
}