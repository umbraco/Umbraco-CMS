using System;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using UmbracoExamine;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using umbraco.interfaces;
using Content = umbraco.cms.businesslogic.Content;
using Document = umbraco.cms.businesslogic.web.Document;
using Member = umbraco.cms.businesslogic.member.Member;

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

			var registeredProviders = ExamineManager.Instance.IndexProviderCollection
				.OfType<BaseUmbracoIndexer>().Count(x => x.EnableDefaultEventHandler);

			LogHelper.Info<ExamineEvents>("Adding examine event handlers for index providers: {0}", () => registeredProviders);

			//don't bind event handlers if we're not suppose to listen
			if (registeredProviders == 0)
				return;

            MediaService.Saved += MediaServiceSaved;
            MediaService.Deleted += MediaServiceDeleted;
            MediaService.Moved += MediaServiceMoved;
            MediaService.Trashed += MediaServiceTrashed;

            ContentService.Saved += ContentServiceSaved;
            ContentService.Deleted += ContentServiceDeleted;
            ContentService.Moved += ContentServiceMoved;
            ContentService.Trashed += ContentServiceTrashed;

			//These should only fire for providers that DONT have SupportUnpublishedContent set to true
			content.AfterUpdateDocumentCache += ContentAfterUpdateDocumentCache;
			content.AfterClearDocumentCache += ContentAfterClearDocumentCache;

            //TODO: Remove the legacy event handlers once we proxy the legacy members stuff through the new services
			Member.AfterSave += MemberAfterSave;
			Member.AfterDelete += MemberAfterDelete;
            MemberService.Saved += MemberServiceSaved;
            MemberService.Deleted += MemberServiceDeleted;

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

	    static void ContentServiceTrashed(IContentService sender, Core.Events.MoveEventArgs<IContent> e)
        {
            IndexConent(e.Entity);
        }

        
	    static void MediaServiceTrashed(IMediaService sender, Core.Events.MoveEventArgs<IMedia> e)
        {
            IndexMedia(e.Entity);
        }

        
        static void ContentServiceMoved(IContentService sender, Umbraco.Core.Events.MoveEventArgs<IContent> e)
        {
            IndexConent(e.Entity);
        }

        
        static void ContentServiceDeleted(IContentService sender, Umbraco.Core.Events.DeleteEventArgs<IContent> e)
        {
            e.DeletedEntities.ForEach(
                content =>
                ExamineManager.Instance.DeleteFromIndex(
                    content.Id.ToString(),
                    ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler)));
        }

        
        static void ContentServiceSaved(IContentService sender, Umbraco.Core.Events.SaveEventArgs<IContent> e)
        {
            e.SavedEntities.ForEach(IndexConent);
        }

        
        static void MediaServiceMoved(IMediaService sender, Umbraco.Core.Events.MoveEventArgs<IMedia> e)
        {
            IndexMedia(e.Entity);
        }

        
        static void MediaServiceDeleted(IMediaService sender, Umbraco.Core.Events.DeleteEventArgs<IMedia> e)
        {
            e.DeletedEntities.ForEach(
                media =>
                ExamineManager.Instance.DeleteFromIndex(
                    media.Id.ToString(),
                    ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler)));
        }

        
        static void MediaServiceSaved(IMediaService sender, Umbraco.Core.Events.SaveEventArgs<IMedia> e)
        {
            e.SavedEntities.ForEach(IndexMedia);
        }

        static void MemberServiceSaved(IMemberService sender, Core.Events.SaveEventArgs<IMember> e)
        {
            foreach (var m in e.SavedEntities)
            {
                var xml = m.ToXml();
                //ensure that only the providers are flagged to listen execute
                var providers = ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
                    .Where(x => x.EnableDefaultEventHandler);
                ExamineManager.Instance.ReIndexNode(xml, IndexTypes.Member, providers);
            }
        }

        static void MemberServiceDeleted(IMemberService sender, Core.Events.DeleteEventArgs<IMember> e)
        {
            foreach (var m in e.DeletedEntities)
            {
                var nodeId = m.Id.ToString(CultureInfo.InvariantCulture);
                //ensure that only the providers are flagged to listen execute
                ExamineManager.Instance.DeleteFromIndex(nodeId,
                    ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
                        .Where(x => x.EnableDefaultEventHandler));
            }
        }

		private static void MemberAfterSave(Member sender, SaveEventArgs e)
		{
			//ensure that only the providers are flagged to listen execute
			var xml = ExamineXmlExtensions.ToXElement(sender.ToXml(new System.Xml.XmlDocument(), false));
			var providers = ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
				.Where(x => x.EnableDefaultEventHandler);
			ExamineManager.Instance.ReIndexNode(xml, IndexTypes.Member, providers);
		}

		
		private static void MemberAfterDelete(Member sender, DeleteEventArgs e)
		{
			var nodeId = sender.Id.ToString(CultureInfo.InvariantCulture);

			//ensure that only the providers are flagged to listen execute
			ExamineManager.Instance.DeleteFromIndex(nodeId,
				ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
					.Where(x => x.EnableDefaultEventHandler));
		}

		/// <summary>
		/// Only Update indexes for providers that dont SupportUnpublishedContent
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		
		private static void ContentAfterUpdateDocumentCache(Document sender, DocumentCacheEventArgs e)
		{
			//ensure that only the providers that have DONT unpublishing support enabled       
			//that are also flagged to listen
			ExamineManager.Instance.ReIndexNode(ToXDocument(sender, true).Root, IndexTypes.Content,
				ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
					.Where(x => !x.SupportUnpublishedContent
						&& x.EnableDefaultEventHandler));
		}

		/// <summary>
		/// Only update indexes for providers that don't SupportUnpublishedContnet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		
		private static void ContentAfterClearDocumentCache(Document sender, DocumentCacheEventArgs e)
		{
			var nodeId = sender.Id.ToString();
			//ensure that only the providers that DONT have unpublishing support enabled           
			//that are also flagged to listen
			ExamineManager.Instance.DeleteFromIndex(nodeId,
				ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
					.Where(x => !x.SupportUnpublishedContent
						&& x.EnableDefaultEventHandler));
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


        private static void IndexMedia(IMedia sender)
        {
            ExamineManager.Instance.ReIndexNode(
                sender.ToXml(), "media",
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler));
        }

        private static void IndexConent(IContent sender)
        {            
            //only index this content if the indexer supports unpublished content. that is because the 
            // content.AfterUpdateDocumentCache will handle anything being published and will only index against indexers
            // that only support published content. 
            // NOTE: The events for publishing have changed slightly from 6.0 to 6.1 and are streamlined in 6.1. Before
            // this event would fire before publishing, then again after publishing. Now the save event fires once before
            // publishing and that is all. 
            
            ExamineManager.Instance.ReIndexNode(
                sender.ToXml(), "content",
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
                    .Where(x => x.SupportUnpublishedContent && x.EnableDefaultEventHandler));
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
                return new XDocument(((Document) node).Content.ToXml());
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