using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using UmbracoExamine;
using Content = umbraco.cms.businesslogic.Content;
using Document = umbraco.cms.businesslogic.web.Document;

namespace Umbraco.Web.Search
{
	/// <summary>
	/// Used to wire up events for Examine
	/// </summary>
	public sealed class ExamineEvents : ApplicationEventHandler
	{
	    // the default enlist priority is 100
	    // enlist with a lower priority to ensure that anything "default" runs after us
        // but greater that SafeXmlReaderWriter priority which is 60
	    private const int EnlistPriority = 80;

	    /// <summary>
		/// Once the application has started we should bind to all events and initialize the providers.
		/// </summary>
		/// <param name="httpApplication"></param>
		/// <param name="applicationContext"></param>
		/// <remarks>
		/// We need to do this on the Started event as to guarantee that all resolvers are setup properly.
		/// </remarks>
		protected override void ApplicationStarted(UmbracoApplicationBase httpApplication, ApplicationContext applicationContext)
		{
            LogHelper.Info<ExamineEvents>("Initializing Examine and binding to business logic events");

			var registeredProviders = ExamineManager.Instance.IndexProviderCollection
				.OfType<BaseUmbracoIndexer>().Count(x => x.EnableDefaultEventHandler);

			LogHelper.Info<ExamineEvents>("Adding examine event handlers for index providers: {0}", () => registeredProviders);

			//don't bind event handlers if we're not suppose to listen
			if (registeredProviders == 0)
				return;

            //Bind to distributed cache events - this ensures that this logic occurs on ALL servers that are taking part
            // in a load balanced environment.
            CacheRefresherBase<UnpublishedPageCacheRefresher>.CacheUpdated += UnpublishedPageCacheRefresherCacheUpdated;
            CacheRefresherBase<PageCacheRefresher>.CacheUpdated += PublishedPageCacheRefresherCacheUpdated;
            CacheRefresherBase<MediaCacheRefresher>.CacheUpdated += MediaCacheRefresherCacheUpdated;
            CacheRefresherBase<MemberCacheRefresher>.CacheUpdated += MemberCacheRefresherCacheUpdated;
            CacheRefresherBase<ContentTypeCacheRefresher>.CacheUpdated += ContentTypeCacheRefresherCacheUpdated;

			var contentIndexer = ExamineManager.Instance.IndexProviderCollection[Constants.Examine.InternalIndexer] as UmbracoContentIndexer;
			if (contentIndexer != null)
			{
				contentIndexer.DocumentWriting += IndexerDocumentWriting;
			}
			var memberIndexer = ExamineManager.Instance.IndexProviderCollection[Constants.Examine.InternalMemberIndexer] as UmbracoMemberIndexer;
			if (memberIndexer != null)
			{
				memberIndexer.DocumentWriting += IndexerDocumentWriting;
			}
		}

        /// <summary>
        /// This is used to refresh content indexers IndexData based on the DataService whenever a content type is changed since
        /// properties may have been added/removed, then we need to re-index any required data if aliases have been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// See: http://issues.umbraco.org/issue/U4-4798, http://issues.umbraco.org/issue/U4-7833
        /// </remarks>
	    static void ContentTypeCacheRefresherCacheUpdated(ContentTypeCacheRefresher sender, CacheRefresherEventArgs e)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            var indexersToUpdated = ExamineManager.Instance.IndexProviderCollection.OfType<UmbracoContentIndexer>();
            foreach (var provider in indexersToUpdated)
            {
                provider.RefreshIndexerDataFromDataService();
            }

