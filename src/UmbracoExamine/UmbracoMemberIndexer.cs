using System;
using System.Collections;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine.LuceneEngine.Config;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using UmbracoExamine.Config;
using Examine.LuceneEngine;
using System.Collections.Generic;
using Examine;
using System.IO;
using UmbracoExamine.DataServices;
using Lucene.Net.Analysis;
using Member = umbraco.cms.businesslogic.member.Member;

namespace UmbracoExamine
{
	/// <summary>
    /// 
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
            if (CanInitialize())
            {           
                var searchableEmail = indexSet.IndexUserFields["_searchEmail"];
                if (searchableEmail == null)
                {
                    indexSet.IndexUserFields.Add(new IndexField
                    {
                        Name = "_searchEmail"
                    });
                }
                return indexSet.ToIndexCriteria(DataService, IndexFieldPolicies);
            }

            return base.GetIndexerData(indexSet);
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

            //Re-index all members in batches of 5000
            IMember[] members;
            const int pageSize = 5000;
            var pageIndex = 0;

	        if (IndexerData.IncludeNodeTypes.Any())
	        {
                //if there are specific node types then just index those
                foreach (var nodeType in IndexerData.IncludeNodeTypes)
	            {
                    do
                    {
                        int total;
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
            foreach (var member in members)
            {
                yield return serializer.Serialize(_dataTypeService, member);
            }
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
        }

        private static XElement GetMemberItem(int nodeId)
        {
			//TODO: Change this so that it is not using the LegacyLibrary, just serialize manually!
            var nodes = LegacyLibrary.GetMember(nodeId);
            return XElement.Parse(nodes.Current.OuterXml);
        }
    }
}
