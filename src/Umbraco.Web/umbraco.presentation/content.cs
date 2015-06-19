using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models;
using Umbraco.Core.Profiling;
using umbraco.DataLayer;
using umbraco.presentation.nodeFactory;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Scheduling;
using Node = umbraco.NodeFactory.Node;
using File = System.IO.File;

namespace umbraco
{
    /// <summary>
    /// Represents the Xml storage for the Xml published cache.
    /// </summary>
    public class content
    {
        private XmlCacheFilePersister _persisterTask;

        #region Constructors

        private content()
        {
            if (SyncToXmlFile)
            {
                // if we write to file, prepare the lock
                // (if we don't use the file, or just read from it, no need to lock)
                InitializeFileLock();

                // and prepare the persister task
            var logger = LoggerResolver.HasCurrent ? LoggerResolver.Current.Logger : new DebugDiagnosticsLogger();
            var profingLogger = new ProfilingLogger(
                logger,
                ProfilerResolver.HasCurrent ? ProfilerResolver.Current.Profiler : new LogProfiler(logger));

                // there's always be one task keeping a ref to the runner
                // so it's safe to just create it as a local var here
                var runner = new BackgroundTaskRunner<XmlCacheFilePersister>("XmlCacheFilePersister", new BackgroundTaskRunnerOptions
                {
                    LongRunning = true,
                    KeepAlive = true
            }, logger);

                // when the runner is terminating we need to ensure that no modifications
                // to content are possible anymore, as they would not be written out to
                // the xml file - unfortunately that is not possible in 7.x because we
                // cannot lock the content service... and so we do nothing...
                //runner.Terminating += (sender, args) =>
                //{
                //};

                // when the runner has terminated we know we will not be writing to the file
                // anymore, so we can release the lock now - no need to wait for the AppDomain
                // unload - which means any "last minute" saves will be lost - but waiting for
                // the AppDomain to unload has issues...
                runner.Terminated += (sender, args) =>
                {
                    if (_fileLock == null) return; // not locking (testing?)
                    if (_fileLocked == null) return; // not locked

                    // thread-safety
                    // lock something that's readonly and not null..
                    lock (_xmlFileName)
                    {
                        // double-check
                        if (_fileLocked == null) return;

                        LogHelper.Debug<content>("Release file lock.");
                        _fileLocked.Dispose();
                        _fileLocked = null;
                        _fileLock = null; // ensure we don't lock again
                    }
                };

                // create (and add to runner)
            _persisterTask = new XmlCacheFilePersister(runner, this, profingLogger);
            }

            // initialize content - populate the cache
            using (var safeXml = GetSafeXmlWriter(false))
            {
                bool registerXmlChange;

                // if we don't use the file then LoadXmlLocked will not even
                // read from the file and will go straight to database
                LoadXmlLocked(safeXml, out registerXmlChange);
                // if we use the file and registerXmlChange is true this will
                // write to file, else it will not
                safeXml.Commit(registerXmlChange);
            }
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

        #region Legacy & Stuff

        // sync database access
        // (not refactoring that part at the moment)
        private static readonly object DbReadSyncLock = new object();

        private const string XmlContextContentItemKey = "UmbracoXmlContextContent";
        private string _umbracoXmlDiskCacheFileName = string.Empty;
        private volatile XmlDocument _xmlContent;

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

        //NOTE: We CANNOT use this for a double check lock because it is a property, not a field and to do double
        // check locking in c# you MUST have a volatile field. Even thoug this wraps a volatile field it will still 
        // not work as expected for a double check lock because properties are treated differently in the clr.
        public virtual bool isInitializing
        {
            get { return _xmlContent == null; }
        }

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        #endregion

        #region Public Methods

        [Obsolete("This is no longer used and will be removed in future versions, if you use this method it will not refresh 'async' it will perform the refresh on the current thread which is how it should be doing it")]
        public virtual void RefreshContentFromDatabaseAsync()
        {
            RefreshContentFromDatabase();
        }

        /// <summary>
        /// Load content from database and replaces active content when done.
        /// </summary>
        public virtual void RefreshContentFromDatabase()
        {
            var e = new RefreshContentEventArgs();
            FireBeforeRefreshContent(e);

            if (!e.Cancel)
            {
                using (var safeXml = GetSafeXmlWriter())
                {
                    safeXml.Xml = LoadContentFromDatabase();
                }
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

                // fix sortOrder - see note in UpdateSortOrder
                var node = GetPreviewOrPublishedNode(d, xmlContentCopy, false);
                var attr = ((XmlElement)node).GetAttributeNode("sortOrder");
                attr.Value = d.sortOrder.ToString();
                AddOrUpdateXmlNode(xmlContentCopy, d.Id, d.Level, parentId, node);

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
        /// <param name="parentId">The parent node identifier.</param>
        public void SortNodes(int parentId)
        {
            var childNodesXPath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema
                ? "./node"
                : "./* [@id]";

            using (var safeXml = GetSafeXmlWriter(false))
            {
                var parentNode = parentId == -1
                    ? safeXml.Xml.DocumentElement
                    : safeXml.Xml.GetElementById(parentId.ToString(CultureInfo.InvariantCulture));

                if (parentNode == null) return;

                var sorted = XmlHelper.SortNodesIfNeeded(
                    parentNode,
                    childNodesXPath,
                    x => x.AttributeValue<int>("sortOrder"));

                if (sorted == false) return;

                safeXml.Commit();
            }
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
                using (var safeXml = GetSafeXmlWriter())
                {
                    safeXml.Xml = PublishNodeDo(d, safeXml.Xml, true);
                }

                ClearContextCache();

                var cachedFieldKeyStart = string.Format("{0}{1}_", CacheKeys.ContentItemCacheKey, d.Id);
                ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(cachedFieldKeyStart);

                FireAfterUpdateDocumentCache(d, e);
            }
        }

        internal virtual void UpdateSortOrder(int contentId)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(contentId);
            if (content == null) return;
            UpdateSortOrder(content);
        }

        internal virtual void UpdateSortOrder(IContent c)
        {
            if (c == null) throw new ArgumentNullException("c");

            // the XML in database is updated only when content is published, and then
            // it contains the sortOrder value at the time the XML was generated. when
            // a document with unpublished changes is sorted, then it is simply saved
            // (see ContentService) and so the sortOrder has changed but the XML has
            // not been updated accordingly.

            // this updates the published cache to take care of the situation
            // without ContentService having to ... what exactly?

            // no need to do it if the content is published without unpublished changes,
            // though, because in that case the XML will get re-generated with the
            // correct sort order.
            if (c.Published)
                return;

            using (var safeXml = GetSafeXmlWriter(false))
            {
                var node = safeXml.Xml.GetElementById(c.Id.ToString(CultureInfo.InvariantCulture));
                if (node == null) return;
                var attr = node.GetAttributeNode("sortOrder");
                if (attr == null) return;
                var sortOrder = c.SortOrder.ToString(CultureInfo.InvariantCulture);
                if (attr.Value == sortOrder) return;

                // only if node was actually modified
                attr.Value = sortOrder;

                safeXml.Commit();
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


            using (var safeXml = GetSafeXmlWriter())
            {
                foreach (Document d in Documents)
                {
                    PublishNodeDo(d, safeXml.Xml, true);
                }
            }

            ClearContextCache();
        }

        [Obsolete("Method obsolete in version 4.1 and later, please use UpdateDocumentCache", true)]
        public virtual void UpdateDocumentCacheAsync(int documentId)
        {
            UpdateDocumentCache(documentId);
        }

        [Obsolete("Method obsolete in version 4.1 and later, please use ClearDocumentCache", true)]
        public virtual void ClearDocumentCacheAsync(int documentId)
        {
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

                // We need to lock content cache here, because we cannot allow other threads
                // making changes at the same time, they need to be queued
                using (var safeXml = GetSafeXmlReader())
                {
                    // Check if node present, before cloning
                    x = safeXml.Xml.GetElementById(doc.Id.ToString());
                    if (x == null)
                        return;

                    safeXml.UpgradeToWriter(false);

                    // Find the document in the xml cache
                    x = safeXml.Xml.GetElementById(doc.Id.ToString());
                    if (x != null)
                    {
                        // The document already exists in cache, so repopulate it
                        x.ParentNode.RemoveChild(x);
                        safeXml.Commit();
                    }
                }

                ClearContextCache();

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

        #endregion

        #region Protected & Private methods

        /// <summary>
        /// Clear HTTPContext cache if any
        /// </summary>
        private void ClearContextCache()
        {
            // If running in a context very important to reset context cache orelse new nodes are missing
            if (UmbracoContext.Current != null && UmbracoContext.Current.HttpContext != null && UmbracoContext.Current.HttpContext.Items.Contains(XmlContextContentItemKey))
                UmbracoContext.Current.HttpContext.Items.Remove(XmlContextContentItemKey);
        }

        /// <summary>
        /// Load content from database
        /// </summary>
        private XmlDocument LoadContentFromDatabase()
        {
            try
            {
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
                        InitializeXml(xmlDoc, dtd);

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

                                // fix sortOrder - see notes in UpdateSortOrder
                                var tmp = new XmlDocument();
                                tmp.LoadXml(xml);
                                var attr = tmp.DocumentElement.GetAttributeNode("sortOrder");
                                attr.Value = dr.GetInt("sortOrder").ToString();
                                xml = tmp.InnerXml;

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
                            InitializeXml(xmlDoc, dtd);

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

        [Obsolete("This method should not be used and does nothing, xml file persistence is done in a queue using a BackgroundTaskRunner")]
        public void PersistXmlToFile()
        {
        }

        /// <summary>
        /// Adds a task to the xml cache file persister
        /// </summary>
        //private void QueueXmlForPersistence()
        //{
        //    _persisterTask = _persisterTask.Touch();
        //}

        internal DateTime GetCacheFileUpdateTime()
        {
            //TODO: Should there be a try/catch here in case the file is being written to while this is trying to be executed?

            if (File.Exists(UmbracoXmlDiskCacheFileName))
            {
                return new FileInfo(UmbracoXmlDiskCacheFileName).LastWriteTimeUtc;
            }

            return DateTime.MinValue;
        }

        #endregion

        #region Configuration

        // gathering configuration options here to document what they mean

        private readonly bool _xmlFileEnabled = true;

        // whether the disk cache is enabled
        private bool XmlFileEnabled
        {
            get { return _xmlFileEnabled && UmbracoConfig.For.UmbracoSettings().Content.XmlCacheEnabled; }
        }

        // whether the disk cache is enabled and to update the disk cache when xml changes
        private bool SyncToXmlFile
        {
            get { return XmlFileEnabled && UmbracoConfig.For.UmbracoSettings().Content.ContinouslyUpdateXmlDiskCache; }
        }

        // whether the disk cache is enabled and to reload from disk cache if it changes
        private bool SyncFromXmlFile
        {
            get { return XmlFileEnabled && UmbracoConfig.For.UmbracoSettings().Content.XmlContentCheckForDiskChanges; }
        }

        // whether _xml is immutable or not (achieved by cloning before changing anything)
        private static bool XmlIsImmutable
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.CloneXmlContent; }
        }

        // whether to use the legacy schema
        private static bool UseLegacySchema
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema; }
        }

        // whether to keep version of everything (incl. medias & members) in cmsPreviewXml
        // for audit purposes - false by default, not in umbracoSettings.config
        // whether to... no idea what that one does
        // it is false by default and not in UmbracoSettings.config anymore - ignoring
        /*
        private static bool GlobalPreviewStorageEnabled
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled; }
        }
        */

        // ensures config is valid
        private void EnsureConfigurationIsValid()
        {
            if (SyncToXmlFile && SyncFromXmlFile)
                throw new Exception("Cannot run with both ContinouslyUpdateXmlDiskCache and XmlContentCheckForDiskChanges being true.");

            if (XmlIsImmutable == false)
                //LogHelper.Warn<XmlStore>("Running with CloneXmlContent being false is a bad idea.");
                LogHelper.Warn<content>("CloneXmlContent is false - ignored, we always clone.");

            // note: if SyncFromXmlFile then we should also disable / warn that local edits are going to cause issues...
        }

        #endregion

        #region Xml

        private readonly AsyncLock _xmlLock = new AsyncLock(); // protects _xml

        /// <remarks>
        /// Get content. First call to this property will initialize xmldoc
        /// subsequent calls will be blocked until initialization is done
        /// Further we cache (in context) xmlContent for each request to ensure that
        /// we always have the same XmlDoc throughout the whole request.
        /// </remarks>
        public virtual XmlDocument XmlContent
        {
            get
            {
                if (UmbracoContext.Current == null || UmbracoContext.Current.HttpContext == null)
                    return XmlContentInternal;
                var content = UmbracoContext.Current.HttpContext.Items[XmlContextContentItemKey] as XmlDocument;
                if (content == null)
                {
                    content = XmlContentInternal;
                    UmbracoContext.Current.HttpContext.Items[XmlContextContentItemKey] = content;
                }
                return content;
            }
        }

        [Obsolete("Please use: content.Instance.XmlContent")]
        public static XmlDocument xmlContent
        {
            get { return Instance.XmlContent; }
        }

        // to be used by content.Instance
        protected internal virtual XmlDocument XmlContentInternal
        {
            get
            {
                ReloadXmlFromFileIfChanged();
                return _xmlContent;
            }
        }

        // assumes xml lock
        private void SetXmlLocked(XmlDocument xml, bool registerXmlChange)
        {
            // this is the ONLY place where we write to _xmlContent
            _xmlContent = xml;

            if (registerXmlChange == false || SyncToXmlFile == false)
                return;

            //_lastXmlChange = DateTime.UtcNow;
            _persisterTask = _persisterTask.Touch(); // _persisterTask != null because SyncToXmlFile == true
        }

        private static XmlDocument Clone(XmlDocument xmlDoc)
        {
            return xmlDoc == null ? null : (XmlDocument)xmlDoc.CloneNode(true);
        }

        private static void EnsureSchema(string contentTypeAlias, XmlDocument xml)
        {
            string subset = null;

            // get current doctype
            var n = xml.FirstChild;
            while (n.NodeType != XmlNodeType.DocumentType && n.NextSibling != null)
                n = n.NextSibling;
            if (n.NodeType == XmlNodeType.DocumentType)
                subset = ((XmlDocumentType)n).InternalSubset;

            // ensure it contains the content type
            if (subset != null && subset.Contains(string.Format("<!ATTLIST {0} id ID #REQUIRED>", contentTypeAlias)))
                return;

            // remove current doctype
            xml.RemoveChild(n);

            // set new doctype
            subset = string.Format("<!ELEMENT {1} ANY>{0}<!ATTLIST {1} id ID #REQUIRED>{0}{2}", Environment.NewLine, contentTypeAlias, subset);
            var doctype = xml.CreateDocumentType("root", null, null, subset);
            xml.InsertAfter(doctype, xml.FirstChild);
        }

        private static void InitializeXml(XmlDocument xml, string dtd)
        {
            // prime the xml document with an inline dtd and a root element
            xml.LoadXml(String.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?>{0}{1}{0}<root id=\"-1\"/>",
                Environment.NewLine, dtd));
        }

