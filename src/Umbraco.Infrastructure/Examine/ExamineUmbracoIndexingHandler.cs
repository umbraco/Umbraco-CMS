using System.Globalization;
using Examine;
using Examine.Search;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Indexing handler for Examine indexes
/// </summary>
internal class ExamineUmbracoIndexingHandler : IUmbracoIndexingHandler
{
    // the default enlist priority is 100
    // enlist with a lower priority to ensure that anything "default" runs after us
    // but greater that SafeXmlReaderWriter priority which is 60
    private const int EnlistPriority = 80;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IContentValueSetBuilder _contentValueSetBuilder;
    private readonly Lazy<bool> _enabled;
    private readonly IExamineManager _examineManager;
    private readonly ILogger<ExamineUmbracoIndexingHandler> _logger;
    private readonly IMainDom _mainDom;
    private readonly IValueSetBuilder<IMedia> _mediaValueSetBuilder;
    private readonly IValueSetBuilder<IMember> _memberValueSetBuilder;
    private readonly IProfilingLogger _profilingLogger;
    private readonly IPublishedContentValueSetBuilder _publishedContentValueSetBuilder;
    private readonly ICoreScopeProvider _scopeProvider;

    public ExamineUmbracoIndexingHandler(
        IMainDom mainDom,
        ILogger<ExamineUmbracoIndexingHandler> logger,
        IProfilingLogger profilingLogger,
        ICoreScopeProvider scopeProvider,
        IExamineManager examineManager,
        IBackgroundTaskQueue backgroundTaskQueue,
        IContentValueSetBuilder contentValueSetBuilder,
        IPublishedContentValueSetBuilder publishedContentValueSetBuilder,
        IValueSetBuilder<IMedia> mediaValueSetBuilder,
        IValueSetBuilder<IMember> memberValueSetBuilder)
    {
        _mainDom = mainDom;
        _logger = logger;
        _profilingLogger = profilingLogger;
        _scopeProvider = scopeProvider;
        _examineManager = examineManager;
        _backgroundTaskQueue = backgroundTaskQueue;
        _contentValueSetBuilder = contentValueSetBuilder;
        _publishedContentValueSetBuilder = publishedContentValueSetBuilder;
        _mediaValueSetBuilder = mediaValueSetBuilder;
        _memberValueSetBuilder = memberValueSetBuilder;
        _enabled = new Lazy<bool>(IsEnabled);
    }

    /// <inheritdoc />
    public bool Enabled => _enabled.Value;

