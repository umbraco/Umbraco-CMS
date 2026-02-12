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
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class ContentServiceNotificationWithCacheTests : UmbracoIntegrationTest
{
    private IContentType _contentType;

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    protected override void ConfigureTestServices(IServiceCollection services)
        => services.AddSingleton(AppCaches.Create(Mock.Of<IRequestCache>()));

    [SetUp]
    public async Task SetupTest()
    {
        ContentRepositoryBase.ThrowOnWarning = true;

        _contentType = ContentTypeBuilder.CreateBasicContentType();
        _contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);
    }

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ContentSavingNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentSavedNotification, ContentNotificationHandler>();

    [Test]
    public async Task Saving_Saved_Get_Value()
    {
        var createAttempt = await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = _contentType.Key,
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

        ContentNotificationHandler.SavingContent = notification =>
        {
            savingWasCalled = true;

            var saved = notification.SavedEntities.First();
            var documentById = ContentService.GetById(saved.Id)!;
            var documentByKey = ContentService.GetById(saved.Key)!;

            Assert.Multiple(() =>
            {
                Assert.AreEqual("Updated name", saved.Name);
                Assert.AreEqual("Initial name", documentById.Name);
                Assert.AreEqual("Initial name", documentByKey.Name);
            });
        };

        ContentNotificationHandler.SavedContent = notification =>
        {
            savedWasCalled = true;

            var saved = notification.SavedEntities.First();
            var documentById = ContentService.GetById(saved.Id)!;
            var documentByKey = ContentService.GetById(saved.Key)!;

            Assert.Multiple(() =>
            {
                Assert.AreEqual("Updated name", saved.Name);
                Assert.AreEqual("Updated name", documentById.Name);
                Assert.AreEqual("Updated name", documentByKey.Name);
            });
        };

        try
        {
            var updateAttempt = await ContentEditingService.UpdateAsync(
                createAttempt.Result.Content!.Key,
                new ContentUpdateModel
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
            ContentNotificationHandler.SavingContent = null;
            ContentNotificationHandler.SavedContent = null;
        }
    }

    internal sealed class ContentNotificationHandler :
        INotificationHandler<ContentSavingNotification>,
        INotificationHandler<ContentSavedNotification>
    {
        public static Action<ContentSavingNotification>? SavingContent { get; set; }

        public static Action<ContentSavedNotification>? SavedContent { get; set; }

        public void Handle(ContentSavedNotification notification) => SavedContent?.Invoke(notification);

        public void Handle(ContentSavingNotification notification) => SavingContent?.Invoke(notification);
    }
}
