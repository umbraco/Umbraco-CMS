using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Services;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Examine.LuceneEngine;
using Examine.Search;
namespace Umbraco.Examine
{
    /// <summary>
    /// An indexer for Umbraco content and media
    /// </summary>
    public class UmbracoContentIndex : UmbracoExamineIndex, IUmbracoContentIndex2
    {
        public const string VariesByCultureFieldName = SpecialFieldPrefix + "VariesByCulture";
        protected ILocalizationService LanguageService { get; }
        private readonly ISet<string> _idOnlyFieldSet = new HashSet<string> { "id" };
        #region Constructors

        /// <summary>
        /// Create an index at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="defaultAnalyzer"></param>
        /// <param name="profilingLogger"></param>
        /// <param name="languageService"></param>
        /// <param name="validator"></param>
        /// <param name="indexValueTypes"></param>
        public UmbracoContentIndex(
            string name,
            Directory luceneDirectory,
            FieldDefinitionCollection fieldDefinitions,
            Analyzer defaultAnalyzer,
            IProfilingLogger profilingLogger,
            ILocalizationService languageService,
            IContentValueSetValidator validator,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypes = null)
            : base(name, luceneDirectory, fieldDefinitions, defaultAnalyzer, profilingLogger, validator, indexValueTypes)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            LanguageService = languageService ?? throw new ArgumentNullException(nameof(languageService));

            if (validator is IContentValueSetValidator contentValueSetValidator)
                PublishedValuesOnly = contentValueSetValidator.PublishedValuesOnly;
        }

        #endregion

        /// <summary>
        /// Special check for invalid paths
        /// </summary>
        /// <param name="values"></param>
        /// <param name="onComplete"></param>
        protected override void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs> onComplete)
        {
            // We don't want to re-enumerate this list, but we need to split it into 2x enumerables: invalid and valid items.
            // The Invalid items will be deleted, these are items that have invalid paths (i.e. moved to the recycle bin, etc...)
            // Then we'll index the Value group all together.
            // We return 0 or 1 here so we can order the results and do the invalid first and then the valid.
            var invalidOrValid = values.GroupBy(v =>
            {
                if (!v.Values.TryGetValue("path", out var paths) || paths.Count <= 0 || paths[0] == null)
                    return 0;

                //we know this is an IContentValueSetValidator
                var validator = (IContentValueSetValidator)ValueSetValidator;
                var path = paths[0].ToString();

                return (!validator.ValidatePath(path, v.Category)
                        || !validator.ValidateRecycleBin(path, v.Category)
                        || !validator.ValidateProtectedContent(path, v.Category))
                    ? 0
                    : 1;
            }).ToList();

            var hasDeletes = false;
            var hasUpdates = false;
            foreach (var group in invalidOrValid.OrderBy(x => x.Key))
            {
                if (group.Key == 0)
                {
                    hasDeletes = true;
                    //these are the invalid items so we'll delete them
                    //since the path is not valid we need to delete this item in case it exists in the index already and has now
                    //been moved to an invalid parent.

                    base.PerformDeleteFromIndex(group.Select(x => x.Id), args => { /*noop*/ });
                }
                else
                {
                    hasUpdates = true;
                    //these are the valid ones, so just index them all at once
                    base.PerformIndexItems(group.ToList(), onComplete);
                }
            }

            if (hasDeletes && !hasUpdates || !hasDeletes && !hasUpdates)
            {
                //we need to manually call the completed method
                onComplete(new IndexOperationEventArgs(this, 0));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Deletes a node from the index.
        /// </summary>
        /// <remarks>
        /// When a content node is deleted, we also need to delete it's children from the index so we need to perform a
        /// custom Lucene search to find all decendents and create Delete item queues for them too.
        /// </remarks>
        /// <param name="itemIds">ID of the node to delete</param>
        /// <param name="onComplete"></param>
        protected override void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs> onComplete)
        {
            var idsAsList = itemIds.ToList();
            foreach (var nodeId in idsAsList)
            {
                //find all descendants based on path
                var descendantPath = $@"\-1\,*{nodeId}\,*";
                var rawQuery = $"{IndexPathFieldName}:{descendantPath}";
                var searcher = GetSearcher();
                var c = searcher.CreateQuery();
                var filtered = c.NativeQuery(rawQuery);

                var selectedFields = filtered.SelectFields(_idOnlyFieldSet);
                var results = selectedFields.Execute();
                ProfilingLogger.Debug<string, long>(GetType(), "DeleteFromIndex with query: {Query} (found {TotalItems} results)", rawQuery, results.TotalItemCount);

                //need to queue a delete item for each one found
                QueueIndexOperation(results.Select(r => new IndexOperation(new ValueSet(r.Id), IndexOperationType.Delete)));
            }

            base.PerformDeleteFromIndex(idsAsList, onComplete);
        }

    }
}