            if (e.MessageType == MessageType.RefreshByJson)
            {
                var contentTypesChanged = new HashSet<string>();
                var mediaTypesChanged = new HashSet<string>();
                var memberTypesChanged = new HashSet<string>();

                var payloads = ContentTypeCacheRefresher.DeserializeFromJsonPayload(e.MessageObject.ToString());
                foreach (var payload in payloads)
                {
                    if (payload.IsNew == false
                        && (payload.WasDeleted || payload.AliasChanged || payload.PropertyRemoved || payload.PropertyTypeAliasChanged))
                    {
                        //if we get here it means that some aliases have changed and the indexes for those particular doc types will need to be updated
                        if (payload.Type == typeof(IContentType).Name)
                        {
                            //if it is content
                            contentTypesChanged.Add(payload.Alias);
                        }
                        else if (payload.Type == typeof(IMediaType).Name)
                        {
                            //if it is media
                            mediaTypesChanged.Add(payload.Alias);
                        }
                        else if (payload.Type == typeof(IMemberType).Name)
                        {
                            //if it is members
                            memberTypesChanged.Add(payload.Alias);
                        }
                    }
                }

                //TODO: We need to update Examine to support re-indexing multiple items at once instead of one by one which will speed up
                // the re-indexing process, we don't want to revert to rebuilding the whole thing!

                if (contentTypesChanged.Count > 0)
                {
                    foreach (var alias in contentTypesChanged)
                    {
                        var ctType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(alias);
                        if (ctType != null)
                        {
                            var contentItems = ApplicationContext.Current.Services.ContentService.GetContentOfContentType(ctType.Id);
                            foreach (var contentItem in contentItems)
                            {
                                ReIndexForContent(contentItem, contentItem.HasPublishedVersion && contentItem.Trashed == false);
                            }
                        }
                    }
                }
                if (mediaTypesChanged.Count > 0)
                {
                    foreach (var alias in mediaTypesChanged)
                    {
                        var ctType = ApplicationContext.Current.Services.ContentTypeService.GetMediaType(alias);
                        if (ctType != null)
                        {
                            var mediaItems = ApplicationContext.Current.Services.MediaService.GetMediaOfMediaType(ctType.Id);
                            foreach (var mediaItem in mediaItems)
                            {
                                ReIndexForMedia(mediaItem, mediaItem.Trashed == false);
                            }
                        }
                    }
                }
                if (memberTypesChanged.Count > 0)
                {
                    foreach (var alias in memberTypesChanged)
                    {
                        var ctType = ApplicationContext.Current.Services.MemberTypeService.Get(alias);
                        if (ctType != null)
                        {
                            var memberItems = ApplicationContext.Current.Services.MemberService.GetMembersByMemberType(ctType.Id);
                            foreach (var memberItem in memberItems)
                            {
                                ReIndexForMember(memberItem);
                            }
                        }
                    }
                }
            }

        }

	    static void MemberCacheRefresherCacheUpdated(MemberCacheRefresher sender, CacheRefresherEventArgs e)
	    {
	        if (Suspendable.ExamineEvents.CanIndex == false)
	            return;

            switch (e.MessageType)
            {
                case MessageType.RefreshById:
                    var c1 = ApplicationContext.Current.Services.MemberService.GetById((int)e.MessageObject);
                    if (c1 != null)
                    {
                        ReIndexForMember(c1);
                    }
                    break;
                case MessageType.RemoveById:

                    // This is triggered when the item is permanently deleted

                    DeleteIndexForEntity((int)e.MessageObject, false);
                    break;
                case MessageType.RefreshByInstance:
                    var c3 = e.MessageObject as IMember;
                    if (c3 != null)
                    {
                        ReIndexForMember(c3);
                    }
                    break;
                case MessageType.RemoveByInstance:

                    // This is triggered when the item is permanently deleted

                    var c4 = e.MessageObject as IMember;
                    if (c4 != null)
                    {
                        DeleteIndexForEntity(c4.Id, false);
                    }
                    break;
                case MessageType.RefreshAll:
                case MessageType.RefreshByJson:
                default:
                    //We don't support these, these message types will not fire for unpublished content
                    break;
            }
	    }

