using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine;
using Examine.Session;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using UmbracoExamine;

namespace Umbraco.Web.Search
{
	/// <summary>
	/// Used to wire up events for Examine
	/// </summary>
	public sealed class ExamineEvents : ApplicationEventHandler
	{

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

            //TODO: For now we'll make this true, it means that indexes will be near real time
            // we'll see about what implications this may have - should be great in most scenarios
		    DefaultExamineSession.RequireImmediateConsistency = true;

			var registeredProviders = ExamineManager.Instance.IndexProviderCollection
				.OfType<BaseUmbracoIndexer>().Count(x => x.EnableDefaultEventHandler);

			LogHelper.Info<ExamineEvents>("Adding examine event handlers for index providers: {0}", () => registeredProviders);

			// don't bind event handlers if we're not suppose to listen
			if (registeredProviders == 0)
				return;

            // bind to distributed cache events - this ensures that this logic occurs on ALL servers
            // that are taking part in a load balanced environment.
		    ContentCacheRefresher.CacheUpdated += ContentCacheRefresherUpdated;
            MediaCacheRefresher.CacheUpdated += MediaCacheRefresherUpdated;
            MemberCacheRefresher.CacheUpdated += MemberCacheRefresherUpdated;

			var contentIndexer = ExamineManager.Instance.IndexProviderCollection["InternalIndexer"] as UmbracoContentIndexer;
			if (contentIndexer != null)
			{
				contentIndexer.DocumentWriting += IndexerDocumentWriting;
			}
			var memberIndexer = ExamineManager.Instance.IndexProviderCollection["InternalMemberIndexer"] as UmbracoMemberIndexer;
			if (memberIndexer != null)
			{
				memberIndexer.DocumentWriting += IndexerDocumentWriting;
			}
		}

	    static void MemberCacheRefresherUpdated(MemberCacheRefresher sender, CacheRefresherEventArgs args)
	    {
            switch (args.MessageType)
            {
                case MessageType.RefreshById:
                    var c1 = ApplicationContext.Current.Services.MemberService.GetById((int)args.MessageObject);
                    if (c1 != null)
                    {
                        ReIndexForMember(c1);
                    }
                    break;
                case MessageType.RemoveById:

                    // This is triggered when the item is permanently deleted

                    DeleteIndexForEntity((int)args.MessageObject, false);
                    break;
                case MessageType.RefreshByInstance:
                    var c3 = args.MessageObject as IMember;
                    if (c3 != null)
                    {
                        ReIndexForMember(c3);
                    }
                    break;
                case MessageType.RemoveByInstance:

                    // This is triggered when the item is permanently deleted

                    var c4 = args.MessageObject as IMember;
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

	    static void MediaCacheRefresherUpdated(MediaCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (args.MessageType != MessageType.RefreshByPayload)
                throw new NotSupportedException();

            var mediaService = ApplicationContext.Current.Services.MediaService;

	        foreach (var payload in (MediaCacheRefresher.JsonPayload[]) args.MessageObject)
	        {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
	            {
                    // remove from *all* indexes
	                DeleteIndexForEntity(payload.Id, false);
	            }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    // ExamineEvents does not support RefreshAll
                    // just ignore that payload
                    // so what?!
                }
                else // RefreshNode or RefreshBranch (maybe trashed)
                {
	                var media = mediaService.GetById(payload.Id);
                    if (media == null || media.Trashed)
                    {
                        // gone fishing, remove entirely
                        DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    // just that media
                    ReIndexForMedia(media, media.Trashed == false);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        var descendants = mediaService.GetDescendants(media);
                        foreach (var descendant in descendants)
                        {
                            ReIndexForMedia(descendant, descendant.Trashed == false);
                        }
                    }
                }
	        }
        }

	    static void ContentCacheRefresherUpdated(ContentCacheRefresher sender, CacheRefresherEventArgs args)
	    {
            if (args.MessageType != MessageType.RefreshByPayload)
                throw new NotSupportedException();

	        var contentService = ApplicationContext.Current.Services.ContentService;

	        foreach (var payload in (ContentCacheRefresher.JsonPayload[]) args.MessageObject)
	        {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
	            {
                    // delete content entirely (with descendants)
                    //  false: remove entirely from all indexes
                    DeleteIndexForEntity(payload.Id, false);
	            }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    // ExamineEvents does not support RefreshAll
                    // just ignore that payload
                    // so what?!
                }
                else // RefreshNode or RefreshBranch (maybe trashed)
                {
    	            // don't try to be too clever - refresh entirely
                    // there has to be race conds in there ;-(

	                var content = contentService.GetById(payload.Id);
                    if (content == null || content.Trashed)
                    {
                        // gone fishing, remove entirely from all indexes (with descendants)
                        DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    IContent published = null;
                    if (content.HasPublishedVersion && ((ContentService)contentService).IsPathPublished(content))
                    {
                        published = content.Published
                            ? content
                            : contentService.GetByVersion(content.PublishedVersionGuid);
                    }

                    // just that content
                    ReIndexForContent(content, published);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        var masked = published == null ? null : new List<int>();
                        var descendants = contentService.GetDescendants(content);
                        foreach (var descendant in descendants)
                        {
                            published = null;
                            if (masked != null) // else everything is masked
                            {
                                if (masked.Contains(descendant.ParentId) || descendant.HasPublishedVersion == false)
                                {
                                    masked.Add(descendant.Id);
                                }
                                else
                                {
                                    published = descendant.Published
                                        ? descendant
                                        : contentService.GetByVersion(descendant.PublishedVersionGuid);
                                }
                            }

                            ReIndexForContent(descendant, published);
                        }
                    }
                }

                // NOTE
                //
                // DeleteIndexForEntity is handled by UmbracoContentIndexer.DeleteFromIndex() which takes
                //  care of also deleting the descendants
                //
                // ReIndexForContent is NOT taking care of descendants so we have to reload everything
                //  again in order to process the branch - we COULD improve that by just reloading the
                //  XML from database instead of reloading content & re-serializing!
	        }
	    }

