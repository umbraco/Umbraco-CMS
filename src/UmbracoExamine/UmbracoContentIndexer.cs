using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Examine;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Umbraco.Core.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using IContentService = Umbraco.Core.Services.IContentService;
using IMediaService = Umbraco.Core.Services.IMediaService;


namespace UmbracoExamine
{
    /// <summary>
    /// An indexer for Umbraco content and media
    /// </summary>
    public class UmbracoContentIndexer : BaseUmbracoIndexer
    {
        protected IContentService ContentService { get; private set; }
        protected IMediaService MediaService { get; private set; }
        protected IUserService UserService { get; private set; }
        private readonly IEnumerable<IUrlSegmentProvider> _urlSegmentProviders;
        private readonly IQueryFactory _queryFactory;
        private int? _parentId;

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoContentIndexer()
            : base()
        {
            ContentService = ApplicationContext.Current.Services.ContentService;
            MediaService = ApplicationContext.Current.Services.MediaService;
            UserService = ApplicationContext.Current.Services.UserService;
            _urlSegmentProviders = UrlSegmentProviderResolver.Current.Providers;
            _queryFactory = ApplicationContext.Current.DatabaseContext.QueryFactory;
        }

        public UmbracoContentIndexer(
            IEnumerable<FieldDefinition> fieldDefinitions, 
            Directory luceneDirectory, 
            Analyzer defaultAnalyzer,
            ProfilingLogger profilingLogger,
            IContentService contentService, 
            IMediaService mediaService, 
            IUserService userService, 
            IEnumerable<IUrlSegmentProvider> urlSegmentProviders, 
            IValueSetValidator validator,
            UmbracoContentIndexerOptions options,
            IQueryFactory queryFactory,
            FacetConfiguration facetConfiguration = null, 
            IDictionary<string, Func<string, IIndexValueType>> indexValueTypes = null) 
            : base(fieldDefinitions, luceneDirectory, defaultAnalyzer, profilingLogger, validator, facetConfiguration, indexValueTypes)
        {
            if (contentService == null) throw new ArgumentNullException("contentService");
            if (mediaService == null) throw new ArgumentNullException("mediaService");
            if (userService == null) throw new ArgumentNullException("userService");
            if (urlSegmentProviders == null) throw new ArgumentNullException("urlSegmentProviders");
            if (validator == null) throw new ArgumentNullException("validator");
            if (options == null) throw new ArgumentNullException("options");
            if (queryFactory == null) throw new ArgumentNullException(nameof(queryFactory));

            SupportProtectedContent = options.SupportProtectedContent;
            SupportUnpublishedContent = options.SupportUnpublishedContent;
            ParentId = options.ParentId;
            //backward compat hack:
            IndexerData = new IndexCriteria(Enumerable.Empty<IIndexField>(), Enumerable.Empty<IIndexField>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), 
                //hack to set the parent Id for backwards compat, when using this ctor the IndexerData will (should) always be null
                options.ParentId);

