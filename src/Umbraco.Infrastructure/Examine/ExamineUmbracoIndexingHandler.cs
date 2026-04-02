using System.Globalization;
using Examine;
using Examine.Search;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Indexing handler for Examine indexes
/// </summary>
internal sealed class ExamineUmbracoIndexingHandler : IUmbracoIndexingHandler
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IContentValueSetBuilder _contentValueSetBuilder;
    private readonly Lazy<bool> _enabled;
    private readonly IExamineManager _examineManager;
    private readonly ILogger<ExamineUmbracoIndexingHandler> _logger;
    private readonly IValueSetBuilder<IMedia> _mediaValueSetBuilder;
    private readonly IValueSetBuilder<IMember> _memberValueSetBuilder;
    private readonly IPublishedContentValueSetBuilder _publishedContentValueSetBuilder;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ExamineIndexingMainDomHandler _mainDomHandler;
    private readonly IPublicAccessService _publicAccessService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Examine.ExamineUmbracoIndexingHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger used for logging diagnostic and operational information.</param>
    /// <param name="scopeProvider">Provides access to the core scope for database operations.</param>
    /// <param name="examineManager">Manages Examine indexes and searchers.</param>
    /// <param name="backgroundTaskQueue">Handles the queuing and execution of background tasks.</param>
    /// <param name="contentValueSetBuilder">Builds value sets for content items to be indexed.</param>
    /// <param name="publishedContentValueSetBuilder">Builds value sets for published content items to be indexed.</param>
    /// <param name="mediaValueSetBuilder">Builds value sets for media items to be indexed.</param>
    /// <param name="memberValueSetBuilder">Builds value sets for member items to be indexed.</param>
    /// <param name="mainDomHandler">Handles main domain (MainDom) events for Examine indexing.</param>
    /// <param name="publicAccessService">Provides services for managing public access to content.</param>
    public ExamineUmbracoIndexingHandler(
        ILogger<ExamineUmbracoIndexingHandler> logger,
        ICoreScopeProvider scopeProvider,
        IExamineManager examineManager,
        IBackgroundTaskQueue backgroundTaskQueue,
        IContentValueSetBuilder contentValueSetBuilder,
        IPublishedContentValueSetBuilder publishedContentValueSetBuilder,
        IValueSetBuilder<IMedia> mediaValueSetBuilder,
        IValueSetBuilder<IMember> memberValueSetBuilder,
        ExamineIndexingMainDomHandler mainDomHandler,
        IPublicAccessService publicAccessService)
    {
        _logger = logger;
        _scopeProvider = scopeProvider;
        _examineManager = examineManager;
        _backgroundTaskQueue = backgroundTaskQueue;
        _contentValueSetBuilder = contentValueSetBuilder;
        _publishedContentValueSetBuilder = publishedContentValueSetBuilder;
        _mediaValueSetBuilder = mediaValueSetBuilder;
        _memberValueSetBuilder = memberValueSetBuilder;
        _mainDomHandler = mainDomHandler;
        _publicAccessService = publicAccessService;
        _enabled = new Lazy<bool>(IsEnabled);
    }

    /// <inheritdoc />
    public bool Enabled => _enabled.Value;

    /// <inheritdoc />
    public void DeleteIndexForEntity(int entityId, bool keepIfUnpublished)
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferredDeleteIndex(this, entityId, keepIfUnpublished));
        }
        else
        {
            DeferredDeleteIndex.Execute(this, entityId, keepIfUnpublished);
        }
    }

    /// <inheritdoc />
    public void DeleteIndexForEntities(IReadOnlyCollection<int> entityIds, bool keepIfUnpublished)
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferredDeleteIndex(this, entityIds, keepIfUnpublished));
        }
        else
        {
            DeferredDeleteIndex.Execute(this, entityIds, keepIfUnpublished);
        }
    }

    /// <inheritdoc />
    public void ReIndexForContent(IContent sender, bool isPublished)
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferredReIndexForContent(_backgroundTaskQueue, this, sender, isPublished));
        }
        else
        {
            DeferredReIndexForContent.Execute(_backgroundTaskQueue, this, sender, isPublished);
        }
    }

    /// <inheritdoc />
    public void ReIndexForMedia(IMedia sender, bool isPublished)
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferredReIndexForMedia(_backgroundTaskQueue, this, sender, isPublished));
        }
        else
        {
            DeferredReIndexForMedia.Execute(_backgroundTaskQueue, this, sender, isPublished);
        }
    }

    /// <inheritdoc />
    public void ReIndexForMember(IMember member)
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferredReIndexForMember(_backgroundTaskQueue, this, member));
        }
        else
        {
            DeferredReIndexForMember.Execute(_backgroundTaskQueue, this, member);
        }
    }

    /// <inheritdoc />
    public void RemoveProtectedContent()
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferredRemoveProtectedContent(_backgroundTaskQueue, this, _publicAccessService));
        }
        else
        {
            DeferredRemoveProtectedContent.Execute(_backgroundTaskQueue, this, _publicAccessService);
        }
    }

    /// <inheritdoc />
    public void DeleteDocumentsForContentTypes(IReadOnlyCollection<int> removedContentTypes)
    {
        const int pageSize = 500;

        //Delete all content of this content/media/member type that is in any content indexer by looking up matched examine docs
        foreach (var id in removedContentTypes)
        {
            foreach (IUmbracoIndex index in _examineManager.Indexes.OfType<IUmbracoIndex>())
            {
                var page = 0;
                var total = long.MaxValue;
                while (page * pageSize < total)
                {
                    //paging with examine, see https://shazwazza.com/post/paging-with-examine/
                    ISearchResults? results = index.Searcher
                        .CreateQuery()
                        .Field("nodeType", id.ToInvariantString())
                        .Execute(QueryOptions.SkipTake(page * pageSize, pageSize));
                    total = results.TotalItemCount;

                    foreach (ISearchResult item in results)
                    {
                        if (int.TryParse(
                                item.Id,
                                NumberStyles.Integer,
                                CultureInfo.InvariantCulture,
                                out var contentId))
                        {
                            DeleteIndexForEntity(contentId, false);
                        }
                    }

                    page++;
                }
            }
        }
    }

    /// <summary>
    ///     Used to lazily check if Examine Index handling is enabled
    /// </summary>
    /// <returns></returns>
    private bool IsEnabled()
    {
        if (_mainDomHandler.IsMainDom() is false)
        {
            return false;
        }

        var registeredIndexers =
            _examineManager.Indexes.OfType<IUmbracoIndex>().Count(x => x.EnableDefaultEventHandler);

        _logger.LogInformation(
            "Adding examine event handlers for {RegisteredIndexers} index providers.",
            registeredIndexers);

        // don't bind event handlers if we're not suppose to listen
        if (registeredIndexers == 0)
        {
            return false;
        }

        return true;
    }

    #region Deferred Actions

    /// <summary>
    ///     Re-indexes an <see cref="IContent" /> item on a background thread
    /// </summary>
    private sealed class DeferredReIndexForContent : IDeferredAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IContent _content;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly bool _isPublished;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredReIndexForContent"/> class.
        /// </summary>
        /// <param name="backgroundTaskQueue">The queue used to schedule background reindexing tasks.</param>
        /// <param name="examineUmbracoIndexingHandler">The handler responsible for managing Examine Umbraco indexing operations.</param>
        /// <param name="content">The content item to be re-indexed.</param>
        /// <param name="isPublished">True if the content is published; otherwise, false.</param>
        public DeferredReIndexForContent(
            IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IContent content,
            bool isPublished)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _content = content;
            _isPublished = isPublished;
        }

        /// <summary>
        /// Executes the deferred re-indexing operation for the specified content item.
        /// This method processes the queued re-indexing task, updating the search index as needed.
        /// </summary>
        public void Execute() =>
            Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _content, _isPublished);

        /// <summary>
        /// Executes a deferred re-indexing operation for a specified content item, updating the search index as required.
        /// </summary>
        /// <param name="backgroundTaskQueue">The queue used to schedule background indexing tasks.</param>
        /// <param name="examineUmbracoIndexingHandler">The handler responsible for managing Examine indexing operations.</param>
        /// <param name="content">The content item to re-index.</param>
        /// <param name="isPublished">True if the content item is published; otherwise, false.</param>
        public static void Execute(
            IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IContent content,
            bool isPublished)
            => backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
            {
                using ICoreScope scope =
                    examineUmbracoIndexingHandler._scopeProvider.CreateCoreScope(autoComplete: true);

                // for content we have a different builder for published vs unpublished
                // we don't want to build more value sets than is needed so we'll lazily build 2 one for published one for non-published
                var builders = new Dictionary<bool, Lazy<List<ValueSet>>>
                {
                    [true] = new(() => examineUmbracoIndexingHandler._publishedContentValueSetBuilder.GetValueSets(content).ToList()),
                    [false] = new(() => examineUmbracoIndexingHandler._contentValueSetBuilder.GetValueSets(content).ToList())
                };

                // This is only for content - so only index items for IUmbracoContentIndex (to exlude members)
                foreach (IUmbracoIndex index in examineUmbracoIndexingHandler._examineManager.Indexes
                             .OfType<IUmbracoContentIndex>()
                             //filter the indexers
                             .Where(x => isPublished || !x.PublishedValuesOnly)
                             .Where(x => x.EnableDefaultEventHandler))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return Task.CompletedTask;
                    }

                    List<ValueSet> valueSet = builders[index.PublishedValuesOnly].Value;
                    index.IndexItems(valueSet);
                }

                return Task.CompletedTask;
            });
    }

    /// <summary>
    ///     Re-indexes an <see cref="IMedia" /> item on a background thread
    /// </summary>
    private sealed class DeferredReIndexForMedia : IDeferredAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly bool _isPublished;
        private readonly IMedia _media;

        /// <summary>Initializes a new instance of the <see cref="DeferredReIndexForMedia"/> class.</summary>
        /// <param name="backgroundTaskQueue">The background task queue to enqueue reindexing tasks.</param>
        /// <param name="examineUmbracoIndexingHandler">The Examine Umbraco indexing handler instance.</param>
        /// <param name="media">The media item to be reindexed.</param>
        /// <param name="isPublished">Indicates whether the media item is published.</param>
        public DeferredReIndexForMedia(
            IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IMedia media,
            bool isPublished)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _media = media;
            _isPublished = isPublished;
        }

        /// <summary>
        /// Executes the process that performs deferred re-indexing of media items in the background.
        /// This ensures that media content is updated in the search index as needed.
        /// </summary>
        public void Execute() =>
            Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _media, _isPublished);

        /// <summary>
        /// Performs deferred re-indexing of a media item by enqueuing the operation to a background task queue.
        /// Ensures that the media item is updated in the search index as required.
        /// </summary>
        /// <param name="backgroundTaskQueue">The queue used to schedule background indexing tasks.</param>
        /// <param name="examineUmbracoIndexingHandler">The handler that manages Examine search indexes.</param>
        /// <param name="media">The media item to re-index.</param>
        /// <param name="isPublished">True if the media item is published; otherwise, false.</param>
        public static void Execute(
            IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IMedia media,
            bool isPublished) =>
            // perform the ValueSet lookup on a background thread
            backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
            {
                using ICoreScope scope =
                    examineUmbracoIndexingHandler._scopeProvider.CreateCoreScope(autoComplete: true);

                var valueSet = examineUmbracoIndexingHandler._mediaValueSetBuilder.GetValueSets(media).ToList();

                // This is only for content - so only index items for IUmbracoContentIndex (to exlude members)
                foreach (IUmbracoIndex index in examineUmbracoIndexingHandler._examineManager.Indexes
                             .OfType<IUmbracoContentIndex>()
                             //filter the indexers
                             .Where(x => isPublished || !x.PublishedValuesOnly)
                             .Where(x => x.EnableDefaultEventHandler))
                {
                    index.IndexItems(valueSet);
                }

                return Task.CompletedTask;
            });
    }

    /// <summary>
    ///     Re-indexes an <see cref="IMember" /> item on a background thread
    /// </summary>
    private sealed class DeferredReIndexForMember : IDeferredAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly IMember _member;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredReIndexForMember"/> class.
        /// </summary>
        /// <param name="backgroundTaskQueue">The queue used to schedule background reindexing tasks.</param>
        /// <param name="examineUmbracoIndexingHandler">The handler responsible for Examine Umbraco indexing operations.</param>
        /// <param name="member">The member entity to be reindexed.</param>
        public DeferredReIndexForMember(
            IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IMember member)
        {
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _member = member;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        /// <summary>
        /// Executes the deferred re-indexing operation for the specified member in the background task queue.
        /// This ensures that the member's data is updated in the search index.
        /// </summary>
        public void Execute() => Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _member);

        /// <summary>
        /// Enqueues a deferred re-indexing operation for the specified member, ensuring the member's data is updated in the search index.
        /// </summary>
        /// <param name="backgroundTaskQueue">The queue used to schedule background tasks.</param>
        /// <param name="examineUmbracoIndexingHandler">The handler responsible for managing Examine indexing operations.</param>
        /// <param name="member">The member whose data should be re-indexed.</param>
        public static void Execute(
            IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IMember member) =>
            // perform the ValueSet lookup on a background thread
            backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
            {
                using ICoreScope scope =
                    examineUmbracoIndexingHandler._scopeProvider.CreateCoreScope(autoComplete: true);

                var valueSet = examineUmbracoIndexingHandler._memberValueSetBuilder.GetValueSets(member).ToList();

                // only process for IUmbracoMemberIndex (not content indexes)
                foreach (IUmbracoIndex index in examineUmbracoIndexingHandler._examineManager.Indexes
                             .OfType<IUmbracoMemberIndex>()
                             //filter the indexers
                             .Where(x => x.EnableDefaultEventHandler))
                {
                    index.IndexItems(valueSet);
                }

                return Task.CompletedTask;
            });
    }

    private sealed class DeferredDeleteIndex : IDeferredAction
    {
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly int _id;
        private readonly IReadOnlyCollection<int>? _ids;
        private readonly bool _keepIfUnpublished;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredDeleteIndex"/> class, which represents a deferred deletion operation for a specific item in the index.
        /// </summary>
        /// <param name="examineUmbracoIndexingHandler">The parent <see cref="ExamineUmbracoIndexingHandler"/> managing the indexing operations.</param>
        /// <param name="id">The identifier of the item to be deleted from the index.</param>
        /// <param name="keepIfUnpublished">If set to <c>true</c>, the item will be retained in the index if it is unpublished; otherwise, it will be deleted.</param>
        public DeferredDeleteIndex(
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            int id,
            bool keepIfUnpublished)
        {
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _id = id;
            _keepIfUnpublished = keepIfUnpublished;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredDeleteIndex"/> class, used to defer deletion of specified items from the index.
        /// </summary>
        /// <param name="examineUmbracoIndexingHandler">The parent <see cref="ExamineUmbracoIndexingHandler"/> managing the operation.</param>
        /// <param name="ids">A read-only collection of item IDs to be deleted from the index.</param>
        /// <param name="keepIfUnpublished">If <c>true</c>, items will be retained in the index if they are unpublished; otherwise, they will be deleted.</param>
        public DeferredDeleteIndex(
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IReadOnlyCollection<int> ids,
            bool keepIfUnpublished)
        {
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _ids = ids;
            _keepIfUnpublished = keepIfUnpublished;
        }

        /// <summary>
        /// Executes the deferred deletion of one or more items from the index, based on the provided identifiers.
        /// </summary>
        public void Execute()
        {
            if (_ids is null)
            {
                Execute(_examineUmbracoIndexingHandler, _id, _keepIfUnpublished);
            }
            else
            {
                Execute(_examineUmbracoIndexingHandler, _ids, _keepIfUnpublished);
            }
        }

        /// <summary>
        /// Executes the deferred deletion of a single item from the index.
        /// </summary>
        /// <param name="examineUmbracoIndexingHandler">The <see cref="ExamineUmbracoIndexingHandler"/> instance used to perform the deletion.</param>
        /// <param name="id">The identifier of the item to delete from the index.</param>
        /// <param name="keepIfUnpublished">If <c>true</c>, the item will be kept in the index if it is unpublished; otherwise, it will be removed.</param>
        public static void Execute(
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            int id,
            bool keepIfUnpublished)
        {
            foreach (IUmbracoIndex index in examineUmbracoIndexingHandler._examineManager.Indexes
                         .OfType<IUmbracoIndex>()
                         .Where(x => x.PublishedValuesOnly || !keepIfUnpublished)
                         .Where(x => x.EnableDefaultEventHandler))
            {
                index.DeleteFromIndex(id.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static void Execute(
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IReadOnlyCollection<int> ids,
            bool keepIfUnpublished)
        {
            foreach (IUmbracoIndex index in examineUmbracoIndexingHandler._examineManager.Indexes
                         .OfType<IUmbracoIndex>()
                         .Where(x => x.PublishedValuesOnly || !keepIfUnpublished)
                         .Where(x => x.EnableDefaultEventHandler))
            {
                index.DeleteFromIndex(ids.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            }
        }
    }

    /// <summary>
    ///     Removes all protected content from applicable indexes on a background thread
    /// </summary>
    private sealed class DeferredRemoveProtectedContent : IDeferredAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly IPublicAccessService _publicAccessService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredRemoveProtectedContent"/> class.
        /// </summary>
        /// <param name="backgroundTaskQueue">The queue used to schedule background tasks for deferred removal operations.</param>
        /// <param name="examineUmbracoIndexingHandler">The handler responsible for Umbraco Examine indexing operations.</param>
        /// <param name="publicAccessService">The service used to determine public access permissions for content.</param>
        public DeferredRemoveProtectedContent(IBackgroundTaskQueue backgroundTaskQueue, ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, IPublicAccessService publicAccessService)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _publicAccessService = publicAccessService;
        }

        /// <summary>
        /// Executes the deferred removal of protected content from the index.
        /// </summary>
        public void Execute() => Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _publicAccessService);

        /// <summary>
        /// Executes the deferred removal of protected content from the index.
        /// </summary>
        /// <param name="backgroundTaskQueue">The queue used to schedule background tasks for deferred execution.</param>
        /// <param name="examineUmbracoIndexingHandler">The indexing handler responsible for managing Examine indexes in Umbraco.</param>
        /// <param name="publicAccessService">The service used to determine which content is protected by public access rules.</param>
        public static void Execute(IBackgroundTaskQueue backgroundTaskQueue, ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, IPublicAccessService publicAccessService)
            => backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
            {
                using ICoreScope scope = examineUmbracoIndexingHandler._scopeProvider.CreateCoreScope(autoComplete: true);

                var protectedContentIds = publicAccessService.GetAll().Select(entry => entry.ProtectedNodeId).ToArray();
                if (protectedContentIds.Any() is false)
                {
                    return Task.CompletedTask;
                }

                foreach (IUmbracoContentIndex index in examineUmbracoIndexingHandler._examineManager.Indexes
                             .OfType<IUmbracoContentIndex>()
                             .Where(x => x is { EnableDefaultEventHandler: true, SupportProtectedContent: false }))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return Task.CompletedTask;
                    }

                    index.DeleteFromIndex(protectedContentIds.Select(id => id.ToString()));
                }

                return Task.CompletedTask;
            });
    }

    #endregion
}
