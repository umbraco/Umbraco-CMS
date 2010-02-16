/// <changelog>
///   <item who="Esben" when="18. november 2006">Rewrote</item>
/// </changelog>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.XPath;

using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.IO;

namespace umbraco
{
    /// <summary>
    /// Handles umbraco content
    /// </summary>
    public class content
    {
        #region Declarations

        private string _umbracoXmlDiskCacheFileName = IOHelper.MapPath(SystemFiles.ContentCacheXml, false);

        /// <summary>
        /// Gets the path of the umbraco XML disk cache file.
        /// </summary>
        /// <value>The name of the umbraco XML disk cache file.</value>
        public string UmbracoXmlDiskCacheFileName
        {
            get
            {
                return _umbracoXmlDiskCacheFileName;
            }
            set
            {
                _umbracoXmlDiskCacheFileName = value;
            }
        }


        /*
            HttpRuntime.AppDomainAppPath + '\\' +  
            SystemFiles.ContentCacheXml.Replace('/', '\\').TrimStart('\\');
        */

        private readonly string XmlContextContentItemKey = "UmbracoXmlContextContent";

        // Current content
        private volatile XmlDocument _xmlContent = null;

        // Sync access to disk file
        private object _readerWriterSyncLock = new object();

        // Sync access to internal cache
        private object _xmlContentInternalSyncLock = new object();

        // Sync database access
        private object _dbReadSyncLock = new object();

        #endregion

        #region Constructors

        public content()
        {
            ;
        }

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
        /// Note that context cache does not need to be locked, because all access
        /// to it is done from a web request which runs in a single thread
        /// </summary>
        public virtual XmlDocument XmlContent
        {
            get
            {
                if (HttpContext.Current == null)
                    return XmlContentInternal;
                XmlDocument content = HttpContext.Current.Items[XmlContextContentItemKey] as XmlDocument;
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
                    requestHandler.ClearProcessedRequests();
                    _xmlContent = value;

                    if (!UmbracoSettings.isXmlContentCacheDisabled && UmbracoSettings.continouslyUpdateXmlDiskCache)
                        SaveContentToDiskAsync(_xmlContent);
                    else
                        // Clear cache...
                        ClearDiskCacheAsync();
                }
            }
        }

        /// <summary>
        /// Triggers the XML content population if necessary.
        /// </summary>
        /// <returns></returns>
        private bool CheckXmlContentPopulation()
        {
            if (isInitializing)
            {
                lock (_xmlContentInternalSyncLock)
                {
                    if (isInitializing)
                    {
                        Trace.WriteLine(string.Format("Initializing content on thread '{0}' (Threadpool? {1})", Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread.ToString()));
                        _xmlContent = LoadContent();
                        Trace.WriteLine("Content initialized (loaded)");

                        // Only save new XML cache to disk if we just repopulated it
                        // TODO: Re-architect this so that a call to this method doesn't invoke a new thread for saving disk cache
                        if (!UmbracoSettings.isXmlContentCacheDisabled && !IsValidDiskCachePresent())
                        {
                            SaveContentToDiskAsync(_xmlContent);
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

        

        #region Public Methods

        /// <summary>
        /// Load content from database in a background thread
        /// Replaces active content when done.
        /// </summary>
        public virtual void RefreshContentFromDatabaseAsync()
        {
            cms.businesslogic.RefreshContentEventArgs e = new umbraco.cms.businesslogic.RefreshContentEventArgs();
            FireBeforeRefreshContent(e);

            if (!e.Cancel)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        XmlDocument xmlDoc = LoadContentFromDatabase();
                        XmlContentInternal = xmlDoc;

                        if (!UmbracoSettings.isXmlContentCacheDisabled)
                            SaveContentToDisk(xmlDoc);
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
                AppendDocumentXml(d.Id, d.Level, d.Parent.Id, getPreviewOrPublishedNode(d, xmlContentCopy, false), xmlContentCopy);

                // update sitemapprovider
                if (updateSitemapProvider && SiteMap.Provider is presentation.nodeFactory.UmbracoSiteMapProvider)
                {
                    presentation.nodeFactory.UmbracoSiteMapProvider prov = (presentation.nodeFactory.UmbracoSiteMapProvider)SiteMap.Provider;
                    prov.UpdateNode(new umbraco.presentation.nodeFactory.Node(d.Id));
                }
            }
        }

        public static void AppendDocumentXml(int id, int level, int parentId, XmlNode docXml, XmlDocument xmlContentCopy)
        {


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
                    int siblingSortOrder = int.Parse(childNodes[childNodes.Count - 1].Attributes.GetNamedItem("sortOrder").Value);
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
            Document d = new Document(pageId);
            UpdateDocumentCache(d);
        }


        /// <summary>
        /// Updates the document cache.
        /// </summary>
        /// <param name="d">The d.</param>
        public virtual void UpdateDocumentCache(Document d)
        {

            cms.businesslogic.DocumentCacheEventArgs e = new umbraco.cms.businesslogic.DocumentCacheEventArgs();
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
                System.Web.Caching.Cache httpCache = HttpContext.Current.Cache;
                string cachedFieldKeyStart = String.Format("contentItem{0}_", d.Id);
                List<string> foundKeys = new List<string>();
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

                umbraco.BusinessLogic.Actions.Action.RunActionHandlers(d, ActionPublish.Instance);

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
                umbraco.BusinessLogic.Actions.Action.RunActionHandlers(d, ActionPublish.Instance);
            }
        }

        /// <summary>
        /// Updates the document cache async.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        public virtual void UpdateDocumentCacheAsync(int documentId)
        {
            ThreadPool.QueueUserWorkItem(delegate { UpdateDocumentCache(documentId); });
        }



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
            Document d = new Document(documentId);

            cms.businesslogic.DocumentCacheEventArgs e = new umbraco.cms.businesslogic.DocumentCacheEventArgs();
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
                    umbraco.BusinessLogic.Actions.Action.RunActionHandlers(d, ActionUnPublish.Instance);
                }

                // update sitemapprovider
                if (SiteMap.Provider is presentation.nodeFactory.UmbracoSiteMapProvider)
                {
                    presentation.nodeFactory.UmbracoSiteMapProvider prov = (presentation.nodeFactory.UmbracoSiteMapProvider)SiteMap.Provider;
                    prov.RemoveNode(d.Id);
                }

                FireAfterClearDocumentCache(d, e);
            }
        }


