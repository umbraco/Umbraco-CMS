﻿using System;
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
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Umbraco.Core;
using umbraco.cms.businesslogic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using UmbracoExamine.DataServices;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using UmbracoExamine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using umbraco.BasePages;
using IContentService = Umbraco.Core.Services.IContentService;
using UmbracoExamine.LocalStorage;
using IMediaService = Umbraco.Core.Services.IMediaService;


namespace UmbracoExamine
{
    /// <summary>
    /// 
    /// </summary>
    public class UmbracoContentIndexer : BaseUmbracoIndexer
    {
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IUserService _userService;

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoContentIndexer()
            : base()
        {
            _contentService = ApplicationContext.Current.Services.ContentService;
            _mediaService = ApplicationContext.Current.Services.MediaService;
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
            _userService = ApplicationContext.Current.Services.UserService;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        [Obsolete("Use the overload that specifies the Umbraco services")]
        public UmbracoContentIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async)
        {
            _contentService = ApplicationContext.Current.Services.ContentService;
            _mediaService = ApplicationContext.Current.Services.MediaService;
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
            _userService = ApplicationContext.Current.Services.UserService;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        [Obsolete("Use the overload that specifies the Umbraco services")]
        public UmbracoContentIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, luceneDirectory, dataService, analyzer, async)
        {
            _contentService = ApplicationContext.Current.Services.ContentService;
            _mediaService = ApplicationContext.Current.Services.MediaService;
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
            _userService = ApplicationContext.Current.Services.UserService;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="dataService"></param>
        /// <param name="contentService"></param>
        /// <param name="mediaService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="userService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        public UmbracoContentIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, IDataService dataService, 
            IContentService contentService, 
            IMediaService mediaService,
            IDataTypeService dataTypeService,
            IUserService userService,
            Analyzer analyzer, bool async)
            : base(indexerData, luceneDirectory, dataService, analyzer, async)
        {
            _contentService = contentService;
            _mediaService = mediaService;
            _dataTypeService = dataTypeService;
            _userService = userService;
        }

        #endregion

        #region Constants & Fields

        private readonly LocalTempStorageIndexer _localTempStorageHelper = new LocalTempStorageIndexer();

        /// <summary>
        /// Used to store the path of a content object
        /// </summary>
        public const string IndexPathFieldName = "__Path";
        public const string NodeTypeAliasFieldName = "__NodeTypeAlias";
        public const string IconFieldName = "__Icon";

        /// <summary>
        /// The prefix added to a field when it is duplicated in order to store the original raw value.
        /// </summary>
        public const string RawFieldPrefix = "__Raw_";

