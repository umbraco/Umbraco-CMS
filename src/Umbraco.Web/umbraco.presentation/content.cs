using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.IO;
using umbraco.presentation.nodeFactory;
using Action = umbraco.BusinessLogic.Actions.Action;
using Node = umbraco.NodeFactory.Node;

namespace umbraco
{
    /// <summary>
    /// Handles umbraco content
    /// </summary>
    public class content
    {
        #region Declarations

        // Sync access to disk file
        private static readonly object _readerWriterSyncLock = new object();

        // Sync access to internal cache
        private static readonly object _xmlContentInternalSyncLock = new object();

        // Sync access to timestamps
        private static readonly object _timestampSyncLock = new object();

        // Sync database access
        private static readonly object _dbReadSyncLock = new object();
        private readonly string XmlContextContentItemKey = "UmbracoXmlContextContent";
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

        #region Constructors

        static content()
        {
            //Trace.Write("Initializing content");
            //ThreadPool.QueueUserWorkItem(
            //    delegate
            //    {
            //        XmlDocument xmlDoc = Instance.XmlContentInternal;
            //        Trace.WriteLine("Content initialized");
            //    });

            Trace.WriteLine("Checking for xml content initialisation...");
            Instance.CheckXmlContentPopulation();
        }

        public content()
        {
            ;
        }

        #endregion

        #region Singleton

