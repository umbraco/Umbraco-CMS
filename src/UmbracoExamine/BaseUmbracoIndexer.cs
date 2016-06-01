using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Umbraco.Core;
using Examine;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Umbraco.Core.Logging;
using Umbraco.Core.Xml;
using UmbracoExamine.LocalStorage;
using Directory = Lucene.Net.Store.Directory;

namespace UmbracoExamine
{
    /// <summary>
    /// An abstract provider containing the basic functionality to be able to query against
    /// Umbraco data.
    /// </summary>
    public abstract class BaseUmbracoIndexer : LuceneIndexer
    {
        /// <summary>
        /// Used to store the path of a content object
        /// </summary>
        public const string IndexPathFieldName = "__Path";        
        public const string IconFieldName = "__Icon";
        public const string PublishedFieldName = "__Published";
        /// <summary>
        /// The prefix added to a field when it is duplicated in order to store the original raw value.
        /// </summary>
        public const string RawFieldPrefix = "__Raw_";

        /// <summary>
        /// Default constructor
        /// </summary>
        protected BaseUmbracoIndexer()
            : base()
        {
            ProfilingLogger = ApplicationContext.Current.ProfilingLogger;
            _configBased = true;
        }

        protected BaseUmbracoIndexer(
            IEnumerable<FieldDefinition> fieldDefinitions,
            Directory luceneDirectory,
            Analyzer defaultAnalyzer,
            ProfilingLogger profilingLogger,
            IValueSetValidator validator = null,
            FacetConfiguration facetConfiguration = null, IDictionary<string, Func<string, IIndexValueType>> indexValueTypes = null)
            : base(fieldDefinitions, luceneDirectory, defaultAnalyzer, validator, facetConfiguration, indexValueTypes)
        {
            if (profilingLogger == null) throw new ArgumentNullException("profilingLogger");
            ProfilingLogger = profilingLogger;
        }

        private readonly bool _configBased = false;
        private LocalTempStorageIndexer _localTempStorageIndexer;

