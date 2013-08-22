using System.Collections;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using System.Xml.XPath;
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

        protected override Dictionary<string, string> GetDataToIndex(XElement node, string type)
        {
            var data = base.GetDataToIndex(node, type);

            if (data.ContainsKey("email"))
            {
                data.Add("__email",data["email"].Replace("."," ").Replace("@"," "));
            }

            return data;
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
