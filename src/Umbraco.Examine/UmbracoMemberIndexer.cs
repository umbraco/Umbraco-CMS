using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using System.Collections.Generic;
using System.ComponentModel;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Examine
{

    /// <summary>
    /// Custom indexer for members
    /// </summary>
    public class UmbracoMemberIndexer : UmbracoExamineIndexer
    {
        private readonly IMemberService _memberService;

        /// <summary>
        /// Constructor for config/provider based indexes
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public UmbracoMemberIndexer()
        {
            _memberService = Current.Services.MemberService;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="profilingLogger"></param>
        /// <param name="validator"></param>
        /// <param name="memberService"></param>
        /// <param name="analyzer"></param>
        public UmbracoMemberIndexer(
            string name, 
            IEnumerable<FieldDefinition> fieldDefinitions,
            Directory luceneDirectory,
            Analyzer analyzer,
            ProfilingLogger profilingLogger,
            IValueSetValidator validator,
            IMemberService memberService) :
            base(name, fieldDefinitions, luceneDirectory, analyzer, profilingLogger, validator)
        {
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
        }


        /// <summary>
        /// Overridden to ensure that the umbraco system field definitions are in place
        /// </summary>
        /// <param name="x"></param>
        /// <param name="indexValueTypesFactory"></param>
        /// <returns></returns>
        protected override FieldValueTypeCollection CreateFieldValueTypes(Directory x, IReadOnlyDictionary<string, Func<string, IIndexValueType>> indexValueTypesFactory = null)
        {
            var keyDef = new FieldDefinition("__key", FieldDefinitionTypes.Raw);
            FieldDefinitionCollection.TryAdd(keyDef.Name, keyDef);

            return base.CreateFieldValueTypes(x, indexValueTypesFactory);
        }

        /// <inheritdoc />
        protected override IEnumerable<string> SupportedTypes => new[] {IndexTypes.Member};

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

            if (ConfigIndexCriteria != null && ConfigIndexCriteria.IncludeItemTypes.Any())
            {
                //if there are specific node types then just index those
                foreach (var nodeType in ConfigIndexCriteria.IncludeItemTypes)
                {
                    do
                    {
                        members = _memberService.GetAll(pageIndex, pageSize, out _, "LoginName", Direction.Ascending, true, null, nodeType).ToArray();

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
                    members = _memberService.GetAll(pageIndex, pageSize, out _).ToArray();

                    IndexItems(GetValueSets(members));

                    pageIndex++;
                } while (members.Length == pageSize);
            }
        }

        public static IEnumerable<ValueSet> GetValueSets(params IMember[] members)
        {
            foreach (var m in members)
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

                foreach (var property in m.Properties)
                {
                    //only add the value if its not null or empty (we'll check for string explicitly here too)
                    var val = property.GetValue();
                    switch (val)
                    {
                        case null:
                            continue;
                        case string strVal when strVal.IsNullOrWhiteSpace() == false:
                            values.Add(property.Alias, new[] { val });
                            break;
                        default:
                            values.Add(property.Alias, new[] { val });
                            break;
                    }
                }

                var vs = new ValueSet(m.Id.ToInvariantString(), IndexTypes.Content, m.ContentType.Alias, values);

                yield return vs;
            }
        }

        /// <summary>
        /// Ensure some custom values are added to the index
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTransformingIndexValues(IndexingItemEventArgs e)
        {
            base.OnTransformingIndexValues(e);

            if (e.IndexItem.ValueSet.Values.TryGetValue("key", out var key) && e.IndexItem.ValueSet.Values.ContainsKey("__key") == false)
            {
                //double __ prefix means it will be indexed as culture invariant
                e.IndexItem.ValueSet.Values["__key"] = key;
            }

            if (e.IndexItem.ValueSet.Values.TryGetValue("email", out var email) && e.IndexItem.ValueSet.Values.ContainsKey("_searchEmail") == false)
            {
                if (email.Count > 0)
                {
                    //will be indexed as full text (the default anaylyzer)
                    e.IndexItem.ValueSet.Values["_searchEmail"] = new List<object> { email[0]?.ToString().Replace(".", " ").Replace("@", " ") };
                }
                
            }
        }

    }
}