        /// <summary>
        /// A type that defines the type of index for each Umbraco field (non user defined fields)
        /// Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
        /// for retreival after searching.
        /// </summary>
        [Obsolete("IndexFieldPolicies is not really used apart for some legacy reasons - use FieldDefinition's instead")]
        internal static readonly List<StaticField> IndexFieldPolicies
            = new List<StaticField>
            {
                new StaticField("id", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField("key", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "version", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "parentID", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "level", FieldIndexTypes.NOT_ANALYZED, true, "NUMBER"),
                new StaticField( "writerID", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "creatorID", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "nodeType", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "template", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "sortOrder", FieldIndexTypes.NOT_ANALYZED, true, "NUMBER"),
                new StaticField( "createDate", FieldIndexTypes.NOT_ANALYZED, false, "DATETIME"),
                new StaticField( "updateDate", FieldIndexTypes.NOT_ANALYZED, false, "DATETIME"),
                new StaticField( "nodeName", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField( "urlName", FieldIndexTypes.NOT_ANALYZED, false, string.Empty),
                new StaticField( "writerName", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField( "creatorName", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField( "nodeTypeAlias", FieldIndexTypes.ANALYZED, false, string.Empty),
                new StaticField( "path", FieldIndexTypes.NOT_ANALYZED, false, string.Empty)
            };
        
        protected ProfilingLogger ProfilingLogger { get; private set; }

        /// <summary>
        /// Overridden to ensure that the umbraco system field definitions are in place
        /// </summary>
        /// <param name="originalDefinitions"></param>
        /// <returns></returns>
        protected override IEnumerable<FieldDefinition> InitializeFieldDefinitions(IEnumerable<FieldDefinition> originalDefinitions)
        {
            var fd = base.InitializeFieldDefinitions(originalDefinitions).ToList();
            fd.AddRange(new[]
            {
                new FieldDefinition("parentID", FieldDefinitionTypes.Integer),
                new FieldDefinition("level", FieldDefinitionTypes.Integer),
                new FieldDefinition("writerID", FieldDefinitionTypes.Integer),
                new FieldDefinition("creatorID", FieldDefinitionTypes.Integer),
                new FieldDefinition("sortOrder", FieldDefinitionTypes.Integer),
                new FieldDefinition("template", FieldDefinitionTypes.Integer),

                new FieldDefinition("createDate", FieldDefinitionTypes.DateTime),
                new FieldDefinition("updateDate", FieldDefinitionTypes.DateTime),

                new FieldDefinition("key", FieldDefinitionTypes.Raw),
                new FieldDefinition("version", FieldDefinitionTypes.Raw),
                new FieldDefinition("nodeType", FieldDefinitionTypes.Raw),
                new FieldDefinition("template", FieldDefinitionTypes.Raw),
                new FieldDefinition("urlName", FieldDefinitionTypes.Raw),
                new FieldDefinition("path", FieldDefinitionTypes.Raw),

                new FieldDefinition(IndexPathFieldName, FieldDefinitionTypes.Raw),
                new FieldDefinition(NodeTypeAliasFieldName, FieldDefinitionTypes.Raw),
                new FieldDefinition(IconFieldName, FieldDefinitionTypes.Raw)
            });
            return fd;
        }

        public bool UseTempStorage
        {
            get { return _localTempStorageIndexer != null && _localTempStorageIndexer.LuceneDirectory != null; }
        }

        public string TempStorageLocation
        {
            get
            {
                if (UseTempStorage == false) return string.Empty;
                return _localTempStorageIndexer.TempPath;
            }
        }

        [Obsolete("This should not be used, it is used by the configuration based indexes but instead to disable Examine event handlers use the ExamineEvents class instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool EnableDefaultEventHandler { get; protected set; }
        
        /// <summary>
        /// the supported indexable types
        /// </summary>
        protected abstract IEnumerable<string> SupportedTypes { get; }        

        #region Initialize


        /// <summary>
        /// Setup the properties for the indexer from the provider settings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <remarks>
        /// This is ONLY used for configuration based indexes
        /// </remarks> 
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {                        
            EnableDefaultEventHandler = true; //set to true by default
            bool enabled;
            if (bool.TryParse(config["enableDefaultEventHandler"], out enabled))
            {
                EnableDefaultEventHandler = enabled;
            }

            ProfilingLogger.Logger.Debug(GetType(), "{0} indexer initializing", () => name); 

            base.Initialize(name, config);

            if (config["useTempStorage"] != null)
            {
                throw new NotImplementedException("Fix how local temp storage works and is synced with Examine v2.0 - since a writer is always open we cannot snapshot it, we need to use the same logic in AzureDirectory");

                _localTempStorageIndexer = new LocalTempStorageIndexer();

                var fsDir = base.GetLuceneDirectory() as FSDirectory;
                if (fsDir != null)
                {
                    //Use the temp storage directory which will store the index in the local/codegen folder, this is useful
                    // for websites that are running from a remove file server and file IO latency becomes an issue
                    var attemptUseTempStorage = config["useTempStorage"].TryConvertTo<LocalStorageType>();
                    if (attemptUseTempStorage)
                    {

                        var indexSet = IndexSets.Instance.Sets[IndexSetName];
                        var configuredPath = indexSet.IndexPath;

                        _localTempStorageIndexer.Initialize(config, configuredPath, fsDir, IndexingAnalyzer, attemptUseTempStorage.Result);
                    }
                }
               
            }
        }

        #endregion
      
        
        public override Directory GetLuceneDirectory()
        {
            //if temp local storage is configured use that, otherwise return the default
            if (UseTempStorage)
            {
                return _localTempStorageIndexer.LuceneDirectory;
            }

            return base.GetLuceneDirectory();

        }        

        /// <summary>
        /// override to check if we can actually initialize. 
        /// </summary>
        /// <remarks>
        /// This check is required since the base examine lib will try to rebuild on startup
        /// </remarks>
        public override void RebuildIndex()
        {
            if (CanInitialize())
            {
                ProfilingLogger.Logger.Debug(GetType(), "Rebuilding index");
                base.RebuildIndex();
            }
        }

        /// <summary>
        /// override to check if we can actually initialize. 
        /// </summary>
        /// <remarks>
        /// This check is required since the base examine lib will try to rebuild on startup
        /// </remarks>
        public override void IndexAll(string type)
        {
            if (CanInitialize())
            {
                base.IndexAll(type);
            }
        }
       
        public override void IndexItems(IEnumerable<ValueSet> nodes)
        {
            if (CanInitialize())
            {               
                base.IndexItems(nodes);
            }
        }

        [Obsolete("Use ValueSets with IndexItems instead")]
        public override void ReIndexNode(XElement node, string type)
        {
            if (CanInitialize())
            {
                if (SupportedTypes.Contains(type) == false)
                    return;

                if (node.Attribute("id") != null)
                {
                    ProfilingLogger.Logger.Debug(GetType(), "ReIndexNode {0} with type {1}", () => node.Attribute("id"), () => type);
                    base.ReIndexNode(node, type);
                }
                else
                {
                    ProfilingLogger.Logger.Error(GetType(), "ReIndexNode cannot proceed, the format of the XElement is invalid",
                        new XmlException("XElement is invalid, the xml has not id attribute"));
                }
            }
        }

        /// <summary>
        /// override to check if we can actually initialize. 
        /// </summary>
        /// <remarks>
        /// This check is required since the base examine lib will try to rebuild on startup
        /// </remarks>
        public override void DeleteFromIndex(string nodeId)
        {
            if (CanInitialize())
            {
                base.DeleteFromIndex(nodeId);
            }            
        }

        /// <summary>
        /// Returns true if the Umbraco application is in a state that we can initialize the examine indexes
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This only affects indexers that are config file based, if an index was created via code then 
        /// this has no affect, it is assumed the index would not be created if it could not be initialized.
        /// </remarks>
        protected bool CanInitialize()
        {
            //We need to check if we actually can initialize, if not then don't continue
            if (_configBased
                && (ApplicationContext.Current == null
                || ApplicationContext.Current.IsConfigured == false
                || ApplicationContext.Current.DatabaseContext.IsDatabaseConfigured == false))
            {
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Reindexes all supported types
        /// </summary>
        protected override void PerformIndexRebuild()
        {
            foreach (var t in SupportedTypes)
            {
                IndexAll(t);
            }
        }
      
        /// <summary>
        /// overridden for logging
        /// </summary>
        /// <param name="e"></param>
        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            ProfilingLogger.Logger.Error(GetType(), e.Message, e.Exception);
            base.OnIndexingError(e);
        }

        /// <summary>
        /// Override for logging
        /// </summary>
        /// <param name="e"></param>
        protected override void OnIgnoringIndexItem(IndexItemEventArgs e)
        {
            ProfilingLogger.Logger.Debug(GetType(), "OnIgnoringIndexItem {0} with type {1}", () => e.IndexItem.ValueSet.Id, () => e.IndexItem.ValueSet.IndexCategory);
            base.OnIgnoringIndexItem(e);
        }

        /// <summary>
        /// This ensures that the special __Raw_ fields are indexed
        /// </summary>
        /// <param name="docArgs"></param>
        protected override void OnDocumentWriting(DocumentWritingEventArgs docArgs)
        {
            var d = docArgs.Document;

            foreach (var f in docArgs.Values.Values.Where(x => x.Key.StartsWith(RawFieldPrefix)))
            {
                if (f.Value.Count > 0)
                {
                    d.Add(new Field(
                        f.Key,
                        f.Value[0].ToString(),
                        Field.Store.YES,
                        Field.Index.NO, //don't index this field, we never want to search by it 
                        Field.TermVector.NO));
                }
            }

            ProfilingLogger.Logger.Debug(GetType(), "OnDocumentWriting {0} with type {1}", () => docArgs.Values.Id, () => docArgs.Values.ItemType);

            base.OnDocumentWriting(docArgs);
        }

        protected override void OnItemIndexed(IndexItemEventArgs e)
        {
            ProfilingLogger.Logger.Debug(GetType(), "Index created for node {0}", () => e.IndexItem.Id);
            base.OnItemIndexed(e);
        }

        protected override void OnIndexDeleted(DeleteIndexEventArgs e)
        {
            ProfilingLogger.Logger.Debug(GetType(), "Index deleted for term: {0} with value {1}", () => e.DeletedTerm.Key, () => e.DeletedTerm.Value);
            base.OnIndexDeleted(e);
        }

        [Obsolete("This is no longer used, index optimization is no longer managed with the LuceneIndexer")]
        protected override void OnIndexOptimizing(EventArgs e)
        {
            ProfilingLogger.Logger.Debug(GetType(), "Index is being optimized");
            base.OnIndexOptimizing(e);
        }

        /// <summary>
        /// Overridden for logging.
        /// </summary>
        /// <param name="values"></param>
        protected override void AddDocument(ValueSet values)
        {
            ProfilingLogger.Logger.Debug(GetType(), "AddDocument {0} with type {1}", () => values.Id, () => values.ItemType);
            base.AddDocument(values);
        }

        protected override void OnTransformingIndexValues(TransformingIndexDataEventArgs e)
        {
            base.OnTransformingIndexValues(e);

            //ensure special __Path field
            if (e.OriginalValues.ContainsKey("path") && e.IndexItem.ValueSet.Values.ContainsKey(IndexPathFieldName) == false)
            {
                e.IndexItem.ValueSet.Values[IndexPathFieldName] = new List<object> { e.OriginalValues["path"].First() };
            }
            
            //strip html of all users fields if we detect it has HTML in it. 
            //if that is the case, we'll create a duplicate 'raw' copy of it so that we can return
            //the value of the field 'as-is'.
            foreach (var originalValue in e.OriginalValues)
            {
                if (originalValue.Value.Any())
                {
                    var str = originalValue.Value.First() as string;
                    if (str != null)
                    {
                        if (XmlHelper.CouldItBeXml(str))
                        {
                            //First save the raw value to a raw field, we will change the policy of this field by detecting the prefix later
                            e.IndexItem.ValueSet.Values[string.Concat(RawFieldPrefix, originalValue.Key)] = new List<object> { str };
                            //now replace the original value with the stripped html
                            //TODO: This should be done with an analzer?!
                            e.IndexItem.ValueSet.Values[originalValue.Key] = new List<object> { str.StripHtml() };
                        }
                    }
                }                
            }

            //icon
            if (e.OriginalValues.ContainsKey("icon") && e.IndexItem.ValueSet.Values.ContainsKey(IconFieldName) == false)
            {
                e.IndexItem.ValueSet.Values[IconFieldName] = new List<object> { e.OriginalValues["icon"] };
            }
            
        }

    }
}
