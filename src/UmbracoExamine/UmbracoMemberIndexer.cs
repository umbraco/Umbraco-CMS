using System;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using Examine.LuceneEngine.Config;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using System.Collections.Generic;
using Examine;
using System.IO;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Umbraco.Core.Logging;
using Directory = Lucene.Net.Store.Directory;

namespace UmbracoExamine
{

    /// <summary>
    /// Custom indexer for members
    /// </summary>
    public class UmbracoMemberIndexer : BaseUmbracoIndexer
    {
        private readonly IMemberService _memberService;

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoMemberIndexer()
            : base()
        {
            _memberService = ApplicationContext.Current.Services.MemberService;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="fieldDefinitions"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="profilingLogger"></param>
        /// <param name="validator"></param>
        /// <param name="memberService"></param>        
        /// <param name="analyzer"></param>
        public UmbracoMemberIndexer(
            IEnumerable<FieldDefinition> fieldDefinitions,
            Directory luceneDirectory,                      
            Analyzer analyzer,
            ProfilingLogger profilingLogger,
            IValueSetValidator validator,
            IMemberService memberService) :
            base(fieldDefinitions, luceneDirectory, analyzer, profilingLogger, validator)
        {            
            if (memberService == null) throw new ArgumentNullException("memberService");
            _memberService = memberService;
        }

        /// <summary>
        /// Ensures that the'_searchEmail' is added to the user fields so that it is indexed - without having to modify the config
        /// </summary>
        /// <param name="indexSet"></param>
        /// <returns></returns>
        [Obsolete("IIndexCriteria is obsolete, this method is used only for configuration based indexes it is recommended to configure indexes on startup with code instead of config")]
        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            //TODO: This is only required for config based index delcaration - We need to change this!

            var indexerData = base.GetIndexerData(indexSet);

            if (CanInitialize())
            {
                //If the fields are missing a custom _searchEmail, then add it

                if (indexerData.UserFields.Any(x => x.Name == "_searchEmail") == false)
                {
                    var field = new IndexField { Name = "_searchEmail" };
                    var policy = IndexFieldPolicies.FirstOrDefault(x => x.Name == "_searchEmail");
                    if (policy != null)
                    {
                        field.Type = policy.Type;
                        field.EnableSorting = policy.EnableSorting;
                    }

                    return new IndexCriteria(
                        indexerData.StandardFields,
                        indexerData.UserFields.Concat(new[] { field }),
                        indexerData.IncludeNodeTypes,
                        indexerData.ExcludeNodeTypes,
                        indexerData.ParentNodeId
                        );
                }
            }

            return indexerData;
        }

        /// <summary>
        /// Overridden to ensure that the umbraco system field definitions are in place
        /// </summary>
        /// <param name="originalDefinitions"></param>
        /// <returns></returns>
        protected override IEnumerable<FieldDefinition> InitializeFieldDefinitions(IEnumerable<FieldDefinition> originalDefinitions)
        {
            var result = base.InitializeFieldDefinitions(originalDefinitions).ToList();
            result.Add(new FieldDefinition("__key", FieldDefinitionTypes.Raw));
            return result;
        }

        /// <summary>
        /// The supported types for this indexer
        /// </summary>
        protected override IEnumerable<string> SupportedTypes
        {
            get { return new[] {IndexTypes.Member}; }
        }

        /// <summary>
        /// Reindex all members
        /// </summary>
        /// <param name="type"></param>
        protected override void PerformIndexAll(string type)
        {
            //This only supports members
            if (SupportedTypes.Contains(type) == false)
                return;

            const int pageSize = 1000;
            var pageIndex = 0;

            IMember[] members;

            if (IndexerData != null && IndexerData.IncludeNodeTypes.Any())
            {
                //if there are specific node types then just index those
                foreach (var nodeType in IndexerData.IncludeNodeTypes)
                {
                    do
                    {
                        long total;
                        members = _memberService.GetAll(pageIndex, pageSize, out total, "LoginName", Direction.Ascending, true, null, nodeType).ToArray();

                        IndexItems(GetValueSets(members));

                        pageIndex++;
                    } while (members.Length == pageSize);
                }
            }
            else
            {
                //no node types specified, do all members
                do
                {
                    long total;
                    members = _memberService.GetAll(pageIndex, pageSize, out total).ToArray();

                    IndexItems(GetValueSets(members));

                    pageIndex++;
                } while (members.Length == pageSize);
            }
        }

        private IEnumerable<ValueSet> GetValueSets(IEnumerable<IMember> member)
        {
            foreach (var m in member)
            {
                var values = new Dictionary<string, object[]>
                {
                    {"icon", new object[] {m.ContentType.Icon}},
                    {"id", new object[] {m.Id}},
                    {"key", new object[] {m.Key}},
                    {"parentID", new object[] {m.Level > 1 ? m.ParentId : -1}},
                    {"level", new object[] {m.Level}},
                    {"creatorID", new object[] {m.CreatorId}},
                    {"sortOrder", new object[] {m.SortOrder}},
                    {"createDate", new object[] {m.CreateDate}},
                    {"updateDate", new object[] {m.UpdateDate}},
                    {"nodeName", new object[] {m.Name}},
                    {"path", new object[] {m.Path}},
                    {"nodeType", new object[] {m.ContentType.Id}},
                    {"loginName", new object[] {m.Username}},
                    {"email", new object[] {m.Email}},
                };

                foreach (var property in m.Properties.Where(p => p != null && p.Value != null && p.Value.ToString().IsNullOrWhiteSpace() == false))
                {
                    values.Add(property.Alias, new[] { property.Value });
                }

                var vs = new ValueSet(m.Id, IndexTypes.Content, m.ContentType.Alias, values);

                yield return vs;
            }
        }

        protected override void OnTransformingIndexValues(TransformingIndexDataEventArgs e)
        {
            base.OnTransformingIndexValues(e);

            if (e.OriginalValues.ContainsKey("key") && e.IndexItem.ValueSet.Values.ContainsKey("__key") == false)
            {
                e.IndexItem.ValueSet.Values["__key"] = new List<object> {e.OriginalValues["key"]};
            }
            if (e.OriginalValues.ContainsKey("email") && e.IndexItem.ValueSet.Values.ContainsKey("_searchEmail") == false)
            {
                e.IndexItem.ValueSet.Values["_searchEmail"] = new List<object> { e.OriginalValues["email"].ToString().Replace(".", " ").Replace("@", " ") };
            }

        }

    }
}
