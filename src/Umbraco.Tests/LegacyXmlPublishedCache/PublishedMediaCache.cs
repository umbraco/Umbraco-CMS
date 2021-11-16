using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.XPath;
using Examine;
using Examine.Search;
using Lucene.Net.Store;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Examine;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    /// <summary>
    /// An IPublishedMediaStore that first checks for the media in Examine, and then reverts to the database
    /// </summary>
    /// <remarks>
    /// NOTE: In the future if we want to properly cache all media this class can be extended or replaced when these classes/interfaces are exposed publicly.
    /// </remarks>
    internal class PublishedMediaCache : PublishedCacheBase, IPublishedMediaCache2
    {
        private readonly IMediaService _mediaService;
        private readonly IUserService _userService;

        // by default these are null unless specified by the ctor dedicated to tests
        // when they are null the cache derives them from the ExamineManager, see
        // method GetExamineManagerSafe().
        //
        private readonly ISearcher _searchProvider;
        private readonly XmlStore _xmlStore;
        private readonly PublishedContentTypeCache _contentTypeCache;
        private readonly IEntityXmlSerializer _entitySerializer;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        // must be specified by the ctor
        private readonly IAppCache _appCache;

        public PublishedMediaCache(XmlStore xmlStore, IMediaService mediaService, IUserService userService,
            IAppCache appCache, PublishedContentTypeCache contentTypeCache, IEntityXmlSerializer entitySerializer,
            IUmbracoContextAccessor umbracoContextAccessor)
            : base(false)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            _appCache = appCache;
            _xmlStore = xmlStore;
            _contentTypeCache = contentTypeCache;
            _entitySerializer = entitySerializer;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Generally used for unit testing to use an explicit examine searcher
        /// </summary>
        /// <param name="mediaService"></param>
        /// <param name="userService"></param>
        /// <param name="searchProvider"></param>
        /// <param name="appCache"></param>
        /// <param name="contentTypeCache"></param>
        /// <param name="entitySerializer"></param>
        internal PublishedMediaCache(IMediaService mediaService, IUserService userService, ISearcher searchProvider, IAppCache appCache, PublishedContentTypeCache contentTypeCache, IEntityXmlSerializer entitySerializer, IUmbracoContextAccessor umbracoContextAccessor)
            : base(false)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _searchProvider = searchProvider ?? throw new ArgumentNullException(nameof(searchProvider));
            _appCache = appCache;
            _contentTypeCache = contentTypeCache;
            _entitySerializer = entitySerializer;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        static PublishedMediaCache()
        {
            InitializeCacheConfig();
        }

        public override IPublishedContent GetById(bool preview, int nodeId)
        {
            return GetUmbracoMedia(nodeId);
        }

        public override IPublishedContent GetById(bool preview, Guid nodeId)
        {
            throw new NotImplementedException();
        }

        public override IPublishedContent GetById(bool preview, Udi nodeId)
            => throw new NotSupportedException();

        public override bool HasById(bool preview, int contentId)
        {
            return GetUmbracoMedia(contentId) != null;
        }

        public override IEnumerable<IPublishedContent> GetAtRoot(bool preview, string culture = null)
        {
            var searchProvider = GetSearchProviderSafe();

            if (searchProvider != null)
            {
                try
                {
                    // first check in Examine for the cache values
                    // +(+parentID:-1) +__IndexType:media

                    var criteria = searchProvider.CreateQuery("media");
                    var filter = criteria.ParentId(-1).Not().Field(UmbracoExamineIndex.IndexPathFieldName, "-1,-21,".MultipleCharacterWildcard());

                    var result = filter.Execute();
                    if (result != null)
                        return result.Select(x => CreateFromCacheValues(ConvertFromSearchResult(x)));
                }
                catch (Exception ex)
                {
                    if (ex is FileNotFoundException)
                    {
                        //Currently examine is throwing FileNotFound exceptions when we have a load balanced filestore and a node is published in umbraco
                        //See this thread: http://examine.cdodeplex.com/discussions/264341
                        //Catch the exception here for the time being, and just fallback to GetMedia
                        // TODO: Need to fix examine in LB scenarios!
                        Current.Logger.Error<PublishedMediaCache>(ex, "Could not load data from Examine index for media");
                    }
                    else if (ex is AlreadyClosedException)
                    {
                        //If the app domain is shutting down and the site is under heavy load the index reader will be closed and it really cannot
                        //be re-opened since the app domain is shutting down. In this case we have no option but to try to load the data from the db.
                        Current.Logger.Error<PublishedMediaCache>(ex, "Could not load data from Examine index for media, the app domain is most likely in a shutdown state");
                    }
                    else throw;
                }
            }

            //something went wrong, fetch from the db

            var rootMedia = _mediaService.GetRootMedia();
            return rootMedia.Select(m => GetUmbracoMedia(m.Id));
        }

        public override IPublishedContent GetSingleByXPath(bool preview, string xpath, XPathVariable[] vars)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
            //var navigator = CreateNavigator(preview);
            //var iterator = navigator.Select(xpath, vars);
            //return GetSingleByXPath(iterator);
        }

        public override IPublishedContent GetSingleByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
            //var navigator = CreateNavigator(preview);
            //var iterator = navigator.Select(xpath, vars);
            //return GetSingleByXPath(iterator);
        }

        private IPublishedContent GetSingleByXPath(XPathNodeIterator iterator)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
            //if (iterator.MoveNext() == false) return null;
            //var idAttr = iterator.Current.GetAttribute("id", "");
            //int id;
            //return int.TryParse(idAttr, out id) ? GetUmbracoMedia(id) : null;
        }

        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, XPathVariable[] vars)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
            //var navigator = CreateNavigator(preview);
            //var iterator = navigator.Select(xpath, vars);
            //return GetByXPath(iterator);
        }

        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
            //var navigator = CreateNavigator(preview);
            //var iterator = navigator.Select(xpath, vars);
            //return GetByXPath(iterator);
        }

        private IEnumerable<IPublishedContent> GetByXPath(XPathNodeIterator iterator)
        {
            while (iterator.MoveNext())
            {
                var idAttr = iterator.Current.GetAttribute("id", "");
                int id;
                if (int.TryParse(idAttr, out id))
                    yield return GetUmbracoMedia(id);
            }
        }

        public override XPathNavigator CreateNavigator(bool preview)
        {
            throw new NotImplementedException("PublishedMediaCache does not support XPath.");
            //var doc = _xmlStore.GetMediaXml();
            //return doc.CreateNavigator();
        }

        public override XPathNavigator CreateNodeNavigator(int id, bool preview)
        {
            // preview is ignored for media cache

            // this code is mostly used when replacing old media.ToXml() code, and that code
            // stored the XML attached to the media itself - so for some time in memory - so
            // unless we implement some sort of cache here, we're probably degrading perfs.

            XPathNavigator navigator = null;
            var node = _xmlStore.GetMediaXmlNode(id);
            if (node != null)
            {
                navigator = node.CreateNavigator();
            }
            return navigator;
        }

        public override bool HasContent(bool preview) { throw new NotImplementedException(); }

        private static IExamineManager GetExamineManagerSafe()
        {
            try
            {
                return ExamineManager.Instance;
            }
            catch (TypeInitializationException)
            {
                return null;
            }
        }

        private ISearcher GetSearchProviderSafe()
        {
            if (_searchProvider != null)
                return _searchProvider;

            var eMgr = GetExamineManagerSafe();
            if (eMgr == null) return null;

            try
            {
                return eMgr.TryGetIndex(Constants.UmbracoIndexes.InternalIndexName, out var index) ? index.GetSearcher() : null;
            }
            catch (FileNotFoundException)
            {
                //Currently examine is throwing FileNotFound exceptions when we have a load balanced filestore and a node is published in umbraco
                //See this thread: http://examine.cdodeplex.com/discussions/264341
                //Catch the exception here for the time being, and just fallback to GetMedia
                // TODO: Need to fix examine in LB scenarios!
            }
            catch (NullReferenceException)
            {
                //This will occur when the search provider cannot be initialized. In newer examine versions the initialization is lazy and therefore
                // the manager will return the singleton without throwing initialization errors, however if examine isn't configured correctly a null
                // reference error will occur because the examine settings are null.
            }
            catch (AlreadyClosedException)
            {
                //If the app domain is shutting down and the site is under heavy load the index reader will be closed and it really cannot
                //be re-opened since the app domain is shutting down. In this case we have no option but to try to load the data from the db.
            }
            return null;
        }

        private IPublishedContent GetUmbracoMedia(int id)
        {
            // this recreates an IPublishedContent and model each time
            // it is called, but at least it should NOT hit the database
            // nor Lucene each time, relying on the memory cache instead

            if (id <= 0) return null; // fail fast

            var cacheValues = GetCacheValues(id, GetUmbracoMediaCacheValues);

            return cacheValues == null ? null : CreateFromCacheValues(cacheValues);
        }

        private CacheValues GetUmbracoMediaCacheValues(int id)
        {
            var searchProvider = GetSearchProviderSafe();

            if (searchProvider != null)
            {
                try
                {
                    // first check in Examine as this is WAY faster
                    //
                    // the filter will create a query like this:
                    // +(+__NodeId:3113 -__Path:-1,-21,*) +__IndexType:media
                    //
                    // note that since the use of the wildcard, it automatically escapes it in Lucene.

                    var criteria = searchProvider.CreateQuery("media");
                    var filter = criteria.Id(id.ToInvariantString()).Not().Field(UmbracoExamineIndex.IndexPathFieldName, "-1,-21,".MultipleCharacterWildcard());

                    var result = filter.Execute().FirstOrDefault();
                    if (result != null) return ConvertFromSearchResult(result);
                }
                catch (Exception ex)
                {
                    if (ex is FileNotFoundException)
                    {
                        //Currently examine is throwing FileNotFound exceptions when we have a load balanced filestore and a node is published in umbraco
                        //See this thread: http://examine.cdodeplex.com/discussions/264341
                        //Catch the exception here for the time being, and just fallback to GetMedia
                        // TODO: Need to fix examine in LB scenarios!
                        Current.Logger.Error<PublishedMediaCache>(ex, "Could not load data from Examine index for media");
                    }
                    else if (ex is AlreadyClosedException)
                    {
                        //If the app domain is shutting down and the site is under heavy load the index reader will be closed and it really cannot
                        //be re-opened since the app domain is shutting down. In this case we have no option but to try to load the data from the db.
                        Current.Logger.Error<PublishedMediaCache>(ex, "Could not load data from Examine index for media, the app domain is most likely in a shutdown state");
                    }
                    else throw;
                }
            }

            // don't log a warning here, as it can flood the log in case of eg a media picker referencing a media
            // that has been deleted, hence is not in the Examine index anymore (for a good reason). try to get
            // the media from the service, first
            var media = _mediaService.GetById(id);
            if (media == null || media.Trashed) return null; // not found, ok

            // so, the media was not found in Examine's index *yet* it exists, which probably indicates that
            // the index is corrupted. Or not up-to-date. Log a warning, but only once, and only if seeing the
            // error more that a number of times.

            var miss = Interlocked.CompareExchange(ref _examineIndexMiss, 0, 0); // volatile read
            if (miss < ExamineIndexMissMax && Interlocked.Increment(ref _examineIndexMiss) == ExamineIndexMissMax)
                Current.Logger.Warn<PublishedMediaCache,int>("Failed ({ExamineIndexMissMax} times) to retrieve medias from Examine index and had to load"
                    + " them from DB. This may indicate that the Examine index is corrupted.", ExamineIndexMissMax);

            return ConvertFromIMedia(media);
        }

        private const int ExamineIndexMissMax = 10;
        private int _examineIndexMiss;

        internal CacheValues ConvertFromXPathNodeIterator(XPathNodeIterator media, int id)
        {
            if (media?.Current != null)
            {
                return media.Current.Name.InvariantEquals("error")
                    ? null
                    : ConvertFromXPathNavigator(media.Current);
            }

            Current.Logger.Warn<PublishedMediaCache,int>("Could not retrieve media {MediaId} from Examine index or from legacy library.GetMedia method", id);

            return null;
        }

        internal CacheValues ConvertFromSearchResult(ISearchResult searchResult)
        {
            // note: fixing fields in 7.x, removed by Shan for 8.0

            return new CacheValues
            {
                Values = searchResult.Values,
                FromExamine = true
            };
        }

        internal CacheValues ConvertFromXPathNavigator(XPathNavigator xpath, bool forceNav = false)
        {
            if (xpath == null) throw new ArgumentNullException(nameof(xpath));

            var values = new Dictionary<string, string> { { "nodeName", xpath.GetAttribute("nodeName", "") } };
            values["nodeTypeAlias"] = xpath.Name;

            var result = xpath.SelectChildren(XPathNodeType.Element);
            //add the attributes e.g. id, parentId etc
            if (result.Current != null && result.Current.HasAttributes)
            {
                if (result.Current.MoveToFirstAttribute())
                {
                    //checking for duplicate keys because of the 'nodeTypeAlias' might already be added above.
                    if (values.ContainsKey(result.Current.Name) == false)
                    {
                        values[result.Current.Name] = result.Current.Value;
                    }
                    while (result.Current.MoveToNextAttribute())
                    {
                        if (values.ContainsKey(result.Current.Name) == false)
                        {
                            values[result.Current.Name] = result.Current.Value;
                        }
                    }
                    result.Current.MoveToParent();
                }
            }
            // because, migration
            if (values.ContainsKey("key") == false)
                values["key"] = Guid.Empty.ToString();
            //add the user props
            while (result.MoveNext())
            {
                if (result.Current != null && result.Current.HasAttributes == false)
                {
                    var value = result.Current.Value;
                    if (string.IsNullOrEmpty(value))
                    {
                        if (result.Current.HasAttributes || result.Current.SelectChildren(XPathNodeType.Element).Count > 0)
                        {
                            value = result.Current.OuterXml;
                        }
                    }
                    values[result.Current.Name] = value;
                }
            }

            return new CacheValues
            {
                Values = values,
                XPath = forceNav ? xpath : null // outside of tests we do NOT want to cache the navigator!
            };
        }

        internal CacheValues ConvertFromIMedia(IMedia media)
        {
            var values = new Dictionary<string, string>();

            var creator = _userService.GetProfileById(media.CreatorId);
            var creatorName = creator == null ? "" : creator.Name;

            values["id"] = media.Id.ToString();
            values["key"] = media.Key.ToString();
            values["parentID"] = media.ParentId.ToString();
            values["level"] = media.Level.ToString();
            values["creatorID"] = media.CreatorId.ToString();
            values["creatorName"] = creatorName;
            values["writerID"] = media.CreatorId.ToString();
            values["writerName"] = creatorName;
            values["template"] = "0";
            values["urlName"] = "";
            values["sortOrder"] = media.SortOrder.ToString();
            values["createDate"] = media.CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
            values["updateDate"] = media.UpdateDate.ToString("yyyy-MM-dd HH:mm:ss");
            values["nodeName"] = media.Name;
            values["path"] = media.Path;
            values["nodeType"] = media.ContentType.Id.ToString();
            values["nodeTypeAlias"] = media.ContentType.Alias;

            // add the user props
            foreach (var prop in media.Properties)
                values[prop.Alias] = prop.GetValue()?.ToString();

            return new CacheValues
            {
                Values = values
            };
        }

        /// <summary>
        /// We will need to first check if the document was loaded by Examine, if so we'll need to check if this property exists
        /// in the results, if it does not, then we'll have to revert to looking up in the db.
        /// </summary>
        /// <param name="dd"> </param>
        /// <param name="alias"></param>
        /// <returns></returns>
        private IPublishedProperty GetProperty(DictionaryPublishedContent dd, string alias)
        {
            //lets check if the alias does not exist on the document.
            //NOTE: Examine will not index empty values and we do not output empty XML Elements to the cache - either of these situations
            // would mean that the property is missing from the collection whether we are getting the value from Examine or from the library media cache.
            if (dd.Properties.All(x => x.Alias.InvariantEquals(alias) == false))
            {
                return null;
            }

            if (dd.LoadedFromExamine)
            {
                //We are going to check for a special field however, that is because in some cases we store a 'Raw'
                //value in the index such as for xml/html.
                var rawValue = dd.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(UmbracoExamineIndex.RawFieldPrefix + alias));
                return rawValue
                       ?? dd.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
            }

            //if its not loaded from examine, then just return the property
            return dd.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        }

        /// <summary>
        /// A Helper methods to return the children for media whether it is based on examine or xml
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        private IEnumerable<IPublishedContent> GetChildrenMedia(int parentId, XPathNavigator xpath = null)
        {
            // if there *is* a navigator, directly look it up
            if (xpath != null)
            {
                return ToIPublishedContent(parentId, xpath);
            }

            // otherwise, try examine first, then re-look it up
            var searchProvider = GetSearchProviderSafe();

            if (searchProvider != null)
            {
                try
                {
                    //first check in Examine as this is WAY faster
                    var criteria = searchProvider.CreateQuery("media");

                    var filter = criteria.ParentId(parentId).Not().Field(UmbracoExamineIndex.IndexPathFieldName, "-1,-21,".MultipleCharacterWildcard())
                        .OrderBy(new SortableField("sortOrder", SortType.Int));
                    //the above filter will create a query like this, NOTE: That since the use of the wildcard, it automatically escapes it in Lucene.
                    //+(+parentId:3113 -__Path:-1,-21,*) +__IndexType:media

                    // sort with the Sort field (updated for 8.0)
                    var results = filter.Execute();

                    if (results.Any())
                    {
                        // var medias = results.Select(ConvertFromSearchResult);
                        var medias = results.Select(x =>
                        {
                            int nid;
                            if (int.TryParse(x["__NodeId"], out nid) == false && int.TryParse(x["NodeId"], out nid) == false)
                                throw new Exception("Failed to extract NodeId from search result.");
                            var cacheValues = GetCacheValues(nid, id => ConvertFromSearchResult(x));
                            return CreateFromCacheValues(cacheValues);
                        });

                        return medias;
                    }

                    //if there's no result then return null. Previously we defaulted back to library.GetMedia below
                    //but this will always get called for when we are getting descendants since many items won't have
                    //children and then we are hitting the database again!
                    //So instead we're going to rely on Examine to have the correct results like it should.
                    return Enumerable.Empty<IPublishedContent>();
                }
                catch (FileNotFoundException)
                {
                    //Currently examine is throwing FileNotFound exceptions when we have a load balanced filestore and a node is published in umbraco
                    //See this thread: http://examine.cdodeplex.com/discussions/264341
                    //Catch the exception here for the time being, and just fallback to GetMedia
                }
            }

            // falling back to get media
            // was library.GetMedia which had its own cache, but MediaService *also* caches
            // so, library.GetMedia is gone and now we directly work with MediaService
            // (code below copied from what library was doing)
            var media = Current.Services.MediaService.GetById(parentId);
            if (media == null)
            {
                return Enumerable.Empty<IPublishedContent>();
            }

            var serialized = _entitySerializer.Serialize(media, true);

            var mediaIterator = serialized.CreateNavigator().Select("/");

            return mediaIterator.Current == null
                ? Enumerable.Empty<IPublishedContent>()
                : ToIPublishedContent(parentId, mediaIterator.Current);
        }


        internal IEnumerable<IPublishedContent> ToIPublishedContent(int parentId, XPathNavigator xpath)
        {
            var mediaList = new List<IPublishedContent>();

            // this is so bad, really
            var item = xpath.Select("//*[@id='" + parentId + "']");
            if (item.Current == null)
                return Enumerable.Empty<IPublishedContent>();
            var items = item.Current.SelectChildren(XPathNodeType.Element);

            // and this does not work, because... meh
            //var q = "//* [@id='" + parentId + "']/* [@id]";
            //var items = xpath.Select(q);

            foreach (XPathNavigator itemm in items)
            {
                int id;
                if (int.TryParse(itemm.GetAttribute("id", ""), out id) == false)
                    continue; // uh?
                var captured = itemm;
                var cacheValues = GetCacheValues(id, idd => ConvertFromXPathNavigator(captured));
                mediaList.Add(CreateFromCacheValues(cacheValues));
            }

            return mediaList;
        }


        internal void Resync()
        {
            // clear recursive properties cached by XmlPublishedContent.GetProperty
            // assume that nothing else is going to cache IPublishedProperty items (else would need to do ByKeySearch)
            // NOTE all properties cleared when clearing the content cache (see content cache)
            //_appCache.ClearCacheObjectTypes<IPublishedProperty>();
            //_appCache.ClearCacheByKeySearch("XmlPublishedCache.PublishedMediaCache:RecursiveProperty-");
        }

        #region Content types

        public override IPublishedContentType GetContentType(int id) => _contentTypeCache.Get(PublishedItemType.Media, id);

        public override IPublishedContentType GetContentType(string alias) => _contentTypeCache.Get(PublishedItemType.Media, alias);

        public override IPublishedContentType GetContentType(Guid key) => _contentTypeCache.Get(PublishedItemType.Media, key);

        public override IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType)
        {
            throw new NotSupportedException();
        }

        #endregion

        // REFACTORING

        // caching the basic atomic values - and the parent id
        // but NOT caching actual parent nor children and NOT even
        // the list of children ids - BUT caching the path

        internal class CacheValues
        {
            public IReadOnlyDictionary<string, string> Values { get; set; }
            public XPathNavigator XPath { get; set; }
            public bool FromExamine { get; set; }
        }

        public const string PublishedMediaCacheKey = "MediaCacheMeh.";
        private const int PublishedMediaCacheTimespanSeconds = 4 * 60; // 4 mins
        private static TimeSpan _publishedMediaCacheTimespan;
        private static bool _publishedMediaCacheEnabled;

        private static void InitializeCacheConfig()
        {
            var value = ConfigurationManager.AppSettings[Constants.AppSettings.PublishedMediaCacheSeconds];
            int seconds;
            if (int.TryParse(value, out seconds) == false)
                seconds = PublishedMediaCacheTimespanSeconds;
            if (seconds > 0)
            {
                _publishedMediaCacheEnabled = true;
                _publishedMediaCacheTimespan = TimeSpan.FromSeconds(seconds);
            }
            else
            {
                _publishedMediaCacheEnabled = false;
            }
        }

        internal IPublishedContent CreateFromCacheValues(CacheValues cacheValues)
        {
            var content = new DictionaryPublishedContent(
                cacheValues.Values,
                parentId => parentId < 0 ? null : GetUmbracoMedia(parentId),
                GetChildrenMedia,
                GetProperty,
                _appCache,
                _contentTypeCache,
                cacheValues.XPath, // though, outside of tests, that should be null
                cacheValues.FromExamine
            );
            return content.CreateModel();
        }

        private static CacheValues GetCacheValues(int id, Func<int, CacheValues> func)
        {
            if (_publishedMediaCacheEnabled == false)
                return func(id);

            var cache = Current.AppCaches.RuntimeCache;
            var key = PublishedMediaCacheKey + id;
            return (CacheValues)cache.Get(key, () => func(id), _publishedMediaCacheTimespan);
        }

        internal static void ClearCache(int id)
        {
            var cache = Current.AppCaches.RuntimeCache;
            var sid = id.ToString();
            var key = PublishedMediaCacheKey + sid;

            // we do clear a lot of things... but the cache refresher is somewhat
            // convoluted and it's hard to tell what to clear exactly ;-(

            // clear the parent - NOT (why?)
            //var exist = (CacheValues) cache.GetCacheItem(key);
            //if (exist != null)
            //    cache.ClearCacheItem(PublishedMediaCacheKey + GetValuesValue(exist.Values, "parentID"));

            // clear the item
            cache.Clear(key);

            // clear all children - in case we moved and their path has changed
            var fid = "/" + sid + "/";
            cache.ClearOfType<CacheValues>((k, v) =>
                GetValuesValue(v.Values, "path", "__Path").Contains(fid));
        }

        private static string GetValuesValue(IReadOnlyDictionary<string, string> d, params string[] keys)
        {
            string value = null;
            var ignored = keys.Any(x => d.TryGetValue(x, out value));
            return value ?? "";
        }
    }
}