        // try to load from file, otherwise database
        // assumes xml lock (file is always locked)
        private void LoadXmlLocked(SafeXmlReaderWriter safeXml, out bool registerXmlChange)
        {
            LogHelper.Debug<content>("Loading Xml...");

            // try to get it from the file
            if (XmlFileEnabled && (safeXml.Xml = LoadXmlFromFile()) != null)
            {
                registerXmlChange = false; // loaded from disk, do NOT write back to disk!
                return;
            }

            // get it from the database, and register
            safeXml.Xml = LoadContentFromDatabase();
            registerXmlChange = true;
        }

        // NOTE
        // - this is NOT a reader/writer lock and each lock is exclusive
        // - these locks are NOT reentrant / recursive

        // gets a locked safe read access to the main xml
        private SafeXmlReaderWriter GetSafeXmlReader()
        {
            var releaser = _xmlLock.Lock();
            return SafeXmlReaderWriter.GetReader(this, releaser);
        }

        // gets a locked safe read accses to the main xml
        private async Task<SafeXmlReaderWriter> GetSafeXmlReaderAsync()
        {
            var releaser = await _xmlLock.LockAsync();
            return SafeXmlReaderWriter.GetReader(this, releaser);
        }

        // gets a locked safe write access to the main xml (cloned)
        private SafeXmlReaderWriter GetSafeXmlWriter(bool auto = true)
        {
            var releaser = _xmlLock.Lock();
            return SafeXmlReaderWriter.GetWriter(this, releaser, auto);
        }

