using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.Services;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Examine
{

    /// <summary>
    /// Performs the data lookups required to rebuild a content index
    /// </summary>
    public class ContentIndexPopulator : IndexPopulator<IUmbracoContentIndex2>
    {
        private readonly IContentService _contentService;
        private readonly IValueSetBuilder<IContent> _contentValueSetBuilder;

        /// <summary>
        /// This is a static query, it's parameters don't change so store statically
        /// </summary>
        private static IQuery<IContent> _publishedQuery;

        private readonly bool _publishedValuesOnly;
        private readonly int? _parentId;

        /// <summary>
        /// Default constructor to lookup all content data
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="sqlContext"></param>
        /// <param name="contentValueSetBuilder"></param>
        public ContentIndexPopulator(IContentService contentService, ISqlContext sqlContext, IContentValueSetBuilder contentValueSetBuilder)
            : this(false, null, contentService, sqlContext, contentValueSetBuilder)
        {
        }

        /// <summary>
        /// Optional constructor allowing specifying custom query parameters
        /// </summary>
        /// <param name="publishedValuesOnly"></param>
        /// <param name="parentId"></param>
        /// <param name="contentService"></param>
        /// <param name="sqlContext"></param>
        /// <param name="contentValueSetBuilder"></param>
        public ContentIndexPopulator(bool publishedValuesOnly, int? parentId, IContentService contentService, ISqlContext sqlContext, IValueSetBuilder<IContent> contentValueSetBuilder)
        {
            if (sqlContext == null) throw new ArgumentNullException(nameof(sqlContext));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _contentValueSetBuilder = contentValueSetBuilder ?? throw new ArgumentNullException(nameof(contentValueSetBuilder));
            if (_publishedQuery == null)
                _publishedQuery = sqlContext.Query<IContent>().Where(x => x.Published);
            _publishedValuesOnly = publishedValuesOnly;
            _parentId = parentId;
        }

        public override bool IsRegistered(IUmbracoContentIndex2 index)
        {
            // check if it should populate based on published values
            return _publishedValuesOnly == index.PublishedValuesOnly;
        }

        protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
        {
            if (indexes.Count == 0) return;

            const int pageSize = 10000;
            var pageIndex = 0;

            var contentParentId = -1;
            if (_parentId.HasValue && _parentId.Value > 0)
            {
                contentParentId = _parentId.Value;
            }

            if (_publishedValuesOnly)
            {
                IndexPublishedContent(contentParentId, pageIndex, pageSize, indexes);
            }
            else
            {
                IndexAllContent(contentParentId, pageIndex, pageSize, indexes);
            }
        }

        protected void IndexAllContent(int contentParentId, int pageIndex, int pageSize, IReadOnlyList<IIndex> indexes)
        {
            IContent[] content;

            do
            {
                content = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out _).ToArray();

                if (content.Length > 0)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    foreach (var index in indexes)
                        index.IndexItems(_contentValueSetBuilder.GetValueSets(content));
                }

                pageIndex++;
            } while (content.Length == pageSize);
        }

        protected void IndexPublishedContent(int contentParentId, int pageIndex, int pageSize,
            IReadOnlyList<IIndex> indexes)
        {
            IContent[] content;

            var publishedPages = new HashSet<int>();

            do
            {
                //add the published filter
                //note: We will filter for published variants in the validator
                content = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out _, _publishedQuery,
                    Ordering.By("Level", Direction.Ascending)).ToArray();
               
                if (content.Length > 0)
                {
                    var indexableContent = new List<IContent>();

                    // get the max level in this result set
                    int maxLevel = content.Max(x => x.Level);

                    // gets the first level pages, these are always published because _publishedQuery filter
                    // and store the id in a hash set so we can track them
                    var firstLevelContent = content.Where(x => x.Level == 1);
                    
                    foreach (var item in firstLevelContent)
                    {
                        publishedPages.Add(item.Id);
                        indexableContent.Add(item);
                    }

                    // get content per level so we can filter the pages that don't have a published parent
                    for (var level = 2; level <= maxLevel; level++)
                    {
                        var levelContent = content.Where(x => x.Level == level && publishedPages.Contains(x.ParentId));

                        foreach (var item in levelContent)
                        {
                            publishedPages.Add(item.Id);
                            indexableContent.Add(item);
                        }
                    }

                    // ReSharper disable once PossibleMultipleEnumeration
                    foreach (var index in indexes)
                        index.IndexItems(_contentValueSetBuilder.GetValueSets(indexableContent.ToArray()));
                }

                pageIndex++;
            } while (content.Length == pageSize);
        }
    }


}
