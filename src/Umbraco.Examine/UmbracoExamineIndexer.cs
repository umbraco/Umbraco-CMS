using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Umbraco.Core;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Indexing;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Xml;
using Umbraco.Examine.Config;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Examine
{
    public class HtmlValueType : IndexValueTypeBase
    {
        public HtmlValueType(string fieldName, bool store = true) : base(fieldName, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            //TODO: Make this happen so we can properly analyze/tokenize html, maybe we only need an analyzer though

            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An abstract provider containing the basic functionality to be able to query against
    /// Umbraco data.
    /// </summary>
    public abstract class UmbracoExamineIndexer : LuceneIndexer
    {
        // note
        // wrapping all operations that end up calling base.SafelyProcessQueueItems in a safe call
        // context because they will fork a thread/task/whatever which should *not* capture our
        // call context (and the database it can contain)! ideally we should be able to override
        // SafelyProcessQueueItems but that's not possible in the current version of Examine.

        /// <summary>
        /// Used to store the path of a content object
        /// </summary>
        public const string IndexPathFieldName = SpecialFieldPrefix + "Path";
        public const string NodeKeyFieldName = SpecialFieldPrefix + "Key";
        public const string IconFieldName = SpecialFieldPrefix + "Icon";
        public const string PublishedFieldName = SpecialFieldPrefix + "Published";

        /// <summary>
        /// The prefix added to a field when it is duplicated in order to store the original raw value.
        /// </summary>
        public const string RawFieldPrefix = SpecialFieldPrefix + "Raw_";

        /// <summary>
        /// Constructor for config provider based indexes
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected UmbracoExamineIndexer()
            : base()
        {
            ProfilingLogger = Current.ProfilingLogger;
            _configBased = true;

            //This is using the config so we'll validate based on that
            ValueSetValidator = new ValueSetValidatorDelegate(set =>
            {

                //check if this document is of a correct type of node type alias
                if (ConfigIndexCriteria.IncludeItemTypes.Any())
                    if (!ConfigIndexCriteria.IncludeItemTypes.Contains(set.ItemType))
                        return false;

                //if this node type is part of our exclusion list, do not validate
                if (ConfigIndexCriteria.ExcludeItemTypes.Any())
                    if (ConfigIndexCriteria.ExcludeItemTypes.Contains(set.ItemType))
                        return false;

                return true;
            });
        }

        /// <summary>
        /// Create a new <see cref="UmbracoExamineIndexer"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="defaultAnalyzer"></param>
        /// <param name="profilingLogger"></param>
        /// <param name="validator"></param>
        /// <param name="indexValueTypes"></param>
        protected UmbracoExamineIndexer(
            string name,
            IEnumerable<FieldDefinition> fieldDefinitions,
            Directory luceneDirectory,
            Analyzer defaultAnalyzer,
            ProfilingLogger profilingLogger,
            IValueSetValidator validator = null,
            IReadOnlyDictionary<string, Func<string, IIndexValueType>> indexValueTypes = null)
            : base(name, fieldDefinitions, luceneDirectory, defaultAnalyzer, validator, indexValueTypes)
        {
            ProfilingLogger = profilingLogger ?? throw new ArgumentNullException(nameof(profilingLogger));
        }

        private readonly bool _configBased = false;

        /// <summary>
        /// A type that defines the type of index for each Umbraco field (non user defined fields)
        /// Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
        /// for retreival after searching.
        /// </summary>
        public static readonly FieldDefinition[] UmbracoIndexFieldDefinitions =
        {
            new FieldDefinition("parentID", FieldDefinitionTypes.Integer),
            new FieldDefinition("level", FieldDefinitionTypes.Integer),
            new FieldDefinition("writerID", FieldDefinitionTypes.Integer),
            new FieldDefinition("creatorID", FieldDefinitionTypes.Integer),
            new FieldDefinition("sortOrder", FieldDefinitionTypes.Integer),
            new FieldDefinition("template", FieldDefinitionTypes.Integer),

            new FieldDefinition("createDate", FieldDefinitionTypes.DateTime),
            new FieldDefinition("updateDate", FieldDefinitionTypes.DateTime),

            new FieldDefinition("key", FieldDefinitionTypes.InvariantCultureIgnoreCase),
            new FieldDefinition("version", FieldDefinitionTypes.Raw),
            new FieldDefinition("nodeType", FieldDefinitionTypes.InvariantCultureIgnoreCase),
            new FieldDefinition("template", FieldDefinitionTypes.Raw),
            new FieldDefinition("urlName", FieldDefinitionTypes.InvariantCultureIgnoreCase),
            new FieldDefinition("path", FieldDefinitionTypes.Raw),

            new FieldDefinition("email", FieldDefinitionTypes.EmailAddress),

            new FieldDefinition(PublishedFieldName, FieldDefinitionTypes.Raw),
            new FieldDefinition(NodeKeyFieldName, FieldDefinitionTypes.Raw),
            new FieldDefinition(IndexPathFieldName, FieldDefinitionTypes.Raw),
            new FieldDefinition(IconFieldName, FieldDefinitionTypes.Raw)
        };

        protected ProfilingLogger ProfilingLogger { get; }

        /// <summary>
        /// Overridden to ensure that the umbraco system field definitions are in place
        /// </summary>
        /// <param name="x"></param>
        /// <param name="indexValueTypesFactory"></param>
        /// <returns></returns>
        protected override FieldValueTypeCollection CreateFieldValueTypes(IReadOnlyDictionary<string, Func<string, IIndexValueType>> indexValueTypesFactory = null)
        {
            //if config based then ensure the value types else it's assumed these were passed in via ctor
            if (_configBased)
            {
                foreach (var field in UmbracoIndexFieldDefinitions)
                {
                    FieldDefinitionCollection.TryAdd(field.Name, field);
                }
            }
            

            return base.CreateFieldValueTypes(indexValueTypesFactory);
        }

        /// <summary>
        /// When set to true Umbraco will keep the index in sync with Umbraco data automatically
        /// </summary>
        public bool EnableDefaultEventHandler { get; set; } = true;

        /// <summary>
        /// the supported indexable types
        /// </summary>
        protected abstract IEnumerable<string> SupportedTypes { get; }

        protected ConfigIndexCriteria ConfigIndexCriteria { get; private set; }

        /// <summary>
        /// The index set name which references an Examine <see cref="IndexSet"/>
        /// </summary>
        public string IndexSetName { get; private set; }

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
            ProfilingLogger.Logger.Debug(GetType(), "{IndexerName} indexer initializing", name);

            if (config["enableDefaultEventHandler"] != null && bool.TryParse(config["enableDefaultEventHandler"], out var enabled))
            {
                EnableDefaultEventHandler = enabled;
            }

            //Need to check if the index set or IndexerData is specified...
            if (config["indexSet"] == null && (FieldDefinitionCollection.Count == 0))
            {
                //if we don't have either, then we'll try to set the index set by naming conventions
                var found = false;
                if (name.EndsWith("Indexer"))
                {
                    var setNameByConvension = name.Remove(name.LastIndexOf("Indexer")) + "IndexSet";
                    //check if we can assign the index set by naming convention
                    var set = IndexSets.Instance.Sets.Cast<IndexSet>().SingleOrDefault(x => x.SetName == setNameByConvension);

                    if (set != null)
                    {
                        //we've found an index set by naming conventions :)
                        IndexSetName = set.SetName;

                        var indexSet = IndexSets.Instance.Sets[IndexSetName];

                        //if tokens are declared in the path, then use them (i.e. {machinename} )
                        indexSet.ReplaceTokensInIndexPath();

                        //get the index criteria and ensure folder
                        ConfigIndexCriteria = CreateFieldDefinitionsFromConfig(indexSet);
                        foreach (var fieldDefinition in ConfigIndexCriteria.StandardFields.Union(ConfigIndexCriteria.UserFields))
                        {
                            FieldDefinitionCollection.TryAdd(fieldDefinition.Name, fieldDefinition);
                        }

                        //now set the index folder
                        LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));

                        found = true;
                    }
                }

                if (!found)
                    throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration and/or the IndexerData property has not been explicitly set");

            }
            else if (config["indexSet"] != null)
            {
                //if an index set is specified, ensure it exists and initialize the indexer based on the set

                if (IndexSets.Instance.Sets[config["indexSet"]] == null)
                {
                    throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");
                }
                else
                {
                    IndexSetName = config["indexSet"];

                    var indexSet = IndexSets.Instance.Sets[IndexSetName];

                    //if tokens are declared in the path, then use them (i.e. {machinename} )
                    indexSet.ReplaceTokensInIndexPath();

                    //get the index criteria and ensure folder
                    ConfigIndexCriteria = CreateFieldDefinitionsFromConfig(indexSet);
                    foreach (var fieldDefinition in ConfigIndexCriteria.StandardFields.Union(ConfigIndexCriteria.UserFields))
                    {
                        FieldDefinitionCollection.TryAdd(fieldDefinition.Name, fieldDefinition);
                    }

                    //now set the index folder
                    LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
                }
            }

            base.Initialize(name, config);
        }

        #endregion


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
                using (new SafeCallContext())
                {
                    base.RebuildIndex();
                }
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
                using (new SafeCallContext())
                {
                    base.IndexAll(type);
                }
            }
        }

        public override void IndexItems(IEnumerable<ValueSet> nodes)
        {
            if (CanInitialize())
            {
                using (new SafeCallContext())
                {
                    base.IndexItems(nodes);
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
                using (new SafeCallContext())
                {
                    base.DeleteFromIndex(nodeId);
                }
            }
        }

        /// <summary>
        /// Returns true if the Umbraco application is in a state that we can initialize the examine indexes
        /// </summary>
        /// <returns></returns>
        protected bool CanInitialize()
        {
            // only affects indexers that are config file based, if an index was created via code then
            // this has no effect, it is assumed the index would not be created if it could not be initialized
            return _configBased == false || Current.RuntimeState.Level == RuntimeLevel.Run;
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
        /// <param name="ex"></param>
        protected override void OnIndexingError(IndexingErrorEventArgs ex)
        {
            ProfilingLogger.Logger.Error(GetType(), ex.InnerException, ex.Message);
            base.OnIndexingError(ex);
        }

        /// <summary>
        /// This ensures that the special __Raw_ fields are indexed correctly
        /// </summary>
        /// <param name="docArgs"></param>
        protected override void OnDocumentWriting(DocumentWritingEventArgs docArgs)
        {
            var d = docArgs.Document;

            foreach (var f in docArgs.ValueSet.Values.Where(x => x.Key.StartsWith(RawFieldPrefix)).ToList())
            {
                if (f.Value.Count > 0)
                {
                    //remove the original value so we can store it the correct way
                    d.RemoveField(f.Key);

                    d.Add(new Field(
                        f.Key,
                        f.Value[0].ToString(),
                        Field.Store.YES,
                        Field.Index.NO, //don't index this field, we never want to search by it
                        Field.TermVector.NO));
                }
            }

            base.OnDocumentWriting(docArgs);
        }

        /// <summary>
        /// Overridden for logging.
        /// </summary>
        protected override void AddDocument(Document doc, IndexItem item, IndexWriter writer)
        {
            ProfilingLogger.Logger.Debug(GetType(),
                "Write lucene doc id:{DocumentId}, category:{DocumentCategory}, type:{DocumentItemType}",
                item.ValueSet.Id,
                item.ValueSet.Category,
                item.ValueSet.ItemType);

            base.AddDocument(doc, item, writer);
        }

        protected override void OnTransformingIndexValues(IndexingItemEventArgs e)
        {
            base.OnTransformingIndexValues(e);

            //ensure special __Path field
            var path = e.IndexItem.ValueSet.GetValue("path");
            if (path != null)
            {
                e.IndexItem.ValueSet.Set(IndexPathFieldName, path);
            }

            //strip html of all users fields if we detect it has HTML in it.
            //if that is the case, we'll create a duplicate 'raw' copy of it so that we can return
            //the value of the field 'as-is'.
            foreach (var value in e.IndexItem.ValueSet.Values.ToList()) //ToList here to make a diff collection else we'll get collection modified errors
            {
                if (value.Value == null) continue;

                if (value.Value.Count > 0)
                {
                    if (value.Value.First() is string str)
                    {
                        if (XmlHelper.CouldItBeXml(str))
                        {
                            //First save the raw value to a raw field, we will change the policy of this field by detecting the prefix later
                            e.IndexItem.ValueSet.Values[string.Concat(RawFieldPrefix, value.Key)] = new List<object> { str };

                            //now replace the original value with the stripped html
                            //TODO: This should be done with an analzer?!
                            e.IndexItem.ValueSet.Values[value.Key] = new List<object> { str.StripHtml() };
                        }
                    }
                }
            }

            //icon
            if (e.IndexItem.ValueSet.Values.TryGetValue("icon", out var icon) && e.IndexItem.ValueSet.Values.ContainsKey(IconFieldName) == false)
            {
                e.IndexItem.ValueSet.Values[IconFieldName] = icon;
            }
        }

        private ConfigIndexCriteria CreateFieldDefinitionsFromConfig(IndexSet indexSet)
        {
            return new ConfigIndexCriteria(
                indexSet.IndexAttributeFields.Cast<ConfigIndexField>().Select(x => new FieldDefinition(x.Name, x.Type)).ToArray(),
                indexSet.IndexUserFields.Cast<ConfigIndexField>().Select(x => new FieldDefinition(x.Name, x.Type)).ToArray(),
                indexSet.IncludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                indexSet.ExcludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                indexSet.IndexParentId);
        }
    }
}