        private class SafeXmlReaderWriter : IDisposable
        {
            private readonly content _instance;
            private IDisposable _releaser;
            private bool _isWriter;
            private bool _auto;
            private bool _committed;
            private XmlDocument _xml;

            private SafeXmlReaderWriter(content instance, IDisposable releaser, bool isWriter, bool auto)
            {
                _instance = instance;
                _releaser = releaser;
                _isWriter = isWriter;
                _auto = auto;

                // cloning for writer is not an option anymore (see XmlIsImmutable)
                _xml = _isWriter ? Clone(instance._xmlContent) : instance._xmlContent;
            }

            public static SafeXmlReaderWriter GetReader(content instance, IDisposable releaser)
            {
                return new SafeXmlReaderWriter(instance, releaser, false, false);
            }

            public static SafeXmlReaderWriter GetWriter(content instance, IDisposable releaser, bool auto)
            {
                return new SafeXmlReaderWriter(instance, releaser, true, auto);
            }

            public void UpgradeToWriter(bool auto)
            {
                if (_isWriter)
                    throw new InvalidOperationException("Already writing.");
                _isWriter = true;
                _auto = auto;
                _xml = Clone(_xml); // cloning for writer is not an option anymore (see XmlIsImmutable)
            }

            public XmlDocument Xml
            {
                get
                {
                    return _xml;
                }
                set
                {
                    if (_isWriter == false)
                        throw new InvalidOperationException("Not writing.");
                    _xml = value;
                }
            }

