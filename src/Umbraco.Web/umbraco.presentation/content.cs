using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.presentation.nodeFactory;
using Action = umbraco.BusinessLogic.Actions.Action;
using Node = umbraco.NodeFactory.Node;
using Umbraco.Core;

namespace umbraco
{
    /// <summary>
    /// Handles umbraco content
    /// </summary>
    public class content
    {
        #region Declarations

        // Sync access to disk file
        private static readonly object ReaderWriterSyncLock = new object();

        // Sync access to internal cache
        private static readonly object XmlContentInternalSyncLock = new object();

        // Sync access to timestamps
        private static readonly object TimestampSyncLock = new object();

        // Sync database access
        private static readonly object DbReadSyncLock = new object();
        private const string XmlContextContentItemKey = "UmbracoXmlContextContent";
        private string _umbracoXmlDiskCacheFileName = string.Empty;
        private volatile XmlDocument _xmlContent;
        private DateTime _lastDiskCacheReadTime = DateTime.MinValue;
        private DateTime _lastDiskCacheCheckTime = DateTime.MinValue;

        /// <summary>
        /// Gets the path of the umbraco XML disk cache file.
        /// </summary>
        /// <value>The name of the umbraco XML disk cache file.</value>
        public string UmbracoXmlDiskCacheFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_umbracoXmlDiskCacheFileName))
                {
                    _umbracoXmlDiskCacheFileName = IOHelper.MapPath(SystemFiles.ContentCacheXml);
                }
                return _umbracoXmlDiskCacheFileName;
            }
            set { _umbracoXmlDiskCacheFileName = value; }
        }

        #endregion


        #region Singleton

        private static readonly Lazy<content> LazyInstance = new Lazy<content>(() => new content());

        public static content Instance
        {
            get
            {
                return LazyInstance.Value;
            }
        }

        #endregion

        #region Properties

        /// <remarks>
        /// Get content. First call to this property will initialize xmldoc
        /// subsequent calls will be blocked until initialization is done
        /// Further we cache(in context) xmlContent for each request to ensure that
        /// we always have the same XmlDoc throughout the whole request.
        /// </remarks>
        public virtual XmlDocument XmlContent
        {
            get
            {
                if (HttpContext.Current == null)
                    return XmlContentInternal;
                var content = HttpContext.Current.Items[XmlContextContentItemKey] as XmlDocument;
                if (content == null)
                {
                    content = XmlContentInternal;
                    HttpContext.Current.Items[XmlContextContentItemKey] = content;
                }
                return content;
            }
        }

        [Obsolete("Please use: content.Instance.XmlContent")]
        public static XmlDocument xmlContent
        {
            get { return Instance.XmlContent; }
        }

        //NOTE: We CANNOT use this for a double check lock because it is a property, not a field and to do double
        // check locking in c# you MUST have a volatile field. Even thoug this wraps a volatile field it will still 
        // not work as expected for a double check lock because properties are treated differently in the clr.
        public virtual bool isInitializing
        {
            get { return _xmlContent == null; }
        }

        /// <summary>
        /// Internal reference to XmlContent
        /// </summary>
        /// <remarks>
        /// Before returning we always check to ensure that the xml is loaded
        /// </remarks>
        protected virtual XmlDocument XmlContentInternal
        {
            get
            {
                CheckXmlContentPopulation();

                return _xmlContent;
            }
            set
            {
                lock (XmlContentInternalSyncLock)
                {                    
                    _xmlContent = value;

                    if (UmbracoConfig.For.UmbracoSettings().Content.XmlCacheEnabled && UmbracoConfig.For.UmbracoSettings().Content.ContinouslyUpdateXmlDiskCache)
                        QueueXmlForPersistence();
                    else
                        // Clear cache...
                        DeleteXmlCache();
                }
            }
        }

        /// <summary>
        /// Checks if the disk cache file has been updated and if so, clears the in-memory cache to force the file to be read.
        /// </summary>
        /// <remarks>
        /// Added to trigger updates of the in-memory cache when the disk cache file is updated.
        /// </remarks>
        private void CheckDiskCacheForUpdate()
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.XmlCacheEnabled == false)
                return;

            lock (TimestampSyncLock)
            {
                if (_lastDiskCacheCheckTime > DateTime.UtcNow.AddSeconds(-1.0))
                    return;

                _lastDiskCacheCheckTime = DateTime.UtcNow;

                lock (XmlContentInternalSyncLock)
                {

                    if (GetCacheFileUpdateTime() <= _lastDiskCacheReadTime)
                        return;

                    _xmlContent = null;
                }
            }
        }

        /// <summary>
        /// Triggers the XML content population if necessary.
        /// </summary>
        /// <returns>Returns true of the XML was not populated, returns false if it was already populated</returns>
        private bool CheckXmlContentPopulation()
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.XmlContentCheckForDiskChanges)
                CheckDiskCacheForUpdate();

            if (_xmlContent == null)
            {
                lock (XmlContentInternalSyncLock)
                {
                    if (_xmlContent == null)
                    {
						LogHelper.Debug<content>("Initializing content on thread '{0}' (Threadpool? {1})", 
							true,
							() => Thread.CurrentThread.Name,
							() => Thread.CurrentThread.IsThreadPoolThread);
                        
                        _xmlContent = LoadContent();
						LogHelper.Debug<content>("Content initialized (loaded)", true);

                        FireAfterRefreshContent(new RefreshContentEventArgs());

                        // Only save new XML cache to disk if we just repopulated it
                        // TODO: Re-architect this so that a call to this method doesn't invoke a new thread for saving disk cache
                        if (UmbracoConfig.For.UmbracoSettings().Content.XmlCacheEnabled && !IsValidDiskCachePresent())
                        {
                            QueueXmlForPersistence();
                        }
                        return true;
                    }
                }
            }
            Trace.WriteLine("Content initialized (was already in context)");
            return false;
        }

        #endregion

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        protected static void ReGenerateSchema(XmlDocument xmlDoc)
        {
            string dtd = DocumentType.GenerateXmlDocumentType();

            // remove current doctype
            XmlNode n = xmlDoc.FirstChild;
            while (n.NodeType != XmlNodeType.DocumentType && n.NextSibling != null)
            {
                n = n.NextSibling;
            }
            if (n.NodeType == XmlNodeType.DocumentType)
            {
                xmlDoc.RemoveChild(n);
            }
            XmlDocumentType docType = xmlDoc.CreateDocumentType("root", null, null, dtd);
            xmlDoc.InsertAfter(docType, xmlDoc.FirstChild);
        }

        protected static XmlDocument ValidateSchema(string docTypeAlias, XmlDocument xmlDoc)
        {
			// check if doctype is defined in schema else add it
			// can't edit the doctype of an xml document, must create a new document

			var doctype = xmlDoc.DocumentType;
			var subset = doctype.InternalSubset;
			if (!subset.Contains(string.Format("<!ATTLIST {0} id ID #REQUIRED>", docTypeAlias)))
			{
				subset = string.Format("<!ELEMENT {0} ANY>\r\n<!ATTLIST {0} id ID #REQUIRED>\r\n{1}", docTypeAlias, subset);
				var xmlDoc2 = new XmlDocument();
				doctype = xmlDoc2.CreateDocumentType("root", null, null, subset);
				xmlDoc2.AppendChild(doctype);
				var root = xmlDoc2.ImportNode(xmlDoc.DocumentElement, true);
				xmlDoc2.AppendChild(root);

				// apply
				xmlDoc = xmlDoc2;
			}

			return xmlDoc;
        }

        #region Public Methods

        #region Delegates

        /// <summary>
        /// Occurs when [after loading the xml string from the database].
        /// </summary>
        public delegate void ContentCacheDatabaseLoadXmlStringEventHandler(
            ref string xml, ContentCacheLoadNodeEventArgs e);

        /// <summary>
        /// Occurs when [after loading the xml string from the database and creating the xml node].
        /// </summary>
        public delegate void ContentCacheLoadNodeEventHandler(XmlNode xmlNode, ContentCacheLoadNodeEventArgs e);

        public delegate void DocumentCacheEventHandler(Document sender, DocumentCacheEventArgs e);

        public delegate void RefreshContentEventHandler(Document sender, RefreshContentEventArgs e);

        #endregion

        /// <summary>
        /// Load content from database in a background thread
        /// Replaces active content when done.
        /// </summary>
        public virtual void RefreshContentFromDatabaseAsync()
        {
            var e = new RefreshContentEventArgs();
            FireBeforeRefreshContent(e);

            if (!e.Cancel)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        XmlDocument xmlDoc = LoadContentFromDatabase();
                        XmlContentInternal = xmlDoc;

                        // It is correct to manually call PersistXmlToFile here event though the setter of XmlContentInternal
                        // queues this up, because this delegate is executing on a different thread and may complete
                        // after the request which invoked it (which would normally persist the file on completion)
                        // So we are responsible for ensuring the content is persisted in this case.
                        if (UmbracoConfig.For.UmbracoSettings().Content.XmlCacheEnabled && UmbracoConfig.For.UmbracoSettings().Content.ContinouslyUpdateXmlDiskCache)
                            PersistXmlToFile(xmlDoc);
                    });

                FireAfterRefreshContent(e);
            }
        }

        public static void TransferValuesFromDocumentXmlToPublishedXml(XmlNode DocumentNode, XmlNode PublishedNode)
        {
            // Remove all attributes and data nodes from the published node
            PublishedNode.Attributes.RemoveAll();
            string xpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "./data" : "./* [not(@id)]";
            foreach (XmlNode n in PublishedNode.SelectNodes(xpath))
                PublishedNode.RemoveChild(n);

            // Append all attributes and datanodes from the documentnode to the publishednode
            foreach (XmlAttribute att in DocumentNode.Attributes)
                ((XmlElement)PublishedNode).SetAttribute(att.Name, att.Value);

            foreach (XmlElement el in DocumentNode.SelectNodes(xpath))
            {
                XmlNode newDatael = PublishedNode.OwnerDocument.ImportNode(el, true);
                PublishedNode.AppendChild(newDatael);
            }
        }

        /// <summary>
        /// Used by all overloaded publish methods to do the actual "noderepresentation to xml"
        /// </summary>
        /// <param name="d"></param>
        /// <param name="xmlContentCopy"></param>
        /// <param name="updateSitemapProvider"></param>
        public static XmlDocument PublishNodeDo(Document d, XmlDocument xmlContentCopy, bool updateSitemapProvider)
        {
            // check if document *is* published, it could be unpublished by an event
            if (d.Published)
            {
                var parentId = d.Level == 1 ? -1 : d.Parent.Id;
                xmlContentCopy = AppendDocumentXml(d.Id, d.Level, parentId,
                                                   GetPreviewOrPublishedNode(d, xmlContentCopy, false), xmlContentCopy);

                // update sitemapprovider
                if (updateSitemapProvider && SiteMap.Provider is UmbracoSiteMapProvider)
                {
                    try
                    {
                        var prov = (UmbracoSiteMapProvider)SiteMap.Provider;
                        var n = new Node(d.Id, true);
                        if (string.IsNullOrEmpty(n.Url) == false && n.Url != "/#")
                        {
                            prov.UpdateNode(n);
                        }
                        else
                        {
                            LogHelper.Debug<content>(string.Format("Can't update Sitemap Provider due to empty Url in node id: {0}", d.Id));
                        }
                    }
                    catch (Exception ee)
                    {
                        LogHelper.Error<content>(string.Format("Error adding node to Sitemap Provider in PublishNodeDo(): {0}", d.Id), ee);
                    }
                }
            }

			return xmlContentCopy;
		}

        public static XmlDocument AppendDocumentXml(int id, int level, int parentId, XmlNode docNode, XmlDocument xmlContentCopy)
        {
            // Find the document in the xml cache
            XmlNode currentNode = xmlContentCopy.GetElementById(id.ToString());

			// if the document is not there already then it's a new document
			// we must make sure that its document type exists in the schema
            var xmlContentCopy2 = xmlContentCopy;
			if (currentNode == null && UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema == false)
			{
				xmlContentCopy = ValidateSchema(docNode.Name, xmlContentCopy);
				if (xmlContentCopy != xmlContentCopy2)
					docNode = xmlContentCopy.ImportNode(docNode, true);
			}

            // Find the parent (used for sortering and maybe creation of new node)
            XmlNode parentNode = level == 1
                                     ? xmlContentCopy.DocumentElement
                                     : xmlContentCopy.GetElementById(parentId.ToString());

            if (parentNode != null)
            {
                if (currentNode == null)
                {
                    currentNode = docNode;
                    parentNode.AppendChild(currentNode);
                }
                else
                {
                    //check the current parent id
                    var currParentId = currentNode.AttributeValue<int>("parentID");

                    //update the node with it's new values
                    TransferValuesFromDocumentXmlToPublishedXml(docNode, currentNode);

                    //If the node is being moved we also need to ensure that it exists under the new parent!
                    // http://issues.umbraco.org/issue/U4-2312
                    // we were never checking this before and instead simply changing the parentId value but not 
                    // changing the actual parent.

                    //check the new parent
                    if (currParentId != currentNode.AttributeValue<int>("parentID"))
                    {
                        //ok, we've actually got to move the node
                        parentNode.AppendChild(currentNode);
                    }
                    
                }

                // TODO: Update with new schema!
                var xpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema
                                ? "./node"
                                : "./* [@id]";

                var childNodes = parentNode.SelectNodes(xpath);

                // Sort the nodes if the added node has a lower sortorder than the last
                if (childNodes != null && childNodes.Count > 0)
                {
                    //get the biggest sort order for all children including the one added
                    var largestSortOrder = childNodes.Cast<XmlNode>().Max(x => x.AttributeValue<int>("sortOrder"));
                    var currentSortOrder = currentNode.AttributeValue<int>("sortOrder");
                    //if the current item's sort order is less than the largest sort order in the list then
                    //we need to resort the xml structure since this item belongs somewhere in the middle.
                    //http://issues.umbraco.org/issue/U4-509
                    if (childNodes.Count > 1 && currentSortOrder < largestSortOrder)
                    {
                        SortNodes(ref parentNode);
                    }
                }
            }

			return xmlContentCopy;
        }

        private static XmlNode GetPreviewOrPublishedNode(Document d, XmlDocument xmlContentCopy, bool isPreview)
        {
            if (isPreview)
            {
                return d.ToPreviewXml(xmlContentCopy);
            }
            else
            {
                return d.ToXml(xmlContentCopy, false);
            }
        }

        /// <summary>
        /// Sorts the documents.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        public static void SortNodes(ref XmlNode parentNode)
        {
            var xpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema
                            ? "./node"
                            : "./* [@id]";

            XmlHelper.SortNodes(
                parentNode,
                xpath,
                element => element.Attribute("id") != null,
                element => element.AttributeValue<int>("sortOrder"));            
        }


        /// <summary>
        /// Updates the document cache.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        public virtual void UpdateDocumentCache(int pageId)
        {
            var d = new Document(pageId);
            UpdateDocumentCache(d);
        }

        /// <summary>
        /// Updates the document cache.
        /// </summary>
        /// <param name="d">The d.</param>
        public virtual void UpdateDocumentCache(Document d)
        {
            var e = new DocumentCacheEventArgs();
            FireBeforeUpdateDocumentCache(d, e);

            if (!e.Cancel)
            {
				// lock the xml cache so no other thread can write to it at the same time
				// note that some threads could read from it while we hold the lock, though
                lock (XmlContentInternalSyncLock)
                {
					// modify a clone of the cache because even though we're into the write-lock
					// we may have threads reading at the same time. why is this an option?
                    XmlDocument wip = UmbracoConfig.For.UmbracoSettings().Content.CloneXmlContent
						? CloneXmlDoc(XmlContentInternal)
						: XmlContentInternal;

					wip = PublishNodeDo(d, wip, true);
					XmlContentInternal = wip;

                    ClearContextCache();
                }

                var cachedFieldKeyStart = string.Format("{0}{1}_", CacheKeys.ContentItemCacheKey, d.Id);
                ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(cachedFieldKeyStart);                    
                
                FireAfterUpdateDocumentCache(d, e);
            }
        }

        /// <summary>
        /// Updates the document cache for multiple documents
        /// </summary>
        /// <param name="Documents">The documents.</param>
        [Obsolete("This is not used and will be removed from the codebase in future versions")]
        public virtual void UpdateDocumentCache(List<Document> Documents)
        {
            // We need to lock content cache here, because we cannot allow other threads
            // making changes at the same time, they need to be queued
            int parentid = Documents[0].Id;

            lock (XmlContentInternalSyncLock)
            {
                // Make copy of memory content, we cannot make changes to the same document
                // the is read from elsewhere
                XmlDocument xmlContentCopy = CloneXmlDoc(XmlContentInternal);
                foreach (Document d in Documents)
                {
                    PublishNodeDo(d, xmlContentCopy, true);
                }
                XmlContentInternal = xmlContentCopy;
                ClearContextCache();
            }
        }
        
        /// <summary>
        /// Updates the document cache async.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        [Obsolete("Method obsolete in version 4.1 and later, please use UpdateDocumentCache", true)]
        public virtual void UpdateDocumentCacheAsync(int documentId)
        {
            //SD: WE've obsoleted this but then didn't make it call the method it should! So we've just 
            // left a bug behind...???? ARGH. 
            //.... changed now.
            //ThreadPool.QueueUserWorkItem(delegate { UpdateDocumentCache(documentId); });

            UpdateDocumentCache(documentId);
        }

        /// <summary>
        /// Clears the document cache async.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        [Obsolete("Method obsolete in version 4.1 and later, please use ClearDocumentCache", true)]
        public virtual void ClearDocumentCacheAsync(int documentId)
        {
            //SD: WE've obsoleted this but then didn't make it call the method it should! So we've just 
            // left a bug behind...???? ARGH.
            //.... changed now.
            //ThreadPool.QueueUserWorkItem(delegate { ClearDocumentCache(documentId); });

            ClearDocumentCache(documentId);
        }

        public virtual void ClearDocumentCache(int documentId)
        {
            // Get the document
            var d = new Document(documentId);
            ClearDocumentCache(d);
        }

        /// <summary>
        /// Clears the document cache and removes the document from the xml db cache.
        /// This means the node gets unpublished from the website.
        /// </summary>
        /// <param name="doc">The document</param>
        internal void ClearDocumentCache(Document doc)
        {
            var e = new DocumentCacheEventArgs();
            FireBeforeClearDocumentCache(doc, e);

            if (!e.Cancel)
            {
                XmlNode x;

                // remove from xml db cache 
                doc.XmlRemoveFromDB();

                // Check if node present, before cloning
                x = XmlContentInternal.GetElementById(doc.Id.ToString());
                if (x == null)
                    return;

                // We need to lock content cache here, because we cannot allow other threads
                // making changes at the same time, they need to be queued
                lock (XmlContentInternalSyncLock)
                {
                    // Make copy of memory content, we cannot make changes to the same document
                    // the is read from elsewhere
                    XmlDocument xmlContentCopy = CloneXmlDoc(XmlContentInternal);

                    // Find the document in the xml cache
                    x = xmlContentCopy.GetElementById(doc.Id.ToString());
                    if (x != null)
                    {
                        // The document already exists in cache, so repopulate it
                        x.ParentNode.RemoveChild(x);
                        XmlContentInternal = xmlContentCopy;
                        ClearContextCache();
                    }
                }

                //SD: changed to fire event BEFORE running the sitemap!! argh.
                FireAfterClearDocumentCache(doc, e);

                // update sitemapprovider
                if (SiteMap.Provider is UmbracoSiteMapProvider)
                {
                    var prov = (UmbracoSiteMapProvider)SiteMap.Provider;
                    prov.RemoveNode(doc.Id);
                }                
            }
        }


        /// <summary>
        /// Unpublishes the  node.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        [Obsolete("Please use: umbraco.content.ClearDocumentCache", true)]
        public virtual void UnPublishNode(int documentId)
        {
            ClearDocumentCache(documentId);
        }

        ///// <summary>
        ///// Uns the publish node async.
        ///// </summary>
        ///// <param name="documentId">The document id.</param>
        //[Obsolete("Please use: umbraco.content.ClearDocumentCacheAsync", true)]
        //public virtual void UnPublishNodeAsync(int documentId)
        //{

        //    ThreadPool.QueueUserWorkItem(delegate { ClearDocumentCache(documentId); });
        //}


        ///// <summary>
        ///// Legacy method - you should use the overloaded publishnode(document d) method whenever possible
        ///// </summary>
        ///// <param name="documentId"></param>
        //[Obsolete("Please use: umbraco.content.UpdateDocumentCache", true)]
        //public virtual void PublishNode(int documentId)
        //{

        //    // Get the document
        //    var d = new Document(documentId);
        //    PublishNode(d);
        //}


        ///// <summary>
        ///// Publishes the node async.
        ///// </summary>
        ///// <param name="documentId">The document id.</param>
        //[Obsolete("Please use: umbraco.content.UpdateDocumentCacheAsync", true)]
        //public virtual void PublishNodeAsync(int documentId)
        //{

        //    UpdateDocumentCacheAsync(documentId);
        //}


        ///// <summary>
        ///// Publishes the node.
        ///// </summary>
        ///// <param name="Documents">The documents.</param>
        //[Obsolete("Please use: umbraco.content.UpdateDocumentCache", true)]
        //public virtual void PublishNode(List<Document> Documents)
        //{

        //    UpdateDocumentCache(Documents);
        //}



        ///// <summary>
        ///// Publishes the node.
        ///// </summary>
        ///// <param name="d">The document.</param>
        //[Obsolete("Please use: umbraco.content.UpdateDocumentCache", true)]
        //public virtual void PublishNode(Document d)
        //{

        //    UpdateDocumentCache(d);
        //}


        /// <summary>
        /// Occurs when [before document cache update].
        /// </summary>
        public static event DocumentCacheEventHandler BeforeUpdateDocumentCache;

        /// <summary>
        /// Fires the before document cache.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.DocumentCacheEventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeUpdateDocumentCache(Document sender, DocumentCacheEventArgs e)
        {
            if (BeforeUpdateDocumentCache != null)
            {
                BeforeUpdateDocumentCache(sender, e);
            }
        }


        /// <summary>
        /// Occurs when [after document cache update].
        /// </summary>
        public static event DocumentCacheEventHandler AfterUpdateDocumentCache;

        /// <summary>
        /// Fires after document cache updater.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.DocumentCacheEventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterUpdateDocumentCache(Document sender, DocumentCacheEventArgs e)
        {
            if (AfterUpdateDocumentCache != null)
            {
                AfterUpdateDocumentCache(sender, e);
            }
        }

        /// <summary>
        /// Occurs when [before document cache unpublish].
        /// </summary>
        public static event DocumentCacheEventHandler BeforeClearDocumentCache;

        /// <summary>
        /// Fires the before document cache unpublish.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.DocumentCacheEventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeClearDocumentCache(Document sender, DocumentCacheEventArgs e)
        {
            if (BeforeClearDocumentCache != null)
            {
                BeforeClearDocumentCache(sender, e);
            }
        }

        public static event DocumentCacheEventHandler AfterClearDocumentCache;

        /// <summary>
        /// Fires the after document cache unpublish.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.DocumentCacheEventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterClearDocumentCache(Document sender, DocumentCacheEventArgs e)
        {
            if (AfterClearDocumentCache != null)
            {
                AfterClearDocumentCache(sender, e);
            }
        }

        /// <summary>
        /// Occurs when [before refresh content].
        /// </summary>
        public static event RefreshContentEventHandler BeforeRefreshContent;

        /// <summary>
        /// Fires the content of the before refresh.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.RefreshContentEventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeRefreshContent(RefreshContentEventArgs e)
        {
            if (BeforeRefreshContent != null)
            {
                BeforeRefreshContent(null, e);
            }
        }

        /// <summary>
        /// Occurs when [after refresh content].
        /// </summary>
        public static event RefreshContentEventHandler AfterRefreshContent;

        /// <summary>
        /// Fires the content of the after refresh.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.RefreshContentEventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterRefreshContent(RefreshContentEventArgs e)
        {
            if (AfterRefreshContent != null)
            {
                AfterRefreshContent(null, e);
            }
        }


        /// <summary>
        /// Occurs when [after loading the xml string from the database].
        /// </summary>
        public static event ContentCacheDatabaseLoadXmlStringEventHandler AfterContentCacheDatabaseLoadXmlString;

        /// <summary>
        /// Fires the before when creating the document cache from database
        /// </summary>
        /// <param name="node">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.ContentCacheLoadNodeEventArgs"/> instance containing the event data.</param>
        internal static void FireAfterContentCacheDatabaseLoadXmlString(ref string xml, ContentCacheLoadNodeEventArgs e)
        {
            if (AfterContentCacheDatabaseLoadXmlString != null)
            {
                AfterContentCacheDatabaseLoadXmlString(ref xml, e);
            }
        }

        /// <summary>
        /// Occurs when [before when creating the document cache from database].
        /// </summary>
        public static event ContentCacheLoadNodeEventHandler BeforeContentCacheLoadNode;

        /// <summary>
        /// Fires the before when creating the document cache from database
        /// </summary>
        /// <param name="node">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.ContentCacheLoadNodeEventArgs"/> instance containing the event data.</param>
        internal static void FireBeforeContentCacheLoadNode(XmlNode node, ContentCacheLoadNodeEventArgs e)
        {
            if (BeforeContentCacheLoadNode != null)
            {
                BeforeContentCacheLoadNode(node, e);
            }
        }

        /// <summary>
        /// Occurs when [after loading document cache xml node from database].
        /// </summary>
        public static event ContentCacheLoadNodeEventHandler AfterContentCacheLoadNodeFromDatabase;

        /// <summary>
        /// Fires the after loading document cache xml node from database
        /// </summary>
        /// <param name="node">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.ContentCacheLoadNodeEventArgs"/> instance containing the event data.</param>
        internal static void FireAfterContentCacheLoadNodeFromDatabase(XmlNode node, ContentCacheLoadNodeEventArgs e)
        {
            if (AfterContentCacheLoadNodeFromDatabase != null)
            {
                AfterContentCacheLoadNodeFromDatabase(node, e);
            }
        }

        /// <summary>
        /// Occurs when [before a publish action updates the content cache].
        /// </summary>
        public static event ContentCacheLoadNodeEventHandler BeforePublishNodeToContentCache;

        /// <summary>
        /// Fires the before a publish action updates the content cache
        /// </summary>
        /// <param name="node">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.ContentCacheLoadNodeEventArgs"/> instance containing the event data.</param>
        public static void FireBeforePublishNodeToContentCache(XmlNode node, ContentCacheLoadNodeEventArgs e)
        {
            if (BeforePublishNodeToContentCache != null)
            {
                BeforePublishNodeToContentCache(node, e);
            }
        }

        #endregion

        #region Protected & Private methods

        internal const string PersistenceFlagContextKey = "vnc38ykjnkjdnk2jt98ygkxjng";

		/// <summary>
		/// Removes the flag that queues the file for persistence
		/// </summary>
		internal void RemoveXmlFilePersistenceQueue()
		{
			HttpContext.Current.Application.Lock();
			HttpContext.Current.Application[PersistenceFlagContextKey] = null;
			HttpContext.Current.Application.UnLock();
		}

        internal bool IsXmlQueuedForPersistenceToFile
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    bool val = HttpContext.Current.Application[PersistenceFlagContextKey] != null;
                    if (val)
                    {
                        DateTime persistenceTime = DateTime.MinValue;
                        try
                        {
                            persistenceTime = (DateTime)HttpContext.Current.Application[PersistenceFlagContextKey];
                            if (persistenceTime > GetCacheFileUpdateTime())
                            {
                                return true;
                            }
                            else
                            {
                                RemoveXmlFilePersistenceQueue();
                            }
                        }
                        catch
                        {
                            // Nothing to catch here - we'll just persist
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Invalidates the disk content cache file. Effectively just deletes it.
        /// </summary>
        private void DeleteXmlCache()
        {
            lock (ReaderWriterSyncLock)
            {
                if (File.Exists(UmbracoXmlDiskCacheFileName))
                {
                    // Reset file attributes, to make sure we can delete file
                    try
                    {
                        File.SetAttributes(UmbracoXmlDiskCacheFileName, FileAttributes.Normal);
                    }
                    finally
                    {
                        File.Delete(UmbracoXmlDiskCacheFileName);
                    }
                }
            }
        }

        /// <summary>
        /// Clear HTTPContext cache if any
        /// </summary>
        private void ClearContextCache()
        {
            // If running in a context very important to reset context cache orelse new nodes are missing
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(XmlContextContentItemKey))
                HttpContext.Current.Items.Remove(XmlContextContentItemKey);
        }

        /// <summary>
        /// Load content from either disk or database
        /// </summary>
        /// <returns></returns>
        private XmlDocument LoadContent()
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.XmlCacheEnabled && IsValidDiskCachePresent())
            {
                try
                {
                    return LoadContentFromDiskCache();
                }
                catch (Exception e)
                {
                    // This is really bad, loading from cache file failed for some reason, now fallback to loading from database
                    Debug.WriteLine("Content file cache load failed: " + e);
                    DeleteXmlCache();
                }
            }
            return LoadContentFromDatabase();
        }

        private bool IsValidDiskCachePresent()
        {
            if (File.Exists(UmbracoXmlDiskCacheFileName))
            {
                // Only return true if we don't have a zero-byte file
                var f = new FileInfo(UmbracoXmlDiskCacheFileName);
                if (f.Length > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Load content from cache file
        /// </summary>
        private XmlDocument LoadContentFromDiskCache()
        {
            lock (ReaderWriterSyncLock)
            {
                var xmlDoc = new XmlDocument();
                LogHelper.Info<content>("Loading content from disk cache...");
                xmlDoc.Load(UmbracoXmlDiskCacheFileName);
                _lastDiskCacheReadTime = DateTime.UtcNow;
                return xmlDoc;
            }
        }

        private static void InitContentDocument(XmlDocument xmlDoc, string dtd)
        {
            // Prime the xml document with an inline dtd and a root element
            xmlDoc.LoadXml(String.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?>{0}{1}{0}<root id=\"-1\"/>",
                                         Environment.NewLine,
                                         dtd));
        }

        /// <summary>
        /// Load content from database
        /// </summary>
        private XmlDocument LoadContentFromDatabase()
        {
            // Alex N - 2010 06 - Very generic try-catch simply because at the moment, unfortunately, this method gets called inside a ThreadPool thread
            // and we need to guarantee it won't tear down the app pool by throwing an unhandled exception
            try
            {
                // Moved User to a local variable - why are we causing user 0 to load from the DB though?
                // Alex N 20100212
                User staticUser = null;
                try
                {
                    staticUser = User.GetCurrent(); //User.GetUser(0);
                }
                catch
                {
                    /* We don't care later if the staticUser is null */
                }

                // Try to log to the DB
                LogHelper.Info<content>("Loading content from database...");

                var hierarchy = new Dictionary<int, List<int>>();
                var nodeIndex = new Dictionary<int, XmlNode>();

                try
                {
					LogHelper.Debug<content>("Republishing starting");

                    lock (DbReadSyncLock)
                    {

                        // Lets cache the DTD to save on the DB hit on the subsequent use
                        string dtd = DocumentType.GenerateDtd();

                        // Prepare an XmlDocument with an appropriate inline DTD to match
                        // the expected content
                        var xmlDoc = new XmlDocument();
                        InitContentDocument(xmlDoc, dtd);

                        // Esben Carlsen: At some point we really need to put all data access into to a tier of its own.
                        // CLN - added checks that document xml is for a document that is actually published.
                        string sql =
                            @"select umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, cmsContentXml.xml from umbracoNode 
inner join cmsContentXml on cmsContentXml.nodeId = umbracoNode.id and umbracoNode.nodeObjectType = @type
where umbracoNode.id in (select cmsDocument.nodeId from cmsDocument where cmsDocument.published = 1)
order by umbracoNode.level, umbracoNode.sortOrder";



                        using (
                            IRecordsReader dr = SqlHelper.ExecuteReader(sql,
                                                                        SqlHelper.CreateParameter("@type",
                                                                                                  new Guid(
                                                                                                      Constants.ObjectTypes.Document)))
                            )
                        {
                            while (dr.Read())
                            {
                                int currentId = dr.GetInt("id");
                                int parentId = dr.GetInt("parentId");
                                string xml = dr.GetString("xml");

                                // Call the eventhandler to allow modification of the string
                                var e1 = new ContentCacheLoadNodeEventArgs();
                                FireAfterContentCacheDatabaseLoadXmlString(ref xml, e1);
                                // check if a listener has canceled the event
                                if (!e1.Cancel)
                                {
                                    // and parse it into a DOM node
                                    xmlDoc.LoadXml(xml);
                                    XmlNode node = xmlDoc.FirstChild;
                                    // same event handler loader form the xml node
                                    var e2 = new ContentCacheLoadNodeEventArgs();
                                    FireAfterContentCacheLoadNodeFromDatabase(node, e2);
                                    // and checking if it was canceled again
                                    if (!e1.Cancel)
                                    {
                                        nodeIndex.Add(currentId, node);

                                        // verify if either of the handlers canceled the children to load
                                        if (!e1.CancelChildren && !e2.CancelChildren)
                                        {
                                            // Build the content hierarchy
                                            List<int> children;
                                            if (!hierarchy.TryGetValue(parentId, out children))
                                            {
                                                // No children for this parent, so add one
                                                children = new List<int>();
                                                hierarchy.Add(parentId, children);
                                            }
                                            children.Add(currentId);
                                        }
                                    }
                                }
                            }
                        }

                        LogHelper.Debug<content>("Xml Pages loaded");

                        try
                        {
                            // If we got to here we must have successfully retrieved the content from the DB so
                            // we can safely initialise and compose the final content DOM. 
                            // Note: We are reusing the XmlDocument used to create the xml nodes above so 
                            // we don't have to import them into a new XmlDocument

                            // Initialise the document ready for the final composition of content
                            InitContentDocument(xmlDoc, dtd);

                            // Start building the content tree recursively from the root (-1) node
                            GenerateXmlDocument(hierarchy, nodeIndex, -1, xmlDoc.DocumentElement);

                            LogHelper.Debug<content>("Done republishing Xml Index");

                            return xmlDoc;
                        }
                        catch (Exception ee)
                        {
                            LogHelper.Error<content>("Error while generating XmlDocument from database", ee);
                        }
                    }
                }
                catch (OutOfMemoryException ee)
                {
                    LogHelper.Error<content>(string.Format("Error Republishing: Out Of Memory. Parents: {0}, Nodes: {1}", hierarchy.Count, nodeIndex.Count), ee);
                }
                catch (Exception ee)
                {
                    LogHelper.Error<content>("Error Republishing", ee);
                }
            }
            catch (Exception ee)
            {
                LogHelper.Error<content>("Error Republishing", ee);
            }

            // An error of some sort must have stopped us from successfully generating
            // the content tree, so lets return null signifying there is no content available
            return null;
        }

        private static void GenerateXmlDocument(IDictionary<int, List<int>> hierarchy,
                                                IDictionary<int, XmlNode> nodeIndex, int parentId, XmlNode parentNode)
        {
            List<int> children;

            if (hierarchy.TryGetValue(parentId, out children))
            {
                XmlNode childContainer = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ||
                                         String.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME)
                                             ? parentNode
                                             : parentNode.SelectSingleNode(
                                                 UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME);

                if (!UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema &&
                    !String.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME))
                {
                    if (childContainer == null)
                    {
                        childContainer = xmlHelper.addTextNode(parentNode.OwnerDocument,
                                                               UmbracoSettings.
                                                                   TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME, "");
                        parentNode.AppendChild(childContainer);
                    }
                }

                foreach (int childId in children)
                {
                    XmlNode childNode = nodeIndex[childId];

                    if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ||
                        String.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME))
                    {
                        parentNode.AppendChild(childNode);
                    }
                    else
                    {
                        childContainer.AppendChild(childNode);
                    }

                    // Recursively build the content tree under the current child
                    GenerateXmlDocument(hierarchy, nodeIndex, childId, childNode);
                }
            }
        }

        public void PersistXmlToFile()
        {
            PersistXmlToFile(_xmlContent);
        }

        /// <summary>
        /// Persist a XmlDocument to the Disk Cache
        /// </summary>
        /// <param name="xmlDoc"></param>
        internal void PersistXmlToFile(XmlDocument xmlDoc)
        {
            lock (ReaderWriterSyncLock)
            {
                if (xmlDoc != null)
                {
                    Trace.Write(string.Format("Saving content to disk on thread '{0}' (Threadpool? {1})",
                                              Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread));

                    // Moved the user into a variable and avoided it throwing an error if one can't be loaded (e.g. empty / corrupt db on initial install)
                    User staticUser = null;
                    try
                    {
                        staticUser = User.GetCurrent();
                    }
                    catch
                    {
                    }

                    try
                    {
                        Stopwatch stopWatch = Stopwatch.StartNew();

                        DeleteXmlCache();

                        // Try to create directory for cache path if it doesn't yet exist
                        string directoryName = Path.GetDirectoryName(UmbracoXmlDiskCacheFileName);
                        if (!File.Exists(UmbracoXmlDiskCacheFileName) && !Directory.Exists(directoryName))
                        {
                            // We're already in a try-catch and saving will fail if this does, so don't need another
                            Directory.CreateDirectory(directoryName);
                        }

                        xmlDoc.Save(UmbracoXmlDiskCacheFileName);

                        Trace.Write(string.Format("Saved content on thread '{0}' in {1} (Threadpool? {2})",
                                                  Thread.CurrentThread.Name, stopWatch.Elapsed,
                                                  Thread.CurrentThread.IsThreadPoolThread));

						LogHelper.Debug<content>(string.Format("Xml saved in {0}", stopWatch.Elapsed));
                    }
                    catch (Exception ee)
                    {
                        // If for whatever reason something goes wrong here, invalidate disk cache
                        DeleteXmlCache();

                        Trace.Write(string.Format(
                            "Error saving content on thread '{0}' due to '{1}' (Threadpool? {2})",
                            Thread.CurrentThread.Name, ee.Message, Thread.CurrentThread.IsThreadPoolThread));

                        LogHelper.Error<content>("Xml wasn't saved", ee);
                    }
                }
            }
        }

        /// <summary>
        /// Marks a flag in the HttpContext so that, upon page execution completion, the Xml cache will
        /// get persisted to disk. Ensure this method is only called from a thread executing a page request
        /// since umbraco.presentation.requestModule is the only monitor of this flag and is responsible
        /// for enacting the persistence at the PostRequestHandlerExecute stage of the page lifecycle.
        /// </summary>
        private void QueueXmlForPersistence()
        {
            /* Alex Norcliffe 2010 06 03 - removing all launching of ThreadPool threads, instead we just 
             * flag on the context that the Xml should be saved and an event in the requestModule
             * will check for this and call PersistXmlToFile() if necessary */
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Application.Lock();
                if (HttpContext.Current.Application[PersistenceFlagContextKey] != null)
                    HttpContext.Current.Application.Add(PersistenceFlagContextKey, null);
                HttpContext.Current.Application[PersistenceFlagContextKey] = DateTime.UtcNow;
                HttpContext.Current.Application.UnLock();
            }
            else
            {
                //// Save copy of content
                if (UmbracoConfig.For.UmbracoSettings().Content.CloneXmlContent)
                {
                    XmlDocument xmlContentCopy = CloneXmlDoc(_xmlContent);

                    ThreadPool.QueueUserWorkItem(
                        delegate { PersistXmlToFile(xmlContentCopy); });
                }
                else
                    ThreadPool.QueueUserWorkItem(
                        delegate { PersistXmlToFile(); });
            }
        }

        internal DateTime GetCacheFileUpdateTime()
        {
            if (File.Exists(UmbracoXmlDiskCacheFileName))
            {
                return new FileInfo(UmbracoXmlDiskCacheFileName).LastWriteTimeUtc;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Make a copy of a XmlDocument
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private static XmlDocument CloneXmlDoc(XmlDocument xmlDoc)
        {
            if (xmlDoc == null) return null;            
            
            // Save copy of content
            var xmlCopy = (XmlDocument)xmlDoc.CloneNode(true);            
            return xmlCopy;
        }

        #endregion
    }
}