        /// <summary>
        /// Unpublishes the  node.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        [Obsolete("Please use: umbraco.content.ClearDocumentCache")]
        public virtual void UnPublishNode(int documentId)
        {
            ClearDocumentCache(documentId);
        }

        /// <summary>
        /// Uns the publish node async.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        [Obsolete("Please use: umbraco.content.ClearDocumentCacheAsync")]
        public virtual void UnPublishNodeAsync(int documentId)
        {
            ThreadPool.QueueUserWorkItem(delegate { ClearDocumentCache(documentId); });
        }

        /// <summary>
        /// Legacy method - you should use the overloaded publishnode(document d) method whenever possible
        /// </summary>
        /// <param name="documentId"></param>
        [Obsolete("Please use: umbraco.content.UpdateDocumentCache")]
        public virtual void PublishNode(int documentId)
        {
            // Get the document
            Document d = new Document(documentId);
            PublishNode(d);
        }

        /// <summary>
        /// Publishes the node async.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        [Obsolete("Please use: umbraco.content.UpdateDocumentCacheAsync")]
        public virtual void PublishNodeAsync(int documentId)
        {
            UpdateDocumentCacheAsync(documentId);
        }

        /// <summary>
        /// Publishes the node.
        /// </summary>
        /// <param name="Documents">The documents.</param>
        [Obsolete("Please use: umbraco.content.UpdateDocumentCache")]
        public virtual void PublishNode(List<Document> Documents)
        {
            UpdateDocumentCache(Documents);
        }


        /// <summary>
        /// Publishes the node.
        /// </summary>
        /// <param name="d">The document.</param>
        [Obsolete("Please use: umbraco.content.UpdateDocumentCache")]
        public virtual void PublishNode(Document d)
        {
            UpdateDocumentCache(d);
        }

        public delegate void DocumentCacheEventHandler(Document sender, cms.businesslogic.DocumentCacheEventArgs e);
        public delegate void RefreshContentEventHandler(Document sender, cms.businesslogic.RefreshContentEventArgs e);

        /// <summary>
        /// Occurs when [before document cache update].
        /// </summary>
        public static event DocumentCacheEventHandler BeforeUpdateDocumentCache;

        /// <summary>
        /// Fires the before document cache.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.DocumentCacheEventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeUpdateDocumentCache(Document sender, cms.businesslogic.DocumentCacheEventArgs e)
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
        protected virtual void FireAfterUpdateDocumentCache(Document sender, cms.businesslogic.DocumentCacheEventArgs e)
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
        protected virtual void FireBeforeClearDocumentCache(Document sender, cms.businesslogic.DocumentCacheEventArgs e)
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
        protected virtual void FireAfterClearDocumentCache(Document sender, cms.businesslogic.DocumentCacheEventArgs e)
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
        protected virtual void FireBeforeRefreshContent(cms.businesslogic.RefreshContentEventArgs e)
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
        protected virtual void FireAfterRefreshContent(cms.businesslogic.RefreshContentEventArgs e)
        {
            if (AfterRefreshContent != null)
            {
                AfterRefreshContent(null, e);
            }
        }



