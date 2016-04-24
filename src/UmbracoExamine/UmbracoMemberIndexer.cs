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
using IContentService = Umbraco.Core.Services.IContentService;
using IMediaService = Umbraco.Core.Services.IMediaService;

namespace UmbracoExamine
{
   
    /// <summary>
    /// Custom indexer for members
    /// </summary>
    public class UmbracoMemberIndexer : BaseUmbracoIndexer
    {
        private readonly IMemberService _memberService;
        private readonly IDataTypeService _dataTypeService;

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoMemberIndexer()
            : base()
        {
            _memberService = ApplicationContext.Current.Services.MemberService;
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
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
        /// <param name="dataTypeService"></param>
        public UmbracoMemberIndexer(
            IEnumerable<FieldDefinition> fieldDefinitions,
            Directory luceneDirectory,                      
            Analyzer analyzer,
            ProfilingLogger profilingLogger,
            IValueSetValidator validator,
            IMemberService memberService,
            IDataTypeService dataTypeService) :
            base(fieldDefinitions, luceneDirectory, analyzer, profilingLogger, validator)
        {            
            if (memberService == null) throw new ArgumentNullException("memberService");
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            _memberService = memberService;
            _dataTypeService = dataTypeService;
        }

        /// <summary>
        /// Ensures that the'_searchEmail' is added to the user fields so that it is indexed - without having to modify the config
        /// </summary>
        /// <param name="indexSet"></param>
        /// <returns></returns>
        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            //TODO: This is only required for config based index delcaration - We need to change this!

            var indexerData = base.GetIndexerData(indexSet);

            if (CanInitialize())
            {
                //If the fields are missing a custom _searchEmail, then add it

                if (indexerData.UserFields.Any(x => x.Name == "_searchEmail") == false)
                {
                    var field = new IndexField {Name = "_searchEmail"};
                    var policy = IndexFieldPolicies.FirstOrDefault(x => x.Name == "_searchEmail");
                    if (policy != null)
                    {
                        field.Type = policy.Type;
                        field.EnableSorting = policy.EnableSorting;
                    }

                    return new IndexCriteria(
                        indexerData.StandardFields,
                        indexerData.UserFields.Concat(new[] {field}),
                        indexerData.IncludeNodeTypes,
                        indexerData.ExcludeNodeTypes,
                        indexerData.ParentNodeId
                        );
                }
            }

	        return indexerData;
        }

        /// <summary>
        /// The supported types for this indexer
        /// </summary>
        protected override IEnumerable<string> SupportedTypes
        {
            get
            {
                return new string[] { IndexTypes.Member };
            }
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

            if (IndexerData.IncludeNodeTypes.Any())
            {
                //if there are specific node types then just index those
                foreach (var nodeType in IndexerData.IncludeNodeTypes)
                {
                    do
                    {
                        long total;
                        members = _memberService.GetAll(pageIndex, pageSize, out total, "LoginName", Direction.Ascending, nodeType).ToArray();

                        AddNodesToIndex(GetSerializedMembers(members), type);

                        pageIndex++;
                    } while (members.Length == pageSize);
                }
            }
            else
            {
                //no node types specified, do all members
                do
                {
                    int total;
                    members = _memberService.GetAll(pageIndex, pageSize, out total).ToArray();

                    AddNodesToIndex(GetSerializedMembers(members), type);

                    pageIndex++;
                } while (members.Length == pageSize);
	        }
	    }

        private IEnumerable<XElement> GetSerializedMembers(IEnumerable<IMember> members)
        {
            var serializer = new EntityXmlSerializer();
            return members.Select(member => serializer.Serialize(_dataTypeService, member));
        }

        protected override void OnTransformingIndexValues(TransformingIndexDataEventArgs e)
        {
            base.OnTransformingIndexValues(e);

            //adds the special path property to the index
            //fields.Add("__key", allValuesForIndexing["__key"]);
        }

        /// <summary>
        /// Add the special __key and _searchEmail fields
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {
            base.OnGatheringNodeData(e);

            if (e.Node.Attribute("key") != null)
            {
                if (e.Fields.ContainsKey("__key") == false)
                    e.Fields.Add("__key", e.Node.Attribute("key").Value);
            }

            if (e.Node.Attribute("email") != null)
            {
                //NOTE: the single underscore = it's not a 'special' field which means it will be indexed normally
                if (e.Fields.ContainsKey("_searchEmail") == false)
                    e.Fields.Add("_searchEmail", e.Node.Attribute("email").Value.Replace(".", " ").Replace("@", " "));
            }
            
            if (e.Fields.ContainsKey(IconFieldName) == false)
                e.Fields.Add(IconFieldName, (string)e.Node.Attribute("icon"));
        }
    }
}
