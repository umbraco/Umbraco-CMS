using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Services;

namespace Umbraco.Examine
{

    /// <summary>
    /// Performs the data lookups required to rebuild a content index
    /// </summary>
    public class ContentIndexPopulator : IndexPopulator<IUmbracoContentIndex>
    {
        private readonly IContentService _contentService;
        private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
        private readonly IValueSetBuilder<IContent> _contentValueSetBuilder;

        /// <summary>
        /// This is a static query, it's parameters don't change so store statically
        /// </summary>
        private IQuery<IContent> _publishedQuery;
        private IQuery<IContent> PublishedQuery => _publishedQuery ??= _umbracoDatabaseFactory.SqlContext.Query<IContent>().Where(x => x.Published);

        private readonly bool _publishedValuesOnly;
        private readonly int? _parentId;

        /// <summary>
        /// Default constructor to lookup all content data
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="sqlContext"></param>
        /// <param name="contentValueSetBuilder"></param>
        public ContentIndexPopulator(IContentService contentService, IUmbracoDatabaseFactory umbracoDatabaseFactory, IContentValueSetBuilder contentValueSetBuilder)
            : this(false, null, contentService, umbracoDatabaseFactory, contentValueSetBuilder)
        {
        }

        /// <summary>
        /// Optional constructor allowing specifying custom query parameters
        /// </summary>
        public ContentIndexPopulator(bool publishedValuesOnly, int? parentId, IContentService contentService, IUmbracoDatabaseFactory umbracoDatabaseFactory, IValueSetBuilder<IContent> contentValueSetBuilder)
        {
            if (umbracoDatabaseFactory == null) throw new ArgumentNullException(nameof(umbracoDatabaseFactory));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _umbracoDatabaseFactory = umbracoDatabaseFactory;
            _contentValueSetBuilder = contentValueSetBuilder ?? throw new ArgumentNullException(nameof(contentValueSetBuilder));
            _publishedValuesOnly = publishedValuesOnly;
            _parentId = parentId;
        }

        public override bool IsRegistered(IUmbracoContentIndex index)
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
                    var valueSets = _contentValueSetBuilder.GetValueSets(content).ToList();

                    // ReSharper disable once PossibleMultipleEnumeration
                    foreach (var index in indexes)
                    {
                        index.IndexItems(valueSets);
                    }
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
                content = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out _, PublishedQuery,
                    Ordering.By("Path", Direction.Ascending)).ToArray();


                if (content.Length > 0)
                {
                    var indexableContent = new List<IContent>();

                    foreach (var item in content)
                    {
                        if (item.Level == 1)
                        {
                            // first level pages are always published so no need to filter them
                            indexableContent.Add(item);
                            publishedPages.Add(item.Id);
                        }
                        else
                        {
                            if (publishedPages.Contains(item.ParentId))
                            {
                                // only index when parent is published
                                publishedPages.Add(item.Id);
                                indexableContent.Add(item);
                            }
                        }
                    }

                    var valueSets = _contentValueSetBuilder.GetValueSets(indexableContent.ToArray()).ToList();

                    // ReSharper disable once PossibleMultipleEnumeration
                    foreach (var index in indexes)
                        index.IndexItems(valueSets);
                }

                pageIndex++;
            } while (content.Length == pageSize);
        }
    }


}