            // registerXmlChange indicates whether to do what should be done when Xml changes,
            // that is, to request that the file be written to disk - something we don't want
            // to do if we're committing Xml precisely after we've read from disk!
            public void Commit(bool registerXmlChange = true)
            {
                if (_isWriter == false)
                    throw new InvalidOperationException("Not writing.");
                _instance.SetXmlLocked(Xml, registerXmlChange);
                _committed = true;
            }

            public void Dispose()
            {
                if (_releaser == null)
                    return;
                if (_isWriter && _auto && _committed == false)
                    Commit();
                _releaser.Dispose();
                _releaser = null;
            }
        }

        private static string ChildNodesXPath
        {
            get
            {
                return UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema
                    ? "./node"
                    : "./* [@id]";
            }
        }

        private static string DataNodesXPath
        {
            get
            {
                return UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema
                    ? "./data"
                    : "./* [not(@id)]";
            }
        }

        #endregion

        #region File

        private readonly string _xmlFileName = IOHelper.MapPath(SystemFiles.ContentCacheXml);
        private DateTime _lastFileRead; // last time the file was read
        private DateTime _nextFileCheck; // last time we checked whether the file was changed
        private AsyncLock _fileLock; // protects the file
        private IDisposable _fileLocked; // protects the file

        private const int FileLockTimeoutMilliseconds = 4 * 60 * 1000; // 4'

