using System;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.Logging;
using UmbracoExamine;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using umbraco.interfaces;
using Document = umbraco.cms.businesslogic.web.Document;

namespace Umbraco.Web.Search
{
	/// <summary>
	/// Used to wire up events for Examine
	/// </summary>
	public class ExamineEvents : IApplicationEventHandler
	{
		public void OnApplicationInitialized(UmbracoApplicationBase httpApplication, ApplicationContext applicationContext)
		{			
		}

		public void OnApplicationStarting(UmbracoApplicationBase httpApplication, ApplicationContext applicationContext)
		{			
		}

		/// <summary>
		/// Once the application has started we should bind to all events and initialize the providers.
		/// </summary>
		/// <param name="httpApplication"></param>
		/// <param name="applicationContext"></param>
		/// <remarks>
		/// We need to do this on the Started event as to guarantee that all resolvers are setup properly.
		/// </remarks>
		[SecuritySafeCritical]
		public void OnApplicationStarted(UmbracoApplicationBase httpApplication, ApplicationContext applicationContext)
		{
			var registeredProviders = ExamineManager.Instance.IndexProviderCollection
				.OfType<BaseUmbracoIndexer>().Count(x => x.EnableDefaultEventHandler);

			LogHelper.Info<ExamineEvents>("Adding examine event handlers for index providers: {0}", () => registeredProviders);

			//don't bind event handlers if we're not suppose to listen
			if (registeredProviders == 0)
				return;

			global::umbraco.cms.businesslogic.media.Media.AfterSave += MediaAfterSave;
			global::umbraco.cms.businesslogic.media.Media.AfterDelete += MediaAfterDelete;
			CMSNode.AfterMove += MediaAfterMove;

			//These should only fire for providers that DONT have SupportUnpublishedContent set to true
			content.AfterUpdateDocumentCache += ContentAfterUpdateDocumentCache;
			content.AfterClearDocumentCache += ContentAfterClearDocumentCache;

			//These should only fire for providers that have SupportUnpublishedContent set to true
			Document.AfterSave += DocumentAfterSave;
			Document.AfterDelete += DocumentAfterDelete;

			Member.AfterSave += MemberAfterSave;
			Member.AfterDelete += MemberAfterDelete;

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

		[SecuritySafeCritical]
		private static void MemberAfterSave(Member sender, SaveEventArgs e)
		{
			//ensure that only the providers are flagged to listen execute
			var xml = sender.ToXml(new System.Xml.XmlDocument(), false).ToXElement();
			var providers = ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
				.Where(x => x.EnableDefaultEventHandler);
			ExamineManager.Instance.ReIndexNode(xml, IndexTypes.Member, providers);
		}

		[SecuritySafeCritical]
		private static void MemberAfterDelete(Member sender, DeleteEventArgs e)
		{
			var nodeId = sender.Id.ToString();

			//ensure that only the providers are flagged to listen execute
			ExamineManager.Instance.DeleteFromIndex(nodeId,
				ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
					.Where(x => x.EnableDefaultEventHandler));
		}

		/// <summary>
		/// Only index using providers that SupportUnpublishedContent
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[SecuritySafeCritical]
		private static void DocumentAfterSave(Document sender, SaveEventArgs e)
		{
			//ensure that only the providers that have unpublishing support enabled     
			//that are also flagged to listen

			//there's a bug in 4.0.x that fires the Document saving event handler for media when media is moved,
			//therefore, we need to try to figure out if this is media or content. Currently one way to do this
			//is by checking the creator ID property as it will be null if it is media. We then need to try to 
			//create the media object, see if it exists, and pass it to the media save event handler... yeah i know, 
			//pretty horrible but has to be done.

			try
			{
				var creator = sender.Creator;
				if (creator != null)
				{
					//it's def a Document
					ExamineManager.Instance.ReIndexNode(ToXDocument(sender, false).Root, IndexTypes.Content,
						ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
							.Where(x => x.SupportUnpublishedContent
								&& x.EnableDefaultEventHandler));

					return; //exit, we've indexed the content
				}
			}
			catch (Exception)
			{
				//if we get this exception, it means it's most likely media, so well do our check next.   

				//TODO: Update this logic for 6.0 as we're not dealing with 4.0x

				//this is most likely media, not sure what kind of exception might get thrown in 4.0.x or 4.1 if accessing a null
				//creator, so we catch everything.
			}



			var m = new global::umbraco.cms.businesslogic.media.Media(sender.Id);
			if (string.IsNullOrEmpty(m.Path)) return;
			//this is a media item, no exception occurred on access to path and it's not empty which means it was found
			MediaAfterSave(m, e);
		}

		/// <summary>
		/// Only remove indexes using providers that SupportUnpublishedContent
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[SecuritySafeCritical]
		private static void DocumentAfterDelete(Document sender, DeleteEventArgs e)
		{
			var nodeId = sender.Id.ToString();

			//ensure that only the providers that have unpublishing support enabled      
			//that are also flagged to listen
			ExamineManager.Instance.DeleteFromIndex(nodeId,
				ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
					.Where(x => x.SupportUnpublishedContent
						&& x.EnableDefaultEventHandler));
		}

		[SecuritySafeCritical]
		private static void MediaAfterDelete(global::umbraco.cms.businesslogic.media.Media sender, DeleteEventArgs e)
		{
			var nodeId = sender.Id.ToString();

			//ensure that only the providers are flagged to listen execute
			ExamineManager.Instance.DeleteFromIndex(nodeId,
				ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
					.Where(x => x.EnableDefaultEventHandler));
		}

		[SecuritySafeCritical]
		private static void MediaAfterSave(global::umbraco.cms.businesslogic.media.Media sender, SaveEventArgs e)
		{
			//ensure that only the providers are flagged to listen execute
			IndexMedia(sender);
		}

		[SecuritySafeCritical]
		private static void IndexMedia(global::umbraco.cms.businesslogic.media.Media sender)
		{
			ExamineManager.Instance.ReIndexNode(ToXDocument(sender, true).Root, IndexTypes.Media,
												ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
													.Where(x => x.EnableDefaultEventHandler));
		}

		/// <summary>
		/// When media is moved, re-index
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[SecuritySafeCritical]
		private static void MediaAfterMove(object sender, MoveEventArgs e)
		{
			var media = sender as global::umbraco.cms.businesslogic.media.Media;
			if (media == null) return;
			global::umbraco.cms.businesslogic.media.Media m = media;
			IndexMedia(m);
		}

		/// <summary>
		/// Only Update indexes for providers that dont SupportUnpublishedContent
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[SecuritySafeCritical]
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
		[SecuritySafeCritical]
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
		[SecuritySafeCritical]
		private static void IndexerDocumentWriting(object sender, DocumentWritingEventArgs e)
		{
			if (e.Fields.Keys.Contains("nodeName"))
			{
				//add the lower cased version
				e.Document.Add(new Field("__nodeName",
										e.Fields["nodeName"].ToLower(),
										Field.Store.YES,
										Field.Index.ANALYZED,
										Field.TermVector.NO
										));
			}
		}


		/// <summary>
		/// Converts a content node to XDocument
		/// </summary>
		/// <param name="node"></param>
		/// <param name="cacheOnly">true if data is going to be returned from cache</param>
		/// <returns></returns>
		/// <remarks>
		/// If the type of node is not a Document, the cacheOnly has no effect, it will use the API to return
		/// the xml. 
		/// </remarks>
		[SecuritySafeCritical]
		public static XDocument ToXDocument(Content node, bool cacheOnly)
		{
			if (cacheOnly && node.GetType().Equals(typeof(Document)))
			{
				var umbXml = library.GetXmlNodeById(node.Id.ToString());
				if (umbXml != null)
				{
					return umbXml.ToXDocument();
				}
			}

			//this will also occur if umbraco hasn't cached content yet....

			//if it's not a using cache and it's not cacheOnly, then retrieve the Xml using the API
			return ToXDocument(node);
		}

		/// <summary>
		/// Converts a content node to Xml
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		[SecuritySafeCritical]
		private static XDocument ToXDocument(Content node)
		{
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

			return new XDocument(xNode.ToXElement());
		}

	}
}