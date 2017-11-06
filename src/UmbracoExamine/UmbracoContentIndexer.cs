using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Examine;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using UmbracoExamine.DataServices;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using UmbracoExamine.Config;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Umbraco.Core.Persistence.Querying;
using IContentService = Umbraco.Core.Services.IContentService;
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
        private readonly IContentTypeService _contentTypeService;
        private readonly EntityXmlSerializer _serializer = new EntityXmlSerializer();
        private const int PageSize = 10000;

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
            _contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
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
            _contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
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
            _contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
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
        [Obsolete("Use the overload that specifies the Umbraco services")]
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
            _contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
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
        /// <param name="contentTypeService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        public UmbracoContentIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, IDataService dataService,
            IContentService contentService,
            IMediaService mediaService,
            IDataTypeService dataTypeService,
            IUserService userService,
            IContentTypeService contentTypeService,
            Analyzer analyzer, bool async)
            : base(indexerData, luceneDirectory, dataService, analyzer, async)
        {
            _contentService = contentService;
            _mediaService = mediaService;
            _dataTypeService = dataTypeService;
            _userService = userService;
            _contentTypeService = contentTypeService;
        }

        /// <summary>
        /// Creates an NRT indexer
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="writer"></param>
        /// <param name="dataService"></param>
        /// <param name="contentTypeService"></param>
        /// <param name="async"></param>
        /// <param name="contentService"></param>
        /// <param name="mediaService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="userService"></param>
        public UmbracoContentIndexer(IIndexCriteria indexerData, IndexWriter writer, IDataService dataService,
            IContentService contentService,
            IMediaService mediaService,
            IDataTypeService dataTypeService,
            IUserService userService,
            IContentTypeService contentTypeService,
            bool async) 
            : base(indexerData, writer, dataService, async)
        {
            _contentService = contentService;
            _mediaService = mediaService;
            _dataTypeService = dataTypeService;
            _userService = userService;
            _contentTypeService = contentTypeService;
        }

        #endregion

        #region Constants & Fields        

        /// <summary>
        /// Used to store the path of a content object
        /// </summary>
        public const string IndexPathFieldName = "__Path";
        public const string NodeKeyFieldName = "__Key";
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
        internal static readonly StaticFieldCollection IndexFieldPolicies
            = new StaticFieldCollection
            {
                new StaticField("id", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("key", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("version", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("parentID", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("level", FieldIndexTypes.NOT_ANALYZED, true, "NUMBER"),
                new StaticField("writerID", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("creatorID", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("nodeType", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("template", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("sortOrder", FieldIndexTypes.NOT_ANALYZED, true, "NUMBER"),
                new StaticField("createDate", FieldIndexTypes.NOT_ANALYZED, false, "DATETIME"),
                new StaticField("updateDate", FieldIndexTypes.NOT_ANALYZED, false, "DATETIME"),
                new StaticField("nodeName", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("urlName", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("writerName", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("creatorName", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField("nodeTypeAlias", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField( "path", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "isPublished", FieldIndexTypes.NOT_ANALYZED, false, string.Empty)
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

            bool disableXmlDocLookup;
            if (config["disableXmlDocLookup"] != null && bool.TryParse(config["disableXmlDocLookup"], out disableXmlDocLookup))
                DisableXmlDocumentLookup = disableXmlDocLookup;
            else
                DisableXmlDocumentLookup = false;

            base.Initialize(name, config);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Whether to use the cmsContentXml data to re-index when possible (i.e. for published content, media and members)
        /// </summary>
        public bool DisableXmlDocumentLookup { get; private set; }

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
            DataService.LogService.AddErrorLog(e.NodeId, string.Format("{0},{1}, IndexSet: {2}", e.Message, e.InnerException != null ? e.InnerException.ToString() : "", this.IndexSetName));
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
            if (SupportedTypes.Contains(type) == false)
                return;
            
            var pageIndex = 0;

            DataService.LogService.AddInfoLog(-1, string.Format("PerformIndexAll - Start data queries - {0}", type));
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                switch (type)
                {
                    case IndexTypes.Content:
                        var contentParentId = -1;
                        if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
                        {
                            contentParentId = IndexerData.ParentNodeId.Value;
                        }
                        
                        if (SupportUnpublishedContent == false && DisableXmlDocumentLookup == false)
                        {
                            //get all node Ids that have a published version - this is a fail safe check, in theory
                            // only document nodes that have a published version would exist in the cmsContentXml table
                            var allNodesWithPublishedVersions = ApplicationContext.Current.DatabaseContext.Database.Fetch<int>(
                                "select DISTINCT cmsDocument.nodeId from cmsDocument where cmsDocument.published = 1");

                            XElement last = null;
                            var trackedIds = new HashSet<string>();

                            ReindexWithXmlEntries(type, contentParentId,
                                () => _contentTypeService.GetAllContentTypes().ToArray(),
                                (path, pIndex, pSize) =>
                                {
                                    long totalContent;

                                    //sorted by: umbracoNode.level, umbracoNode.parentID, umbracoNode.sortOrder
                                    var result = _contentService.GetPagedXmlEntries(path, pIndex, pSize, out totalContent).ToArray();
                                    var more = result.Length == pSize;

                                    //then like we do in the ContentRepository.BuildXmlCache we need to track what Parents have been processed
                                    // already so that we can then exclude implicitly unpublished content items
                                    var filtered = new List<XElement>();

                                    foreach (var xml in result)
                                    {
                                        var id = xml.AttributeValue<int>("id");
                                        
                                        //don't include this if it doesn't have a published version
                                        if (allNodesWithPublishedVersions.Contains(id) == false)
                                            continue;

                                        var parentId = xml.AttributeValue<string>("parentID");

                                        if (parentId == null) continue; //this shouldn't happen

                                        //if the parentid is changing
                                        if (last != null && last.AttributeValue<string>("parentID") != parentId)
                                        {
                                            var found = trackedIds.Contains(parentId);
                                            if (found == false)
                                            {
                                                //Need to short circuit here, if the parent is not there it means that the parent is unpublished
                                                // and therefore the child is not published either so cannot be included in the xml cache
                                                continue;
                                            }                                            
                                        }
                                        
                                        last = xml;
                                        trackedIds.Add(xml.AttributeValue<string>("id"));

                                        filtered.Add(xml);
                                    }

                                    return Tuple.Create(filtered.ToArray(), more);
                                },
                                i => _contentService.GetById(i));
                        }
                        else
                        {
                            //used to track non-published entities so we can determine what items are implicitly not published
                            //currently this is not in use apart form in tests
                            var notPublished = new HashSet<string>();

                            int currentPageSize;
                            do
                            {
                                long total;

                                IContent[] descendants;
                                if (SupportUnpublishedContent)
                                {
                                    descendants = _contentService.GetPagedDescendants(contentParentId, pageIndex, PageSize, out total, "umbracoNode.id").ToArray();
                                }
                                else
                                {
                                    //get all paged records but order by level ascending, we need to do this because we need to track which nodes are not published so that we can determine
                                    // which descendent nodes are implicitly not published
                                    descendants = _contentService.GetPagedDescendants(contentParentId, pageIndex, PageSize, out total, "level", Direction.Ascending, true, (string)null).ToArray();
                                }

                                // need to store decendants count before filtering, in order for loop to work correctly
                                currentPageSize = descendants.Length;

                                //if specific types are declared we need to post filter them
                                //TODO: Update the service layer to join the cmsContentType table so we can query by content type too
                                IEnumerable<IContent> content;
                                if (IndexerData.IncludeNodeTypes.Any())
                                {
                                    content = descendants.Where(x => IndexerData.IncludeNodeTypes.Contains(x.ContentType.Alias));
                                }
                                else
                                {
                                    content = descendants;
                                }

                                AddNodesToIndex(GetSerializedContent(
                                    SupportUnpublishedContent,
                                    c => _serializer.Serialize(_contentService, _dataTypeService, _userService, c),
                                    content, notPublished).WhereNotNull(), type);

                                pageIndex++;
                            } while (currentPageSize == PageSize);
                        }

                        break;
                    case IndexTypes.Media:
                        var mediaParentId = -1;

                        if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
                        {
                            mediaParentId = IndexerData.ParentNodeId.Value;
                        }

                        ReindexWithXmlEntries(type, mediaParentId,
                            () => _contentTypeService.GetAllMediaTypes().ToArray(),
                            (path, pIndex, pSize) =>
                            {
                                long totalMedia;
                                var result = _mediaService.GetPagedXmlEntries(path, pIndex, pSize, out totalMedia).ToArray();
                                var more = result.Length == pSize;
                                return Tuple.Create(result, more);
                            },
                            i => _mediaService.GetById(i));

                        break;
                }
            }
            finally
            {
                stopwatch.Stop();
            }
            
            DataService.LogService.AddInfoLog(-1, string.Format("PerformIndexAll - End data queries - {0}, took {1}ms", type, stopwatch.ElapsedMilliseconds));
        }

        /// <summary>
        /// Performs a reindex of a type based on looking up entries from the cmsContentXml table - but using callbacks to get this data since
        /// we don't have a common underlying service interface for the media/content stuff
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parentId"></param>
        /// <param name="getContentTypes"></param>
        /// <param name="getPagedXmlEntries"></param>
        /// <param name="getContent"></param>
        internal void ReindexWithXmlEntries<TContentType>(
            string type, 
            int parentId,
            Func<TContentType[]> getContentTypes, 
            Func<string, int, int, Tuple<XElement[], bool>> getPagedXmlEntries,
            Func<int, IContentBase> getContent)
            where TContentType: IContentTypeComposition
        {
            var pageIndex = 0;

            var contentTypes = getContentTypes();
            var icons = contentTypes.ToDictionary(x => x.Id, y => y.Icon);
            var parent = parentId == -1 ? null : getContent(parentId);
            bool more;

            do
            {
                XElement[] xElements;

                if (parentId == -1)
                {
                    var pagedElements = getPagedXmlEntries("-1", pageIndex, PageSize);
                    xElements = pagedElements.Item1;
                    more = pagedElements.Item2;
                }
                else if (parent == null)
                {
                    xElements = new XElement[0];
                    more = false;
                }
                else
                {
                    var pagedElements = getPagedXmlEntries(parent.Path, pageIndex, PageSize);
                    xElements = pagedElements.Item1;
                    more = pagedElements.Item2;
                }

                //if specific types are declared we need to post filter them
                //TODO: Update the service layer to join the cmsContentType table so we can query by content type too
                if (IndexerData.IncludeNodeTypes.Any())
                {
                    var includeNodeTypeIds = contentTypes.Where(x => IndexerData.IncludeNodeTypes.Contains(x.Alias)).Select(x => x.Id);
                    xElements = xElements.Where(elm => includeNodeTypeIds.Contains(elm.AttributeValue<int>("nodeType"))).ToArray();
                }

                foreach (var element in xElements)
                {
                    if (element.Attribute("icon") == null)
                    {
                        element.Add(new XAttribute("icon", icons[element.AttributeValue<int>("nodeType")]));
                    }
                }

                AddNodesToIndex(xElements, type);
                pageIndex++;
            } while (more);
        }

        internal static IEnumerable<XElement> GetSerializedContent(
            bool supportUnpublishdContent, 
            Func<IContent, XElement> serializer, 
            IEnumerable<IContent> content, 
            ISet<string> notPublished)
        {            
            foreach (var c in content)
            {
                if (supportUnpublishdContent == false)
                {
                    //if we don't support published content and this is not published then track it and return null
                    if (c.Published == false)
                    {
                        notPublished.Add(c.Path);
                        yield return null;
                        continue;
                    }

                    //if we don't support published content, check if this content item exists underneath any already tracked
                    //unpublished content and if so return null;
                    if (notPublished.Any(path => c.Path.StartsWith(string.Format("{0},", path))))
                    {
                        yield return null;
                        continue;
                    }
                }                

                var xml = serializer(c);                

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
            DataService.LogService.AddInfoLog(-1, "Rebuilding index");
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
                string fieldVal;
                if (e.Fields.TryGetValue(field.Name, out fieldVal))
                {
                    //check if the field value has html
                    if (XmlHelper.CouldItBeXml(fieldVal))
                    {
                        //First save the raw value to a raw field, we will change the policy of this field by detecting the prefix later
                        e.Fields[RawFieldPrefix + field.Name] = fieldVal;
                        //now replace the original value with the stripped html
                        e.Fields[field.Name] = DataService.ContentService.StripHtml(fieldVal);
                    }
                }
            }

            base.OnGatheringNodeData(e);

            //ensure the special path and node type alias fields is added to the dictionary to be saved to file
            var path = e.Node.Attribute("path").Value;
            if (e.Fields.ContainsKey(IndexPathFieldName) == false)
                e.Fields.Add(IndexPathFieldName, path);

            //this needs to support both schema's so get the nodeTypeAlias if it exists, otherwise the name
            var nodeTypeAlias = e.Node.Attribute("nodeTypeAlias") == null ? e.Node.Name.LocalName : e.Node.Attribute("nodeTypeAlias").Value;
            if (e.Fields.ContainsKey(NodeTypeAliasFieldName) == false)
                e.Fields.Add(NodeTypeAliasFieldName, nodeTypeAlias);

            //add icon 
            var icon = (string)e.Node.Attribute("icon");
            if (e.Fields.ContainsKey(IconFieldName) == false)
                e.Fields.Add(IconFieldName, icon);

            //add guid 
            var guid = (string)e.Node.Attribute("key");
            if (e.Fields.ContainsKey(NodeKeyFieldName) == false)
                e.Fields.Add(NodeKeyFieldName, guid);
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

            //guid
            string guidVal;
            if (allValuesForIndexing.TryGetValue(NodeKeyFieldName, out guidVal) && guidVal.IsNullOrWhiteSpace() == false)
            {
                fields.Add(NodeKeyFieldName, guidVal);
            }

            //icon
            string iconVal;
            if (allValuesForIndexing.TryGetValue(IconFieldName, out iconVal) && iconVal.IsNullOrWhiteSpace() == false)
            {
                fields.Add(IconFieldName, iconVal);    
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
            StaticField def;
            if (IndexFieldPolicies.TryGetValue(fieldName, out def))
            {
                return def.IndexType;
            }
            return FieldIndexTypes.ANALYZED;
        }

        /// <summary>
        /// Ensure that the content of this node is available for indexing (i.e. don't allow protected
        /// content to be indexed when this is disabled).
        /// <returns></returns>
        /// </summary>
        protected override bool ValidateDocument(XElement node)
        {
            // Test for access if we're only indexing published content
            // return nothing if we're not supporting protected content and it is protected, and we're not supporting unpublished content
            if (SupportUnpublishedContent == false
                && SupportProtectedContent == false)
            {

                var nodeId = int.Parse(node.Attribute("id").Value);

                if (DataService.ContentService.IsProtected(nodeId, node.Attribute("path").Value))
                {
                    return false;
                }               
            }
            return base.ValidateDocument(node);
        }

        #endregion
    }
}