        #endregion

        #region Protected & Private methods

        /// <summary>
        /// Invalidates the disk content cache file. Effectively just deletes it.
        /// </summary>
        private void ClearDiskCache()
        {
            lock (_readerWriterSyncLock)
            {
                if (File.Exists(UmbracoXmlDiskCacheFileName))
                {
                    // Reset file attributes, to make sure we can delete file
                    File.SetAttributes(UmbracoXmlDiskCacheFileName, FileAttributes.Normal);
                    File.Delete(UmbracoXmlDiskCacheFileName);
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
        private void ClearDiskCacheAsync()
        {
            // Queue file deletion
            // We queue this function, because there can be a write process running at the same time
            // and we don't want this method to block web request
            ThreadPool.QueueUserWorkItem(
                delegate { ClearDiskCache(); });
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
                    ClearDiskCache();
                }
            }
            return LoadContentFromDatabase();
        }

        private bool IsValidDiskCachePresent()
        {
            return File.Exists(UmbracoXmlDiskCacheFileName);
        }

        /// <summary>
        /// Load content from cache file
        /// </summary>
        private XmlDocument LoadContentFromDiskCache()
        {
            lock (_readerWriterSyncLock)
            {
                XmlDocument xmlDoc = new XmlDocument();
                Log.Add(LogTypes.System, User.GetUser(0), -1, "Loading content from disk cache...");
                xmlDoc.Load(UmbracoXmlDiskCacheFileName);
                return xmlDoc;
            }
        }

        private void InitContentDocumentBase(XmlDocument xmlDoc)
        {
            // Create id -1 attribute
            xmlDoc.LoadXml(String.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "{0}" + Environment.NewLine +
                "<root id=\"-1\"/>", DocumentType.GenerateDtd()));
        }

