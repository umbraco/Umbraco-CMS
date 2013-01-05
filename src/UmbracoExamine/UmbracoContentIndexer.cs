using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Examine;
using Examine.Config;
using Examine.Providers;
using umbraco.cms.businesslogic;
using UmbracoExamine.DataServices;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using UmbracoExamine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using umbraco.BasePages;


namespace UmbracoExamine
{
    /// <summary>
    /// 
    /// </summary>
    public class UmbracoContentIndexer : BaseUmbracoIndexer
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoContentIndexer()
            : base() { }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
		[SecuritySafeCritical]
		public UmbracoContentIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async) { }

		/// <summary>
		/// Constructor to allow for creating an indexer at runtime
		/// </summary>
		/// <param name="indexerData"></param>
		/// <param name="luceneDirectory"></param>
		/// <param name="dataService"></param>
		/// <param name="analyzer"></param>
		/// <param name="async"></param>
		[SecuritySafeCritical]
		public UmbracoContentIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, IDataService dataService, Analyzer analyzer, bool async)
			: base(indexerData, luceneDirectory, dataService, analyzer, async) { }

        #endregion

        #region Constants & Fields

        /// <summary>
        /// Used to store the path of a content object
        /// </summary>
        public const string IndexPathFieldName = "__Path";
        public const string NodeTypeAliasFieldName = "__NodeTypeAlias";

        /// <summary>
        /// A type that defines the type of index for each Umbraco field (non user defined fields)
        /// Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
        /// for retreival after searching.
        /// </summary>
        internal static readonly Dictionary<string, FieldIndexTypes> IndexFieldPolicies
            = new Dictionary<string, FieldIndexTypes>()
            {
                { "id", FieldIndexTypes.NOT_ANALYZED},
                { "version", FieldIndexTypes.NOT_ANALYZED},
                { "parentID", FieldIndexTypes.NOT_ANALYZED},
                { "level", FieldIndexTypes.NOT_ANALYZED},
                { "writerID", FieldIndexTypes.NOT_ANALYZED},
                { "creatorID", FieldIndexTypes.NOT_ANALYZED},
                { "nodeType", FieldIndexTypes.NOT_ANALYZED},
                { "template", FieldIndexTypes.NOT_ANALYZED},
                { "sortOrder", FieldIndexTypes.NOT_ANALYZED},
                { "createDate", FieldIndexTypes.NOT_ANALYZED},
                { "updateDate", FieldIndexTypes.NOT_ANALYZED},
                { "nodeName", FieldIndexTypes.ANALYZED},
                { "urlName", FieldIndexTypes.NOT_ANALYZED},
                { "writerName", FieldIndexTypes.ANALYZED},
                { "creatorName", FieldIndexTypes.ANALYZED},
                { "nodeTypeAlias", FieldIndexTypes.ANALYZED},
                { "path", FieldIndexTypes.NOT_ANALYZED}
            };

        #endregion

        #region Initialize

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
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {

            //check if there's a flag specifying to support unpublished content,
            //if not, set to false;
            bool supportUnpublished;
            if (config["supportUnpublished"] != null && bool.TryParse(config["supportUnpublished"], out supportUnpublished))
                SupportUnpublishedContent = supportUnpublished;
            else
                SupportUnpublishedContent = false;


            //check if there's a flag specifying to support protected content,
            //if not, set to false;
            bool supportProtected;
            if (config["supportProtected"] != null && bool.TryParse(config["supportProtected"], out supportProtected))
                SupportProtectedContent = supportProtected;
            else
                SupportProtectedContent = false;


            base.Initialize(name, config);
        }

        #endregion

        #region Properties

        /// <summary>
        /// By default this is false, if set to true then the indexer will include indexing content that is flagged as publicly protected.
        /// This property is ignored if SupportUnpublishedContent is set to true.
        /// </summary>
        public bool SupportProtectedContent { get; protected internal set; }

        protected override IEnumerable<string> SupportedTypes
        {
            get
            {
                return new string[] { IndexTypes.Content, IndexTypes.Media };
            }
        }

        #endregion

        #region Event handlers

        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            DataService.LogService.AddErrorLog(e.NodeId, string.Format("{0},{1}, IndexSet: {2}", e.Message, e.InnerException != null ? e.InnerException.Message : "", this.IndexSetName));
            base.OnIndexingError(e);
        }

        //protected override void OnDocumentWriting(DocumentWritingEventArgs docArgs)
        //{
        //    DataService.LogService.AddVerboseLog(docArgs.NodeId, string.Format("({0}) DocumentWriting event for node ({1})", this.Name, LuceneIndexFolder.FullName));
        //    base.OnDocumentWriting(docArgs);
        //}

        protected override void OnNodeIndexed(IndexedNodeEventArgs e)
        {
            DataService.LogService.AddVerboseLog(e.NodeId, string.Format("Index created for node"));
            base.OnNodeIndexed(e);
        }

        protected override void OnIndexDeleted(DeleteIndexEventArgs e)
        {
            DataService.LogService.AddVerboseLog(-1, string.Format("Index deleted for term: {0} with value {1}", e.DeletedTerm.Key, e.DeletedTerm.Value));
            base.OnIndexDeleted(e);
        }

        protected override void OnIndexOptimizing(EventArgs e)
        {
            DataService.LogService.AddInfoLog(-1, string.Format("Index is being optimized"));
            base.OnIndexOptimizing(e);
        }

        #endregion

        #region Public methods

       
        /// <summary>
        /// Overridden for logging
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        public override void ReIndexNode(XElement node, string type)
        {
            if (!SupportedTypes.Contains(type))
                return;

            DataService.LogService.AddVerboseLog((int)node.Attribute("id"), string.Format("ReIndexNode with type: {0}", type));
            base.ReIndexNode(node, type);
        }

        /// <summary>
        /// Deletes a node from the index.                
        /// </summary>
        /// <remarks>
        /// When a content node is deleted, we also need to delete it's children from the index so we need to perform a 
        /// custom Lucene search to find all decendents and create Delete item queues for them too.
        /// </remarks>
        /// <param name="nodeId">ID of the node to delete</param>
        public override void DeleteFromIndex(string nodeId)
        {
            //find all descendants based on path
            var descendantPath = string.Format(@"\-1\,*{0}\,*", nodeId);
            var rawQuery = string.Format("{0}:{1}", IndexPathFieldName, descendantPath);
            var c = InternalSearcher.CreateSearchCriteria();
            var filtered = c.RawQuery(rawQuery);
            var results = InternalSearcher.Search(filtered);

            DataService.LogService.AddVerboseLog(int.Parse(nodeId), string.Format("DeleteFromIndex with query: {0} (found {1} results)", rawQuery, results.Count()));

            //need to create a delete queue item for each one found
            foreach (var r in results)
            {
                EnqueueIndexOperation(new IndexOperation()
                    {
                        Operation = IndexOperationType.Delete,
                        Item = new IndexItem(null, "", r.Id.ToString())
                    });
                //SaveDeleteIndexQueueItem(new KeyValuePair<string, string>(IndexNodeIdFieldName, r.Id.ToString()));
            }

            base.DeleteFromIndex(nodeId);
        }
        #endregion

        #region Protected



        /// <summary>
        /// Overridden for logging.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        protected override void AddSingleNodeToIndex(XElement node, string type)
        {
            DataService.LogService.AddVerboseLog((int)node.Attribute("id"), string.Format("AddSingleNodeToIndex with type: {0}", type));
            base.AddSingleNodeToIndex(node, type);
        }

        public override void RebuildIndex()
        {
            DataService.LogService.AddVerboseLog(-1, "Rebuilding index");
            base.RebuildIndex();
        }

        /// <summary>
        /// Override this method to strip all html from all user fields before raising the event, then after the event 
        /// ensure our special Path field is added to the collection
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {
            //strip html of all users fields
            // Get all user data that we want to index and store into a dictionary 
            foreach (var field in IndexerData.UserFields)
            {
                if (e.Fields.ContainsKey(field.Name))
                {
                    e.Fields[field.Name] = DataService.ContentService.StripHtml(e.Fields[field.Name]);
                }
            }

            base.OnGatheringNodeData(e);

            //ensure the special path and node type alis fields is added to the dictionary to be saved to file
            var path = e.Node.Attribute("path").Value;
            if (!e.Fields.ContainsKey(IndexPathFieldName))
                e.Fields.Add(IndexPathFieldName, path);

            //this needs to support both schemas so get the nodeTypeAlias if it exists, otherwise the name
            var nodeTypeAlias = e.Node.Attribute("nodeTypeAlias") == null ? e.Node.Name.LocalName : e.Node.Attribute("nodeTypeAlias").Value;
            if (!e.Fields.ContainsKey(NodeTypeAliasFieldName))
                e.Fields.Add(NodeTypeAliasFieldName, nodeTypeAlias);
        }

        /// <summary>
        /// Called when a duplicate field is detected in the dictionary that is getting indexed.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="indexSetName"></param>
        /// <param name="fieldName"></param>
        protected override void OnDuplicateFieldWarning(int nodeId, string indexSetName, string fieldName)
        {
            base.OnDuplicateFieldWarning(nodeId, indexSetName, fieldName);

            this.DataService.LogService.AddInfoLog(nodeId, "Field \"" + fieldName + "\" is listed multiple times in the index set \"" + indexSetName + "\". Please ensure all names are unique");
        }

        /// <summary>
        /// Overridden to add the path property to the special fields to index
        /// </summary>
        /// <param name="allValuesForIndexing"></param>
        /// <returns></returns>
        protected override Dictionary<string, string> GetSpecialFieldsToIndex(Dictionary<string, string> allValuesForIndexing)
        {
            var fields = base.GetSpecialFieldsToIndex(allValuesForIndexing);

            //adds the special path property to the index
            fields.Add(IndexPathFieldName, allValuesForIndexing[IndexPathFieldName]);

            //adds the special node type alias property to the index
            fields.Add(NodeTypeAliasFieldName, allValuesForIndexing[NodeTypeAliasFieldName]);

            return fields;

        }

        /// <summary>
        /// Creates an IIndexCriteria object based on the indexSet passed in and our DataService
        /// </summary>
        /// <param name="indexSet"></param>
        /// <returns></returns>
        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            return indexSet.ToIndexCriteria(DataService);
        }

        /// <summary>
        /// return the index policy for the field name passed in, if not found, return normal
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected override FieldIndexTypes GetPolicy(string fieldName)
        {
            var def = IndexFieldPolicies.Where(x => x.Key == fieldName);
            return (def.Count() == 0 ? FieldIndexTypes.ANALYZED : def.Single().Value);
        }

        /// <summary>
        /// Ensure that the content of this node is available for indexing (i.e. don't allow protected
        /// content to be indexed when this is disabled).
        /// <returns></returns>
        /// </summary>
        protected override bool ValidateDocument(XElement node)
        {
            var nodeId = int.Parse(node.Attribute("id").Value);
            // Test for access if we're only indexing published content
            // return nothing if we're not supporting protected content and it is protected, and we're not supporting unpublished content
            if (!SupportUnpublishedContent
                && (!SupportProtectedContent
                && DataService.ContentService.IsProtected(nodeId, node.Attribute("path").Value)))
            {
                return false;
            }

            return base.ValidateDocument(node);
        }

        #endregion
    }
}