        private void InitializeFileLock()
        {
            // initialize file lock
            // ApplicationId will look like "/LM/W3SVC/1/Root/AppName"
            // name is system-wide and must be less than 260 chars
            //
            // From MSDN C++ CreateSemaphore doc:
            // "The name can have a "Global\" or "Local\" prefix to explicitly create the object in
            // the global or session namespace. The remainder of the name can contain any character 
            // except the backslash character (\). For more information, see Kernel Object Namespaces."
            //
            // From MSDN "Kernel object namespaces" doc:
            // "The separate client session namespaces enable multiple clients to run the same 
            // applications without interfering with each other. For processes started under 
            // a client session, the system uses the session namespace by default. However, these 
            // processes can use the global namespace by prepending the "Global\" prefix to the object name."
            //
            // just use "default" (whatever it is) for now - ie, no prefix
            //
            var name = HostingEnvironment.ApplicationID + "/XmlStore/XmlFile";
            _fileLock = new AsyncLock(name);

            // the file lock works with a shared, system-wide, semaphore - and we don't want
            // to leak a count on that semaphore else the whole process will hang - so we have
            // to ensure we dispose of the locker when the domain goes down - in theory the
            // async lock should do it via its finalizer, but then there are some weird cases
            // where the semaphore has been disposed of before it's been released, and then
            // we'd need to GC-pin the semaphore... better dispose the locker explicitely
            // when the app domain unloads.

            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                LogHelper.Debug<content>("Registering Unload handler for default app domain.");
                AppDomain.CurrentDomain.ProcessExit += OnDomainUnloadReleaseFileLock;
            }
            else
            {
                LogHelper.Debug<content>("Registering Unload handler for non-default app domain.");
                AppDomain.CurrentDomain.DomainUnload += OnDomainUnloadReleaseFileLock;
            }
        }

