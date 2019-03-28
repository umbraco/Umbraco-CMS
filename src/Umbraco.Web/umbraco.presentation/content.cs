using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.presentation.nodeFactory;
using umbraco.presentation.preview;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Profiling;
using Umbraco.Core.Scoping;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Scheduling;
using File = System.IO.File;
using Node = umbraco.NodeFactory.Node;

namespace umbraco
{
    /// <summary>
    /// Represents the Xml storage for the Xml published cache.
    /// </summary>
    public class content
    {
        private readonly IScopeProviderInternal _scopeProvider = (IScopeProviderInternal) ApplicationContext.Current.ScopeProvider;
        private XmlCacheFilePersister _persisterTask;

        private volatile bool _released;

        #region Constructors

        private content()
        {
            if (SyncToXmlFile)
            {
                var logger = LoggerResolver.HasCurrent ? LoggerResolver.Current.Logger : new DebugDiagnosticsLogger();
                var profingLogger = new ProfilingLogger(
                    logger,
                    ProfilerResolver.HasCurrent ? ProfilerResolver.Current.Profiler : new LogProfiler(logger));

                // prepare the persister task
                // there's always be one task keeping a ref to the runner
                // so it's safe to just create it as a local var here
                var runner = new BackgroundTaskRunner<XmlCacheFilePersister>("XmlCacheFilePersister", new BackgroundTaskRunnerOptions
                {
                    LongRunning = true,
                    KeepAlive = true,
                    Hosted = false // main domain will take care of stopping the runner (see below)
                }, logger);

                // create (and add to runner)
                _persisterTask = new XmlCacheFilePersister(runner, this, profingLogger);

                var registered = ApplicationContext.Current.MainDom.Register(
                    null,
                    () =>
                    {
                        // once released, the cache still works but does not write to file anymore,
                        // which is OK with database server messenger but will cause data loss with
                        // another messenger...

                        runner.Shutdown(false, true); // wait until flushed
                        _released = true;
                    });

                // failed to become the main domain, we will never use the file
                if (registered == false)
                    runner.Shutdown(false, true);

                _released = (registered == false);
            }

            // initialize content - populate the cache
            using (var safeXml = GetSafeXmlWriter())
            {
                bool registerXmlChange;

                // if we don't use the file then LoadXmlLocked will not even
                // read from the file and will go straight to database
                LoadXmlLocked(safeXml, out registerXmlChange);
                // if we use the file and registerXmlChange is true this will
                // write to file, else it will not
                safeXml.AcceptChanges(registerXmlChange);
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

        internal const string XmlContextContentItemKey = "UmbracoXmlContextContent";
        private static string _umbracoXmlDiskCacheFileName = string.Empty;
        // internal for SafeXmlReaderWriter
        internal volatile XmlDocument _xmlContent;

        /// <summary>
        /// Gets the path of the umbraco XML disk cache file.
        /// </summary>
        /// <value>The name of the umbraco XML disk cache file.</value>
        public static string GetUmbracoXmlDiskFileName()
        {
            if (string.IsNullOrEmpty(_umbracoXmlDiskCacheFileName))
            {
                _umbracoXmlDiskCacheFileName = IOHelper.MapPath(SystemFiles.ContentCacheXml);
            }
            return _umbracoXmlDiskCacheFileName;
        }

        [Obsolete("Use the safer static GetUmbracoXmlDiskFileName() method instead to retrieve this value")]
        public string UmbracoXmlDiskCacheFileName
        {
            get { return GetUmbracoXmlDiskFileName(); }
            set { _umbracoXmlDiskCacheFileName = value; }
        }

        //NOTE: We CANNOT use this for a double check lock because it is a property, not a field and to do double
        // check locking in c# you MUST have a volatile field. Even thoug this wraps a volatile field it will still
        // not work as expected for a double check lock because properties are treated differently in the clr.
        public virtual bool isInitializing
        {
            get
            {
                // ok to access _xmlContent here
                return _xmlContent == null;
            }
        }

        /// <summary>
        /// Unused, please do not use
        /// </summary>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
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

            if (e.Cancel) return;

            using (var safeXml = GetSafeXmlWriter())
            {
                safeXml.Xml = LoadContentFromDatabase();
                safeXml.AcceptChanges();
            }
        }

        internal static bool TestingUpdateSitemapProvider = true;

        /// <summary>
        /// Used by all overloaded publish methods to do the actual "noderepresentation to xml"
        /// </summary>
        /// <param name="d"></param>
        /// <param name="xmlContentCopy"></param>
        /// <param name="updateSitemapProvider"></param>
        public static XmlDocument PublishNodeDo(Document d, XmlDocument xmlContentCopy, bool updateSitemapProvider)
        {
            updateSitemapProvider &= TestingUpdateSitemapProvider;

            // check if document *is* published, it could be unpublished by an event
            if (d.Published)
            {
                var parentId = d.Level == 1 ? -1 : d.ParentId;

                // fix sortOrder - see note in UpdateSortOrder
                var node = GetPreviewOrPublishedNode(d, xmlContentCopy, false);
                var attr = ((XmlElement)node).GetAttributeNode("sortOrder");
                attr.Value = d.sortOrder.ToString();
                xmlContentCopy = GetAddOrUpdateXmlNode(xmlContentCopy, d.Id, d.Level, parentId, node);

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
            using (var safeXml = GetSafeXmlWriter())
            {
                var parentNode = parentId == -1
                    ? safeXml.Xml.DocumentElement
                    : safeXml.Xml.GetElementById(parentId.ToString(CultureInfo.InvariantCulture));

                if (parentNode == null) return;

                var sorted = XmlHelper.SortNodesIfNeeded(
                    parentNode,
                    ChildNodesXPath,
                    x => x.AttributeValue<int>("sortOrder"));

                if (sorted == false) return;

                safeXml.AcceptChanges();
            }

            ClearPreviewXmlContent();
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

            if (e.Cancel) return;

            // lock the xml cache so no other thread can write to it at the same time
            // note that some threads could read from it while we hold the lock, though
            using (var safeXml = GetSafeXmlWriter())
            {
                safeXml.Xml = PublishNodeDo(d, safeXml.Xml, true);
                safeXml.AcceptChanges();
            }

            var cachedFieldKeyStart = string.Format("{0}{1}_", CacheKeys.ContentItemCacheKey, d.Id);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(cachedFieldKeyStart);

            FireAfterUpdateDocumentCache(d, e);
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

            // no need to do it if
            // - the content is published without unpublished changes (XML will be re-gen anyways)
            // - the content has no published version (not in XML)
            // - the sort order has not changed
            // note that
            // - if it is a new entity is has not published version
            // - if Published is dirty and false it's getting unpublished and has no published version
            //
            if (c.Published) return;
            if (c.HasPublishedVersion == false) return;
            if (c.WasPropertyDirty("SortOrder") == false) return;

            using (var safeXml = GetSafeXmlWriter())
            {
                //TODO: This can be null: safeXml.Xml!!!!


                var node = safeXml.Xml.GetElementById(c.Id.ToString(CultureInfo.InvariantCulture));
                if (node == null) return;
                var attr = node.GetAttributeNode("sortOrder");
                if (attr == null) return;
                var sortOrder = c.SortOrder.ToString(CultureInfo.InvariantCulture);
                if (attr.Value == sortOrder) return;

                // only if node was actually modified
                attr.Value = sortOrder;

                safeXml.AcceptChanges();
            }
        }

        /// <summary>
        /// Updates the document cache for multiple documents
        /// </summary>
        /// <param name="Documents">The documents.</param>
        [Obsolete("This is not used and will be removed from the codebase in future versions")]
        public virtual void UpdateDocumentCache(List<Document> Documents)
        {
            using (var safeXml = GetSafeXmlWriter())
            {
                foreach (var d in Documents)
                {
                    safeXml.Xml = PublishNodeDo(d, safeXml.Xml, true);
                }
                safeXml.AcceptChanges();
            }
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
            ClearDocumentCache(documentId, true);
        }

        internal virtual void ClearDocumentCache(int documentId, bool removeDbXmlEntry)
        {
            // Get the document
            Document d;
            try
            {
                d = new Document(documentId);
            }
            catch
            {
                // if we need the document to remove it... this cannot be LB?!
                // shortcut everything here
                ClearDocumentXmlCache(documentId);
                return;
            }
            ClearDocumentCache(d, removeDbXmlEntry);
        }

        /// <summary>
        /// Clears the document cache and removes the document from the xml db cache.
        /// This means the node gets unpublished from the website.
        /// </summary>
        /// <param name="doc">The document</param>
        /// <param name="removeDbXmlEntry"></param>
        internal void ClearDocumentCache(Document doc, bool removeDbXmlEntry)
        {
            var e = new DocumentCacheEventArgs();
            FireBeforeClearDocumentCache(doc, e);

            if (!e.Cancel)
            {
                //Hack: this is here purely for backwards compat if someone for some reason is using the
                // ClearDocumentCache(int documentId) method and expecting it to remove the xml
                if (removeDbXmlEntry)
                {
                    // remove from xml db cache
                    doc.XmlRemoveFromDB();
                }

                // clear xml cache
                ClearDocumentXmlCache(doc.Id);

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

        internal void ClearDocumentXmlCache(int id)
        {
            // We need to lock content cache here, because we cannot allow other threads
            // making changes at the same time, they need to be queued
            using (var safeXml = GetSafeXmlReader())
            {
                // Check if node present, before cloning
                var x = safeXml.Xml.GetElementById(id.ToString());
                if (x == null)
                    return;

                if (safeXml.IsWriter == false)
                    safeXml.UpgradeToWriter();

                // Find the document in the xml cache
                x = safeXml.Xml.GetElementById(id.ToString());
                if (x != null)
                {
                    // The document already exists in cache, so repopulate it
                    x.ParentNode.RemoveChild(x);
                    safeXml.AcceptChanges();
                }
            }

            ClearPreviewXmlContent(id);
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

        // this is for tests exclusively until we have a proper accessor in v8
        internal static Func<IDictionary> HttpContextItemsGetter { get; set; }

        private static IDictionary HttpContextItems
        {
            get
            {
                return HttpContextItemsGetter == null
                    ? (HttpContext.Current == null ? null : HttpContext.Current.Items)
                    : HttpContextItemsGetter();
            }
        }

        // clear the current xml capture in http context
        // used when applying changes from SafeXmlReaderWriter,
        // to force a new capture - so that changes become
        // visible for the current request
        private void ClearContextCache()
        {
            var items = HttpContextItems;
            if (items == null || items.Contains(XmlContextContentItemKey) == false) return;
            items.Remove(XmlContextContentItemKey);
        }

        // replaces the current xml capture in http context
        // used for temp changes from SafeXmlReaderWriter
        // so the current request immediately sees changes
        private void SetContextCache(XmlDocument xml)
        {
            var items = HttpContextItems;
            if (items == null) return;
            items[XmlContextContentItemKey] = xml;
        }

        /// <summary>
        /// Load content from database
        /// </summary>
        private XmlDocument LoadContentFromDatabase()
        {
            try
            {
                LogHelper.Info<content>("Loading content from database...");

                lock (DbReadSyncLock)
                {
                    var xmlDoc = ApplicationContext.Current.Services.ContentService.BuildXmlCache();
                    LogHelper.Debug<content>("Done republishing Xml Index");
                    return xmlDoc;
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

        [Obsolete("This method should not be used and does nothing, xml file persistence is done in a queue using a BackgroundTaskRunner")]
        public void PersistXmlToFile()
        {
        }

        internal DateTime GetCacheFileUpdateTime()
        {
            //TODO: Should there be a try/catch here in case the file is being written to while this is trying to be executed?

            if (File.Exists(GetUmbracoXmlDiskFileName()))
            {
                return new FileInfo(GetUmbracoXmlDiskFileName()).LastWriteTimeUtc;
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

        // whether to use the legacy schema
        private static bool UseLegacySchema
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema; }
        }

        #endregion

        #region Xml

        // internal for SafeXmlReaderWriter
        internal readonly AsyncLock _xmlLock = new AsyncLock(); // protects _xml

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
                // if there's a current enlisted reader/writer, use its xml
                var safeXml = SafeXmlReaderWriter.Get(_scopeProvider);
                if (safeXml != null) return safeXml.Xml;

                var items = HttpContextItems;
                if (items == null)
                    return XmlContentInternal;

                // capture or return the current xml in http context
                // so that it remains stable over the entire request
                var content = (XmlDocument) items[XmlContextContentItemKey];
                if (content == null)
                {
                    content = XmlContentInternal;
                    items[XmlContextContentItemKey] = content;
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
        // ok to access _xmlContent here - just capturing
        protected internal virtual XmlDocument XmlContentInternal
        {
            get
            {
                ReloadXmlFromFileIfChanged();
                return _xmlContent;
            }
        }

        // assumes xml lock
        // ok to access _xmlContent here since this is called from the safe reader/writer
        // internal for SafeXmlReaderWriter
        internal void SetXmlLocked(XmlDocument xml, bool registerXmlChange)
        {
            // this is the ONLY place where we write to _xmlContent
            _xmlContent = xml;

            if (registerXmlChange == false || SyncToXmlFile == false)
                return;

            //_lastXmlChange = DateTime.UtcNow;
            _persisterTask = _persisterTask.Touch(); // _persisterTask != null because SyncToXmlFile == true
        }

        private static bool HasSchema(string contentTypeAlias, XmlDocument xml)
        {
            string subset = null;

            // get current doctype
            var n = xml.FirstChild;
            while (n.NodeType != XmlNodeType.DocumentType && n.NextSibling != null)
                n = n.NextSibling;
            if (n.NodeType == XmlNodeType.DocumentType)
                subset = ((XmlDocumentType)n).InternalSubset;

            // ensure it contains the content type
            return subset != null && subset.Contains(string.Format("<!ATTLIST {0} id ID #REQUIRED>", contentTypeAlias));
        }

        private static XmlDocument EnsureSchema(string contentTypeAlias, XmlDocument xml)
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
                return xml;

            // alas, that does not work, replacing a doctype is ignored and GetElementById fails
            //
            //// remove current doctype, set new doctype
            //xml.RemoveChild(n);
            //subset = string.Format("<!ELEMENT {1} ANY>{0}<!ATTLIST {1} id ID #REQUIRED>{0}{2}", Environment.NewLine, contentTypeAlias, subset);
            //var doctype = xml.CreateDocumentType("root", null, null, subset);
            //xml.InsertAfter(doctype, xml.FirstChild);

            var xml2 = new XmlDocument();
            subset = string.Format("<!ELEMENT {1} ANY>{0}<!ATTLIST {1} id ID #REQUIRED>{0}{2}", Environment.NewLine, contentTypeAlias, subset);
            var doctype = xml2.CreateDocumentType("root", null, null, subset);
            xml2.AppendChild(doctype);
            xml2.AppendChild(xml2.ImportNode(xml.DocumentElement, true));
            return xml2;
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
            return SafeXmlReaderWriter.Get(_scopeProvider, _xmlLock, _xmlContent,
                SetContextCache,
                (xml, registerXmlChange) =>
                {
                    SetXmlLocked(xml, registerXmlChange);
                    ClearContextCache();
                }, false);
        }

        // gets a locked safe write access to the main xml (cloned)
        private SafeXmlReaderWriter GetSafeXmlWriter()
        {
            return SafeXmlReaderWriter.Get(_scopeProvider, _xmlLock, _xmlContent,
                SetContextCache,
                (xml, registerXmlChange) =>
                {
                    SetXmlLocked(xml, registerXmlChange);
                    ClearContextCache();
                }, true);
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

        // invoked by XmlCacheFilePersister ONLY and that one manages the MainDom, ie it
        // will NOT try to save once the current app domain is not the main domain anymore
        // (no need to test _released)
        internal void SaveXmlToFile()
        {
            LogHelper.Info<content>("Save Xml to file...");
            try
            {
                // ok to access _xmlContent here - capture (atomic + volatile), immutable anyway
                var xml = _xmlContent;
                if (xml == null) return;

                // delete existing file, if any
                DeleteXmlFile();

                // ensure cache directory exists
                var directoryName = Path.GetDirectoryName(_xmlFileName);
                if (directoryName == null)
                    throw new Exception(string.Format("Invalid XmlFileName \"{0}\".", _xmlFileName));
                if (File.Exists(_xmlFileName) == false && Directory.Exists(directoryName) == false)
                    Directory.CreateDirectory(directoryName);

                // save
                using (var fs = new FileStream(_xmlFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    SaveXmlToStream(xml, fs);
                }

                LogHelper.Info<content>("Saved Xml to file.");
            }
            catch (Exception e)
            {
                // if something goes wrong remove the file
                try
                {
                    DeleteXmlFile();
                }
                catch
                {
                    // don't make it worse: could be that we failed to write because we cannot
                    // access the file, in which case we won't be able to delete it either
                }
                LogHelper.Error<content>("Failed to save Xml to file.", e);
            }
        }

        private void SaveXmlToStream(XmlDocument xml, Stream writeStream)
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

            if (writeStream.CanSeek)
            {
                writeStream.Position = 0;
            }

            using (var xmlWriter = XmlWriter.Create(writeStream, new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                //OmitXmlDeclaration = true
            }))
            {
                //xmlWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
                xml.WriteTo(xmlWriter); // already contains the xml declaration
            }
        }

        private XmlDocument LoadXmlFromFile()
        {
            // do NOT try to load if we are not the main domain anymore
            if (_released) return null;

            LogHelper.Info<content>("Load Xml from file...");

            try
            {
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
                try
                {
                    DeleteXmlFile();
                }
                catch
                {
                    // don't make it worse: could be that we failed to read because we cannot
                    // access the file, in which case we won't be able to delete it either
                }
                return null;
            }
        }

        private void DeleteXmlFile()
        {
            if (File.Exists(_xmlFileName) == false) return;
            File.SetAttributes(_xmlFileName, FileAttributes.Normal);
            File.Delete(_xmlFileName);
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

            using (var safeXml = GetSafeXmlWriter())
            {
                bool registerXmlChange;
                LoadXmlLocked(safeXml, out registerXmlChange); // updates _lastFileRead
                safeXml.AcceptChanges(registerXmlChange);
            }
        }

        #endregion

        #region Manage change

        //TODO remove as soon as we can break backward compatibility
        [Obsolete("Use GetAddOrUpdateXmlNode which returns an updated Xml document.", false)]
        public static void AddOrUpdateXmlNode(XmlDocument xml, int id, int level, int parentId, XmlNode docNode)
        {
            GetAddOrUpdateXmlNode(xml, id, level, parentId, docNode);
        }

        // adds or updates a node (docNode) into a cache (xml)
        public static XmlDocument GetAddOrUpdateXmlNode(XmlDocument xml, int id, int level, int parentId, XmlNode docNode)
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
            {
                var xml2 = EnsureSchema(docNode.Name, xml);
                if (ReferenceEquals(xml, xml2) == false)
                    docNode = xml2.ImportNode(docNode, true);
                xml = xml2;
            }

            // find the parent
            XmlNode parentNode = level == 1
                ? xml.DocumentElement
                : xml.GetElementById(parentId.ToInvariantString());

            // no parent = cannot do anything
            if (parentNode == null)
                return xml;

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
            return xml;
        }

        private static void TransferValuesFromDocumentXmlToPublishedXml(XmlNode documentNode, XmlNode publishedNode)
        {
            // remove all attributes from the published node
            if (publishedNode.Attributes == null) throw new Exception("oops");
            publishedNode.Attributes.RemoveAll();

            // remove all data nodes from the published node
            //TODO: This could be faster, might as well just iterate all children and filter
            // instead of selecting matching children (i.e. iterating all) and then iterating the
            // filtered items to remove, this also allocates more memory to store the list of children.
            // Below we also then do another filtering of child nodes, if we just iterate all children we
            // can perform both functions more efficiently
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

        [Obsolete("This is no used, do not use this for any reason")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static event ContentCacheDatabaseLoadXmlStringEventHandler AfterContentCacheDatabaseLoadXmlString;

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

        [Obsolete("This is no used, do not use this for any reason")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static event ContentCacheLoadNodeEventHandler AfterContentCacheLoadNodeFromDatabase;

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

        #region Preview

        private const string PreviewCacheKey = "umbraco.content.preview";

        internal void ClearPreviewXmlContent()
        {
            if (PreviewContent.IsSinglePreview == false) return;

            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            runtimeCache.ClearCacheItem(PreviewCacheKey);
        }

        internal void ClearPreviewXmlContent(int id)
        {
            if (PreviewContent.IsSinglePreview == false) return;

            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            var xml = runtimeCache.GetCacheItem<XmlDocument>(PreviewCacheKey);
            if (xml == null) return;

            // Check if node present, before cloning
            var x = xml.GetElementById(id.ToString());
            if (x == null)
                return;

            // Find the document in the xml cache
            // The document already exists in cache, so repopulate it
            x.ParentNode.RemoveChild(x);
        }

        internal void UpdatePreviewXmlContent(Document d)
        {
            if (PreviewContent.IsSinglePreview == false) return;

            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            var xml = runtimeCache.GetCacheItem<XmlDocument>(PreviewCacheKey);
            if (xml == null) return;

            var pnode = GetPreviewOrPublishedNode(d, xml, true);
            var pattr = ((XmlElement)pnode).GetAttributeNode("sortOrder");
            pattr.Value = d.sortOrder.ToString();
            AddOrUpdatePreviewXmlNode(d.Id, d.Level, d.Level == 1 ? -1 : d.ParentId, pnode);
        }

        private void AddOrUpdatePreviewXmlNode(int id, int level, int parentId, XmlNode docNode)
        {
            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            var xml = runtimeCache.GetCacheItem<XmlDocument>(PreviewCacheKey);
            if (xml == null) return;

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
            {
                if (HasSchema(docNode.Name, xml) == false)
                {
                    runtimeCache.ClearCacheItem(PreviewCacheKey);
                    return;
                }
            }

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

        // UpdateSortOrder is meant to update the Xml cache sort order on Save, 'cos that change
        // should be applied immediately, even though the Xml cache is not updated on Saves - we
        // don't have to do it for preview Xml since it is always fully updated - OTOH we have
        // to ensure it *is* updated, in UnpublishedPageCacheRefresher

        private XmlDocument LoadPreviewXmlContent()
        {
            try
            {
                LogHelper.Info<content>("Loading preview content from database...");
                var xml = ApplicationContext.Current.Services.ContentService.BuildPreviewXmlCache();
                LogHelper.Debug<content>("Done loading preview content");
                return xml;
            }
            catch (Exception ee)
            {
                LogHelper.Error<content>("Error loading preview content", ee);
            }

            // An error of some sort must have stopped us from successfully generating
            // the content tree, so lets return null signifying there is no content available
            return null;
        }

        public XmlDocument PreviewXmlContent
        {
            get
            {
                if (PreviewContent.IsSinglePreview == false)
                    throw new InvalidOperationException();

                var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
                return runtimeCache.GetCacheItem<XmlDocument>(PreviewCacheKey, LoadPreviewXmlContent, TimeSpan.FromSeconds(PreviewContent.SinglePreviewCacheDurationSeconds), true,
                    removedCallback: (key, removed, reason) => LogHelper.Debug<content>($"Removed preview xml from cache ({reason})"));
            }
        }

        #endregion
    }
}
