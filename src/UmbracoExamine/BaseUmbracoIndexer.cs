using System;
using System.Collections.Generic;
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
        public const string NodeTypeAliasFieldName = "__NodeTypeAlias";
        public const string IconFieldName = "__Icon";
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

        private bool _configBased = false;
        private readonly LocalTempStorageIndexer _localTempStorageIndexer = new LocalTempStorageIndexer();

        /// <summary>
        /// A type that defines the type of index for each Umbraco field (non user defined fields)
        /// Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
        /// for retreival after searching.
        /// </summary>
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

        public bool UseTempStorage
        {
            get { return _localTempStorageIndexer.LuceneDirectory != null; }
        }

        public string TempStorageLocation
        {
            get
            {
                if (UseTempStorage == false) return string.Empty;
                return _localTempStorageIndexer.TempPath;
            }
        }

        /// <summary>
        /// If true, the IndexingActionHandler will be run to keep the default index up to date.
        /// </summary>
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
      
        
        public override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            //if temp local storage is configured use that, otherwise return the default
            if (UseTempStorage)
            {
                return _localTempStorageIndexer.LuceneDirectory;
            }

            return base.GetLuceneDirectory();

        }        

        ///// <summary>
        ///// Override to check if we can actually initialize.
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>
        ///// This check is required since the base examine lib will try to check this method on app startup. If the app
        ///// is not ready then we need to deal with it otherwise the base class will throw exceptions since we've bypassed initialization.
        ///// </remarks>
        //public override bool IndexExists()
        //{
        //    return base.IndexExists();
        //}

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

        public override void ReIndexNode(XElement node, string type)
        {
            if (CanInitialize())
            {
                if (!SupportedTypes.Contains(type))
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

                base.ReIndexNode(node, type);
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

        #region Protected

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
            if (_configBased)
            {
                //We need to check if we actually can initialize, if not then don't continue
                if (ApplicationContext.Current == null
                    || ApplicationContext.Current.IsConfigured == false
                    || ApplicationContext.Current.DatabaseContext.IsDatabaseConfigured == false)
                {
                    return false;
                }
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
        
        ///// <summary>
        ///// Builds an xpath statement to query against Umbraco data for the index type specified, then
        ///// initiates the re-indexing of the data matched.
        ///// </summary>
        ///// <param name="type"></param>
        //protected override void PerformIndexAll(string type)
        //{
        //    //NOTE: the logic below is ONLY used for published content, for media and members and non-published content, this method is overridden
        //    // and we query directly against the umbraco service layer.

        //    if (SupportedTypes.Contains(type) == false)
        //        return;

        //    var xPath = "//*[(number(@id) > 0 and (@isDoc or @nodeTypeAlias)){0}]"; //we'll add more filters to this below if needed

        //    var sb = new StringBuilder();

        //    //create the xpath statement to match node type aliases if specified
        //    if (IndexerData.IncludeNodeTypes.Any())
        //    {
        //        sb.Append("(");
        //        foreach (var field in IndexerData.IncludeNodeTypes)
        //        {
        //            //this can be used across both schemas
        //            const string nodeTypeAlias = "(@nodeTypeAlias='{0}' or (count(@nodeTypeAlias)=0 and name()='{0}'))";

        //            sb.Append(string.Format(nodeTypeAlias, field));
        //            sb.Append(" or ");
        //        }
        //        sb.Remove(sb.Length - 4, 4); //remove last " or "
        //        sb.Append(")");
        //    }

        //    //create the xpath statement to match all children of the current node.
        //    if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
        //    {
        //        if (sb.Length > 0)
        //            sb.Append(" and ");
        //        sb.Append("(");
        //        sb.Append("contains(@path, '," + IndexerData.ParentNodeId.Value + ",')"); //if the path contains comma - id - comma then the nodes must be a child
        //        sb.Append(")");
        //    }

        //    //create the full xpath statement to match the appropriate nodes. If there is a filter
        //    //then apply it, otherwise just select all nodes.
        //    var filter = sb.ToString();
        //    xPath = string.Format(xPath, filter.Length > 0 ? " and " + filter : "");

        //    //raise the event and set the xpath statement to the value returned
        //    var args = new IndexingNodesEventArgs(IndexerData, xPath, type);
        //    OnNodesIndexing(args);
        //    if (args.Cancel)
        //    {
        //        return;
        //    }

        //    xPath = args.XPath;

        //    ProfilingLogger.Logger.Debug(GetType(), "({0}) PerformIndexAll with XPATH: {1}", () => Name, () => xPath);
            
        //    AddNodesToIndex(xPath, type);
        //}

        #endregion

        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            ProfilingLogger.Logger.Error(GetType(), e.Message, e.Exception);

            base.OnIndexingError(e);
        }

        /// <summary>
        /// This ensures that the special __Raw_ fields are indexed
        /// </summary>
        /// <param name="docArgs"></param>

        protected override void OnDocumentWriting(DocumentWritingEventArgs docArgs)
        {
            var d = docArgs.Document;
            foreach (var f in docArgs.Fields.Where(x => x.Key.StartsWith(RawFieldPrefix)))
            {
                d.Add(new Field(
                    f.Key,
                    f.Value,
                    Field.Store.YES,
                    Field.Index.NO, //don't index this field, we never want to search by it 
                    Field.TermVector.NO));
            }

            base.OnDocumentWriting(docArgs);
        }

        protected override void OnNodeIndexed(IndexedNodeEventArgs e)
        {
            ProfilingLogger.Logger.Debug(GetType(), "Index created for node {0}", () => e.NodeId);
            base.OnNodeIndexed(e);
        }

        protected override void OnIndexDeleted(DeleteIndexEventArgs e)
        {
            ProfilingLogger.Logger.Debug(GetType(), "Index deleted for term: {0} with value {1}", () => e.DeletedTerm.Key, () => e.DeletedTerm.Value);
            base.OnIndexDeleted(e);
        }

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

        /// <summary>
        /// Overridden for logging.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        protected override void AddSingleNodeToIndex(XElement node, string type)
        {
            ProfilingLogger.Logger.Debug(GetType(), "AddSingleNodeToIndex {0} with type {1}", () => (int)node.Attribute("id"), () => type);
            base.AddSingleNodeToIndex(node, type);
        }

        protected override void OnTransformingIndexValues(TransformingIndexDataEventArgs e)
        {
            base.OnTransformingIndexValues(e);

            if (e.OriginalValues.ContainsKey("path"))
            {
                e.IndexItem.ValueSet.Values[IndexPathFieldName] = new List<object> { e.OriginalValues["path"].First() };
            }           

            ////adds the special node type alias property to the index
            //fields.Add(NodeTypeAliasFieldName, allValuesForIndexing[NodeTypeAliasFieldName]);

            ////icon
            //if (allValuesForIndexing[IconFieldName].IsNullOrWhiteSpace() == false)
            //{
            //    fields.Add(IconFieldName, allValuesForIndexing[IconFieldName]);
            //}

            //return fields;
        }

        /// <summary>
        /// Override this method to strip all html from all user fields before raising the event, then after the event 
        /// ensure our special Path field is added to the collection
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {
            //strip html of all users fields if we detect it has HTML in it. 
            //if that is the case, we'll create a duplicate 'raw' copy of it so that we can return
            //the value of the field 'as-is'.
            // Get all user data that we want to index and store into a dictionary 
            foreach (var field in IndexerData.UserFields)
            {
                if (e.Fields.ContainsKey(field.Name))
                {
                    //check if the field value has html
                    if (XmlHelper.CouldItBeXml(e.Fields[field.Name]))
                    {
                        //First save the raw value to a raw field, we will change the policy of this field by detecting the prefix later
                        e.Fields[RawFieldPrefix + field.Name] = e.Fields[field.Name];
                        //now replace the original value with the stripped html
                        //TODO: This should be done with an analzer?!
                        e.Fields[field.Name] = e.Fields[field.Name].StripHtml();
                    }
                }
            }

            base.OnGatheringNodeData(e);

            //ensure the special path and node type alias fields is added to the dictionary to be saved to file
            var path = e.Node.Attribute("path").Value;
            if (!e.Fields.ContainsKey(IndexPathFieldName))
                e.Fields.Add(IndexPathFieldName, path);

            //this needs to support both schema's so get the nodeTypeAlias if it exists, otherwise the name
            var nodeTypeAlias = e.Node.Attribute("nodeTypeAlias") == null ? e.Node.Name.LocalName : e.Node.Attribute("nodeTypeAlias").Value;
            if (!e.Fields.ContainsKey(NodeTypeAliasFieldName))
                e.Fields.Add(NodeTypeAliasFieldName, nodeTypeAlias);

            //add icon 
            var icon = (string)e.Node.Attribute("icon");
            if (!e.Fields.ContainsKey(IconFieldName))
                e.Fields.Add(IconFieldName, icon);  
            
        }

        /// <summary>
        /// Called when a duplicate field is detected in the dictionary that is getting indexed.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="indexSetName"></param>
        /// <param name="fieldName"></param>
        protected override void OnDuplicateFieldWarning(int nodeId, string indexSetName, string fieldName)
        {
            base.OnDuplicateFieldWarning(nodeId, indexSetName, fieldName);

            ProfilingLogger.Logger.Debug(
                GetType(),
                "Field \"{0}\" is listed multiple times in the index set \"{1}\". Please ensure all names are unique. Node id {2}",
                () => fieldName, () => indexSetName, () => nodeId);
        }

        /// <summary>
        /// return the index policy for the field name passed in, if not found, return normal
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected override FieldIndexTypes GetPolicy(string fieldName)
        {
            var def = IndexFieldPolicies.Where(x => x.Name == fieldName).ToArray();
            return (def.Any() == false ? FieldIndexTypes.ANALYZED : def.Single().IndexType);
        }
    }
}