        private void EnsureFileLock()
        {
            if (_fileLock == null) return; // not locking (testing?)
            if (_fileLocked != null) return; // locked already

            // thread-safety, acquire lock only once!
            // lock something that's readonly and not null..
            lock (_xmlFileName)
            {
                // double-check
                if (_fileLock == null) return;
                if (_fileLocked != null) return;

                // don't hang forever, throws if it cannot lock within the timeout
                LogHelper.Debug<content>("Acquiring exclusive access to file for this AppDomain...");
                _fileLocked = _fileLock.Lock(FileLockTimeoutMilliseconds);
                LogHelper.Debug<content>("Acquired exclusive access to file for this AppDomain.");
            }
        }

        private void OnDomainUnloadReleaseFileLock(object sender, EventArgs args)
        {
            // the unload event triggers AFTER all hosted objects (eg the file persister
            // background task runner) have been stopped, so we should have released the
            // lock already - this is for safety - might be possible to get rid of it

            // NOTE
            // trying to write to the log via LogHelper at that point is a BAD idea
            // it can lead to ugly deadlocks with the named semaphore - DONT do it

            if (_fileLock == null) return; // not locking (testing?)
            if (_fileLocked == null) return; // not locked

            // thread-safety
            // lock something that's readonly and not null..
            lock (_xmlFileName)
            {
                // double-check
                if (_fileLocked == null) return;

                // in case you really need to debug... that should be safe...
                //System.IO.File.AppendAllText(HostingEnvironment.MapPath("~/App_Data/log.txt"), string.Format("{0} {1} unlock", DateTime.Now, AppDomain.CurrentDomain.Id));
                _fileLocked.Dispose();

                _fileLock = null; // ensure we don't lock again
            }
        }

        // not used - just try to read the file
        //private bool XmlFileExists
        //{
        //    get
        //    {
        //        // check that the file exists and has content (is not empty)
        //        var fileInfo = new FileInfo(_xmlFileName);
        //        return fileInfo.Exists && fileInfo.Length > 0;
        //    }
        //}

        private DateTime XmlFileLastWriteTime
        {
            get
            {
                var fileInfo = new FileInfo(_xmlFileName);
                return fileInfo.Exists ? fileInfo.LastWriteTimeUtc : DateTime.MinValue;
            }
        }

