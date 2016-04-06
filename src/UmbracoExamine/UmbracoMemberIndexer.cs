using System;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using Examine.LuceneEngine.Config;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using UmbracoExamine.Config;
using System.Collections.Generic;
using Examine;
using System.IO;
using UmbracoExamine.DataServices;
using Lucene.Net.Analysis;

namespace UmbracoExamine
{

    /// <summary>
    /// Custom indexer for members
    /// </summary>
    public class UmbracoMemberIndexer : UmbracoContentIndexer
    {

        private readonly IMemberService _memberService;
        private readonly IDataTypeService _dataTypeService;

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoMemberIndexer() : base()
        {
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
            _memberService = ApplicationContext.Current.Services.MemberService;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        [Obsolete("Use the overload that specifies the Umbraco services")]
        public UmbracoMemberIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async)
        {
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
            _memberService = ApplicationContext.Current.Services.MemberService;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="memberService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        public UmbracoMemberIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService,
              IDataTypeService dataTypeService,
              IMemberService memberService,
              Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async)
        {
            _dataTypeService = dataTypeService;
            _memberService = memberService;
        }



        /// <summary>
        /// Ensures that the'_searchEmail' is added to the user fields so that it is indexed - without having to modify the config
        /// </summary>
        /// <param name="indexSet"></param>
        /// <returns></returns>
        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
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
                        members = _memberService.GetAll(pageIndex, pageSize, out total, "LoginName", Direction.Ascending, true, null, nodeType).ToArray();

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

        protected override XDocument GetXDocument(string xPath, string type)
        {
            throw new NotSupportedException();
        }

        protected override Dictionary<string, string> GetSpecialFieldsToIndex(Dictionary<string, string> allValuesForIndexing)
        {
            var fields = base.GetSpecialFieldsToIndex(allValuesForIndexing);

            //adds the special path property to the index
            fields.Add("__key", allValuesForIndexing["__key"]);

            return fields;

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

        private static XElement GetMemberItem(int nodeId)
        {
            //TODO: Change this so that it is not using the LegacyLibrary, just serialize manually!
            var nodes = LegacyLibrary.GetMember(nodeId);
            return XElement.Parse(nodes.Current.OuterXml);
        }
    }
}
