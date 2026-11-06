using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Handlers of the published and unpublished notifications commonly notify external systems that then read the
/// published content back, so by the time those handlers run the published state must already be visible to
/// other requests.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheNotificationConsistencyTests : UmbracoIntegrationTestWithContentEditing
{
    private const string UpdatedName = "Updated Name";

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();

        builder.Services.AddSingleton<PublishedStateRecorder>();
        builder.AddNotificationHandler<ContentPublishedNotification, PublishedStateRecordingHandler>();
        builder.AddNotificationHandler<ContentUnpublishedNotification, PublishedStateRecordingHandler>();
        builder.AddNotificationHandler<ContentCacheRefresherNotification, PublishedStateRecordingHandler>();

        builder.Services.AddSingleton<UnpublishedUrlRecorder>();
        builder.AddNotificationHandler<ContentUnpublishingNotification, UnpublishedUrlCapturingHandler>();
        builder.AddNotificationHandler<ContentUnpublishedNotification, UnpublishedUrlCapturingHandler>();
    }

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private PublishedStateRecorder Recorder => GetRequiredService<PublishedStateRecorder>();

    [Test]
    public async Task Published_State_Is_Visible_When_Published_Notification_Is_Handled()
    {
        var key = PublishedTextPage.Key!.Value;
        await UpdateNameAsync(key, UpdatedName);
        Recorder.Start(key);

        var publishAttempt = await ContentPublishingService.PublishAsync(key, CultureAndSchedule, Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);

        AssertPublished(Recorder.Get(nameof(ContentPublishedNotification)));
    }

    [Test]
    public async Task Published_State_Is_Visible_When_Cache_Refresher_Notification_Is_Handled_After_Publish()
    {
        var key = PublishedTextPage.Key!.Value;
        await UpdateNameAsync(key, UpdatedName);
        Recorder.Start(key);

        var publishAttempt = await ContentPublishingService.PublishAsync(key, CultureAndSchedule, Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);

        AssertPublished(Recorder.Get(nameof(ContentCacheRefresherNotification)));
    }

    [Test]
    public async Task Unpublished_State_Is_Visible_When_Unpublished_Notification_Is_Handled()
    {
        var key = PublishedTextPage.Key!.Value;
        Recorder.Start(key);

        var unpublishAttempt = await ContentPublishingService.UnpublishAsync(key, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(unpublishAttempt.Success);

        AssertUnpublished(Recorder.Get(nameof(ContentUnpublishedNotification)));
    }

    [Test]
    public async Task Unpublished_State_Is_Visible_When_Cache_Refresher_Notification_Is_Handled_After_Unpublish()
    {
        var key = PublishedTextPage.Key!.Value;
        Recorder.Start(key);

        var unpublishAttempt = await ContentPublishingService.UnpublishAsync(key, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(unpublishAttempt.Success);

        AssertUnpublished(Recorder.Get(nameof(ContentCacheRefresherNotification)));
    }

    [Test]
    public async Task Url_Captured_When_Unpublishing_Is_Available_When_Unpublished_Notification_Is_Handled()
    {
        var key = PublishedTextPage.Key!.Value;
        UnpublishedUrlRecorder recorder = GetRequiredService<UnpublishedUrlRecorder>();
        recorder.Key = key;

        var unpublishAttempt = await ContentPublishingService.UnpublishAsync(key, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(unpublishAttempt.Success);

        Assert.Multiple(() =>
        {
            Assert.AreEqual("/", recorder.UrlFromState);
            Assert.AreEqual("#", recorder.UrlWhenUnpublished);
        });
    }

    private async Task UpdateNameAsync(Guid key, string name)
    {
        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Name = name }],
            Properties = PublishedTextPage.Properties,
            TemplateKey = PublishedTextPage.TemplateKey,
        };

        var updateAttempt = await ContentEditingService.UpdateAsync(key, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(updateAttempt.Success);
    }

    private static void AssertPublished(PublishedState? state)
    {
        Assert.IsNotNull(state);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(UpdatedName, state!.PublishedName);
            Assert.IsTrue(state.IsPublished);
        });
    }

    private static void AssertUnpublished(PublishedState? state)
    {
        Assert.IsNotNull(state);
        Assert.Multiple(() =>
        {
            Assert.IsNull(state!.PublishedName);
            Assert.IsFalse(state.IsPublished);
        });
    }

    internal sealed record PublishedState(string? PublishedName, bool IsPublished);

    internal sealed class PublishedStateRecorder
    {
        private readonly Dictionary<string, PublishedState> _states = new();

        public Guid Key { get; private set; }

        public bool IsRecording { get; private set; }

        public void Start(Guid key)
        {
            Key = key;
            IsRecording = true;
            _states.Clear();
        }

        public void Record(string notificationName, PublishedState state) => _states.TryAdd(notificationName, state);

        public PublishedState? Get(string notificationName) => _states.GetValueOrDefault(notificationName);
    }

    private sealed class PublishedStateRecordingHandler :
        INotificationHandler<ContentPublishedNotification>,
        INotificationHandler<ContentUnpublishedNotification>,
        INotificationHandler<ContentCacheRefresherNotification>
    {
        private readonly PublishedStateRecorder _recorder;
        private readonly IPublishedContentCache _publishedContentCache;
        private readonly IPublishStatusQueryService _publishStatusQueryService;

        public PublishedStateRecordingHandler(
            PublishedStateRecorder recorder,
            IPublishedContentCache publishedContentCache,
            IPublishStatusQueryService publishStatusQueryService)
        {
            _recorder = recorder;
            _publishedContentCache = publishedContentCache;
            _publishStatusQueryService = publishStatusQueryService;
        }

        public void Handle(ContentPublishedNotification notification) => Record(nameof(ContentPublishedNotification));

        public void Handle(ContentUnpublishedNotification notification) => Record(nameof(ContentUnpublishedNotification));

        public void Handle(ContentCacheRefresherNotification notification) => Record(nameof(ContentCacheRefresherNotification));

        private void Record(string notificationName)
        {
            if (_recorder.IsRecording is false)
            {
                return;
            }

            // Read the way a separate request would: without the ambient scope or context of the operation that
            // raised the notification.
            Task<PublishedState> read;
            using (ExecutionContext.SuppressFlow())
            {
                read = Task.Run(ReadAsync);
            }

            _recorder.Record(notificationName, read.GetAwaiter().GetResult());
        }

        private async Task<PublishedState> ReadAsync()
        {
            IPublishedContent? published = await _publishedContentCache.GetByIdAsync(_recorder.Key);
            return new PublishedState(published?.Name, _publishStatusQueryService.IsDocumentPublishedInAnyCulture(_recorder.Key));
        }
    }

    internal sealed class UnpublishedUrlRecorder
    {
        public Guid Key { get; set; }

        public string? UrlFromState { get; set; }

        public string? UrlWhenUnpublished { get; set; }
    }

    /// <summary>
    /// Captures the route while it still resolves and reads it back once the content has been unpublished, as a
    /// handler that purges an external cache by URL would.
    /// </summary>
    private sealed class UnpublishedUrlCapturingHandler :
        INotificationHandler<ContentUnpublishingNotification>,
        INotificationHandler<ContentUnpublishedNotification>
    {
        private const string StateKey = "UnpublishedUrlCapturingHandler.Routes";

        private readonly UnpublishedUrlRecorder _recorder;
        private readonly IDocumentUrlService _documentUrlService;

        public UnpublishedUrlCapturingHandler(UnpublishedUrlRecorder recorder, IDocumentUrlService documentUrlService)
        {
            _recorder = recorder;
            _documentUrlService = documentUrlService;
        }

        public void Handle(ContentUnpublishingNotification notification)
            => notification.State[StateKey] = notification.UnpublishedEntities
                .ToDictionary(x => x.Key, x => GetRoute(x.Key));

        public void Handle(ContentUnpublishedNotification notification)
        {
            if (notification.State.TryGetValue(StateKey, out var value) && value is Dictionary<Guid, string> routes)
            {
                _recorder.UrlFromState = routes.GetValueOrDefault(_recorder.Key);
            }

            _recorder.UrlWhenUnpublished = GetRoute(_recorder.Key);
        }

        private string GetRoute(Guid key) => _documentUrlService.GetLegacyRouteFormat(key, null, false);
    }
}