        public static content Instance
        {
            get { return Singleton<content>.Instance; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get content. First call to this property will initialize xmldoc
        /// subsequent calls will be blocked until initialization is done
        /// Further we cache(in context) xmlContent for each request to ensure that
        /// we always have the same XmlDoc throughout the whole request.
        /// </summary>
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

        public virtual bool isInitializing
        {
            get { return _xmlContent == null; }
        }

        /// <summary>
        /// Internal reference to XmlContent
        /// </summary>
        protected virtual XmlDocument XmlContentInternal
        {
            get
            {
                CheckXmlContentPopulation();

                return _xmlContent;
            }
            set
            {
                lock (_xmlContentInternalSyncLock)
                {
                    // Clear macro cache
                    Cache.ClearCacheObjectTypes("umbraco.MacroCacheContent");
                    Cache.ClearCacheByKeySearch("macroHtml_");

                    // Clear library cache
                    if (UmbracoSettings.UmbracoLibraryCacheDuration > 0)
                    {
                        Cache.ClearCacheObjectTypes("MS.Internal.Xml.XPath.XPathSelectionIterator");
                    }
                    requestHandler.ClearProcessedRequests();
                    _xmlContent = value;

                    if (!UmbracoSettings.isXmlContentCacheDisabled && UmbracoSettings.continouslyUpdateXmlDiskCache)
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
            if (UmbracoSettings.isXmlContentCacheDisabled)
                return;

            lock (_timestampSyncLock)
            {
                if (_lastDiskCacheCheckTime > DateTime.UtcNow.AddSeconds(-1.0))
                    return;

                _lastDiskCacheCheckTime = DateTime.UtcNow;

                lock (_xmlContentInternalSyncLock)
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
        /// <returns></returns>
        private bool CheckXmlContentPopulation()
        {
            if (UmbracoSettings.XmlContentCheckForDiskChanges)
                CheckDiskCacheForUpdate();

            if (isInitializing)
            {
                lock (_xmlContentInternalSyncLock)
                {
                    if (isInitializing)
                    {
                        Trace.WriteLine(string.Format("Initializing content on thread '{0}' (Threadpool? {1})",
                                                      Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread));
                        _xmlContent = LoadContent();
                        Trace.WriteLine("Content initialized (loaded)");

                        FireAfterRefreshContent(new RefreshContentEventArgs());

                        // Only save new XML cache to disk if we just repopulated it
                        // TODO: Re-architect this so that a call to this method doesn't invoke a new thread for saving disk cache
                        if (!UmbracoSettings.isXmlContentCacheDisabled && !IsValidDiskCachePresent())
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

        protected static void ValidateSchema(string docTypeAlias, XmlDocument xmlDoc)
        {
            // if doctype is not defined i)n schema, then regenerate it
            if (
                !xmlDoc.DocumentType.InternalSubset.Contains(String.Format("<!ATTLIST {0} id ID #REQUIRED>",
                                                                           docTypeAlias)))
            {
                // we need to re-load the content, else the dtd changes won't be picked up by the XmlDocument
                Instance.XmlContentInternal = Instance.LoadContentFromDatabase();
            }
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
                        if (!UmbracoSettings.isXmlContentCacheDisabled &&
                            UmbracoSettings.continouslyUpdateXmlDiskCache)
                            PersistXmlToFile(xmlDoc);
                    });

                FireAfterRefreshContent(e);
            }
        }

        public static void TransferValuesFromDocumentXmlToPublishedXml(XmlNode DocumentNode, XmlNode PublishedNode)
        {
            // Remove all attributes and data nodes from the published node
            PublishedNode.Attributes.RemoveAll();
            string xpath = UmbracoSettings.UseLegacyXmlSchema ? "./data" : "./* [not(@id)]";
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
        public static void PublishNodeDo(Document d, XmlDocument xmlContentCopy, bool updateSitemapProvider)
        {
            // check if document *is* published, it could be unpublished by an event
            if (d.Published)
            {
                int parentId = d.Level == 1 ? -1 : d.Parent.Id;
                AppendDocumentXml(d.Id, d.Level, parentId, getPreviewOrPublishedNode(d, xmlContentCopy, false),
                                  xmlContentCopy);

                // update sitemapprovider
                if (updateSitemapProvider && SiteMap.Provider is UmbracoSiteMapProvider)
                {
                    try
                    {
                        var prov = (UmbracoSiteMapProvider)SiteMap.Provider;
                        Node n = new Node(d.Id, true);
                        if (!String.IsNullOrEmpty(n.Url) && n.Url != "/#")
                            prov.UpdateNode(n);
                        else
                        {
                            Log.Add(LogTypes.Error, d.Id, "Can't update Sitemap Provider due to empty Url in node");
                        }
                    }
                    catch (Exception ee)
                    {
                        Log.Add(LogTypes.Error, d.Id, string.Format("Error adding node to Sitemap Provider in PublishNodeDo(): {0}", ee));
                    }
                }
            }
        }

        public static void AppendDocumentXml(int id, int level, int parentId, XmlNode docXml, XmlDocument xmlContentCopy)
        {
            // Validate schema (that a definition of the current document type exists in the DTD
            if (!UmbracoSettings.UseLegacyXmlSchema)
            {
                ValidateSchema(docXml.Name, xmlContentCopy);
            }

            // Find the document in the xml cache
            XmlNode x = xmlContentCopy.GetElementById(id.ToString());

            // Find the parent (used for sortering and maybe creation of new node)
            XmlNode parentNode;
            if (level == 1)
                parentNode = xmlContentCopy.DocumentElement;
            else
                parentNode = xmlContentCopy.GetElementById(parentId.ToString());

            if (parentNode != null)
            {
                if (x == null)
                {
                    x = docXml;
                    parentNode.AppendChild(x);
                }
                else
                    TransferValuesFromDocumentXmlToPublishedXml(docXml, x);

                // TODO: Update with new schema!
                string xpath = UmbracoSettings.UseLegacyXmlSchema ? "./node" : "./* [@id]";
                XmlNodeList childNodes = parentNode.SelectNodes(xpath);

                // Maybe sort the nodes if the added node has a lower sortorder than the last
                if (childNodes.Count > 0)
                {
                    int siblingSortOrder =
                        int.Parse(childNodes[childNodes.Count - 1].Attributes.GetNamedItem("sortOrder").Value);
                    int currentSortOrder = int.Parse(x.Attributes.GetNamedItem("sortOrder").Value);
                    if (childNodes.Count > 1 && siblingSortOrder > currentSortOrder)
                    {
                        SortNodes(ref parentNode);
                    }
                }
            }
        }

        private static XmlNode getPreviewOrPublishedNode(Document d, XmlDocument xmlContentCopy, bool isPreview)
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
            XmlNode n = parentNode.CloneNode(true);

            // remove all children from original node
            string xpath = UmbracoSettings.UseLegacyXmlSchema ? "./node" : "./* [@id]";
            foreach (XmlNode child in parentNode.SelectNodes(xpath))
                parentNode.RemoveChild(child);


            XPathNavigator nav = n.CreateNavigator();
            XPathExpression expr = nav.Compile(xpath);
            expr.AddSort("@sortOrder", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Number);
            XPathNodeIterator iterator = nav.Select(expr);
            while (iterator.MoveNext())
                parentNode.AppendChild(
                    ((IHasXmlNode)iterator.Current).GetNode());
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
                // We need to lock content cache here, because we cannot allow other threads
                // making changes at the same time, they need to be queued
                // Adding log entry before locking the xmlfile
                lock (_xmlContentInternalSyncLock)
                {
                    // Make copy of memory content, we cannot make changes to the same document
                    // the is read from elsewhere
                    if (UmbracoSettings.CloneXmlCacheOnPublish)
                    {
                        XmlDocument xmlContentCopy = CloneXmlDoc(XmlContentInternal);

                        PublishNodeDo(d, xmlContentCopy, true);
                        XmlContentInternal = xmlContentCopy;
                    }
                    else
                    {
                        PublishNodeDo(d, XmlContentInternal, true);
                        XmlContentInternal = _xmlContent;
                    }

                    ClearContextCache();
                }

                // clear cached field values
                if (HttpContext.Current != null)
                {
                    System.Web.Caching.Cache httpCache = HttpContext.Current.Cache;
                    string cachedFieldKeyStart = String.Format("contentItem{0}_", d.Id);
                    var foundKeys = new List<string>();
                    foreach (DictionaryEntry cacheItem in httpCache)
                    {
                        string key = cacheItem.Key.ToString();
                        if (key.StartsWith(cachedFieldKeyStart))
                            foundKeys.Add(key);
                    }
                    foreach (string foundKey in foundKeys)
                    {
                        httpCache.Remove(foundKey);
                    }
                }
                Action.RunActionHandlers(d, ActionPublish.Instance);

                FireAfterUpdateDocumentCache(d, e);
            }
        }

        /// <summary>
        /// Updates the document cache for multiple documents
        /// </summary>
        /// <param name="Documents">The documents.</param>
        public virtual void UpdateDocumentCache(List<Document> Documents)
        {
            // We need to lock content cache here, because we cannot allow other threads
            // making changes at the same time, they need to be queued
            int parentid = Documents[0].Id;

            lock (_xmlContentInternalSyncLock)
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

            foreach (Document d in Documents)
            {
                Action.RunActionHandlers(d, ActionPublish.Instance);
            }
        }

        [Obsolete("Method obsolete in version 4.1 and later, please use UpdateDocumentCache", true)]
        /// <summary>
        /// Updates the document cache async.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        public virtual void UpdateDocumentCacheAsync(int documentId)
        {
            ThreadPool.QueueUserWorkItem(delegate { UpdateDocumentCache(documentId); });
        }


        [Obsolete("Method obsolete in version 4.1 and later, please use ClearDocumentCache", true)]
        /// <summary>
        /// Clears the document cache async.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        public virtual void ClearDocumentCacheAsync(int documentId)
        {
            ThreadPool.QueueUserWorkItem(delegate { ClearDocumentCache(documentId); });
        }


        /// <summary>
        /// Clears the document cache and removes the document from the xml db cache.
        /// This means the node gets unpublished from the website.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        public virtual void ClearDocumentCache(int documentId)
        {
            // Get the document
            var d = new Document(documentId);

            var e = new DocumentCacheEventArgs();
            FireBeforeClearDocumentCache(d, e);

            if (!e.Cancel)
            {
                XmlNode x;

                // remove from xml db cache 
                d.XmlRemoveFromDB();

                // Check if node present, before cloning
                x = XmlContentInternal.GetElementById(d.Id.ToString());
                if (x == null)
                    return;

                // We need to lock content cache here, because we cannot allow other threads
                // making changes at the same time, they need to be queued
                lock (_xmlContentInternalSyncLock)
                {
                    // Make copy of memory content, we cannot make changes to the same document
                    // the is read from elsewhere
                    XmlDocument xmlContentCopy = CloneXmlDoc(XmlContentInternal);

                    // Find the document in the xml cache
                    x = xmlContentCopy.GetElementById(d.Id.ToString());
                    if (x != null)
                    {
                        // The document already exists in cache, so repopulate it
                        x.ParentNode.RemoveChild(x);
                        XmlContentInternal = xmlContentCopy;
                        ClearContextCache();
                    }
                }

                if (x != null)
                {
                    // Run Handler				
                    Action.RunActionHandlers(d, ActionUnPublish.Instance);
                }

                // update sitemapprovider
                if (SiteMap.Provider is UmbracoSiteMapProvider)
                {
                    var prov = (UmbracoSiteMapProvider)SiteMap.Provider;
                    prov.RemoveNode(d.Id);
                }

                FireAfterClearDocumentCache(d, e);
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
            lock (_readerWriterSyncLock)
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
        /// Invalidates the disk content cache file. Effectively just deletes it.
        /// </summary>
        [Obsolete("This method is obsolete in version 4.1 and above, please use DeleteXmlCache", true)]
        private void ClearDiskCacheAsync()
        {
            // Queue file deletion
            // We queue this function, because there can be a write process running at the same time
            // and we don't want this method to block web request
            ThreadPool.QueueUserWorkItem(
                delegate { DeleteXmlCache(); });
        }

        /// <summary>
        /// Load content from either disk or database
        /// </summary>
        /// <returns></returns>
        private XmlDocument LoadContent()
        {
            if (!UmbracoSettings.isXmlContentCacheDisabled && IsValidDiskCachePresent())
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
            lock (_readerWriterSyncLock)
            {
                var xmlDoc = new XmlDocument();
                Log.Add(LogTypes.System, User.GetUser(0), -1, "Loading content from disk cache...");
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
                Log.Add(LogTypes.System, staticUser, -1, "Loading content from database...");

                var hierarchy = new Dictionary<int, List<int>>();
                var nodeIndex = new Dictionary<int, XmlNode>();

                try
                {
                    Log.Add(LogTypes.Debug, staticUser, -1, "Republishing starting");

                    // Lets cache the DTD to save on the DB hit on the subsequent use
                    string dtd = DocumentType.GenerateDtd();

                    // Prepare an XmlDocument with an appropriate inline DTD to match
                    // the expected content
                    var xmlDoc = new XmlDocument();
                    InitContentDocument(xmlDoc, dtd);

                    // Esben Carlsen: At some point we really need to put all data access into to a tier of its own.
                    string sql =
                        @"select umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, cmsContentXml.xml from umbracoNode 
inner join cmsContentXml on cmsContentXml.nodeId = umbracoNode.id and umbracoNode.nodeObjectType = @type
order by umbracoNode.level, umbracoNode.sortOrder";

                    lock (_dbReadSyncLock)
                    {
                        using (
                            IRecordsReader dr = SqlHelper.ExecuteReader(sql,
                                                                        SqlHelper.CreateParameter("@type",
                                                                                                  new Guid(
                                                                                                      "C66BA18E-EAF3-4CFF-8A22-41B16D66A972")))
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
                    }

                    Log.Add(LogTypes.Debug, staticUser, -1, "Xml Pages loaded");

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

                        Log.Add(LogTypes.Debug, staticUser, -1, "Done republishing Xml Index");

                        return xmlDoc;
                    }
                    catch (Exception ee)
                    {
                        Log.Add(LogTypes.Error, staticUser, -1,
                                string.Format("Error while generating XmlDocument from database: {0}", ee));
                    }
                }
                catch (OutOfMemoryException)
                {
                    Log.Add(LogTypes.Error, staticUser, -1,
                            string.Format("Error Republishing: Out Of Memory. Parents: {0}, Nodes: {1}",
                                          hierarchy.Count, nodeIndex.Count));
                }
                catch (Exception ee)
                {
                    Log.Add(LogTypes.Error, staticUser, -1, string.Format("Error Republishing: {0}", ee));
                }
            }
            catch (Exception ee)
            {
                Log.Add(LogTypes.Error, -1, string.Format("Error Republishing: {0}", ee));
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
                XmlNode childContainer = UmbracoSettings.UseLegacyXmlSchema ||
                                         String.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME)
                                             ? parentNode
                                             : parentNode.SelectSingleNode(
                                                 UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME);

                if (!UmbracoSettings.UseLegacyXmlSchema &&
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

                    if (UmbracoSettings.UseLegacyXmlSchema ||
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
            lock (_readerWriterSyncLock)
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

                        Log.Add(LogTypes.Debug, staticUser, -1, string.Format("Xml saved in {0}", stopWatch.Elapsed));
                    }
                    catch (Exception ee)
                    {
                        // If for whatever reason something goes wrong here, invalidate disk cache
                        DeleteXmlCache();

                        Trace.Write(string.Format(
                            "Error saving content on thread '{0}' due to '{1}' (Threadpool? {2})",
                            Thread.CurrentThread.Name, ee.Message, Thread.CurrentThread.IsThreadPoolThread));
                        Log.Add(LogTypes.Error, staticUser, -1, string.Format("Xml wasn't saved: {0}", ee));
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
                if (UmbracoSettings.CloneXmlCacheOnPublish)
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

            Log.Add(LogTypes.Debug, -1, "Cloning...");
            // Save copy of content
            var xmlCopy = (XmlDocument)xmlDoc.CloneNode(true);
            Log.Add(LogTypes.Debug, -1, "Cloning ended...");
            return xmlCopy;
        }

        #endregion
    }
}