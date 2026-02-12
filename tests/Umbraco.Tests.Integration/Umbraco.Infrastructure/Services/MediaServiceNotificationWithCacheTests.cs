using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class MediaServiceNotificationWithCacheTests : UmbracoIntegrationTest
{
    private IMediaType _mediaType;

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    protected override void ConfigureTestServices(IServiceCollection services)
        => services.AddSingleton(AppCaches.Create(Mock.Of<IRequestCache>()));

    [SetUp]
    public void SetupTest()
    {
        ContentRepositoryBase.ThrowOnWarning = true;

        _mediaType = MediaTypeService.Get("folder")
                     ?? throw new ApplicationException("Could not find the \"folder\" media type");
    }

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<MediaSavingNotification, MediaNotificationHandler>()
        .AddNotificationHandler<MediaSavedNotification, MediaNotificationHandler>();

    [Test]
    public async Task Saving_Saved_Get_Value()
    {
        var createAttempt = await MediaEditingService.CreateAsync(
            new MediaCreateModel
            {
                ContentTypeKey = _mediaType.Key,
                Variants = [
                    new() { Name = "Initial name" }
                ],
            },
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(createAttempt.Success);
            Assert.IsNotNull(createAttempt.Result.Content);
        });

        var savingWasCalled = false;
        var savedWasCalled = false;

        MediaNotificationHandler.SavingMedia = notification =>
        {
            savingWasCalled = true;

            var saved = notification.SavedEntities.First();
            var documentById = MediaService.GetById(saved.Id)!;
            var documentByKey = MediaService.GetById(saved.Key)!;

            Assert.Multiple(() =>
            {
                Assert.AreEqual("Updated name", saved.Name);
                Assert.AreEqual("Initial name", documentById.Name);
                Assert.AreEqual("Initial name", documentByKey.Name);
            });
        };

        MediaNotificationHandler.SavedMedia = notification =>
        {
            savedWasCalled = true;

            var saved = notification.SavedEntities.First();
            var documentById = MediaService.GetById(saved.Id)!;
            var documentByKey = MediaService.GetById(saved.Key)!;

            Assert.Multiple(() =>
            {
                Assert.AreEqual("Updated name", saved.Name);
                Assert.AreEqual("Updated name", documentById.Name);
                Assert.AreEqual("Updated name", documentByKey.Name);
            });
        };

        try
        {
            var updateAttempt = await MediaEditingService.UpdateAsync(
                createAttempt.Result.Content!.Key,
                new MediaUpdateModel
                {
                    Variants = [
                        new() { Name = "Updated name" }
                    ],
                },
                Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(updateAttempt.Success);
                Assert.IsNotNull(updateAttempt.Result.Content);
            });

            Assert.IsTrue(savingWasCalled);
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            MediaNotificationHandler.SavingMedia = null;
            MediaNotificationHandler.SavedMedia = null;
        }
    }

    internal sealed class MediaNotificationHandler :
        INotificationHandler<MediaSavingNotification>,
        INotificationHandler<MediaSavedNotification>
    {
        public static Action<MediaSavingNotification>? SavingMedia { get; set; }

        public static Action<MediaSavedNotification>? SavedMedia { get; set; }

        public void Handle(MediaSavedNotification notification) => SavedMedia?.Invoke(notification);

        public void Handle(MediaSavingNotification notification) => SavingMedia?.Invoke(notification);
    }
}
