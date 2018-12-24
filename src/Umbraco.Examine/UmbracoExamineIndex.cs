﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Umbraco.Core;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Store;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Examine.Config;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Examine
{

    /// <summary>
    /// An abstract provider containing the basic functionality to be able to query against Umbraco data.
    /// </summary>
    public abstract class UmbracoExamineIndex : LuceneIndex, IUmbracoIndex, IIndexDiagnostics
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
        protected UmbracoExamineIndex()
            : base()
        {
            ProfilingLogger = Current.ProfilingLogger;
            _configBased = true;
            _diagnostics = new UmbracoExamineIndexDiagnostics(this, ProfilingLogger.Logger);
        }

        /// <summary>
        /// Create a new <see cref="UmbracoExamineIndex"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="defaultAnalyzer"></param>
        /// <param name="profilingLogger"></param>
        /// <param name="validator"></param>
        /// <param name="indexValueTypes"></param>
        protected UmbracoExamineIndex(
            string name,
            FieldDefinitionCollection fieldDefinitions,
            Directory luceneDirectory,
            Analyzer defaultAnalyzer,
            ProfilingLogger profilingLogger,
            IValueSetValidator validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypes = null)
            : base(name, fieldDefinitions, luceneDirectory, defaultAnalyzer, validator, indexValueTypes)
        {
            ProfilingLogger = profilingLogger ?? throw new ArgumentNullException(nameof(profilingLogger));

            //try to set the value of `LuceneIndexFolder` for diagnostic reasons
            if (luceneDirectory is FSDirectory fsDir)
                LuceneIndexFolder = fsDir.Directory;

            _diagnostics = new UmbracoExamineIndexDiagnostics(this, ProfilingLogger.Logger);
        }

        private readonly bool _configBased = false;

        

        protected ProfilingLogger ProfilingLogger { get; }

        /// <summary>
        /// When set to true Umbraco will keep the index in sync with Umbraco data automatically
        /// </summary>
        public bool EnableDefaultEventHandler { get; set; } = true;

        public bool PublishedValuesOnly { get; protected set; } = false;

        /// <inheritdoc />
        public IEnumerable<string> GetFields()
        {
            //we know this is a LuceneSearcher
            var searcher = (LuceneSearcher) GetSearcher();
            return searcher.GetAllIndexedFields();
        }

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

            //this is config based, so add the default definitions
            foreach (var field in UmbracoFieldDefinitionCollection.UmbracoIndexFieldDefinitions)
            {
                FieldDefinitionCollection.TryAdd(field);
            }

            //Need to check if the index set is specified...
            if (config["indexSet"] == null)
            {
                var possibleSuffixes = new[] {"Index", "Indexer"};
                foreach (var suffix in possibleSuffixes)
                {
                    if (!name.EndsWith(suffix)) continue;

                    var setNameByConvension = name.Remove(name.LastIndexOf(suffix, StringComparison.Ordinal)) + "IndexSet";
                    //check if we can assign the index set by naming convention
                    var set = IndexSets.Instance.Sets.Cast<IndexSet>().SingleOrDefault(x => x.SetName == setNameByConvension);

                    if (set == null) continue;

                    //we've found an index set by naming conventions :)
                    IndexSetName = set.SetName;

                    var indexSet = IndexSets.Instance.Sets[IndexSetName];

                    //get the index criteria and ensure folder
                    ConfigIndexCriteria = CreateFieldDefinitionsFromConfig(indexSet);
                    foreach (var fieldDefinition in ConfigIndexCriteria.StandardFields.Union(ConfigIndexCriteria.UserFields))
                    {
                        //replace any existing or add
                        FieldDefinitionCollection.AddOrUpdate(fieldDefinition);
                    }
                    break;
                }
            }
            else
            {
                //if an index set is specified, ensure it exists and initialize the indexer based on the set

                if (IndexSets.Instance.Sets[config["indexSet"]] == null)
                    throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

                IndexSetName = config["indexSet"];

                var indexSet = IndexSets.Instance.Sets[IndexSetName];

                //get the index criteria and ensure folder
                ConfigIndexCriteria = CreateFieldDefinitionsFromConfig(indexSet);
                foreach (var fieldDefinition in ConfigIndexCriteria.StandardFields.Union(ConfigIndexCriteria.UserFields))
                {
                    //replace any existing or add
                    FieldDefinitionCollection.AddOrUpdate(fieldDefinition);
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
        protected override void PerformDeleteFromIndex(string nodeId, Action<IndexOperationEventArgs> onComplete)
        {
            if (CanInitialize())
            {
                using (new SafeCallContext())
                {
                    base.PerformDeleteFromIndex(nodeId, onComplete);
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
        protected override void AddDocument(Document doc, ValueSet valueSet, IndexWriter writer)
        {
            ProfilingLogger.Logger.Debug(GetType(),
                "Write lucene doc id:{DocumentId}, category:{DocumentCategory}, type:{DocumentItemType}",
                valueSet.Id,
                valueSet.Category,
                valueSet.ItemType);

            base.AddDocument(doc, valueSet, writer);
        }

        protected override void OnTransformingIndexValues(IndexingItemEventArgs e)
        {
            base.OnTransformingIndexValues(e);

            //ensure special __Path field
            var path = e.ValueSet.GetValue("path");
            if (path != null)
            {
                e.ValueSet.Set(IndexPathFieldName, path);
            }

            //icon
            if (e.ValueSet.Values.TryGetValue("icon", out var icon) && e.ValueSet.Values.ContainsKey(IconFieldName) == false)
            {
                e.ValueSet.Values[IconFieldName] = icon;
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

        #region IIndexDiagnostics

        private readonly UmbracoExamineIndexDiagnostics _diagnostics;

        public int DocumentCount => _diagnostics.DocumentCount;
        public int FieldCount => _diagnostics.FieldCount;
        public Attempt<string> IsHealthy() => _diagnostics.IsHealthy();
        public virtual IReadOnlyDictionary<string, object> Metadata => _diagnostics.Metadata;

        #endregion
    }
}