	    /// <summary>
        /// Handles index management for all media events - basically handling saving/copying/trashing/deleting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
	    static void MediaCacheRefresherCacheUpdated(MediaCacheRefresher sender, CacheRefresherEventArgs e)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            switch (e.MessageType)
            {
                case MessageType.RefreshById:
                    var c1 = ApplicationContext.Current.Services.MediaService.GetById((int)e.MessageObject);
                    if (c1 != null)
                    {
                        ReIndexForMedia(c1, c1.Trashed == false);
                    }
                    break;
                case MessageType.RemoveById:
                    var c2 = ApplicationContext.Current.Services.MediaService.GetById((int)e.MessageObject);
                    if (c2 != null)
                    {
                        //This is triggered when the item has trashed.
                        // So we need to delete the index from all indexes not supporting unpublished content.

                        DeleteIndexForEntity(c2.Id, true);

                        //We then need to re-index this item for all indexes supporting unpublished content

                        ReIndexForMedia(c2, false);
                    }
                    break;
                case MessageType.RefreshByJson:

                    var jsonPayloads = MediaCacheRefresher.DeserializeFromJsonPayload((string)e.MessageObject);
                    if (jsonPayloads.Any())
                    {
                        foreach (var payload in jsonPayloads)
                        {
                            switch (payload.Operation)
                            {
                                case MediaCacheRefresher.OperationType.Saved:
                                    var media1 = ApplicationContext.Current.Services.MediaService.GetById(payload.Id);
                                    if (media1 != null)
                                    {
                                        ReIndexForMedia(media1, media1.Trashed == false);
                                    }
                                    break;
                                case MediaCacheRefresher.OperationType.Trashed:

                                    //keep if trashed for indexes supporting unpublished
                                    //(delete the index from all indexes not supporting unpublished content)

                                    DeleteIndexForEntity(payload.Id, true);

                                    //We then need to re-index this item for all indexes supporting unpublished content
                                    var media2 = ApplicationContext.Current.Services.MediaService.GetById(payload.Id);
                                    if (media2 != null)
                                    {
                                        ReIndexForMedia(media2, false);
                                    }

                                    break;
                                case MediaCacheRefresher.OperationType.Deleted:

                                    //permanently remove from all indexes

                                    DeleteIndexForEntity(payload.Id, false);

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }

                    break;
                case MessageType.RefreshByInstance:
                case MessageType.RemoveByInstance:
                case MessageType.RefreshAll:
                default:
                    //We don't support these, these message types will not fire for media
                    break;
            }
        }

        /// <summary>
        /// Handles index management for all published content events - basically handling published/unpublished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This will execute on all servers taking part in load balancing
        /// </remarks>
        static void PublishedPageCacheRefresherCacheUpdated(PageCacheRefresher sender, CacheRefresherEventArgs e)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            switch (e.MessageType)
            {
                case MessageType.RefreshById:
                    var c1 = ApplicationContext.Current.Services.ContentService.GetById((int)e.MessageObject);
                    if (c1 != null)
                    {
                        ReIndexForContent(c1, true);
                    }
                    break;
                case MessageType.RemoveById:

                    //This is triggered when the item has been unpublished or trashed (which also performs an unpublish).

                    var c2 = ApplicationContext.Current.Services.ContentService.GetById((int)e.MessageObject);
                    if (c2 != null)
                    {
                        // So we need to delete the index from all indexes not supporting unpublished content.

                        DeleteIndexForEntity(c2.Id, true);

                        // We then need to re-index this item for all indexes supporting unpublished content

                        ReIndexForContent(c2, false);
                    }
                    break;
                case MessageType.RefreshByInstance:
                    var c3 = e.MessageObject as IContent;
                    if (c3 != null)
                    {
                        ReIndexForContent(c3, true);
                    }
                    break;
                case MessageType.RemoveByInstance:

                    //This is triggered when the item has been unpublished or trashed (which also performs an unpublish).

                    var c4 = e.MessageObject as IContent;
                    if (c4 != null)
                    {
                        // So we need to delete the index from all indexes not supporting unpublished content.

                        DeleteIndexForEntity(c4.Id, true);

                        // We then need to re-index this item for all indexes supporting unpublished content

                        ReIndexForContent(c4, false);
                    }
                    break;
                case MessageType.RefreshAll:
                case MessageType.RefreshByJson:
                default:
                    //We don't support these for examine indexing
                    break;
            }
        }

