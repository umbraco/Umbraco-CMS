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
internal class ExamineUmbracoIndexingHandler : IUmbracoIndexingHandler
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
                        if (int.TryParse(item.Id, NumberStyles.Integer, CultureInfo.InvariantCulture,
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
    private class DeferredReIndexForContent : IDeferredAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IContent _content;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly bool _isPublished;

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

        public void Execute() =>
            Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _content, _isPublished);

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
    private class DeferredReIndexForMedia : IDeferredAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly bool _isPublished;
        private readonly IMedia _media;

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

        public void Execute() =>
            Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _media, _isPublished);

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
    private class DeferredReIndexForMember : IDeferredAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly IMember _member;

        public DeferredReIndexForMember(
            IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IMember member)
        {
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _member = member;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public void Execute() => Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _member);

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

    private class DeferredDeleteIndex : IDeferredAction
    {
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly int _id;
        private readonly IReadOnlyCollection<int>? _ids;
        private readonly bool _keepIfUnpublished;

        public DeferredDeleteIndex(
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            int id,
            bool keepIfUnpublished)
        {
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _id = id;
            _keepIfUnpublished = keepIfUnpublished;
        }

        public DeferredDeleteIndex(
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IReadOnlyCollection<int> ids,
            bool keepIfUnpublished)
        {
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _ids = ids;
            _keepIfUnpublished = keepIfUnpublished;
        }

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

        public static void Execute(ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, int id,
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
    private class DeferredRemoveProtectedContent : IDeferredAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly IPublicAccessService _publicAccessService;

        public DeferredRemoveProtectedContent(IBackgroundTaskQueue backgroundTaskQueue, ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, IPublicAccessService publicAccessService)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _publicAccessService = publicAccessService;
        }

        public void Execute() => Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _publicAccessService);

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