        /// <summary>
        /// A type that defines the type of index for each Umbraco field (non user defined fields)
        /// Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
        /// for retreival after searching.
        /// </summary>
        internal static readonly List<StaticField> IndexFieldPolicies
            = new List<StaticField>
            {
                new StaticField("id", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "version", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "parentID", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "level", FieldIndexTypes.NOT_ANALYZED, true, "NUMBER"),
                new StaticField( "writerID", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "creatorID", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "nodeType", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "template", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "sortOrder", FieldIndexTypes.NOT_ANALYZED, true, "NUMBER"),
                new StaticField( "createDate", FieldIndexTypes.NOT_ANALYZED, false, "DATETIME"),
                new StaticField( "updateDate", FieldIndexTypes.NOT_ANALYZED, false, "DATETIME"),
                new StaticField( "nodeName", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField( "urlName", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "writerName", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField( "creatorName", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField( "nodeTypeAlias", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField( "path", FieldIndexTypes.NOT_ANALYZED, false, string.Empty)
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
        
        public override void Initialize(string name, NameValueCollection config)
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

        /// <summary>
        /// This ensures that the special __Raw_ fields are indexed
        /// </summary>
        /// <param name="docArgs"></param>

        protected override void OnDocumentWriting(DocumentWritingEventArgs docArgs)
        {
            var d = docArgs.Document;
            foreach (var f in docArgs.Fields.Where(x => x.Key.StartsWith(RawFieldPrefix)))
            {
                d.Add(new Field(
                   f.Key,
                   f.Value,
                   Field.Store.YES,
                   Field.Index.NO, //don't index this field, we never want to search by it 
                   Field.TermVector.NO));
            }

            base.OnDocumentWriting(docArgs);
        }

        protected override void OnNodeIndexed(IndexedNodeEventArgs e)
        {
            DataService.LogService.AddVerboseLog(e.NodeId, string.Format("Index created for node {0}", e.NodeId));
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
        /// Overridden for logging
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        public override void ReIndexNode(XElement node, string type)
        {
            if (!SupportedTypes.Contains(type))
                return;

            if (node.Attribute("id") != null)
            {
                DataService.LogService.AddVerboseLog((int)node.Attribute("id"), string.Format("ReIndexNode with type: {0}", type));
                base.ReIndexNode(node, type);
            }
            else
            {
                DataService.LogService.AddErrorLog(-1, string.Format("ReIndexNode cannot proceed, the format of the XElement is invalid, the xml has no 'id' attribute. {0}", node));
            }

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

        protected override void PerformIndexAll(string type)
        {
            
            const int pageSize = 5000;
            var pageIndex = 0;

            switch (type)
            {
                case IndexTypes.Content:
                    if (this.SupportUnpublishedContent == false)
                    {
                        //use the base implementation which will use the published XML cache to perform the lookups
                        base.PerformIndexAll(type);
                    }
                    else
                    {
                        var contentParentId = -1;
                        if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
                        {
                            contentParentId = IndexerData.ParentNodeId.Value;
                        }
                        IContent[] content;

                        do
                        {
                            int total;
                            var descendants = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out total);

                            //if specific types are declared we need to post filter them
                            //TODO: Update the service layer to join the cmsContentType table so we can query by content type too
                            if (IndexerData.IncludeNodeTypes.Any())
                            {
                                content = descendants.Where(x => IndexerData.IncludeNodeTypes.Contains(x.ContentType.Alias)).ToArray();
                            }
                            else
                            {
                                content = descendants.ToArray();
                            }

                            AddNodesToIndex(GetSerializedContent(content), type);
                            pageIndex++;
                        } while (content.Length == pageSize);

                    }
                    break;
                case IndexTypes.Media:

                    var mediaParentId = -1;
                    if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
                    {
                        mediaParentId = IndexerData.ParentNodeId.Value;
                    }
                    IMedia[] media;
                    
                    do
                    {
                        int total;
                        var descendants = _mediaService.GetPagedDescendants(mediaParentId, pageIndex, pageSize, out total);

                        //if specific types are declared we need to post filter them
                        //TODO: Update the service layer to join the cmsContentType table so we can query by content type too
                        if (IndexerData.IncludeNodeTypes.Any())
                        {
                            media = descendants.Where(x => IndexerData.IncludeNodeTypes.Contains(x.ContentType.Alias)).ToArray();
                        }
                        else
                        {
                            media = descendants.ToArray();
                        }
                        
                        AddNodesToIndex(GetSerializedMedia(media), type);
                        pageIndex++;
                    } while (media.Length == pageSize);

                    break;
            }
        }

        private IEnumerable<XElement> GetSerializedMedia(IEnumerable<IMedia> media)
        {
            var serializer = new EntityXmlSerializer();
            foreach (var m in media)
            {
                var xml = serializer.Serialize(
                    _mediaService,
                    _dataTypeService,
                    _userService,
                    m);

                //add a custom 'icon' attribute
                if (m.ContentType.Icon.IsNullOrWhiteSpace() == false)
                {
                    xml.Add(new XAttribute("icon", m.ContentType.Icon));    
                }
                

                yield return xml;
            }
        }

        private IEnumerable<XElement> GetSerializedContent(IEnumerable<IContent> content)
        {
            var serializer = new EntityXmlSerializer();
            foreach (var c in content)
            {
                var xml = serializer.Serialize(
                    _contentService,
                    _dataTypeService,
                    _userService,
                    c);

                //add a custom 'icon' attribute
                xml.Add(new XAttribute("icon", c.ContentType.Icon));

                yield return xml;
            }
        }

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
        /// Used to refresh the current IndexerData from the data in the DataService. This can be used
        /// if there are more properties added/removed from the database
        /// </summary>
        public void RefreshIndexerDataFromDataService()
        {
            //TODO: This would be much better done if the IndexerData property had read/write locks applied
            // to it! Unless we update the base class there's really no way to prevent the IndexerData from being
            // changed during an operation that is reading from it.
            var newIndexerData = GetIndexerData(IndexSets.Instance.Sets[IndexSetName]);
            IndexerData = newIndexerData;
        }

        /// <summary>
        /// Override this method to strip all html from all user fields before raising the event, then after the event 
        /// ensure our special Path field is added to the collection
        /// </summary>
        /// <param name="e"></param>

        protected override void OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {

            //strip html of all users fields if we detect it has HTML in it. 
            //if that is the case, we'll create a duplicate 'raw' copy of it so that we can return
            //the value of the field 'as-is'.
            // Get all user data that we want to index and store into a dictionary 
            foreach (var field in IndexerData.UserFields)
            {
                if (e.Fields.ContainsKey(field.Name))
                {
                    //check if the field value has html
                    if (XmlHelper.CouldItBeXml(e.Fields[field.Name]))
                    {
                        //First save the raw value to a raw field, we will change the policy of this field by detecting the prefix later
                        e.Fields[RawFieldPrefix + field.Name] = e.Fields[field.Name];
                        //now replace the original value with the stripped html
                        e.Fields[field.Name] = DataService.ContentService.StripHtml(e.Fields[field.Name]);
                    }
                }
            }

            base.OnGatheringNodeData(e);

            //ensure the special path and node type alias fields is added to the dictionary to be saved to file
            var path = e.Node.Attribute("path").Value;
            if (!e.Fields.ContainsKey(IndexPathFieldName))
                e.Fields.Add(IndexPathFieldName, path);

            //this needs to support both schema's so get the nodeTypeAlias if it exists, otherwise the name
            var nodeTypeAlias = e.Node.Attribute("nodeTypeAlias") == null ? e.Node.Name.LocalName : e.Node.Attribute("nodeTypeAlias").Value;
            if (!e.Fields.ContainsKey(NodeTypeAliasFieldName))
                e.Fields.Add(NodeTypeAliasFieldName, nodeTypeAlias);

            //add icon 
            var icon = (string)e.Node.Attribute("icon");
            if (!e.Fields.ContainsKey(IconFieldName))
                e.Fields.Add(IconFieldName, icon);  
            
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

            //icon
            if (allValuesForIndexing[IconFieldName].IsNullOrWhiteSpace() == false)
            {
                fields.Add(IconFieldName, allValuesForIndexing[IconFieldName]);    
            }

            return fields;

        }

        /// <summary>
        /// Creates an IIndexCriteria object based on the indexSet passed in and our DataService
        /// </summary>
        /// <param name="indexSet"></param>
        /// <returns></returns>
        /// <remarks>
        /// If we cannot initialize we will pass back empty indexer data since we cannot read from the database
        /// </remarks>
        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            if (CanInitialize())
            {
                return indexSet.ToIndexCriteria(DataService, IndexFieldPolicies);
            }
            else
            {
                return base.GetIndexerData(indexSet);
            }

        }

        /// <summary>
        /// return the index policy for the field name passed in, if not found, return normal
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected override FieldIndexTypes GetPolicy(string fieldName)
        {
            var def = IndexFieldPolicies.Where(x => x.Name == fieldName).ToArray();
            return (def.Any() == false ? FieldIndexTypes.ANALYZED : def.Single().IndexType);
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