        /// <summary>
        /// Handles index management for all unpublished content events - basically handling saving/copying/deleting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This will execute on all servers taking part in load balancing
        /// </remarks>
	    static void UnpublishedPageCacheRefresherCacheUpdated(UnpublishedPageCacheRefresher sender, CacheRefresherEventArgs e)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            switch (e.MessageType)
            {
                case MessageType.RefreshById:
                    var c1 = ApplicationContext.Current.Services.ContentService.GetById((int) e.MessageObject);
                    if (c1 != null)
                    {
                        ReIndexForContent(c1, false);
                    }
                    break;
                case MessageType.RemoveById:

                    // This is triggered when the item is permanently deleted

                    DeleteIndexForEntity((int)e.MessageObject, false);
                    break;
                case MessageType.RefreshByInstance:
                    var c3 = e.MessageObject as IContent;
                    if (c3 != null)
                    {
                        ReIndexForContent(c3, false);
                    }
                    break;
                case MessageType.RemoveByInstance:

                    // This is triggered when the item is permanently deleted

                    var c4 = e.MessageObject as IContent;
                    if (c4 != null)
                    {
                        DeleteIndexForEntity(c4.Id, false);
                    }
                    break;
                case MessageType.RefreshByJson:

                    var jsonPayloads = UnpublishedPageCacheRefresher.DeserializeFromJsonPayload((string)e.MessageObject);
                    if (jsonPayloads.Any())
                    {
                        foreach (var payload in jsonPayloads)
                        {
                            switch (payload.Operation)
                            {
                                case UnpublishedPageCacheRefresher.OperationType.Deleted:

                                    //permanently remove from all indexes

                                    DeleteIndexForEntity(payload.Id, false);

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }

                    break;

                case MessageType.RefreshAll:
                default:
                    //We don't support these, these message types will not fire for unpublished content
                    break;
            }
        }

        private static void ReIndexForMember(IMember member)
		{
		    var actions = DeferedActions.Get(ApplicationContext.Current.ScopeProvider);
		    if (actions != null)
		        actions.Add(new DeferedReIndexForMember(member));
		    else
		        DeferedReIndexForMember.Execute(member);
		}

		/// <summary>
		/// Event handler to create a lower cased version of the node name, this is so we can support case-insensitive searching and still
		/// use the Whitespace Analyzer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private static void IndexerDocumentWriting(object sender, DocumentWritingEventArgs e)
		{
			if (e.Fields.Keys.Contains("nodeName"))
			{
                //TODO: This logic should really be put into the content indexer instead of hidden here!!

				//add the lower cased version
				e.Document.Add(new Field("__nodeName",
										e.Fields["nodeName"].ToLower(),
										Field.Store.YES,
										Field.Index.ANALYZED,
										Field.TermVector.NO
										));
			}
		}

        private static void ReIndexForMedia(IMedia sender, bool isMediaPublished)
        {
            var actions = DeferedActions.Get(ApplicationContext.Current.ScopeProvider);
            if (actions != null)
                actions.Add(new DeferedReIndexForMedia(sender, isMediaPublished));
            else
                DeferedReIndexForMedia.Execute(sender, isMediaPublished);
        }

	    /// <summary>
	    /// Remove items from any index that doesn't support unpublished content
	    /// </summary>
        /// <param name="entityId"></param>
	    /// <param name="keepIfUnpublished">
	    /// If true, indicates that we will only delete this item from indexes that don't support unpublished content.
	    /// If false it will delete this from all indexes regardless.
	    /// </param>
	    private static void DeleteIndexForEntity(int entityId, bool keepIfUnpublished)
	    {
	        var actions = DeferedActions.Get(ApplicationContext.Current.ScopeProvider);
	        if (actions != null)
	            actions.Add(new DeferedDeleteIndex(entityId, keepIfUnpublished));
	        else
	            DeferedDeleteIndex.Execute(entityId, keepIfUnpublished);
	    }

	    /// <summary>
	    /// Re-indexes a content item whether published or not but only indexes them for indexes supporting unpublished content
	    /// </summary>
	    /// <param name="sender"></param>
	    /// <param name="isContentPublished">
	    /// Value indicating whether the item is published or not
	    /// </param>
	    private static void ReIndexForContent(IContent sender, bool isContentPublished)
	    {
	        var actions = DeferedActions.Get(ApplicationContext.Current.ScopeProvider);
	        if (actions != null)
                actions.Add(new DeferedReIndexForContent(sender, isContentPublished));
            else
                DeferedReIndexForContent.Execute(sender, isContentPublished);
	    }

	    private class DeferedActions
	    {
	        private readonly List<DeferedAction> _actions = new List<DeferedAction>();

	        public static DeferedActions Get(IScopeProvider scopeProvider)
	        {
	            var scopeContext = scopeProvider.Context;
	            if (scopeContext == null) return null;

	            return scopeContext.Enlist("examineEvents",
	                () => new DeferedActions(), // creator
	                (completed, actions) => // action
	                {
	                    if (completed) actions.Execute();
	                }, EnlistPriority);
	        }

	        public void Add(DeferedAction action)
	        {
                _actions.Add(action);
	        }

	        private void Execute()
	        {
	            foreach (var action in _actions)
	                action.Execute();
	        }
	    }

	    private abstract class DeferedAction
	    {
            public virtual void Execute()
            { }
	    }

	    private class DeferedReIndexForContent : DeferedAction
	    {
	        private readonly IContent _content;
	        private readonly bool _isPublished;

	        public DeferedReIndexForContent(IContent content, bool isPublished)
	        {
	            _content = content;
	            _isPublished = isPublished;
	        }

	        public override void Execute()
	        {
                Execute(_content, _isPublished);
	        }

	        public static void Execute(IContent content, bool isPublished)
	        {
	            var xml = content.ToXml();
	            //add an icon attribute to get indexed
	            xml.Add(new XAttribute("icon", content.ContentType.Icon));

	            ExamineManager.Instance.ReIndexNode(
	                xml, IndexTypes.Content,
	                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()

	                    //Index this item for all indexers if the content is published, otherwise if the item is not published
	                    // then only index this for indexers supporting unpublished content

	                    .Where(x => isPublished || (x.SupportUnpublishedContent))
	                    .Where(x => x.EnableDefaultEventHandler));
	        }
	    }

	    private class DeferedReIndexForMedia : DeferedAction
	    {
	        private readonly IMedia _media;
	        private readonly bool _isPublished;

	        public DeferedReIndexForMedia(IMedia media, bool isPublished)
	        {
	            _media = media;
	            _isPublished = isPublished;
	        }

	        public override void Execute()
	        {
	            Execute(_media, _isPublished);
	        }

	        public static void Execute(IMedia media, bool isPublished)
	        {
	            var xml = media.ToXml();
	            //add an icon attribute to get indexed
	            xml.Add(new XAttribute("icon", media.ContentType.Icon));

	            ExamineManager.Instance.ReIndexNode(
	                xml, IndexTypes.Media,
	                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()

	                    //Index this item for all indexers if the media is not trashed, otherwise if the item is trashed
	                    // then only index this for indexers supporting unpublished media

	                    .Where(x => isPublished || (x.SupportUnpublishedContent))
	                    .Where(x => x.EnableDefaultEventHandler));
	        }
	    }

	    private class DeferedReIndexForMember : DeferedAction
	    {
	        private readonly IMember _member;

	        public DeferedReIndexForMember(IMember member)
	        {
	            _member = member;
	        }

	        public override void Execute()
	        {
	            Execute(_member);
	        }

	        public static void Execute(IMember member)
	        {
	            ExamineManager.Instance.ReIndexNode(
	                member.ToXml(), IndexTypes.Member,
	                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
	                    //ensure that only the providers are flagged to listen execute
	                    .Where(x => x.EnableDefaultEventHandler));
	        }
	    }

	    private class DeferedDeleteIndex : DeferedAction
	    {
	        private readonly int _id;
	        private readonly bool _keepIfUnpublished;

	        public DeferedDeleteIndex(int id, bool keepIfUnpublished)
	        {
	            _id = id;
	            _keepIfUnpublished = keepIfUnpublished;
	        }

	        public override void Execute()
	        {
	            Execute(_id, _keepIfUnpublished);
	        }

	        public static void Execute(int id, bool keepIfUnpublished)
	        {
	            ExamineManager.Instance.DeleteFromIndex(
	                id.ToString(CultureInfo.InvariantCulture),
	                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()

	                    //if keepIfUnpublished == true then only delete this item from indexes not supporting unpublished content,
	                    // otherwise if keepIfUnpublished == false then remove from all indexes

	                    .Where(x => keepIfUnpublished == false || x.SupportUnpublishedContent == false)
	                    .Where(x => x.EnableDefaultEventHandler));
	        }
	    }

	    /// <summary>
		/// Converts a content node to XDocument
		/// </summary>
		/// <param name="node"></param>
		/// <param name="cacheOnly">true if data is going to be returned from cache</param>
		/// <returns></returns>
        [Obsolete("This method is no longer used and will be removed from the core in future versions, the cacheOnly parameter has no effect. Use the other ToXDocument overload instead")]
		public static XDocument ToXDocument(Content node, bool cacheOnly)
		{
			return ToXDocument(node);
		}

		/// <summary>
		/// Converts a content node to Xml
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private static XDocument ToXDocument(Content node)
		{
            if (TypeHelper.IsTypeAssignableFrom<Document>(node))
            {
                return new XDocument(((Document) node).ContentEntity.ToXml());
            }

            if (TypeHelper.IsTypeAssignableFrom<global::umbraco.cms.businesslogic.media.Media>(node))
            {
                return new XDocument(((global::umbraco.cms.businesslogic.media.Media) node).MediaItem.ToXml());
            }

			var xDoc = new XmlDocument();
			var xNode = xDoc.CreateNode(XmlNodeType.Element, "node", "");
			node.XmlPopulate(xDoc, ref xNode, false);

			if (xNode.Attributes["nodeTypeAlias"] == null)
			{
				//we'll add the nodeTypeAlias ourselves
				XmlAttribute d = xDoc.CreateAttribute("nodeTypeAlias");
				d.Value = node.ContentType.Alias;
				xNode.Attributes.Append(d);
			}

			return new XDocument(ExamineXmlExtensions.ToXElement(xNode));
		}
	}
}
