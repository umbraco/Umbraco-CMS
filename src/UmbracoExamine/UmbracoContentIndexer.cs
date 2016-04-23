using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Umbraco.Core.Logging;
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
        protected IDataTypeService DataTypeService { get; private set; }
        protected IUserService UserService { get; private set; }
        private readonly IEnumerable<IUrlSegmentProvider> _urlSegmentProviders; 

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoContentIndexer()
            : base()
        {
            ContentService = ApplicationContext.Current.Services.ContentService;
            MediaService = ApplicationContext.Current.Services.MediaService;
            DataTypeService = ApplicationContext.Current.Services.DataTypeService;
            UserService = ApplicationContext.Current.Services.UserService;
            _urlSegmentProviders = UrlSegmentProviderResolver.Current.Providers;
        }

        public UmbracoContentIndexer(
            IEnumerable<FieldDefinition> fieldDefinitions, 
            Directory luceneDirectory, 
            Analyzer defaultAnalyzer,
            ProfilingLogger profilingLogger,
            IContentService contentService, 
            IMediaService mediaService, 
            IDataTypeService dataTypeService,             
            IUserService userService, 
            IEnumerable<IUrlSegmentProvider> urlSegmentProviders, 
            IValueSetValidator validator,
            UmbracoContentIndexerOptions options,
            FacetConfiguration facetConfiguration = null, 
            IDictionary<string, Func<string, IIndexValueType>> indexValueTypes = null) 
            : base(fieldDefinitions, luceneDirectory, defaultAnalyzer, profilingLogger, validator, facetConfiguration, indexValueTypes)
        {
            if (contentService == null) throw new ArgumentNullException("contentService");
            if (mediaService == null) throw new ArgumentNullException("mediaService");
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            if (userService == null) throw new ArgumentNullException("userService");
            if (urlSegmentProviders == null) throw new ArgumentNullException("urlSegmentProviders");
            if (validator == null) throw new ArgumentNullException("validator");
            if (options == null) throw new ArgumentNullException("options");

            SupportProtectedContent = options.SupportProtectedContent;
            SupportUnpublishedContent = options.SupportUnpublishedContent;

            ContentService = contentService;
            MediaService = mediaService;
            DataTypeService = dataTypeService;
            UserService = userService;
            _urlSegmentProviders = urlSegmentProviders;
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

        protected override IEnumerable<string> SupportedTypes
        {
            get
            {
                return new string[] { IndexTypes.Content, IndexTypes.Media };
            }
        }

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
            throw new NotImplementedException("Fix DeleteFromIndex!");

            ////find all descendants based on path
            //var descendantPath = string.Format(@"\-1\,*{0}\,*", nodeId);
            //var rawQuery = string.Format("{0}:{1}", IndexPathFieldName, descendantPath);
            //var c = InternalSearcher.CreateSearchCriteria();
            //var filtered = c.RawQuery(rawQuery);
            //var results = InternalSearcher.Search(filtered);

            //DataService.LogService.AddVerboseLog(int.Parse(nodeId), string.Format("DeleteFromIndex with query: {0} (found {1} results)", rawQuery, results.Count()));

            ////need to create a delete queue item for each one found
            //foreach (var r in results)
            //{
            //    EnqueueIndexOperation(new IndexOperation()
            //        {
            //            Operation = IndexOperationType.Delete,
            //            Item = new IndexItem(null, "", r.Id.ToString())
            //        });
            //    //SaveDeleteIndexQueueItem(new KeyValuePair<string, string>(IndexNodeIdFieldName, r.Id.ToString()));
            //}

            //base.DeleteFromIndex(nodeId);
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
                    if (this.SupportUnpublishedContent == false)
                    {
                        //TODO: Need to deal with Published Content here

                        throw new NotImplementedException("NEED TO FIX PUBLISHED CONTENT INDEXING");

                        //use the base implementation which will use the published XML cache to perform the lookups
                        //base.PerformIndexAll(type);
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
                            long total;
                            var descendants = ContentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out total);

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
                        long total;
                        var descendants = MediaService.GetPagedDescendants(mediaParentId, pageIndex, pageSize, out total);

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
                    MediaService,
                    DataTypeService,
                    UserService,
                    _urlSegmentProviders,
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
                    ContentService,
                    DataTypeService,
                    UserService,
                    _urlSegmentProviders,
                    c);

                //add a custom 'icon' attribute
                xml.Add(new XAttribute("icon", c.ContentType.Icon));

                yield return xml;
            }
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
                //NOTE: We are using a singleton here because: This is only ever used for configuration based scenarios, this is never
                // used when the index is configured via code (the default), in which case IIndexCriteria is never used. When this is used
                // the DI ctor is not used.
                return indexSet.ToIndexCriteria(ApplicationContext.Current.Services.ContentTypeService);
            }
            else
            {
                return base.GetIndexerData(indexSet);
            }

        }

        #endregion
    }
}
