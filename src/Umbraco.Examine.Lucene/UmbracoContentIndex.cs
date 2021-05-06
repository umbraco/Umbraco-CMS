// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine
{
    /// <summary>
    /// An indexer for Umbraco content and media
    /// </summary>
    public class UmbracoContentIndex : UmbracoExamineIndex, IUmbracoContentIndex, IDisposable
    {
        private readonly ILogger<UmbracoContentIndex> _logger;

        public UmbracoContentIndex(
            ILoggerFactory loggerFactory,
            string name,
            IOptionsSnapshot<LuceneDirectoryIndexOptions> indexOptions,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState,
            ILocalizationService languageService = null)
            : base(loggerFactory, name, indexOptions, hostingEnvironment, runtimeState)
        {
            LanguageService = languageService;
            _logger = loggerFactory.CreateLogger<UmbracoContentIndex>();

            LuceneDirectoryIndexOptions namedOptions = indexOptions.Get(name);
            if (namedOptions == null)
            {
                throw new InvalidOperationException($"No named {typeof(LuceneDirectoryIndexOptions)} options with name {name}");
            }

            if (namedOptions.Validator is IContentValueSetValidator contentValueSetValidator)
            {
                PublishedValuesOnly = contentValueSetValidator.PublishedValuesOnly;
            }
        }

        protected ILocalizationService LanguageService { get; }

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

            for (int i = 0; i < idsAsList.Count; i++)
            {
                string nodeId = idsAsList[i];

                //find all descendants based on path
                var descendantPath = $@"\-1\,*{nodeId}\,*";
                var rawQuery = $"{UmbracoExamineFieldNames.IndexPathFieldName}:{descendantPath}";
                var c = Searcher.CreateQuery();
                var filtered = c.NativeQuery(rawQuery);
                var results = filtered.Execute();

                _logger.
                    LogDebug("DeleteFromIndex with query: {Query} (found {TotalItems} results)", rawQuery, results.TotalItemCount);

                var toRemove = results.Select(x => x.Id).ToList();
                // delete those descendants (ensure base. is used here so we aren't calling ourselves!)
                base.PerformDeleteFromIndex(toRemove, null);

                // remove any ids from our list that were part of the descendants
                idsAsList.RemoveAll(x => toRemove.Contains(x));
            }

            base.PerformDeleteFromIndex(idsAsList, onComplete);
        }

    }
}