        /// <summary>
        /// Load content from database
        /// </summary>
        private XmlDocument LoadContentFromDatabase()
        {
            XmlDocument xmlDoc = new XmlDocument();
            
            // Moved User to a local variable - why are we causing user 0 to load from the DB though?
            // Alex N 20100212
            User staticUser = null;
            try
            {
                staticUser = User.GetCurrent();  //User.GetUser(0);
            }
            catch
            {
                /* We don't care later if the staticUser is null */
            }

            // Try to log to the DB
            Log.Add(LogTypes.System, staticUser, -1, "Loading content from database...");

            // Moved this to after the logging since the 2010 schema accesses the DB just to generate the DTD
            InitContentDocumentBase(xmlDoc);           

            Hashtable nodes = new Hashtable();
            Hashtable parents = new Hashtable();
            try
            {
                Log.Add(LogTypes.Debug, staticUser, -1, "Republishing starting");

                // Esben Carlsen: At some point we really need to put all data access into to a tier of its own.
                string sql =
                    @"select umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, cmsContentXml.xml from umbracoNode 
inner join cmsContentXml on cmsContentXml.nodeId = umbracoNode.id and umbracoNode.nodeObjectType = @type
order by umbracoNode.level, umbracoNode.sortOrder";

                lock (_dbReadSyncLock)
                {
                    using (IRecordsReader dr = SqlHelper.ExecuteReader(sql, SqlHelper.CreateParameter("@type", new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"))))
                    {
                        while (dr.Read())
                        {
                            int currentId = dr.GetInt("id");
                            int parentId = dr.GetInt("parentId");

                            xmlDoc.LoadXml(dr.GetString("xml"));
                            nodes.Add(currentId, xmlDoc.FirstChild);

                            if (parents.ContainsKey(parentId))
                                ((ArrayList)parents[parentId]).Add(currentId);
                            else
                            {
                                ArrayList a = new ArrayList();
                                a.Add(currentId);
                                parents.Add(parentId, a);
                            }
                        }
                    }
                }

                Log.Add(LogTypes.Debug, staticUser, -1, "Xml Pages loaded");

                // TODO: Why is the following line here, it should have already been generated? Alex N 20100212
                // Reset
                InitContentDocumentBase(xmlDoc);

                try
                {
                    GenerateXmlDocument(parents, nodes, -1, xmlDoc.DocumentElement);
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
                                      parents.Count, nodes.Count));
            }
            catch (Exception ee)
            {
                Log.Add(LogTypes.Error, staticUser, -1, string.Format("Error Republishing: {0}", ee));
            }
            finally
            {
                Log.Add(LogTypes.Debug, staticUser, -1, "Done republishing Xml Index");
            }

            return xmlDoc;
        }

        private void GenerateXmlDocument(Hashtable parents, Hashtable nodes, int parentId, XmlNode parentNode)
        {
            if (parents.ContainsKey(parentId))
            {
                ArrayList children = (ArrayList)parents[parentId];
                XmlNode childContainer = UmbracoSettings.UseLegacyXmlSchema || String.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME)
                                             ? parentNode
                                             : parentNode.SelectSingleNode(
                                                   UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME);

                if (!UmbracoSettings.UseLegacyXmlSchema && !String.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME))
                {
                    if (childContainer == null)
                    {
                        childContainer = xmlHelper.addTextNode(parentNode.OwnerDocument, UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME, "");
                        parentNode.AppendChild(childContainer);
                    }
                }
                foreach (int i in children)
                {
                    XmlNode childNode = (XmlNode)nodes[i];
                    if (UmbracoSettings.UseLegacyXmlSchema || String.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME))
                    {
                        parentNode.AppendChild(childNode);
                    }
                    else
                    {
                        childContainer.AppendChild(childNode);
                    }
                    GenerateXmlDocument(parents, nodes, i, childNode);
                }
            }
        }
        /// <summary>
        /// Persist a XmlDocument to the Disk Cache
        /// </summary>
        /// <param name="xmlDoc"></param>
        internal void SaveContentToDisk(XmlDocument xmlDoc)
        {
            lock (_readerWriterSyncLock)
            {
                Trace.Write(string.Format("Saving content to disk on thread '{0}' (Threadpool? {1})", Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread.ToString()));

                // Moved the user into a variable and avoided it throwing an error if one can't be loaded (e.g. empty / corrupt db on initial install)
                User staticUser = null;
                try
                {
                    staticUser = User.GetCurrent();
                }
                catch
                {}

                try
                {
                    Stopwatch stopWatch = Stopwatch.StartNew();

                    ClearDiskCache();

                    // Try to create directory for cache path if it doesn't yet exist
                    if (!File.Exists(UmbracoXmlDiskCacheFileName) && !Directory.Exists(Path.GetDirectoryName(UmbracoXmlDiskCacheFileName)))
                    {
                        // We're already in a try-catch and saving will fail if this does, so don't need another
                        Directory.CreateDirectory(UmbracoXmlDiskCacheFileName);
                    }

                    xmlDoc.Save(UmbracoXmlDiskCacheFileName);

                    Trace.Write(string.Format("Saved content on thread '{0}' in {1} (Threadpool? {2})", Thread.CurrentThread.Name, stopWatch.Elapsed, Thread.CurrentThread.IsThreadPoolThread.ToString()));
                   Log.Add(LogTypes.Debug, staticUser, -1, string.Format("Xml saved in {0}", stopWatch.Elapsed));
                }
                catch (Exception ee)
                {
                    // If for whatever reason something goes wrong here, invalidate disk cache
                    ClearDiskCache();

                    Trace.Write(string.Format("Error saving content on thread '{0}' due to '{1}' (Threadpool? {2})", Thread.CurrentThread.Name, ee.Message, Thread.CurrentThread.IsThreadPoolThread.ToString()));
                    Log.Add(LogTypes.Error, staticUser, -1, string.Format("Xml wasn't saved: {0}", ee));
                }
            }
        }

        /// <summary>
        /// Persist xml document to disk cache in a background thread
        /// </summary>
        /// <param name="xmlDoc"></param>
        private void SaveContentToDiskAsync(XmlDocument xmlDoc)
        {
            // Save copy of content
            if (UmbracoSettings.CloneXmlCacheOnPublish)
            {
                XmlDocument xmlContentCopy = CloneXmlDoc(xmlDoc);

                ThreadPool.QueueUserWorkItem(
                    delegate { SaveContentToDisk(xmlContentCopy); });

            }
            else
                ThreadPool.QueueUserWorkItem(
                    delegate { SaveContentToDisk(xmlDoc); });
        }

        /// <summary>
        /// Make a copy of a XmlDocument
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        private XmlDocument CloneXmlDoc(XmlDocument xmlDoc)
        {
            Log.Add(LogTypes.Debug, -1, "Cloning...");
            // Save copy of content
            XmlDocument xmlCopy = new XmlDocument();
            xmlCopy.LoadXml(xmlDoc.OuterXml);
            Log.Add(LogTypes.Debug, -1, "Cloning ended...");
            return xmlCopy;
        }

        #endregion
    }
}