    /// <inheritdoc />
    public void DeleteIndexForEntity(int entityId, bool keepIfUnpublished)
    {
        var actions = DeferedActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferedDeleteIndex(this, entityId, keepIfUnpublished));
        }
        else
        {
            DeferedDeleteIndex.Execute(this, entityId, keepIfUnpublished);
        }
    }

    /// <inheritdoc />
    public void DeleteIndexForEntities(IReadOnlyCollection<int> entityIds, bool keepIfUnpublished)
    {
        var actions = DeferedActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferedDeleteIndex(this, entityIds, keepIfUnpublished));
        }
        else
        {
            DeferedDeleteIndex.Execute(this, entityIds, keepIfUnpublished);
        }
    }

    /// <inheritdoc />
    public void ReIndexForContent(IContent sender, bool isPublished)
    {
        var actions = DeferedActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferedReIndexForContent(_backgroundTaskQueue, this, sender, isPublished));
        }
        else
        {
            DeferedReIndexForContent.Execute(_backgroundTaskQueue, this, sender, isPublished);
        }
    }

    /// <inheritdoc />
    public void ReIndexForMedia(IMedia sender, bool isPublished)
    {
        var actions = DeferedActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferedReIndexForMedia(_backgroundTaskQueue, this, sender, isPublished));
        }
        else
        {
            DeferedReIndexForMedia.Execute(_backgroundTaskQueue, this, sender, isPublished);
        }
    }

    /// <inheritdoc />
    public void ReIndexForMember(IMember member)
    {
        var actions = DeferedActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferedReIndexForMember(_backgroundTaskQueue, this, member));
        }
        else
        {
            DeferedReIndexForMember.Execute(_backgroundTaskQueue, this, member);
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
        //let's deal with shutting down Examine with MainDom
        var examineShutdownRegistered = _mainDom.Register(release: () =>
        {
            using (_profilingLogger.TraceDuration<ExamineUmbracoIndexingHandler>("Examine shutting down"))
            {
                _examineManager.Dispose();
            }
        });

        if (!examineShutdownRegistered)
        {
            _logger.LogInformation(
                "Examine shutdown not registered, this AppDomain is not the MainDom, Examine will be disabled");

            //if we could not register the shutdown examine ourselves, it means we are not maindom! in this case all of examine should be disabled!
            Suspendable.ExamineEvents.SuspendIndexers(_logger);
            return false; //exit, do not continue
        }

        _logger.LogDebug("Examine shutdown registered with MainDom");

        var registeredIndexers =
            _examineManager.Indexes.OfType<IUmbracoIndex>().Count(x => x.EnableDefaultEventHandler);

        _logger.LogInformation("Adding examine event handlers for {RegisteredIndexers} index providers.",
            registeredIndexers);

        // don't bind event handlers if we're not suppose to listen
        if (registeredIndexers == 0)
        {
            return false;
        }

        return true;
    }

    #region Deferred Actions

    private class DeferedActions
    {
        private readonly List<DeferedAction> _actions = new();

        public static DeferedActions? Get(ICoreScopeProvider scopeProvider)
        {
            IScopeContext? scopeContext = scopeProvider.Context;

            return scopeContext?.Enlist("examineEvents",
                () => new DeferedActions(), // creator
                (completed, actions) => // action
                {
                    if (completed)
                    {
                        actions?.Execute();
                    }
                }, EnlistPriority);
        }

        public void Add(DeferedAction action) => _actions.Add(action);

        private void Execute()
        {
            foreach (DeferedAction action in _actions)
            {
                action.Execute();
            }
        }
    }

    /// <summary>
    ///     An action that will execute at the end of the Scope being completed
    /// </summary>
    private abstract class DeferedAction
    {
        public virtual void Execute()
        {
        }
    }

    /// <summary>
    ///     Re-indexes an <see cref="IContent" /> item on a background thread
    /// </summary>
    private class DeferedReIndexForContent : DeferedAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IContent _content;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly bool _isPublished;

        public DeferedReIndexForContent(IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, IContent content, bool isPublished)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _content = content;
            _isPublished = isPublished;
        }

        public override void Execute() =>
            Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _content, _isPublished);

        public static void Execute(IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, IContent content, bool isPublished)
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
    private class DeferedReIndexForMedia : DeferedAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly bool _isPublished;
        private readonly IMedia _media;

        public DeferedReIndexForMedia(IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, IMedia media, bool isPublished)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _media = media;
            _isPublished = isPublished;
        }

        public override void Execute() =>
            Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _media, _isPublished);

        public static void Execute(IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, IMedia media, bool isPublished) =>
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
    private class DeferedReIndexForMember : DeferedAction
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly IMember _member;

        public DeferedReIndexForMember(IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, IMember member)
        {
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _member = member;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public override void Execute() => Execute(_backgroundTaskQueue, _examineUmbracoIndexingHandler, _member);

        public static void Execute(IBackgroundTaskQueue backgroundTaskQueue,
            ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, IMember member) =>
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

    private class DeferedDeleteIndex : DeferedAction
    {
        private readonly ExamineUmbracoIndexingHandler _examineUmbracoIndexingHandler;
        private readonly int _id;
        private readonly IReadOnlyCollection<int>? _ids;
        private readonly bool _keepIfUnpublished;

        public DeferedDeleteIndex(ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler, int id,
            bool keepIfUnpublished)
        {
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _id = id;
            _keepIfUnpublished = keepIfUnpublished;
        }

        public DeferedDeleteIndex(ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IReadOnlyCollection<int> ids, bool keepIfUnpublished)
        {
            _examineUmbracoIndexingHandler = examineUmbracoIndexingHandler;
            _ids = ids;
            _keepIfUnpublished = keepIfUnpublished;
        }

        public override void Execute()
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

        public static void Execute(ExamineUmbracoIndexingHandler examineUmbracoIndexingHandler,
            IReadOnlyCollection<int> ids, bool keepIfUnpublished)
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

    #endregion
}
