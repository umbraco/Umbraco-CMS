using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine.LuceneEngine.Config;
using Lucene.Net.Index;
using Umbraco.Core;
using UmbracoExamine.Config;
using umbraco.cms.businesslogic.member;
using Examine.LuceneEngine;
using System.Collections.Generic;
using Examine;
using System.IO;
using UmbracoExamine.DataServices;
using Lucene.Net.Analysis;
using UmbracoExamine.LocalStorage;

namespace UmbracoExamine
{
	/// <summary>
    /// 
    /// </summary>
    public class UmbracoMemberIndexer : UmbracoContentIndexer
    {

        private readonly LocalTempStorageIndexer _localTempStorageHelper = new LocalTempStorageIndexer();

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
	    /// Set up all properties for the indexer based on configuration information specified. This will ensure that
	    /// all of the folders required by the indexer are created and exist. This will also create an instruction
	    /// file declaring the computer name that is part taking in the indexing. This file will then be used to
	    /// determine the master indexer machine in a load balanced environment (if one exists).
	    /// </summary>
	    /// <param name="name">The friendly name of the provider.</param>
	    /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
	    /// <exception cref="T:System.ArgumentNullException">
	    /// The name of the provider is null.
	    /// </exception>
	    /// <exception cref="T:System.ArgumentException">
	    /// The name of the provider has a length of zero.
	    /// </exception>
	    /// <exception cref="T:System.InvalidOperationException">
	    /// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
	    /// </exception>
	    public override void Initialize(string name, NameValueCollection config)
	    {
	        base.Initialize(name, config);

            if (config != null && config["useTempStorage"] != null)
            {
                //Use the temp storage directory which will store the index in the local/codegen folder, this is useful
                // for websites that are running from a remove file server and file IO latency becomes an issue
                var attemptUseTempStorage = config["useTempStorage"].TryConvertTo<bool>();
                if (attemptUseTempStorage)
                {
                    var indexSet = IndexSets.Instance.Sets[IndexSetName];
                    var configuredPath = indexSet.IndexPath;

                    _localTempStorageHelper.Initialize(config, configuredPath, base.GetLuceneDirectory(), IndexingAnalyzer);
                }
            }
	    }

        public override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            //if temp local storage is configured use that, otherwise return the default
            if (_localTempStorageHelper.LuceneDirectory != null)
            {
                return _localTempStorageHelper.LuceneDirectory;
            }

            return base.GetLuceneDirectory();

        }

        public override IndexWriter GetIndexWriter()
        {
            //if temp local storage is configured use that, otherwise return the default
            if (_localTempStorageHelper.LuceneDirectory != null)
            {
                return new IndexWriter(GetLuceneDirectory(), IndexingAnalyzer,
                    //create the writer with the snapshotter, though that won't make too much a difference because we are not keeping the writer open unless using nrt
                    // which we are not currently.
                    _localTempStorageHelper.Snapshotter,
                    IndexWriter.MaxFieldLength.UNLIMITED);
            }

            return base.GetIndexWriter();
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
