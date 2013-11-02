using System.Collections;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine.LuceneEngine.Config;
using UmbracoExamine.Config;
using umbraco.cms.businesslogic.member;
using Examine.LuceneEngine;
using System.Collections.Generic;
using Examine;
using System.IO;
using UmbracoExamine.DataServices;
using Lucene.Net.Analysis;

namespace UmbracoExamine
{
	/// <summary>
    /// 
    /// </summary>
    public class UmbracoMemberIndexer : UmbracoContentIndexer
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoMemberIndexer()
            : base() { }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
		[SecuritySafeCritical]
		public UmbracoMemberIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async) { }

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

		[SecuritySafeCritical]
        protected override XDocument GetXDocument(string xPath, string type)
        {
            if (type == IndexTypes.Member)
            {
                Member[] rootMembers = Member.GetAll;
                var xmlMember = XDocument.Parse("<member></member>");
                foreach (Member member in rootMembers)
                {
                    xmlMember.Root.Add(GetMemberItem(member.Id));
                }
                var result = ((IEnumerable)xmlMember.XPathEvaluate(xPath)).Cast<XElement>();
                return result.ToXDocument(); 
            }

            return null;
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

		[SecuritySafeCritical]
        private static XElement GetMemberItem(int nodeId)
        {
			//TODO: Change this so that it is not using the LegacyLibrary, just serialize manually!
            var nodes = LegacyLibrary.GetMember(nodeId);
            return XElement.Parse(nodes.Current.OuterXml);
        }
    }
}
