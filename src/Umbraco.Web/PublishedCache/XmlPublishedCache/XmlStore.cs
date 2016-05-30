using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Strings;
using Umbraco.Core.Xml;
using Umbraco.Web.Cache;
using Umbraco.Web.Scheduling;
using Content = umbraco.cms.businesslogic.Content;
using File = System.IO.File;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    /// <summary>
    /// Represents the Xml storage for the Xml published cache.
    /// </summary>
    /// <remarks>
    /// <para>One instance of <see cref="XmlStore"/> is instanciated by the <see cref="FacadeService"/> and
    /// then passed to all <see cref="PublishedContentCache"/> instances that are created (one per request).</para>
    /// <para>This class should *not* be public.</para>
    /// </remarks>
    class XmlStore : IDisposable
    {
        private Func<IContent, XElement> _xmlContentSerializer;
        private Func<IMember, XElement> _xmlMemberSerializer;
        private Func<IMedia, XElement> _xmlMediaSerializer;
        private XmlStoreFilePersister _persisterTask;
        private volatile bool _released;
        private bool _withRepositoryEvents;
        private bool _withOtherEvents;

        private readonly PublishedContentTypeCache _contentTypeCache;
        private readonly RoutesCache _routesCache;
        private readonly ServiceContext _serviceContext; // fixme WHY
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStore"/> class.
        /// </summary>
        /// <remarks>The default constructor will boot the cache, load data from file or database,
        /// wire events in order to manage changes, etc.</remarks>
        public XmlStore(ServiceContext serviceContext, IDatabaseUnitOfWorkProvider uowProvider, RoutesCache routesCache, PublishedContentTypeCache contentTypeCache, IEnumerable<IUrlSegmentProvider> segmentProviders)
            : this(serviceContext, uowProvider, routesCache, contentTypeCache, segmentProviders, false, false)
        { }

        // internal for unit tests
        // no file nor db, no config check
        // fixme - er, we DO have a DB?
        internal XmlStore(ServiceContext serviceContext, IDatabaseUnitOfWorkProvider uowProvider, RoutesCache routesCache, PublishedContentTypeCache contentTypeCache,
            IEnumerable<IUrlSegmentProvider> segmentProviders, bool testing, bool enableRepositoryEvents)
        {
            if (testing == false)
                EnsureConfigurationIsValid();

            _serviceContext = serviceContext;
            _uowProvider = uowProvider;
            _routesCache = routesCache;
            _contentTypeCache = contentTypeCache;

            InitializeSerializers(segmentProviders);

            if (testing)
            {
                _xmlFileEnabled = false;
            }
            else
            {
                InitializeFilePersister();
            }

            // need to wait for resolution to be frozen
            if (Resolution.IsFrozen)
                OnResolutionFrozen(testing, enableRepositoryEvents);
            else
                Resolution.Frozen += (sender, args) => OnResolutionFrozen(testing, enableRepositoryEvents);
        }

        // internal for unit tests
        // initialize with an xml document
        // no events, no file nor db, no config check
        internal XmlStore(XmlDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
            _xmlFileEnabled = false;

            // do not plug events, we may not have what it takes to handle them
        }

        // internal for unit tests
        // initialize with a function returning an xml document
        // no events, no file nor db, no config check
        internal XmlStore(Func<XmlDocument> getXmlDocument)
        {
            if (getXmlDocument == null)
                throw new ArgumentNullException(nameof(getXmlDocument));
            GetXmlDocument = getXmlDocument;
            _xmlFileEnabled = false;

            // do not plug events, we may not have what it takes to handle them
        }

        private void InitializeSerializers(IEnumerable<IUrlSegmentProvider> segmentProviders)
        {
            var exs = new EntityXmlSerializer();
            _xmlContentSerializer = c => exs.Serialize(_serviceContext.ContentService, _serviceContext.DataTypeService, _serviceContext.UserService, segmentProviders, c);
            _xmlMemberSerializer = m => exs.Serialize(_serviceContext.DataTypeService, m);
            _xmlMediaSerializer = m => exs.Serialize(_serviceContext.MediaService, _serviceContext.DataTypeService, _serviceContext.UserService, segmentProviders, m);
        }

        private void InitializeFilePersister()
        {
            if (SyncToXmlFile == false) return;

            var logger = LoggerResolver.Current.Logger;

            // there's always be one task keeping a ref to the runner
            // so it's safe to just create it as a local var here
            var runner = new BackgroundTaskRunner<XmlStoreFilePersister>(new BackgroundTaskRunnerOptions
            {
                LongRunning = true,
                KeepAlive = true,
                Hosted = false // main domain will take care of stopping the runner (see below)
            }, logger);

            // create (and add to runner)
            _persisterTask = new XmlStoreFilePersister(runner, this, logger);

            var registered = ApplicationContext.Current.MainDom.Register(
                null,
                () =>
                {
                    // once released, the cache still works but does not write to file anymore,
                    // which is OK with database server messenger but will cause data loss with
                    // another messenger...

                    runner.Shutdown(false, true); // wait until flushed
                    _persisterTask = null; // fail fast
                    _released = true;
                });

            // failed to become the main domain, we will never use the file
            if (registered == false)
                runner.Shutdown(false, true);

            _released = (registered == false);
        }

        private void OnResolutionFrozen(bool testing, bool enableRepositoryEvents)
        {
            if (testing == false || enableRepositoryEvents)
                InitializeRepositoryEvents();
            if (testing)
                return;
            InitializeOtherEvents();

            // not so soon! if eg installing we may not be able to load content yet
            // so replace this by LazyInitializeContent() called in Xml ppty getter
            //InitializeContent();
        }

        private void InitializeRepositoryEvents()
        {
            // plug repository event handlers
            // these trigger within the transaction to ensure consistency
            // and are used to maintain the central, database-level XML cache
            ContentRepository.UowRemovingEntity += OnContentRemovingEntity;
            ContentRepository.UowRemovingVersion += OnContentRemovingVersion;
            ContentRepository.UowRefreshedEntity += OnContentRefreshedEntity;
            MediaRepository.UowRemovingEntity += OnMediaRemovingEntity;
            MediaRepository.UowRemovingVersion += OnMediaRemovingVersion;
            MediaRepository.UowRefreshedEntity += OnMediaRefreshedEntity;
            MemberRepository.UowRemovingEntity += OnMemberRemovingEntity;
            MemberRepository.UowRemovingVersion += OnMemberRemovingVersion;
            MemberRepository.UowRefreshedEntity += OnMemberRefreshedEntity;

            // plug
            ContentTypeService.UowRefreshedEntity += OnContentTypeRefreshedEntity;
            MediaTypeService.UowRefreshedEntity += OnMediaTypeRefreshedEntity;
            MemberTypeService.UowRefreshedEntity += OnMemberTypeRefreshedEntity;

            _withRepositoryEvents = true;
        }

        private void InitializeOtherEvents()
        {
            // temp - until we get rid of Content
            Content.DeletedContent += OnDeletedContent;

            _withOtherEvents = true;
        }

        private void ClearEvents()
        {
            ContentRepository.UowRemovingEntity -= OnContentRemovingEntity;
            ContentRepository.UowRemovingVersion -= OnContentRemovingVersion;
            ContentRepository.UowRefreshedEntity -= OnContentRefreshedEntity;
            MediaRepository.UowRemovingEntity -= OnMediaRemovingEntity;
            MediaRepository.UowRemovingVersion -= OnMediaRemovingVersion;
            MediaRepository.UowRefreshedEntity -= OnMediaRefreshedEntity;
            MemberRepository.UowRemovingEntity -= OnMemberRemovingEntity;
            MemberRepository.UowRemovingVersion -= OnMemberRemovingVersion;
            MemberRepository.UowRefreshedEntity -= OnMemberRefreshedEntity;

            ContentTypeService.UowRefreshedEntity -= OnContentTypeRefreshedEntity;
            MediaTypeService.UowRefreshedEntity -= OnMediaTypeRefreshedEntity;
            MemberTypeService.UowRefreshedEntity -= OnMemberTypeRefreshedEntity;

            Content.DeletedContent -= OnDeletedContent;

            _withRepositoryEvents = false;
            _withOtherEvents = false;
        }

        private void LazyInitializeContent()
        {
            if (_xml != null) return;

            // and populate the cache
            using (var safeXml = GetSafeXmlWriter(false))
            {
                if (_xml != null) return; // double-check

                bool registerXmlChange;
                LoadXmlLocked(safeXml, out registerXmlChange);
                safeXml.Commit(registerXmlChange);
            }
        }

        public void Dispose()
        {
            ClearEvents();
        }

        #endregion

        #region Configuration

        // gathering configuration options here to document what they mean

        private readonly bool _xmlFileEnabled = true;

        // whether the disk cache is enabled
        private bool XmlFileEnabled => _xmlFileEnabled && UmbracoConfig.For.UmbracoSettings().Content.XmlCacheEnabled;

        // whether the disk cache is enabled and to update the disk cache when xml changes
        private bool SyncToXmlFile => XmlFileEnabled && UmbracoConfig.For.UmbracoSettings().Content.ContinouslyUpdateXmlDiskCache;

        // whether the disk cache is enabled and to reload from disk cache if it changes
        private bool SyncFromXmlFile => XmlFileEnabled && UmbracoConfig.For.UmbracoSettings().Content.XmlContentCheckForDiskChanges;

        // whether _xml is immutable or not (achieved by cloning before changing anything)
        private static bool XmlIsImmutable => UmbracoConfig.For.UmbracoSettings().Content.CloneXmlContent;

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
                LogHelper.Warn<XmlStore>("CloneXmlContent is false - ignored, we always clone.");

            // note: if SyncFromXmlFile then we should also disable / warn that local edits are going to cause issues...
        }

        #endregion

        #region Xml

        /// <summary>
        /// Gets or sets the delegate used to retrieve the Xml content, used for unit tests, else should
        /// be null and then the default content will be used. For non-preview content only.
        /// </summary>
        /// <remarks>
        /// The default content ONLY works when in the context an Http Request mostly because the
        /// 'content' object heavily relies on HttpContext, SQL connections and a bunch of other stuff
        /// that when run inside of a unit test fails.
        /// </remarks>
        public Func<XmlDocument> GetXmlDocument { get; set; }

        private readonly XmlDocument _xmlDocument; // supplied xml document (for tests)
        private volatile XmlDocument _xml; // master xml document
        private readonly AsyncLock _xmlLock = new AsyncLock(); // protects _xml

        // to be used by PublishedContentCache only
        // for non-preview content only
        public XmlDocument Xml
        {
            get
            {
                if (_xmlDocument != null)
                    return _xmlDocument;
                if (GetXmlDocument != null)
                    return GetXmlDocument();

                LazyInitializeContent();
                ReloadXmlFromFileIfChanged();
                return _xml;
            }
        }

        // assumes xml lock
        private void SetXmlLocked(XmlDocument xml, bool registerXmlChange)
        {
            // this is the ONLY place where we write to _xml
            _xml = xml;

            if (_routesCache != null)
                _routesCache.Clear(); // anytime we set _xml

            if (registerXmlChange == false || SyncToXmlFile == false)
                return;

            if (_persisterTask != null)
                _persisterTask = _persisterTask.Touch();
        }

        private static XmlDocument Clone(XmlDocument xmlDoc)
        {
            return xmlDoc == null ? null : (XmlDocument)xmlDoc.CloneNode(true);
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

        private static void InitializeXml(XmlDocument xml, string dtd)
        {
            // prime the xml document with an inline dtd and a root element
            xml.LoadXml(String.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?>{0}{1}{0}<root id=\"-1\"/>", Environment.NewLine, dtd));
        }

        private static void PopulateXml(IDictionary<int, List<int>> hierarchy, IDictionary<int, XmlNode> nodeIndex, int parentId, XmlNode parentNode)
        {
            List<int> children;
            if (hierarchy.TryGetValue(parentId, out children) == false) return;

            foreach (var childId in children)
            {
                // append child node to parent node and recursively take care of the child
                var childNode = nodeIndex[childId];
                parentNode.AppendChild(childNode);
                PopulateXml(hierarchy, nodeIndex, childId, childNode);
            }
        }

        /// <summary>
        /// Generates the complete (simplified) XML DTD.
        /// </summary>
        /// <returns>The DTD as a string</returns>
        private string GetDtd()
        {
            var dtd = new StringBuilder();
            dtd.AppendLine("<!DOCTYPE root [ ");

            // that whole thing does not make real sense? how could it fail?
            try
            {
                var dtdInner = new StringBuilder();
                var contentTypes = _serviceContext.ContentTypeService.GetAll();
                // though aliases should be safe and non null already?
                var aliases = contentTypes.Select(x => x.Alias.ToSafeAlias()).WhereNotNull();
                foreach (var alias in aliases)
                {
                    dtdInner.AppendLine($"<!ELEMENT {alias} ANY>");
                    dtdInner.AppendLine($"<!ATTLIST {alias} id ID #REQUIRED>");
                }
                dtd.Append(dtdInner);
            }
            catch (Exception exception)
            {
                LogHelper.Error<ContentTypeService>("Failed to build a DTD for the Xml cache.", exception);
            }

            dtd.AppendLine("]>");
            return dtd.ToString();
        }

        // try to load from file, otherwise database
        // assumes xml lock (file is always locked)
        private void LoadXmlLocked(SafeXmlReaderWriter safeXml, out bool registerXmlChange)
        {
            LogHelper.Debug<XmlStore>("Loading Xml...");

            // try to get it from the file
            if (XmlFileEnabled && (safeXml.Xml = LoadXmlFromFile()) != null)
            {
                registerXmlChange = false; // loaded from disk, do NOT write back to disk!
                return;
            }

            // get it from the database, and register
            LoadXmlTreeFromDatabaseLocked(safeXml);
            registerXmlChange = true;
        }

        public XmlNode GetMediaXmlNode(int mediaId)
        {
            // there's only one version for medias

            const string sql = @"SELECT umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, umbracoNode.Level,
cmsContentXml.xml, 1 AS published
FROM umbracoNode
JOIN cmsContentXml ON (cmsContentXml.nodeId=umbracoNode.id)
WHERE umbracoNode.nodeObjectType = @nodeObjectType
AND (umbracoNode.id=@id)";

            XmlDto xmlDto;
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var xmlDtos = uow.Database.Query<XmlDto>(sql,
                    new
                    {
                        @nodeObjectType = new Guid(Constants.ObjectTypes.Media),
                        @id = mediaId
                    });
                xmlDto = xmlDtos.FirstOrDefault();
                uow.Complete();
            }

            if (xmlDto == null) return null;

            var doc = new XmlDocument();
            var xml = doc.ReadNode(XmlReader.Create(new StringReader(xmlDto.Xml)));
            return xml;
        }

        public XmlNode GetMemberXmlNode(int memberId)
        {
            // there's only one version for members

            const string sql = @"SELECT umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, umbracoNode.Level,
cmsContentXml.xml, 1 AS published
FROM umbracoNode
JOIN cmsContentXml ON (cmsContentXml.nodeId=umbracoNode.id)
WHERE umbracoNode.nodeObjectType = @nodeObjectType
AND (umbracoNode.id=@id)";

            XmlDto xmlDto;
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var xmlDtos = uow.Database.Query<XmlDto>(sql,
                    new
                    {
                        @nodeObjectType = new Guid(Constants.ObjectTypes.Member),
                        @id = memberId
                    });
                xmlDto = xmlDtos.FirstOrDefault();
                uow.Complete();
            }

            if (xmlDto == null) return null;

            var doc = new XmlDocument();
            var xml = doc.ReadNode(XmlReader.Create(new StringReader(xmlDto.Xml)));
            return xml;
        }

        public XmlNode GetPreviewXmlNode(int contentId)
        {
            const string sql = @"SELECT umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, umbracoNode.Level,
cmsPreviewXml.xml, cmsDocument.published
FROM umbracoNode
JOIN cmsPreviewXml ON (cmsPreviewXml.nodeId=umbracoNode.id)
JOIN cmsDocument ON (cmsDocument.nodeId=umbracoNode.id)
WHERE umbracoNode.nodeObjectType = @nodeObjectType AND cmsDocument.newest=1
AND (umbracoNode.id=@id)";

            XmlDto xmlDto;
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var xmlDtos = uow.Database.Query<XmlDto>(sql,
                    new
                    {
                        @nodeObjectType = new Guid(Constants.ObjectTypes.Document),
                        @id = contentId
                    });
                xmlDto = xmlDtos.FirstOrDefault();
                uow.Complete();
            }
            if (xmlDto == null) return null;

            var doc = new XmlDocument();
            var xml = doc.ReadNode(XmlReader.Create(new StringReader(xmlDto.Xml)));
            if (xml == null || xml.Attributes == null) return null;

            if (xmlDto.Published == false)
                xml.Attributes.Append(doc.CreateAttribute("isDraft"));
            return xml;
        }

        public XmlDocument GetMediaXml()
        {
            // this is not efficient at all, not cached, nothing
            // just here to replicate what uQuery was doing and show it can be done
            // but really - should not be used

            return LoadMoreXmlFromDatabase(new Guid(Constants.ObjectTypes.Media));
        }

        public XmlDocument GetMemberXml()
        {
            // this is not efficient at all, not cached, nothing
            // just here to replicate what uQuery was doing and show it can be done
            // but really - should not be used

            return LoadMoreXmlFromDatabase(new Guid(Constants.ObjectTypes.Member));
        }

        public XmlDocument GetPreviewXml(int contentId, bool includeSubs)
        {
            var content = _serviceContext.ContentService.GetById(contentId);

            var doc = (XmlDocument)Xml.Clone();
            if (content == null) return doc;

            IEnumerable<XmlDto> xmlDtos;
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var sqlSyntax = uow.Database.SqlSyntax;
                var sql = ReadCmsPreviewXmlSql1;
                sql += " @path LIKE " + sqlSyntax.GetConcat("umbracoNode.Path", "',%"); // concat(umbracoNode.path, ',%')
                if (includeSubs) sql += " OR umbracoNode.path LIKE " + sqlSyntax.GetConcat("@path", "',%"); // concat(@path, ',%')
                sql += ReadCmsPreviewXmlSql2;
                xmlDtos = uow.Database.Query<XmlDto>(sql,
                    new
                    {
                        @nodeObjectType = new Guid(Constants.ObjectTypes.Document),
                        @path = content.Path,
                    }).ToArray(); // todo - do we want to load them all?
                uow.Complete();
            }

            foreach (var xmlDto in xmlDtos)
            {
                var xml = xmlDto.XmlNode = doc.ReadNode(XmlReader.Create(new StringReader(xmlDto.Xml)));
                if (xml == null || xml.Attributes == null) continue;
                if (xmlDto.Published == false)
                    xml.Attributes.Append(doc.CreateAttribute("isDraft"));
                doc = AddOrUpdateXmlNode(doc, xmlDto);
            }

            return doc;
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
            private readonly XmlStore _store;
            private IDisposable _releaser;
            private bool _isWriter;
            private bool _auto;
            private bool _committed;
            private XmlDocument _xml;

            private SafeXmlReaderWriter(XmlStore store, IDisposable releaser, bool isWriter, bool auto)
            {
                _store = store;
                _releaser = releaser;
                _isWriter = isWriter;
                _auto = auto;

                // cloning for writer is not an option anymore (see XmlIsImmutable)
                _xml = _isWriter ? Clone(store._xml) : store._xml;
            }

            public static SafeXmlReaderWriter GetReader(XmlStore store, IDisposable releaser)
            {
                return new SafeXmlReaderWriter(store, releaser, false, false);
            }

            public static SafeXmlReaderWriter GetWriter(XmlStore store, IDisposable releaser, bool auto)
            {
                return new SafeXmlReaderWriter(store, releaser, true, auto);
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
                _store.SetXmlLocked(_xml, registerXmlChange);
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

        private const string ChildNodesXPath = "./* [@id]";
        private const string DataNodesXPath = "./* [not(@id)]";

        #endregion

        #region File

        private readonly string _xmlFileName = IOHelper.MapPath(SystemFiles.ContentCacheXml);
        private DateTime _lastFileRead; // last time the file was read
        private DateTime _nextFileCheck; // last time we checked whether the file was changed

        public void EnsureFilePermission()
        {
            // FIXME - but do we really have a store, initialized, at that point?
            var filename = _xmlFileName + ".temp";
            File.WriteAllText(filename, "TEMP");
            File.Delete(filename);
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

        // invoked by XmlStoreFilePersister ONLY and that one manages the MainDom, ie it
        // will NOT try to save once the current app domain is not the main domain anymore
        // (no need to test _released)
        internal void SaveXmlToFile()
        {
            LogHelper.Info<XmlStore>("Save Xml to file...");

            try
            {
                var xml = _xml; // capture (atomic + volatile), immutable anyway
                if (xml == null) return;

                // delete existing file, if any
                DeleteXmlFile();

                // ensure cache directory exists
                var directoryName = Path.GetDirectoryName(_xmlFileName);
                if (directoryName == null)
                    throw new Exception($"Invalid XmlFileName \"{_xmlFileName}\".");
                if (File.Exists(_xmlFileName) == false && Directory.Exists(directoryName) == false)
                    Directory.CreateDirectory(directoryName);

                // save
                using (var fs = new FileStream(_xmlFileName, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    var bytes = Encoding.UTF8.GetBytes(SaveXmlToString(xml));
                    fs.Write(bytes, 0, bytes.Length);
                }

                LogHelper.Info<XmlStore>("Saved Xml to file.");
            }
            catch (Exception e)
            {
                // if something goes wrong remove the file
                DeleteXmlFile();

                LogHelper.Error<XmlStore>("Failed to save Xml to file.", e);
            }
        }

        // invoked by XmlStoreFilePersister ONLY and that one manages the MainDom, ie it
        // will NOT try to save once the current app domain is not the main domain anymore
        // (no need to test _released)
        internal async Task SaveXmlToFileAsync()
        {
            LogHelper.Info<XmlStore>("Save Xml to file...");

            try
            {
                var xml = _xml; // capture (atomic + volatile), immutable anyway
                if (xml == null) return;

                // delete existing file, if any
                DeleteXmlFile();

                // ensure cache directory exists
                var directoryName = Path.GetDirectoryName(_xmlFileName);
                if (directoryName == null)
                    throw new Exception($"Invalid XmlFileName \"{_xmlFileName}\".");
                if (File.Exists(_xmlFileName) == false && Directory.Exists(directoryName) == false)
                    Directory.CreateDirectory(directoryName);

                // save
                using (var fs = new FileStream(_xmlFileName, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    var bytes = Encoding.UTF8.GetBytes(SaveXmlToString(xml));
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                }

                LogHelper.Info<XmlStore>("Saved Xml to file.");
            }
            catch (Exception e)
            {
                // if something goes wrong remove the file
                DeleteXmlFile();

                LogHelper.Error<XmlStore>("Failed to save Xml to file.", e);
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

        private XmlDocument LoadXmlFromFile()
        {
            // do NOT try to load if we are not the main domain anymore
            if (_released) return null;

            LogHelper.Info<XmlStore>("Load Xml from file...");

            try
            {
                var xml = new XmlDocument();
                using (var fs = new FileStream(_xmlFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    xml.Load(fs);
                }
                _lastFileRead = DateTime.UtcNow;
                LogHelper.Info<XmlStore>("Loaded Xml from file.");
                return xml;
            }
            catch (FileNotFoundException)
            {
                LogHelper.Warn<XmlStore>("Failed to load Xml, file does not exist.");
                return null;
            }
            catch (Exception e)
            {
                LogHelper.Error<XmlStore>("Failed to load Xml from file.", e);
                DeleteXmlFile();
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

            LogHelper.Debug<XmlStore>("Xml file change detected, reloading.");

            // time to read

            using (var safeXml = GetSafeXmlWriter(false))
            {
                bool registerXmlChange;
                LoadXmlLocked(safeXml, out registerXmlChange); // updates _lastFileRead
                safeXml.Commit(registerXmlChange);
            }
        }

        #endregion

        #region Database

        const string ReadTreeCmsContentXmlSql = @"SELECT
    umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, umbracoNode.level, umbracoNode.path,
    cmsContentXml.xml, cmsContentXml.rv, cmsDocument.published
FROM umbracoNode
JOIN cmsContentXml ON (cmsContentXml.nodeId=umbracoNode.id)
JOIN cmsDocument ON (cmsDocument.nodeId=umbracoNode.id)
WHERE umbracoNode.nodeObjectType = @nodeObjectType AND cmsDocument.published=1
ORDER BY umbracoNode.level, umbracoNode.sortOrder";

        const string ReadBranchCmsContentXmlSql = @"SELECT
    umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, umbracoNode.level, umbracoNode.path,
    cmsContentXml.xml, cmsContentXml.rv, cmsDocument.published
FROM umbracoNode
JOIN cmsContentXml ON (cmsContentXml.nodeId=umbracoNode.id)
JOIN cmsDocument ON (cmsDocument.nodeId=umbracoNode.id)
WHERE umbracoNode.nodeObjectType = @nodeObjectType AND cmsDocument.published=1 AND (umbracoNode.id = @id OR umbracoNode.path LIKE @path)
ORDER BY umbracoNode.level, umbracoNode.sortOrder";

        const string ReadCmsContentXmlForContentTypesSql = @"SELECT
    umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, umbracoNode.level, umbracoNode.path,
    cmsContentXml.xml, cmsContentXml.rv, cmsDocument.published
FROM umbracoNode
JOIN cmsContentXml ON (cmsContentXml.nodeId=umbracoNode.id)
JOIN cmsDocument ON (cmsDocument.nodeId=umbracoNode.id)
JOIN cmsContent ON (cmsDocument.nodeId=cmsContent.nodeId)
WHERE umbracoNode.nodeObjectType = @nodeObjectType AND cmsDocument.published=1 AND cmsContent.contentType IN (@ids)
ORDER BY umbracoNode.level, umbracoNode.sortOrder";

        const string ReadMoreCmsContentXmlSql = @"SELECT
    umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, umbracoNode.level, umbracoNode.path,
    cmsContentXml.xml, cmsContentXml.rv, 1 AS published
FROM umbracoNode
JOIN cmsContentXml ON (cmsContentXml.nodeId=umbracoNode.id)
WHERE umbracoNode.nodeObjectType = @nodeObjectType
ORDER BY umbracoNode.level, umbracoNode.sortOrder";

        private const string ReadCmsPreviewXmlSql1 = @"SELECT
    umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, umbracoNode.level, umbracoNode.path,
    cmsPreviewXml.xml, cmsPreviewXml.rv, cmsDocument.published
FROM umbracoNode
JOIN cmsPreviewXml ON (cmsPreviewXml.nodeId=umbracoNode.id)
JOIN cmsDocument ON (cmsDocument.nodeId=umbracoNode.id)
WHERE umbracoNode.nodeObjectType = @nodeObjectType AND cmsDocument.newest=1
AND (umbracoNode.path=@path OR"; // @path LIKE concat(umbracoNode.path, ',%')";
        const string ReadCmsPreviewXmlSql2 = @")
ORDER BY umbracoNode.level, umbracoNode.sortOrder";

        // ReSharper disable once ClassNeverInstantiated.Local
        private class XmlDto
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local

            public int Id { get; set; }
            public long Rv { get; set; }
            public int ParentId { get; set; }
            //public int SortOrder { get; set; }
            public int Level { get; set; }
            public string Path { get; set; }
            public string Xml { get; set; }
            public bool Published { get; set; }

            [Ignore]
            public XmlNode XmlNode { get; set; }

            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        // assumes xml lock
        private void LoadXmlTreeFromDatabaseLocked(SafeXmlReaderWriter safeXml)
        {
            // initialise the document ready for the composition of content
            var xml = new XmlDocument();
            InitializeXml(xml, GetDtd());

            var parents = new Dictionary<int, XmlNode>();

            var dtoQuery = LoadXmlTreeDtoFromDatabaseLocked(xml);
            foreach (var dto in dtoQuery)
            {
                XmlNode parent;
                if (parents.TryGetValue(dto.ParentId, out parent) == false)
                {
                    parent = dto.ParentId == -1
                        ? xml.DocumentElement
                        : xml.GetElementById(dto.ParentId.ToInvariantString());

                    if (parent == null)
                        continue;

                    parents[dto.ParentId] = parent;
                }

                parent.AppendChild(dto.XmlNode);
            }

            safeXml.Xml = xml;
        }

        // assumes xml lock
        private IEnumerable<XmlDto> LoadXmlTreeDtoFromDatabaseLocked(XmlDocument xmlDoc)
        {
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);

                // get xml
                var xmlDtos = uow.Database.Query<XmlDto>(ReadTreeCmsContentXmlSql,
                    new
                    {
                        @nodeObjectType = new Guid(Constants.ObjectTypes.Document)
                    });

                // create nodes
                var nodes = xmlDtos.Select(x =>
                {
                    // parse into a DOM node
                    x.XmlNode = ImportContent(xmlDoc, x);
                    return x;
                }).ToArray(); // todo - could move uow mgt up to avoid ToArray here

                uow.Complete();
                return nodes;
            }
        }

        // assumes xml lock
        private IEnumerable<XmlDto> LoadXmlBranchDtoFromDatabaseLocked(XmlDocument xmlDoc, int id, string path)
        {
            // get xml
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);

                var xmlDtos = uow.Database.Query<XmlDto>(ReadBranchCmsContentXmlSql,
                    new
                    {
                        @nodeObjectType = new Guid(Constants.ObjectTypes.Document),
                        @path = path + ",%",
                        /*@id =*/
                        id
                    });

                // create nodes
                var nodes = xmlDtos.Select(x =>
                {
                    // parse into a DOM node
                    x.XmlNode = ImportContent(xmlDoc, x);
                    return x;
                }).ToArray(); // todo - could move uow mgt up to avoid ToArray here

                uow.Complete();
                return nodes;
            }
        }

        private XmlDocument LoadMoreXmlFromDatabase(Guid nodeObjectType)
        {
            // FIXME - has been optimized in 7.4 - see how!
            var hierarchy = new Dictionary<int, List<int>>();
            var nodeIndex = new Dictionary<int, XmlNode>();

            var xmlDoc = new XmlDocument();

            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                if (nodeObjectType == Constants.ObjectTypes.DocumentGuid)
                    uow.ReadLock(Constants.Locks.ContentTree);
                else if (nodeObjectType == Constants.ObjectTypes.MediaGuid)
                    uow.ReadLock(Constants.Locks.MediaTree);
                else if (nodeObjectType == Constants.ObjectTypes.MemberGuid)
                    uow.ReadLock(Constants.Locks.MemberTree);

                var xmlDtos = uow.Database.Query<XmlDto>(ReadMoreCmsContentXmlSql,
                    new { /*@nodeObjectType =*/ nodeObjectType });

                foreach (var xmlDto in xmlDtos)
                {
                    // and parse it into a DOM node
                    xmlDoc.LoadXml(xmlDto.Xml);
                    var node = xmlDoc.FirstChild;
                    nodeIndex.Add(xmlDto.Id, node);

                    // Build the content hierarchy
                    List<int> children;
                    if (hierarchy.TryGetValue(xmlDto.ParentId, out children) == false)
                    {
                        // No children for this parent, so add one
                        children = new List<int>();
                        hierarchy.Add(xmlDto.ParentId, children);
                    }
                    children.Add(xmlDto.Id);
                }

                // If we got to here we must have successfully retrieved the content from the DB so
                // we can safely initialise and compose the final content DOM.
                // Note: We are reusing the XmlDocument used to create the xml nodes above so
                // we don't have to import them into a new XmlDocument

                // Initialise the document ready for the final composition of content
                InitializeXml(xmlDoc, string.Empty);

                // Start building the content tree recursively from the root (-1) node
                PopulateXml(hierarchy, nodeIndex, -1, xmlDoc.DocumentElement);

                uow.Complete();
            }

            return xmlDoc;
        }

        // internal - used by umbraco.content.RefreshContentFromDatabase[Async]
        internal void ReloadXmlFromDatabase()
        {
            // event - cancel

            // nobody should work on the Xml while we load
            using (var safeXml = GetSafeXmlWriter())
            {
                LoadXmlTreeFromDatabaseLocked(safeXml);
            }
        }

        #endregion

        #region Handle Distributed Notifications for Memory Xml

        // NOT using events, see notes in IPublishedCachesService

        public void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged)
        {
            draftChanged = publishedChanged = false;
            if (_xml == null) return; // not initialized yet!

            draftChanged = true; // by default - we don't track drafts
            publishedChanged = false;

            if (_withOtherEvents == false)
                return;

            // process all changes on one xml clone
            using (var safeXml = GetSafeXmlWriter(false)) // not auto-commit
            {
                foreach (var payload in payloads)
                {
                    LogHelper.Debug<XmlStore>($"Notified {payload.ChangeTypes} for content {payload.Id}.");

                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                    {
                        LoadXmlTreeFromDatabaseLocked(safeXml);
                        publishedChanged = true;
                        continue;
                    }

                    if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                    {
                        var toRemove = safeXml.Xml.GetElementById(payload.Id.ToInvariantString());
                        if (toRemove != null)
                        {
                            if (toRemove.ParentNode == null) throw new Exception("oops");
                            toRemove.ParentNode.RemoveChild(toRemove);
                            publishedChanged = true;
                        }
                        continue;
                    }

                    if (payload.ChangeTypes.HasTypesNone(TreeChangeTypes.RefreshNode | TreeChangeTypes.RefreshBranch))
                    {
                        // ?!
                        continue;
                    }

                    var content = _serviceContext.ContentService.GetById(payload.Id);
                    var current = safeXml.Xml.GetElementById(payload.Id.ToInvariantString());

                    if (content == null || content.HasPublishedVersion == false || content.Trashed)
                    {
                        // no published version
                        LogHelper.Debug<XmlStore>($"Notified, content {payload.Id} has no published version.");
                        if (current != null)
                        {
                            // remove from xml if exists
                            if (current.ParentNode == null) throw new Exception("oops");
                            current.ParentNode.RemoveChild(current);
                            publishedChanged = true;
                        }

                        continue;
                    }

                    // else we have a published version

                    // that query is yielding results so will only load what's needed
                    //
                    // 'using' the enumerator ensures that the enumeration is properly terminated even if abandonned
                    // otherwise, it would leak an open reader & an un-released database connection
                    // see PetaPoco.Query<TRet>(Type[] types, Delegate cb, string sql, params object[] args)
                    // and read http://blogs.msdn.com/b/oldnewthing/archive/2008/08/14/8862242.aspx
                    var dtoQuery = LoadXmlBranchDtoFromDatabaseLocked(safeXml.Xml, content.Id, content.Path);
                    using (var dtos = dtoQuery.GetEnumerator())
                    {
                        if (dtos.MoveNext() == false)
                        {
                            // gone fishing, remove (possible race condition)
                            LogHelper.Debug<XmlStore>($"Notifified, content {payload.Id} gone fishing.");
                            if (current != null)
                            {
                                // remove from xml if exists
                                if (current.ParentNode == null) throw new Exception("oops");
                                current.ParentNode.RemoveChild(current);
                                publishedChanged = true;
                            }
                            continue;
                        }

                        if (dtos.Current.Id != content.Id)
                            throw new Exception("oops"); // first one should be 'current'
                        var currentDto = dtos.Current;

                        // note: if anything eg parentId or path or level has changed, then rv has changed too
                        var currentRv = current == null ? -1 : int.Parse(current.Attributes["rv"].Value);

                        // if exists and unchanged and not refreshing the branch, skip entirely
                        if (current != null
                            && currentRv == currentDto.Rv
                            && payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch) == false)
                            continue;

                        // note: Examine would not be able to do the path trick below, and we cannot help for
                        // unpublished content, so it *is* possible that Examine is inconsistent for a while,
                        // though events should get it consistent eventually.

                        // note: if path has changed we must do a branch refresh, even if the event is not requiring
                        // it, otherwise we would update the local node and not its children, who would then have
                        // inconsistent level (and path) attributes.

                        var refreshBranch = current == null
                            || payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch)
                            || current.Attributes["path"].Value != currentDto.Path;

                        if (refreshBranch)
                        {
                            // remove node if exists
                            if (current != null)
                            {
                                if (current.ParentNode == null) throw new Exception("oops");
                                current.ParentNode.RemoveChild(current);
                            }

                            // insert node
                            var newParent = currentDto.ParentId == -1
                                ? safeXml.Xml.DocumentElement
                                : safeXml.Xml.GetElementById(currentDto.ParentId.ToInvariantString());
                            if (newParent == null) continue;
                            newParent.AppendChild(currentDto.XmlNode);
                            XmlHelper.SortNode(newParent, ChildNodesXPath, currentDto.XmlNode,
                                x => x.AttributeValue<int>("sortOrder"));

                            // add branch (don't try to be clever)
                            while (dtos.MoveNext())
                            {
                                // dtos are ordered by sortOrder already
                                var dto = dtos.Current;

                                // if node is already there, somewhere, remove
                                var n = safeXml.Xml.GetElementById(dto.Id.ToInvariantString());
                                if (n != null)
                                {
                                    if (n.ParentNode == null) throw new Exception("oops");
                                    n.ParentNode.RemoveChild(n);
                                }

                                // find parent, add node
                                var p = safeXml.Xml.GetElementById(dto.ParentId.ToInvariantString()); // branch, so parentId > 0
                                // takes care of out-of-sync & masked
                                p?.AppendChild(dto.XmlNode);
                            }
                        }
                        else
                        {
                            // in-place
                            safeXml.Xml = AddOrUpdateXmlNode(safeXml.Xml, currentDto);
                        }
                    }

                    publishedChanged = true;
                }

                if (publishedChanged)
                {
                    safeXml.Commit(); // not auto!
                    Facade.ResyncCurrent();
                }
            }
        }

        public void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads)
        {
            if (_xml == null) return; // not initialized yet!
            
            // see ContentTypeServiceBase
            // in all cases we just want to clear the content type cache
            // the type will be reloaded if/when needed
            foreach (var payload in payloads)
                _contentTypeCache.ClearContentType(payload.Id);

            // process content types / content cache
            // only those that have been changed - with impact on content - RefreshMain
            // for those that have been removed, content is removed already
            var ids = payloads
                .Where(x => x.ItemType == typeof(IContentType).Name && x.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain))
                .Select(x => x.Id)
                .ToArray();

            foreach (var payload in payloads)
                LogHelper.Debug<XmlStore>($"Notified {payload.ChangeTypes} for content type {payload.Id}.");

            if (ids.Length > 0) // must have refreshes, not only removes
                RefreshContentTypes(ids);

            // ignore media and member types - we're not caching them

            Facade.ResyncCurrent();
        }

        public void Notify(DataTypeCacheRefresher.JsonPayload[] payloads)
        {
            if (_xml == null) return; // not initialized yet!

            // see above
            // in all cases we just want to clear the content type cache
            // the types will be reloaded if/when needed
            foreach (var payload in payloads)
                _contentTypeCache.ClearDataType(payload.Id);

            foreach (var payload in payloads)
                LogHelper.Debug<XmlStore>($"Notified {(payload.Removed ? "Removed" : "Refreshed")} for data type {payload.Id}.");

            // that's all we need to do as the changes have NO impact whatsoever on the Xml content

            // ignore media and member types - we're not caching them

            Facade.ResyncCurrent();
        }

        #endregion

        #region Manage change

        private void RefreshContentTypes(IEnumerable<int> ids)
        {
            using (var safeXml = GetSafeXmlWriter())
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var xmlDtos = uow.Database.Query<XmlDto>(ReadCmsContentXmlForContentTypesSql,
                    new { @nodeObjectType = new Guid(Constants.ObjectTypes.Document), /*@ids =*/ ids });

                foreach (var xmlDto in xmlDtos)
                {
                    xmlDto.XmlNode = safeXml.Xml.ReadNode(XmlReader.Create(new StringReader(xmlDto.Xml)));
                    safeXml.Xml = AddOrUpdateXmlNode(safeXml.Xml, xmlDto);
                }

                uow.Complete();
            }
        }

        private void RefreshMediaTypes(IEnumerable<int> ids)
        {
            // nothing to do, we have no cache
        }

        private void RefreshMemberTypes(IEnumerable<int> ids)
        {
            // nothing to do, we have no cache
        }

        // adds or updates a node (docNode) into a cache (xml)
        private static XmlDocument AddOrUpdateXmlNode(XmlDocument xml, XmlDto xmlDto)
        {
            // sanity checks
            var docNode = xmlDto.XmlNode;
            if (xmlDto.Id != docNode.AttributeValue<int>("id"))
                throw new ArgumentException("Values of id and docNode/@id are different.");
            if (xmlDto.ParentId != docNode.AttributeValue<int>("parentID"))
                throw new ArgumentException("Values of parentId and docNode/@parentID are different.");

            // find the document in the cache
            XmlNode currentNode = xml.GetElementById(xmlDto.Id.ToInvariantString());

            // if the document is not there already then it's a new document
            // we must make sure that its document type exists in the schema
            if (currentNode == null)
            {
                var xml2 = EnsureSchema(docNode.Name, xml);
                if (ReferenceEquals(xml, xml2) == false)
                    docNode = xml2.ImportNode(docNode, true);
                xml = xml2;
            }

            // find the parent
            XmlNode parentNode = xmlDto.Level == 1
                ? xml.DocumentElement
                : xml.GetElementById(xmlDto.ParentId.ToInvariantString());

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
                var moving = currentNode.AttributeValue<int>("parentID") != xmlDto.ParentId;

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

            var attrs = currentNode.Attributes;
            if (attrs == null) throw new Exception("oops.");

            var attr = attrs["rv"] ?? attrs.Append(xml.CreateAttribute("rv"));
            attr.Value = xmlDto.Rv.ToString(CultureInfo.InvariantCulture);

            attr = attrs["path"] ?? attrs.Append(xml.CreateAttribute("path"));
            attr.Value = xmlDto.Path;

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

        private static XmlNode ImportContent(XmlDocument xml, XmlDto dto)
        {
            var node = xml.ReadNode(XmlReader.Create(new StringReader(dto.Xml), new XmlReaderSettings
            {
                IgnoreWhitespace = true
            }));

            if (node == null) throw new Exception("oops");
            if (node.Attributes == null) throw new Exception("oops");

            var attr = xml.CreateAttribute("rv");
            attr.Value = dto.Rv.ToString(CultureInfo.InvariantCulture);
            node.Attributes.Append(attr);

            attr = xml.CreateAttribute("path");
            attr.Value = dto.Path;
            node.Attributes.Append(attr);

            return node;
        }

        #endregion

        #region Handle Repository Events For Database Xml

        // we need them to be "repository" events ie to trigger from within the repository transaction,
        // because they need to be consistent with the content that is being refreshed/removed - and that
        // should be guaranteed by a DB transaction
        // it is not the case at the moment, instead a global lock is used whenever content is modified - well,
        // almost: rollback or unpublish do not implement it - nevertheless

        private void OnContentRemovingEntity(ContentRepository sender, ContentRepository.UnitOfWorkEntityEventArgs args)
        {
            OnRemovedEntity(args.UnitOfWork.Database, args.Entity);
        }

        private void OnMediaRemovingEntity(MediaRepository sender, MediaRepository.UnitOfWorkEntityEventArgs args)
        {
            OnRemovedEntity(args.UnitOfWork.Database, args.Entity);
        }

        private void OnMemberRemovingEntity(MemberRepository sender, MemberRepository.UnitOfWorkEntityEventArgs args)
        {
            OnRemovedEntity(args.UnitOfWork.Database, args.Entity);
        }

        private void OnRemovedEntity(UmbracoDatabase db, IContentBase item)
        {
            var parms = new { id = item.Id };
            db.Execute("DELETE FROM cmsContentXml WHERE nodeId=@id", parms);
            db.Execute("DELETE FROM cmsPreviewXml WHERE nodeId=@id", parms);

            // note: could be optimized by using "WHERE nodeId IN (...)" delete clauses
        }

        private void OnContentRemovingVersion(ContentRepository sender, ContentRepository.UnitOfWorkVersionEventArgs args)
        {
            OnRemovedVersion(args.UnitOfWork.Database, args.EntityId, args.VersionId);
        }

        private void OnMediaRemovingVersion(MediaRepository sender, MediaRepository.UnitOfWorkVersionEventArgs args)
        {
            OnRemovedVersion(args.UnitOfWork.Database, args.EntityId, args.VersionId);
        }

        private void OnMemberRemovingVersion(MemberRepository sender, MemberRepository.UnitOfWorkVersionEventArgs args)
        {
            OnRemovedVersion(args.UnitOfWork.Database, args.EntityId, args.VersionId);
        }

        private void OnRemovedVersion(UmbracoDatabase db, int entityId, Guid versionId)
        {
            // we do not version cmsPreviewXml anymore - nothing to do here
        }

        private static readonly string[] PropertiesImpactingAllVersions = { "SortOrder", "ParentId", "Level", "Path", "Trashed" };

        private static bool HasChangesImpactingAllVersions(IContent icontent)
        {
            var content = (Core.Models.Content)icontent;

            // UpdateDate will be dirty
            // Published may be dirty if saving a Published entity
            // so cannot do this (would always be true):
            //return content.IsEntityDirty();

            // have to be more precise & specify properties
            return PropertiesImpactingAllVersions.Any(content.IsPropertyDirty);
        }

        private void OnContentRefreshedEntity(ContentRepository sender, ContentRepository.UnitOfWorkEntityEventArgs args)
        {
            var db = args.UnitOfWork.Database;
            var entity = args.Entity;

            var xml = _xmlContentSerializer(entity).ToDataString();

            // change below to write only one row - not one per version
            var dto1 = new PreviewXmlDto
            {
                NodeId = entity.Id,
                Xml = xml
            };
            OnRepositoryRefreshed(db, dto1);

            // if unpublishing, remove from table

            if (((Core.Models.Content)entity).PublishedState == PublishedState.Unpublishing)
            {
                db.Execute("DELETE FROM cmsContentXml WHERE nodeId=@id", new { id = entity.Id });
                return;
            }

            // need to update the published xml if we're saving the published version,
            // or having an impact on that version - we update the published xml even when masked

            IContent pc = null;
            if (entity.Published)
            {
                // saving the published version = update xml
                pc = entity;
            }
            else
            {
                // saving the non-published version, but there is a published version
                // check whether we have changes that impact the published version (move...)
                if (entity.HasPublishedVersion && HasChangesImpactingAllVersions(entity))
                    pc = sender.GetByVersion(entity.PublishedVersionGuid);
            }

            if (pc == null)
                return;

            xml = _xmlContentSerializer(pc).ToDataString();
            var dto2 = new ContentXmlDto { NodeId = entity.Id, Xml = xml };
            OnRepositoryRefreshed(db, dto2);

        }

        private void OnMediaRefreshedEntity(MediaRepository sender, MediaRepository.UnitOfWorkEntityEventArgs args)
        {
            var db = args.UnitOfWork.Database;
            var entity = args.Entity;

            // for whatever reason we delete some xml when the media is trashed
            // at least that's what the MediaService implementation did
            if (entity.Trashed)
                db.Execute("DELETE FROM cmsContentXml WHERE nodeId=@id", new { id = entity.Id });

            var xml = _xmlMediaSerializer(entity).ToDataString();

            var dto1 = new ContentXmlDto { NodeId = entity.Id, Xml = xml };
            OnRepositoryRefreshed(db, dto1);
        }

        private void OnMemberRefreshedEntity(MemberRepository sender, MemberRepository.UnitOfWorkEntityEventArgs args)
        {
            var db = args.UnitOfWork.Database;
            var entity = args.Entity;

            var xml = _xmlMemberSerializer(entity).ToDataString();

            var dto1 = new ContentXmlDto { NodeId = entity.Id, Xml = xml };
            OnRepositoryRefreshed(db, dto1);
        }

        private void OnRepositoryRefreshed(UmbracoDatabase db, ContentXmlDto dto)
        {
            // use a custom SQL to update row version on each update
            //db.InsertOrUpdate(dto);

            db.InsertOrUpdate(dto,
                "SET xml=@xml, rv=rv+1 WHERE nodeId=@id",
                new
                {
                    xml = dto.Xml,
                    id = dto.NodeId
                });
        }

        private void OnRepositoryRefreshed(UmbracoDatabase db, PreviewXmlDto dto)
        {
            // cannot simply update because of PetaPoco handling of the composite key ;-(
            // read http://stackoverflow.com/questions/11169144/how-to-modify-petapoco-class-to-work-with-composite-key-comprising-of-non-numeri
            // it works in https://github.com/schotime/PetaPoco and then https://github.com/schotime/NPoco but not here
            //
            // not important anymore as we don't manage version anymore,
            // but:
            //
            // also
            // use a custom SQL to update row version on each update
            //db.InsertOrUpdate(dto);

            db.InsertOrUpdate(dto,
                "SET xml=@xml, rv=rv+1 WHERE nodeId=@id",
                new
                {
                    xml = dto.Xml,
                    id = dto.NodeId,
                });
        }

        private void OnDeletedContent(object sender, Content.ContentDeleteEventArgs args)
        {
            var db = args.Database;
            var parms = new { @nodeId = args.Id };
            db.Execute("DELETE FROM cmsPreviewXml WHERE nodeId=@nodeId", parms);
            db.Execute("DELETE FROM cmsContentXml WHERE nodeId=@nodeId", parms);
        }

        private void OnContentTypeRefreshedEntity(IContentTypeService sender, ContentTypeChange<IContentType>.EventArgs args)
        {
            // handling a transaction event that does not play well with cache...
            //RepositoryBase.SetCacheEnabledForCurrentRequest(false); // fixme wtf!!

            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var contentTypeIds = args.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (contentTypeIds.Any())
                RebuildContentAndPreviewXml(contentTypeIds: contentTypeIds);
        }

        private void OnMediaTypeRefreshedEntity(IMediaTypeService sender, ContentTypeChange<IMediaType>.EventArgs args)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var mediaTypeIds = args.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (mediaTypeIds.Any())
                RebuildMediaXml(contentTypeIds: mediaTypeIds);
        }

        private void OnMemberTypeRefreshedEntity(IMemberTypeService sender, ContentTypeChange<IMemberType>.EventArgs args)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var memberTypeIds = args.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (memberTypeIds.Any())
                RebuildMemberXml(contentTypeIds: memberTypeIds);
        }

        #endregion

        #region Rebuild Database Xml

        public void RebuildContentAndPreviewXml(int groupSize = 5000, IEnumerable<int> contentTypeIds = null)
        {
            var contentTypeIdsA = contentTypeIds?.ToArray();

            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                RebuildContentXmlLocked(uow, repository, groupSize, contentTypeIdsA);
                RebuildPreviewXmlLocked(uow, repository, groupSize, contentTypeIdsA);
                uow.Complete();
            }
        }

        public void RebuildContentXml(int groupSize = 5000, IEnumerable<int> contentTypeIds = null)
        {
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                RebuildContentXmlLocked(uow, repository, groupSize, contentTypeIds);
                uow.Complete();
            }
        }

        // assumes content tree lock
        private void RebuildContentXmlLocked(IDatabaseUnitOfWork unitOfWork, IContentRepository repository, int groupSize, IEnumerable<int> contentTypeIds)
        {
            var contentTypeIdsA = contentTypeIds?.ToArray();
            var contentObjectType = Guid.Parse(Constants.ObjectTypes.Document);
            var db = unitOfWork.Database;

            // remove all - if anything fails the transaction will rollback
            if (contentTypeIds == null || contentTypeIdsA.Length == 0)
            {
                // must support SQL-CE
                //                    db.Execute(@"DELETE cmsContentXml
                //FROM cmsContentXml
                //JOIN umbracoNode ON (cmsContentXml.nodeId=umbracoNode.Id)
                //WHERE umbracoNode.nodeObjectType=@objType",
                db.Execute(@"DELETE FROM cmsContentXml
WHERE cmsContentXml.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                    new { objType = contentObjectType });
            }
            else
            {
                // assume number of ctypes won't blow IN(...)
                // must support SQL-CE
                //                    db.Execute(@"DELETE cmsContentXml
                //FROM cmsContentXml
                //JOIN umbracoNode ON (cmsContentXml.nodeId=umbracoNode.Id)
                //JOIN cmsContent ON (cmsContentXml.nodeId=cmsContent.nodeId)
                //WHERE umbracoNode.nodeObjectType=@objType
                //AND cmsContent.contentType IN (@ctypes)",
                db.Execute(@"DELETE FROM cmsContentXml
WHERE cmsContentXml.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN cmsContent ON cmsContent.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND cmsContent.contentType IN (@ctypes)
)",
                    new { objType = contentObjectType, ctypes = contentTypeIdsA });
            }

            // insert back - if anything fails the transaction will rollback
            var query = repository.Query.Where(x => x.Published);
            if (contentTypeIds != null && contentTypeIdsA.Length > 0)
                query = query.WhereIn(x => x.ContentTypeId, contentTypeIdsA); // assume number of ctypes won't blow IN(...)

            long pageIndex = 0;
            long processed = 0;
            long total;
            do
            {
                // make sure we do NOT add (cmsDocument.newest = 1) to the query
                // because we already have the condition on the content being published
                var descendants = repository.GetPagedResultsByQuery(query, pageIndex++, groupSize, out total, "Path", Direction.Ascending, true, newest: false);
                var items = descendants.Select(c => new ContentXmlDto { NodeId = c.Id, Xml = _xmlContentSerializer(c).ToDataString() }).ToArray();
                db.BulkInsertRecords(db.SqlSyntax, items);
                processed += items.Length;
            } while (processed < total);
        }

        public void RebuildPreviewXml(int groupSize = 5000, IEnumerable<int> contentTypeIds = null)
        {
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                RebuildPreviewXmlLocked(uow, repository, groupSize, contentTypeIds);
                uow.Complete();
            }
        }

        // assumes content tree lock
        private void RebuildPreviewXmlLocked(IDatabaseUnitOfWork unitOfWork, IContentRepository repository, int groupSize, IEnumerable<int> contentTypeIds)
        {
            var contentTypeIdsA = contentTypeIds?.ToArray();
            var contentObjectType = Guid.Parse(Constants.ObjectTypes.Document);
            var db = unitOfWork.Database;

            // remove all - if anything fails the transaction will rollback
            if (contentTypeIds == null || contentTypeIdsA.Length == 0)
            {
                // must support SQL-CE
                //                    db.Execute(@"DELETE cmsPreviewXml
                //FROM cmsPreviewXml
                //JOIN umbracoNode ON (cmsPreviewXml.nodeId=umbracoNode.Id)
                //WHERE umbracoNode.nodeObjectType=@objType",
                db.Execute(@"DELETE FROM cmsPreviewXml
WHERE cmsPreviewXml.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                    new { objType = contentObjectType });
            }
            else
            {
                // assume number of ctypes won't blow IN(...)
                // must support SQL-CE
                //                    db.Execute(@"DELETE cmsPreviewXml
                //FROM cmsPreviewXml
                //JOIN umbracoNode ON (cmsPreviewXml.nodeId=umbracoNode.Id)
                //JOIN cmsContent ON (cmsPreviewXml.nodeId=cmsContent.nodeId)
                //WHERE umbracoNode.nodeObjectType=@objType
                //AND cmsContent.contentType IN (@ctypes)",
                db.Execute(@"DELETE FROM cmsPreviewXml
WHERE cmsPreviewXml.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN cmsContent ON cmsContent.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND cmsContent.contentType IN (@ctypes)
)",
                    new { objType = contentObjectType, ctypes = contentTypeIdsA });
            }

            // insert back - if anything fails the transaction will rollback
            var query = repository.Query;
            if (contentTypeIds != null && contentTypeIdsA.Length > 0)
                query = query.WhereIn(x => x.ContentTypeId, contentTypeIdsA); // assume number of ctypes won't blow IN(...)

            long pageIndex = 0;
            long processed = 0;
            long total;
            do
            {
                // .GetPagedResultsByQuery implicitely adds (cmsDocument.newest = 1) which
                // is what we want for preview (ie latest version of a content, published or not)
                var descendants = repository.GetPagedResultsByQuery(query, pageIndex++, groupSize, out total, "Path", Direction.Ascending, true);
                var items = descendants.Select(c => new PreviewXmlDto
                {
                    NodeId = c.Id,
                    Xml = _xmlContentSerializer(c).ToDataString()
                }).ToArray();
                db.BulkInsertRecords(db.SqlSyntax, items);
                processed += items.Length;
            } while (processed < total);
        }

        public void RebuildMediaXml(int groupSize = 5000, IEnumerable<int> contentTypeIds = null)
        {
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                RebuildMediaXmlLocked(uow, repository, groupSize, contentTypeIds);
                uow.Complete();
            }
        }

        // assumes media tree lock
        public void RebuildMediaXmlLocked(IDatabaseUnitOfWork unitOfWork, IMediaRepository repository, int groupSize, IEnumerable<int> contentTypeIds)
        {
            var contentTypeIdsA = contentTypeIds?.ToArray();
            var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);
            var db = unitOfWork.Database;

            // remove all - if anything fails the transaction will rollback
            if (contentTypeIds == null || contentTypeIdsA.Length == 0)
            {
                // must support SQL-CE
                //                    db.Execute(@"DELETE cmsContentXml
                //FROM cmsContentXml
                //JOIN umbracoNode ON (cmsContentXml.nodeId=umbracoNode.Id)
                //WHERE umbracoNode.nodeObjectType=@objType",
                db.Execute(@"DELETE FROM cmsContentXml
WHERE cmsContentXml.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                    new { objType = mediaObjectType });
            }
            else
            {
                // assume number of ctypes won't blow IN(...)
                // must support SQL-CE
                //                    db.Execute(@"DELETE cmsContentXml
                //FROM cmsContentXml
                //JOIN umbracoNode ON (cmsContentXml.nodeId=umbracoNode.Id)
                //JOIN cmsContent ON (cmsContentXml.nodeId=cmsContent.nodeId)
                //WHERE umbracoNode.nodeObjectType=@objType
                //AND cmsContent.contentType IN (@ctypes)",
                db.Execute(@"DELETE FROM cmsContentXml
WHERE cmsContentXml.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN cmsContent ON cmsContent.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND cmsContent.contentType IN (@ctypes)
)",
                    new { objType = mediaObjectType, ctypes = contentTypeIdsA });
            }

            // insert back - if anything fails the transaction will rollback
            var query = repository.Query;
            if (contentTypeIds != null && contentTypeIdsA.Length > 0)
                query = query.WhereIn(x => x.ContentTypeId, contentTypeIdsA); // assume number of ctypes won't blow IN(...)

            long pageIndex = 0;
            long processed = 0;
            long total;
            do
            {
                var descendants = repository.GetPagedResultsByQuery(query, pageIndex++, groupSize, out total, "Path", Direction.Ascending, true);
                var items = descendants.Select(m => new ContentXmlDto { NodeId = m.Id, Xml = _xmlMediaSerializer(m).ToDataString() }).ToArray();
                db.BulkInsertRecords(db.SqlSyntax, items);
                processed += items.Length;
            } while (processed < total);
        }

        public void RebuildMemberXml(int groupSize = 5000, IEnumerable<int> contentTypeIds = null)
        {
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                RebuildMemberXmlLocked(uow, repository, groupSize, contentTypeIds);
                uow.Complete();
            }
        }

        // assumes member tree lock
        public void RebuildMemberXmlLocked(IDatabaseUnitOfWork unitOfWork, IMemberRepository repository, int groupSize, IEnumerable<int> contentTypeIds)
        {
            var contentTypeIdsA = contentTypeIds?.ToArray();
            var memberObjectType = Guid.Parse(Constants.ObjectTypes.Member);
            var db = unitOfWork.Database;

            // remove all - if anything fails the transaction will rollback
            if (contentTypeIds == null || contentTypeIdsA.Length == 0)
            {
                // must support SQL-CE
                //                    db.Execute(@"DELETE cmsContentXml
                //FROM cmsContentXml
                //JOIN umbracoNode ON (cmsContentXml.nodeId=umbracoNode.Id)
                //WHERE umbracoNode.nodeObjectType=@objType",
                db.Execute(@"DELETE FROM cmsContentXml
WHERE cmsContentXml.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                    new { objType = memberObjectType });
            }
            else
            {
                // assume number of ctypes won't blow IN(...)
                // must support SQL-CE
                //                    db.Execute(@"DELETE cmsContentXml
                //FROM cmsContentXml
                //JOIN umbracoNode ON (cmsContentXml.nodeId=umbracoNode.Id)
                //JOIN cmsContent ON (cmsContentXml.nodeId=cmsContent.nodeId)
                //WHERE umbracoNode.nodeObjectType=@objType
                //AND cmsContent.contentType IN (@ctypes)",
                db.Execute(@"DELETE FROM cmsContentXml
WHERE cmsContentXml.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN cmsContent ON cmsContent.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND cmsContent.contentType IN (@ctypes)
)",
                    new { objType = memberObjectType, ctypes = contentTypeIdsA });
            }

            // insert back - if anything fails the transaction will rollback
            var query = repository.Query;
            if (contentTypeIds != null && contentTypeIdsA.Length > 0)
                query = query.WhereIn(x => x.ContentTypeId, contentTypeIdsA); // assume number of ctypes won't blow IN(...)

            long pageIndex = 0;
            long processed = 0;
            long total;
            do
            {
                var descendants = repository.GetPagedResultsByQuery(query, pageIndex++, groupSize, out total, "Path", Direction.Ascending, true);
                var items = descendants.Select(m => new ContentXmlDto { NodeId = m.Id, Xml = _xmlMemberSerializer(m).ToDataString() }).ToArray();
                db.BulkInsertRecords(db.SqlSyntax, items);
                processed += items.Length;
            } while (processed < total);
        }

        public bool VerifyContentAndPreviewXml()
        {
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var ok = VerifyContentAndPreviewXmlLocked(uow, repository);
                uow.Complete();
                return ok;
            }
        }

        // assumes content tree lock
        private bool VerifyContentAndPreviewXmlLocked(IDatabaseUnitOfWork unitOfWork, IContentRepository repository)
        {
            // every published content item should have a corresponding row in cmsContentXml
            // every content item should have a corresponding row in cmsPreviewXml

            var contentObjectType = Guid.Parse(Constants.ObjectTypes.Document);
            var db = unitOfWork.Database;

            var count = db.ExecuteScalar<int>(@"SELECT COUNT(*)
FROM umbracoNode
JOIN cmsDocument ON (umbracoNode.id=cmsDocument.nodeId and cmsDocument.published=1)
LEFT JOIN cmsContentXml ON (umbracoNode.id=cmsContentXml.nodeId)
WHERE umbracoNode.nodeObjectType=@objType
AND cmsContentXml.nodeId IS NULL
", new { objType = contentObjectType });

            if (count > 0) return false;

            count = db.ExecuteScalar<int>(@"SELECT COUNT(*)
FROM umbracoNode
LEFT JOIN cmsPreviewXml ON (umbracoNode.id=cmsPreviewXml.nodeId)
WHERE umbracoNode.nodeObjectType=@objType
AND cmsPreviewXml.nodeId IS NULL
", new { objType = contentObjectType });

            return count == 0;
        }

        public bool VerifyMediaXml()
        {
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var ok = VerifyMediaXmlLocked(uow, repository);
                uow.Complete();
                return ok;
            }
        }

        // assumes media tree lock
        public bool VerifyMediaXmlLocked(IDatabaseUnitOfWork unitOfWork, IMediaRepository repository)
        {
            // every non-trashed media item should have a corresponding row in cmsContentXml

            var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);
            var db = unitOfWork.Database;

            var count = db.ExecuteScalar<int>(@"SELECT COUNT(*)
FROM umbracoNode
JOIN cmsDocument ON (umbracoNode.id=cmsDocument.nodeId and cmsDocument.published=1)
LEFT JOIN cmsContentXml ON (umbracoNode.id=cmsContentXml.nodeId)
WHERE umbracoNode.nodeObjectType=@objType
AND cmsContentXml.nodeId IS NULL
", new { objType = mediaObjectType });

            return count == 0;
        }

        public bool VerifyMemberXml()
        {
            using (var uow = _uowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var ok = VerifyMemberXmlLocked(uow, repository);
                uow.Complete();
                return ok;
            }
        }

        // assumes member tree lock
        public bool VerifyMemberXmlLocked(IDatabaseUnitOfWork unitOfWork, IMemberRepository repository)
        {
            // every member item should have a corresponding row in cmsContentXml

            var memberObjectType = Guid.Parse(Constants.ObjectTypes.Member);
            var db = unitOfWork.Database;

            var count = db.ExecuteScalar<int>(@"SELECT COUNT(*)
FROM umbracoNode
LEFT JOIN cmsContentXml ON (umbracoNode.id=cmsContentXml.nodeId)
WHERE umbracoNode.nodeObjectType=@objType
AND cmsContentXml.nodeId IS NULL
", new { objType = memberObjectType });

            return count == 0;
        }

        #endregion
    }
}