        // assumes file lock
        internal void SaveXmlToFile()
        {
            LogHelper.Info<content>("Save Xml to file...");

            try
            {
                var xml = _xmlContent; // capture (atomic + volatile), immutable anyway
                if (xml == null) return;

                EnsureFileLock();

                // delete existing file, if any
                DeleteXmlFile();

                // ensure cache directory exists
                var directoryName = Path.GetDirectoryName(_xmlFileName);
                if (directoryName == null)
                    throw new Exception(string.Format("Invalid XmlFileName \"{0}\".", _xmlFileName));
                if (System.IO.File.Exists(_xmlFileName) == false && Directory.Exists(directoryName) == false)
                    Directory.CreateDirectory(directoryName);

                // save
                using (var fs = new FileStream(_xmlFileName, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    var bytes = Encoding.UTF8.GetBytes(SaveXmlToString(xml));
                    fs.Write(bytes, 0, bytes.Length);
                }

                LogHelper.Info<content>("Saved Xml to file.");
            }
            catch (Exception e)
            {
                // if something goes wrong remove the file
                DeleteXmlFile();

                LogHelper.Error<content>("Failed to save Xml to file.", e);
            }
        }

        // assumes file lock
        internal async System.Threading.Tasks.Task SaveXmlToFileAsync()
        {
            LogHelper.Info<content>("Save Xml to file...");

            try
            {
                var xml = _xmlContent; // capture (atomic + volatile), immutable anyway
                if (xml == null) return;

                EnsureFileLock();

                // delete existing file, if any
                DeleteXmlFile();

                // ensure cache directory exists
                var directoryName = Path.GetDirectoryName(_xmlFileName);
                if (directoryName == null)
                    throw new Exception(string.Format("Invalid XmlFileName \"{0}\".", _xmlFileName));
                if (System.IO.File.Exists(_xmlFileName) == false && Directory.Exists(directoryName) == false)
                    Directory.CreateDirectory(directoryName);

                // save
                using (var fs = new FileStream(_xmlFileName, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    var bytes = Encoding.UTF8.GetBytes(SaveXmlToString(xml));
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                }

                LogHelper.Info<content>("Saved Xml to file.");
            }
            catch (Exception e)
            {
                // if something goes wrong remove the file
                DeleteXmlFile();

                LogHelper.Error<content>("Failed to save Xml to file.", e);
            }
        }

        private string SaveXmlToString(XmlDocument xml)
        {
            // using that one method because we want to have proper indent
            // and in addition, writing async is never fully async because
            // althouth the writer is async, xml.WriteTo() will not async

            // that one almost works but... "The elements are indented as long as the element 
            // does not contain mixed content. Once the WriteString or WriteWhitespace method
            // is called to write out a mixed element content, the XmlWriter stops indenting. 
            // The indenting resumes once the mixed content element is closed." - says MSDN
            // about XmlWriterSettings.Indent

            // so ImportContent must also make sure of ignoring whitespaces!

            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                //OmitXmlDeclaration = true
            }))
            {
                //xmlWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
                xml.WriteTo(xmlWriter); // already contains the xml declaration
            }
            return sb.ToString();
        }

        // assumes file lock
        private XmlDocument LoadXmlFromFile()
        {
            LogHelper.Info<content>("Load Xml from file...");

            try
            {
                // if we're not writing back to the file, no need to lock
                if (SyncToXmlFile)
                    EnsureFileLock();

                var xml = new XmlDocument();
                using (var fs = new FileStream(_xmlFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    xml.Load(fs);
                }
                _lastFileRead = DateTime.UtcNow;
                LogHelper.Info<content>("Loaded Xml from file.");
                return xml;
            }
            catch (FileNotFoundException)
            {
                LogHelper.Warn<content>("Failed to load Xml, file does not exist.");
                return null;
            }
            catch (Exception e)
            {
                LogHelper.Error<content>("Failed to load Xml from file.", e);
                DeleteXmlFile();
                return null;
            }
        }

        // (files is always locked)
        private void DeleteXmlFile()
        {
            if (System.IO.File.Exists(_xmlFileName) == false) return;
            System.IO.File.SetAttributes(_xmlFileName, FileAttributes.Normal);
            System.IO.File.Delete(_xmlFileName);
        }

        private void ReloadXmlFromFileIfChanged()
        {
            if (SyncFromXmlFile == false) return;

            var now = DateTime.UtcNow;
            if (now < _nextFileCheck) return;

            // time to check
            _nextFileCheck = now.AddSeconds(1); // check every 1s
            if (XmlFileLastWriteTime <= _lastFileRead) return;

            LogHelper.Debug<content>("Xml file change detected, reloading.");

            // time to read

            using (var safeXml = GetSafeXmlWriter(false))
            {
                bool registerXmlChange;
                LoadXmlLocked(safeXml, out registerXmlChange); // updates _lastFileRead
                safeXml.Commit(registerXmlChange);
            }
        }

        #endregion

        #region Manage change

        // adds or updates a node (docNode) into a cache (xml)
        public static void AddOrUpdateXmlNode(XmlDocument xml, int id, int level, int parentId, XmlNode docNode)
        {
            // sanity checks
            if (id != docNode.AttributeValue<int>("id"))
                throw new ArgumentException("Values of id and docNode/@id are different.");
            if (parentId != docNode.AttributeValue<int>("parentID"))
                throw new ArgumentException("Values of parentId and docNode/@parentID are different.");

            // find the document in the cache
            XmlNode currentNode = xml.GetElementById(id.ToInvariantString());

            // if the document is not there already then it's a new document
            // we must make sure that its document type exists in the schema
            if (currentNode == null && UseLegacySchema == false)
                EnsureSchema(docNode.Name, xml);

            // find the parent
            XmlNode parentNode = level == 1
                ? xml.DocumentElement
                : xml.GetElementById(parentId.ToInvariantString());

            // no parent = cannot do anything
            if (parentNode == null)
                return;

            // insert/move the node under the parent
            if (currentNode == null)
            {
                // document not there, new node, append
                currentNode = docNode;
                parentNode.AppendChild(currentNode);
            }
            else
            {
                // document found... we could just copy the currentNode children nodes over under
                // docNode, then remove currentNode and insert docNode... the code below tries to
                // be clever and faster, though only benchmarking could tell whether it's worth the
                // pain...

                // first copy current parent ID - so we can compare with target parent
                var moving = currentNode.AttributeValue<int>("parentID") != parentId;

                if (docNode.Name == currentNode.Name)
                {
                    // name has not changed, safe to just update the current node
                    // by transfering values eg copying the attributes, and importing the data elements
                    TransferValuesFromDocumentXmlToPublishedXml(docNode, currentNode);

                    // if moving, move the node to the new parent
                    // else it's already under the right parent
                    // (but maybe the sort order has been updated)
                    if (moving)
                        parentNode.AppendChild(currentNode); // remove then append to parentNode
                }
                else
                {
                    // name has changed, must use docNode (with new name)
                    // move children nodes from currentNode to docNode (already has properties)
                    var children = currentNode.SelectNodes(ChildNodesXPath);
                    if (children == null) throw new Exception("oops");
                    foreach (XmlNode child in children)
                        docNode.AppendChild(child); // remove then append to docNode

                    // and put docNode in the right place - if parent has not changed, then
                    // just replace, else remove currentNode and insert docNode under the right parent
                    // (but maybe not at the right position due to sort order)
                    if (moving)
                    {
                        if (currentNode.ParentNode == null) throw new Exception("oops");
                        currentNode.ParentNode.RemoveChild(currentNode);
                        parentNode.AppendChild(docNode);
                    }
                    else
                    {
                        // replacing might screw the sort order
                        parentNode.ReplaceChild(docNode, currentNode);
                    }

                    currentNode = docNode;
                }
            }

            // if the nodes are not ordered, must sort
            // (see U4-509 + has to work with ReplaceChild too)
            //XmlHelper.SortNodesIfNeeded(parentNode, childNodesXPath, x => x.AttributeValue<int>("sortOrder"));

            // but...
            // if we assume that nodes are always correctly sorted
            // then we just need to ensure that currentNode is at the right position.
            // should be faster that moving all the nodes around.
            XmlHelper.SortNode(parentNode, ChildNodesXPath, currentNode, x => x.AttributeValue<int>("sortOrder"));
        }

        private static void TransferValuesFromDocumentXmlToPublishedXml(XmlNode documentNode, XmlNode publishedNode)
        {
            // remove all attributes from the published node
            if (publishedNode.Attributes == null) throw new Exception("oops");
            publishedNode.Attributes.RemoveAll();

            // remove all data nodes from the published node
            var dataNodes = publishedNode.SelectNodes(DataNodesXPath);
            if (dataNodes == null) throw new Exception("oops");
            foreach (XmlNode n in dataNodes)
                publishedNode.RemoveChild(n);

            // append all attributes from the document node to the published node
            if (documentNode.Attributes == null) throw new Exception("oops");
            foreach (XmlAttribute att in documentNode.Attributes)
                ((XmlElement)publishedNode).SetAttribute(att.Name, att.Value);

            // find the first child node, if any
            var childNodes = publishedNode.SelectNodes(ChildNodesXPath);
            if (childNodes == null) throw new Exception("oops");
            var firstChildNode = childNodes.Count == 0 ? null : childNodes[0];

            // append all data nodes from the document node to the published node
            dataNodes = documentNode.SelectNodes(DataNodesXPath);
            if (dataNodes == null) throw new Exception("oops");
            foreach (XmlNode n in dataNodes)
            {
                if (publishedNode.OwnerDocument == null) throw new Exception("oops");
                var imported = publishedNode.OwnerDocument.ImportNode(n, true);
                if (firstChildNode == null)
                    publishedNode.AppendChild(imported);
                else
                    publishedNode.InsertBefore(imported, firstChildNode);
            }
        }

        #endregion

        #region Events

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
    }
}