            ContentService = contentService;
            MediaService = mediaService;
            UserService = userService;
            _urlSegmentProviders = urlSegmentProviders;
            _queryFactory = queryFactory;
        }
    

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
        }

        #endregion

        #region Properties

        /// <summary>
        /// By default this is false, if set to true then the indexer will include indexing content that is flagged as publicly protected.
        /// This property is ignored if SupportUnpublishedContent is set to true.
        /// </summary>
        public bool SupportProtectedContent { get; protected set; }

        /// <summary>
        /// Determines if the manager will call the indexing methods when content is saved or deleted as
        /// opposed to cache being updated.
        /// </summary>
        public bool SupportUnpublishedContent { get; protected set; }

        /// <summary>
        /// If set this will filter the content items allowed to be indexed
        /// </summary>
        public int? ParentId
        {
            get
            {
                //fallback to the legacy data
                return _parentId ?? (IndexerData == null ? (int?)null : IndexerData.ParentNodeId);
            }
            protected set { _parentId = value; }
        }

        protected override IEnumerable<string> SupportedTypes => new[] {IndexTypes.Content, IndexTypes.Media};

        #endregion

     
        #region Public methods

       

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
            var descendantPath = $@"\-1\,*{nodeId}\,*";
            var rawQuery = $"{IndexPathFieldName}:{descendantPath}";
            var searcher = GetSearcher();
            var c = searcher.CreateCriteria();
            var filtered = c.RawQuery(rawQuery);
            var results = searcher.Find(filtered);

            ProfilingLogger.Logger.Debug(GetType(), $"DeleteFromIndex with query: {rawQuery} (found {results.TotalItemCount} results)");

            //need to create a delete queue item for each one found
            foreach (var r in results)
            {
                ProcessIndexOperation(new IndexOperation()
                {
                    Operation = IndexOperationType.Delete,
                    Item = new IndexItem(new ValueSet(r.LongId, string.Empty))
                });
            }

            base.DeleteFromIndex(nodeId);
        }
        #endregion

        #region Protected

        protected override void PerformIndexAll(string type)
        {
            
            const int pageSize = 1000;
            var pageIndex = 0;

            switch (type)
            {
                case IndexTypes.Content:

                    var contentParentId = -1;
                    if (ParentId.HasValue && ParentId.Value > 0)
                    {
                        contentParentId = ParentId.Value;
                    }
                    IContent[] content;

                    do
                    {
                        long total;

                        IEnumerable<IContent> descendants;
                        if (SupportUnpublishedContent)
                        {
                            descendants = ContentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out total);
                        }
                        else
                        {
                            //add the published filter
                            var qry = _queryFactory.Create<IContent>().Where(x => x.Published == true);

                            descendants = ContentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out total, "Path", Direction.Ascending, true, qry);
                        }

                        //if specific types are declared we need to post filter them
                        //TODO: Update the service layer to join the cmsContentType table so we can query by content type too
                        if (IndexerData != null && IndexerData.IncludeNodeTypes.Any())
                        {
                            content = descendants.Where(x => IndexerData.IncludeNodeTypes.Contains(x.ContentType.Alias)).ToArray();
                        }
                        else
                        {
                            content = descendants.ToArray();
                        }

                        IndexItems(GetValueSets(content));

                        pageIndex++;


                    } while (content.Length == pageSize);

                    
                    break;
                case IndexTypes.Media:

                    var mediaParentId = -1;
                    if (ParentId.HasValue && ParentId.Value > 0)
                    {
                        mediaParentId = ParentId.Value;
                    }
                    IMedia[] media;
                    
                    do
                    {
                        long total;
                        var descendants = MediaService.GetPagedDescendants(mediaParentId, pageIndex, pageSize, out total);

                        //if specific types are declared we need to post filter them
                        //TODO: Update the service layer to join the cmsContentType table so we can query by content type too
                        if (IndexerData != null && IndexerData.IncludeNodeTypes.Any())
                        {
                            media = descendants.Where(x => IndexerData.IncludeNodeTypes.Contains(x.ContentType.Alias)).ToArray();
                        }
                        else
                        {
                            media = descendants.ToArray();
                        }

                        IndexItems(GetValueSets(media));
                        
                        pageIndex++;
                    } while (media.Length == pageSize);

                    break;
            }
        }

        private IEnumerable<ValueSet> GetValueSets(IEnumerable<IContent> content)
        {
            foreach (var c in content)
            {
                var urlValue = c.GetUrlSegment(_urlSegmentProviders);
                var values = new Dictionary<string, object[]>
                {
                    {"icon", new object[] {c.ContentType.Icon}},
                    {PublishedFieldName, new object[] {c.Published ? 1 : 0}},
                    {"id", new object[] {c.Id}},
                    {"key", new object[] {c.Key}},
                    {"parentID", new object[] {c.Level > 1 ? c.ParentId : -1}},
                    {"level", new object[] {c.Level}},
                    {"creatorID", new object[] {c.CreatorId}},
                    {"sortOrder", new object[] {c.SortOrder}},
                    {"createDate", new object[] {c.CreateDate}},
                    {"updateDate", new object[] {c.UpdateDate}},
                    {"nodeName", new object[] {c.Name}},
                    {"urlName", new object[] {urlValue}},
                    {"path", new object[] {c.Path}},
                    {"nodeType", new object[] {c.ContentType.Id}},
                    {"creatorName", new object[] {c.GetCreatorProfile(UserService).Name}},
                    {"writerName", new object[] {c.GetWriterProfile(UserService).Name}},        
                    {"writerID", new object[] {c.WriterId}},
                    {"version", new object[] {c.Version}},
                    {"template", new object[] {c.Template == null ? 0 : c.Template.Id}}
                };

                foreach (var property in c.Properties.Where(p => p != null && p.Value != null && p.Value.ToString().IsNullOrWhiteSpace() == false))
                {
                    values.Add(property.Alias, new[] {property.Value});
                }

                var vs = new ValueSet(c.Id, IndexTypes.Content, c.ContentType.Alias, values);

                yield return vs;
            }
        }

        private IEnumerable<ValueSet> GetValueSets(IEnumerable<IMedia> media)
        {
            foreach (var m in media)
            {
                var urlValue = m.GetUrlSegment(_urlSegmentProviders);
                var values = new Dictionary<string, object[]>
                {
                    {"icon", new object[] {m.ContentType.Icon}},
                    {"id", new object[] {m.Id}},
                    {"key", new object[] {m.Key}},
                    {"parentID", new object[] {m.Level > 1 ? m.ParentId : -1}},
                    {"level", new object[] {m.Level}},
                    {"creatorID", new object[] {m.CreatorId}},
                    {"sortOrder", new object[] {m.SortOrder}},
                    {"createDate", new object[] {m.CreateDate}},
                    {"updateDate", new object[] {m.UpdateDate}},
                    {"nodeName", new object[] {m.Name}},
                    {"urlName", new object[] {urlValue}},
                    {"path", new object[] {m.Path}},
                    {"nodeType", new object[] {m.ContentType.Id}},
                    {"creatorName", new object[] {m.GetCreatorProfile(UserService).Name}}
                };

                foreach (var property in m.Properties.Where(p => p != null && p.Value != null && p.Value.ToString().IsNullOrWhiteSpace() == false))
                {
                    values.Add(property.Alias, new[] { property.Value });
                }

                var vs = new ValueSet(m.Id, IndexTypes.Media, m.ContentType.Alias, values);

                yield return vs;
            }
        }

        /// <summary>
        /// Creates an IIndexCriteria object based on the indexSet passed in and our DataService
        /// </summary>
        /// <param name="indexSet"></param>
        /// <returns></returns>
        /// <remarks>
        /// If we cannot initialize we will pass back empty indexer data since we cannot read from the database
        /// </remarks>
        [Obsolete("IIndexCriteria is obsolete, this method is used only for configuration based indexes it is recommended to configure indexes on startup with code instead of config")]
        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            if (CanInitialize())
            {
                //NOTE: We are using a singleton here because: This is only ever used for configuration based scenarios, this is never
                // used when the index is configured via code (the default), in which case IIndexCriteria is never used. When this is used
                // the DI ctor is not used.
                return indexSet.ToIndexCriteria(ApplicationContext.Current.Services.ContentTypeService);
            }
            return base.GetIndexerData(indexSet);
        }

        #endregion
    }
}