        private static void ReIndexForContent(IContent content, IContent published)
        {
            if (published != null && content.Version == published.Version)
            {
                ReIndexForContent(content); // same = both
            }
            else
            {
                if (published == null)
                {
                    // remove 'published' - keep 'draft'
                    DeleteIndexForEntity(content.Id, true);
                }
                else
                {
                    // index 'published' - don't overwrite 'draft'
                    ReIndexForContent(published, false);
                }
                ReIndexForContent(content, true); // index 'draft'
            }
        }

        private static void ReIndexForContent(IContent sender, bool? supportUnpublished = null)
        {
            var xml = sender.ToXml();
            //add an icon attribute to get indexed
            xml.Add(new XAttribute("icon", sender.ContentType.Icon));

            ExamineManager.Instance.ReIndexNode(
                xml, IndexTypes.Content,
                ExamineManager.Instance.IndexProviderCollection.OfType<UmbracoContentIndexer>()
                    // only for the specified indexers
                    .Where(x => supportUnpublished.HasValue == false || supportUnpublished.Value == x.SupportUnpublishedContent)
                    .Where(x => x.EnableDefaultEventHandler));
        }

        private static void ReIndexForMember(IMember member)
		{
		    ExamineManager.Instance.ReIndexNode(
		        member.ToXml(), IndexTypes.Member,
		        ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
                    //ensure that only the providers are flagged to listen execute
		            .Where(x => x.EnableDefaultEventHandler));
		}

        private static void ReIndexForMedia(IMedia sender, bool isMediaPublished)
        {
            var xml = sender.ToXml();
            //add an icon attribute to get indexed
            xml.Add(new XAttribute("icon", sender.ContentType.Icon));

            ExamineManager.Instance.ReIndexNode(
                xml, IndexTypes.Media,
                ExamineManager.Instance.IndexProviderCollection.OfType<UmbracoContentIndexer>()
                    // index this item for all indexers if the media is not trashed, otherwise if the item is trashed
                    // then only index this for indexers supporting unpublished media
                    .Where(x => isMediaPublished || (x.SupportUnpublishedContent))
                    .Where(x => x.EnableDefaultEventHandler));
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
            ExamineManager.Instance.DeleteFromIndex(
                entityId.ToString(CultureInfo.InvariantCulture),
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
                    // if keepIfUnpublished == true then only delete this item from indexes not supporting unpublished content,
                    // otherwise if keepIfUnpublished == false then remove from all indexes
                    .Where(x => keepIfUnpublished == false || (x is UmbracoContentIndexer && ((UmbracoContentIndexer)x).SupportUnpublishedContent == false))
                    .Where(x => x.EnableDefaultEventHandler));
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
	}
}