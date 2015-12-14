using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Umbraco.Core;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using UmbracoExamine.DataServices;
using Examine;
using System.IO;
using System.Xml.Linq;
using Lucene.Net.Store;
using UmbracoExamine.LocalStorage;

namespace UmbracoExamine
{

    /// <summary>
    /// An abstract provider containing the basic functionality to be able to query against
    /// Umbraco data.
    /// </summary>
    public abstract class BaseUmbracoIndexer : LuceneIndexer
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        protected BaseUmbracoIndexer()
            : base()
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        protected BaseUmbracoIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, analyzer, async)
        {
            DataService = dataService;
        }

		protected BaseUmbracoIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, IDataService dataService, Analyzer analyzer, bool async)
			: base(indexerData, luceneDirectory, analyzer, async)
		{
			DataService = dataService;
		}

        #endregion

        /// <summary>
        /// Used for unit tests
        /// </summary>
        internal static bool? DisableInitializationCheck = null;
        private readonly LocalTempStorageIndexer _localTempStorageIndexer = new LocalTempStorageIndexer();
        private BaseLuceneSearcher _internalTempStorageSearcher = null;

        #region Properties

        public bool UseTempStorage
        {
            get { return _localTempStorageIndexer.LuceneDirectory != null; }
        }

        public string TempStorageLocation
        {
            get
            {
                if (UseTempStorage == false) return string.Empty;
                return _localTempStorageIndexer.TempPath;
            }
        }

        /// <summary>
        /// If true, the IndexingActionHandler will be run to keep the default index up to date.
        /// </summary>
        public bool EnableDefaultEventHandler { get; protected set; }

        /// <summary>
        /// Determines if the manager will call the indexing methods when content is saved or deleted as
        /// opposed to cache being updated.
        /// </summary>
        public bool SupportUnpublishedContent { get; protected set; }

        /// <summary>
        /// The data service used for retreiving and submitting data to the cms
        /// </summary>
        public IDataService DataService { get; protected internal set; }

        /// <summary>
        /// the supported indexable types
        /// </summary>
        protected abstract IEnumerable<string> SupportedTypes { get; }

        #endregion

        #region Initialize


        /// <summary>
        /// Setup the properties for the indexer from the provider settings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {

            if (config["dataService"] != null && !string.IsNullOrEmpty(config["dataService"]))
            {
                //this should be a fully qualified type
                var serviceType = Type.GetType(config["dataService"]);
                DataService = (IDataService)Activator.CreateInstance(serviceType);
            }
            else if (DataService == null)
            {
                //By default, we will be using the UmbracoDataService
                //generally this would only need to be set differently for unit testing
                DataService = new UmbracoDataService();
            }

            DataService.LogService.LogLevel = LoggingLevel.Normal;

            if (config["logLevel"] != null && !string.IsNullOrEmpty(config["logLevel"]))
            {
                try
                {
                    var logLevel = (LoggingLevel)Enum.Parse(typeof(LoggingLevel), config["logLevel"]);
                    DataService.LogService.LogLevel = logLevel;
                }
                catch (ArgumentException)
                {                    
                    //FAILED
                    DataService.LogService.LogLevel = LoggingLevel.Normal;
                }
            }

            DataService.LogService.ProviderName = name;

            EnableDefaultEventHandler = true; //set to true by default
            bool enabled;
            if (bool.TryParse(config["enableDefaultEventHandler"], out enabled))
            {
                EnableDefaultEventHandler = enabled;
            }         

            DataService.LogService.AddVerboseLog(-1, string.Format("{0} indexer initializing", name));               

            base.Initialize(name, config);

            if (config["useTempStorage"] != null)
            {
                var fsDir = base.GetLuceneDirectory() as FSDirectory;
                if (fsDir != null)
                {
                    //Use the temp storage directory which will store the index in the local/codegen folder, this is useful
                    // for websites that are running from a remove file server and file IO latency becomes an issue
                    var attemptUseTempStorage = config["useTempStorage"].TryConvertTo<LocalStorageType>();
                    if (attemptUseTempStorage)
                    {

                        var indexSet = IndexSets.Instance.Sets[IndexSetName];
                        var configuredPath = indexSet.IndexPath;

                        _localTempStorageIndexer.Initialize(config, configuredPath, fsDir, IndexingAnalyzer, attemptUseTempStorage.Result);
                    }
                }
               
            }
        }

        #endregion

        /// <summary>
        /// Used to aquire the internal searcher
        /// </summary>
        private readonly object _internalSearcherLocker = new object();

        protected override BaseSearchProvider InternalSearcher
        {
            get
            {
                //if temp local storage is configured use that, otherwise return the default
                if (UseTempStorage)
                {
                    if (_internalTempStorageSearcher == null)
                    {
                        lock (_internalSearcherLocker)
                        {
                            if (_internalTempStorageSearcher == null)
                            {
                                _internalTempStorageSearcher = new LuceneSearcher(GetIndexWriter(), IndexingAnalyzer);
                            }
                        }
                    }
                    return _internalTempStorageSearcher;
                }

                return base.InternalSearcher;
            }
        }
        
        public override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            //if temp local storage is configured use that, otherwise return the default
            if (UseTempStorage)
            {
                return _localTempStorageIndexer.LuceneDirectory;
            }

            return base.GetLuceneDirectory();

        }

        protected override IndexWriter CreateIndexWriter()
        {
            //if temp local storage is configured use that, otherwise return the default
            if (UseTempStorage)
            {
                var directory = GetLuceneDirectory();
                return new IndexWriter(GetLuceneDirectory(), IndexingAnalyzer,
                    DeletePolicyTracker.Current.GetPolicy(directory),
                    IndexWriter.MaxFieldLength.UNLIMITED);
            }

            return base.CreateIndexWriter();
        }

        ///// <summary>
        ///// Override to check if we can actually initialize.
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>
        ///// This check is required since the base examine lib will try to check this method on app startup. If the app
        ///// is not ready then we need to deal with it otherwise the base class will throw exceptions since we've bypassed initialization.
        ///// </remarks>
        //public override bool IndexExists()
        //{
        //    return base.IndexExists();
        //}

        /// <summary>
        /// override to check if we can actually initialize. 
        /// </summary>
        /// <remarks>
        /// This check is required since the base examine lib will try to rebuild on startup
        /// </remarks>
        public override void RebuildIndex()
        {
            if (CanInitialize())
            {
                base.RebuildIndex();
            }
        }

        /// <summary>
        /// override to check if we can actually initialize. 
        /// </summary>
        /// <remarks>
        /// This check is required since the base examine lib will try to rebuild on startup
        /// </remarks>
        public override void IndexAll(string type)
        {
            if (CanInitialize())
            {
                base.IndexAll(type);
            }
        }

        public override void ReIndexNode(XElement node, string type)
        {
            if (CanInitialize())
            {
                if (!SupportedTypes.Contains(type))
                    return;

                base.ReIndexNode(node, type);
            }
        }

        /// <summary>
        /// override to check if we can actually initialize. 
        /// </summary>
        /// <remarks>
        /// This check is required since the base examine lib will try to rebuild on startup
        /// </remarks>
        public override void DeleteFromIndex(string nodeId)
        {
            if (CanInitialize())
            {
                base.DeleteFromIndex(nodeId);
            }            
        }

        #region Protected

        /// <summary>
        /// Returns true if the Umbraco application is in a state that we can initialize the examine indexes
        /// </summary>
        /// <returns></returns>
        protected bool CanInitialize()
        {
            //check the DisableInitializationCheck and ensure that it is not set to true
            if (!DisableInitializationCheck.HasValue || !DisableInitializationCheck.Value)
            {
                //We need to check if we actually can initialize, if not then don't continue
                if (ApplicationContext.Current == null
                    || !ApplicationContext.Current.IsConfigured
                    || !ApplicationContext.Current.DatabaseContext.IsDatabaseConfigured)
                {
                    return false;
                }    
            }
            
            return true;
        }

        /// <summary>
        /// Ensures that the node being indexed is of a correct type and is a descendent of the parent id specified.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override bool ValidateDocument(XElement node)
        {
            //check if this document is a descendent of the parent
            if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
                if (!((string)node.Attribute("path")).Contains("," + IndexerData.ParentNodeId.Value.ToString() + ","))
                    return false;

            return base.ValidateDocument(node);
        }

        /// <summary>
        /// Reindexes all supported types
        /// </summary>
        protected override void PerformIndexRebuild()
        {
            foreach (var t in SupportedTypes)
            {
                IndexAll(t);
            }
        }
        
        /// <summary>
        /// Builds an xpath statement to query against Umbraco data for the index type specified, then
        /// initiates the re-indexing of the data matched.
        /// </summary>
        /// <param name="type"></param>
        protected override void PerformIndexAll(string type)
        {
            //NOTE: the logic below is ONLY used for published content, for media and members and non-published content, this method is overridden
            // and we query directly against the umbraco service layer.

            if (!SupportedTypes.Contains(type))
                return;

            var xPath = "//*[(number(@id) > 0 and (@isDoc or @nodeTypeAlias)){0}]"; //we'll add more filters to this below if needed

            var sb = new StringBuilder();

            //create the xpath statement to match node type aliases if specified
            if (IndexerData.IncludeNodeTypes.Any())
            {
                sb.Append("(");
                foreach (var field in IndexerData.IncludeNodeTypes)
                {
                    //this can be used across both schemas
                    const string nodeTypeAlias = "(@nodeTypeAlias='{0}' or (count(@nodeTypeAlias)=0 and name()='{0}'))";

                    sb.Append(string.Format(nodeTypeAlias, field));
                    sb.Append(" or ");
                }
                sb.Remove(sb.Length - 4, 4); //remove last " or "
                sb.Append(")");
            }

            //create the xpath statement to match all children of the current node.
            if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
            {
                if (sb.Length > 0)
                    sb.Append(" and ");
                sb.Append("(");
                sb.Append("contains(@path, '," + IndexerData.ParentNodeId.Value + ",')"); //if the path contains comma - id - comma then the nodes must be a child
                sb.Append(")");
            }

            //create the full xpath statement to match the appropriate nodes. If there is a filter
            //then apply it, otherwise just select all nodes.
            var filter = sb.ToString();
            xPath = string.Format(xPath, filter.Length > 0 ? " and " + filter : "");

            //raise the event and set the xpath statement to the value returned
            var args = new IndexingNodesEventArgs(IndexerData, xPath, type);
            OnNodesIndexing(args);
            if (args.Cancel)
            {
                return;
            }

            xPath = args.XPath;

            DataService.LogService.AddVerboseLog(-1, string.Format("({0}) PerformIndexAll with XPATH: {1}", this.Name, xPath));

            AddNodesToIndex(xPath, type);
        }

        /// <summary>
        /// Returns an XDocument for the entire tree stored for the IndexType specified.
        /// </summary>
        /// <param name="xPath">The xpath to the node.</param>
        /// <param name="type">The type of data to request from the data service.</param>
        /// <returns>Either the Content or Media xml. If the type is not of those specified null is returned</returns>
        protected virtual XDocument GetXDocument(string xPath, string type)
        {
            //TODO: We need to get rid of this! it will now only ever be called for published content - but we're keeping the other
            // logic here for backwards compatibility in case inheritors are calling this for some reason.

            if (type == IndexTypes.Content)
            {
                if (this.SupportUnpublishedContent)
                {
                    return DataService.ContentService.GetLatestContentByXPath(xPath);
                }
                else
                {
                    return DataService.ContentService.GetPublishedContentByXPath(xPath);
                }
            }
            else if (type == IndexTypes.Media)
            {
                return DataService.MediaService.GetLatestMediaByXpath(xPath);
            }
            return null;
        }
        #endregion

        #region Private
        /// <summary>
        /// Adds all nodes with the given xPath root.
        /// </summary>
        /// <param name="xPath">The x path.</param>
        /// <param name="type">The type.</param>
        private void AddNodesToIndex(string xPath, string type)
        {
            // Get all the nodes of nodeTypeAlias == nodeTypeAlias
            XDocument xDoc = GetXDocument(xPath, type);
            if (xDoc != null)
            {
                var rootNode = xDoc.Root;

                AddNodesToIndex(rootNode.Elements(), type);
            }

        }

        

        #endregion
    }
}
