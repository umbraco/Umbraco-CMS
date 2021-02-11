// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Examine
{

    /// <summary>
    /// An abstract provider containing the basic functionality to be able to query against Umbraco data.
    /// </summary>
    public abstract class UmbracoExamineIndex : LuceneIndex, IUmbracoIndex, IIndexDiagnostics
    {
        private readonly ILogger<UmbracoExamineIndex> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private readonly IRuntimeState _runtimeState;
        // note
        // wrapping all operations that end up calling base.SafelyProcessQueueItems in a safe call
        // context because they will fork a thread/task/whatever which should *not* capture our
        // call context (and the database it can contain)! ideally we should be able to override
        // SafelyProcessQueueItems but that's not possible in the current version of Examine.


        /// <summary>
        /// Create a new <see cref="UmbracoExamineIndex"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="defaultAnalyzer"></param>
        /// <param name="profilingLogger"></param>
        /// <param name="logger"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="runtimeState"></param>
        /// <param name="validator"></param>
        /// <param name="indexValueTypes"></param>
        protected UmbracoExamineIndex(
            string name,
            Directory luceneDirectory,
            FieldDefinitionCollection fieldDefinitions,
            Analyzer defaultAnalyzer,
            IProfilingLogger profilingLogger,
            ILogger<UmbracoExamineIndex> logger,
            ILoggerFactory loggerFactory,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState,
            IValueSetValidator validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypes = null)
            : base(name, luceneDirectory, fieldDefinitions, defaultAnalyzer, validator, indexValueTypes)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _runtimeState = runtimeState;
            ProfilingLogger = profilingLogger ?? throw new ArgumentNullException(nameof(profilingLogger));

            //try to set the value of `LuceneIndexFolder` for diagnostic reasons
            if (luceneDirectory is FSDirectory fsDir)
                LuceneIndexFolder = fsDir.Directory;

            _diagnostics = new UmbracoExamineIndexDiagnostics(this, _loggerFactory.CreateLogger<UmbracoExamineIndexDiagnostics>(), hostingEnvironment);
        }

        private readonly bool _configBased = false;

        protected IProfilingLogger ProfilingLogger { get; }

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

        /// <summary>
        /// override to check if we can actually initialize.
        /// </summary>
        /// <remarks>
        /// This check is required since the base examine lib will try to rebuild on startup
        /// </remarks>
        protected override void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs> onComplete)
        {
            if (CanInitialize())
            {
                using (new SafeCallContext())
                {
                    base.PerformDeleteFromIndex(itemIds, onComplete);
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
            return _configBased == false || _runtimeState.Level == RuntimeLevel.Run;
        }

        /// <summary>
        /// overridden for logging
        /// </summary>
        /// <param name="ex"></param>
        protected override void OnIndexingError(IndexingErrorEventArgs ex)
        {
            _logger.LogError(ex.InnerException, ex.Message);
            base.OnIndexingError(ex);
        }

        /// <summary>
        /// This ensures that the special __Raw_ fields are indexed correctly
        /// </summary>
        /// <param name="docArgs"></param>
        protected override void OnDocumentWriting(DocumentWritingEventArgs docArgs)
        {
            var d = docArgs.Document;

            foreach (var f in docArgs.ValueSet.Values.Where(x => x.Key.StartsWith(UmbracoExamineFieldNames.RawFieldPrefix)).ToList())
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
            _logger.LogDebug("Write lucene doc id:{DocumentId}, category:{DocumentCategory}, type:{DocumentItemType}",
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
                e.ValueSet.Set(UmbracoExamineFieldNames.IndexPathFieldName, path);
            }

            //icon
            if (e.ValueSet.Values.TryGetValue("icon", out var icon) && e.ValueSet.Values.ContainsKey(UmbracoExamineFieldNames.IconFieldName) == false)
            {
                e.ValueSet.Values[UmbracoExamineFieldNames.IconFieldName] = icon;
            }
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
