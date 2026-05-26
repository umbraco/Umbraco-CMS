// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RedirectUrlServiceTests : UmbracoIntegrationTestWithContent
{
    private IContent _firstSubPage;
    private IContent _secondSubPage;
    private IContent _thirdSubPage;
    private const string Url = "blah";
    private const string UrlAlt = "alternativeUrl";
    private const string CultureEnglish = "en";
    private const string CultureGerman = "de";
    private const string UnusedCulture = "es";

    private IRedirectUrlService RedirectUrlService => GetRequiredService<IRedirectUrlService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<RedirectUrlCreatingNotification, RedirectUrlNotificationHandler>()
        .AddNotificationHandler<RedirectUrlCreatedNotification, RedirectUrlNotificationHandler>()
        .AddNotificationHandler<RedirectUrlDeletingNotification, RedirectUrlNotificationHandler>()
        .AddNotificationHandler<RedirectUrlDeletedNotification, RedirectUrlNotificationHandler>();

    public override async Task CreateTestDataAsync()
    {
        await base.CreateTestDataAsync();

        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = new RedirectUrlRepository(
                (IScopeAccessor)ScopeProvider,
                AppCaches.Disabled,
                Mock.Of<ILogger<RedirectUrlRepository>>(),
                Mock.Of<IRepositoryCacheVersionService>(),
                Mock.Of<ICacheSyncService>());
            var rootContent = ContentService.GetRootContent().First();
            var subPages = ContentService.GetPagedChildren(rootContent.Id, 0, 3, out _, propertyAliases: null, filter: null, ordering: null).ToList();
            _firstSubPage = subPages[0];
            _secondSubPage = subPages[1];
            _thirdSubPage = subPages[2];


            repository.Save(new RedirectUrl { ContentKey = _firstSubPage.Key, Url = Url, Culture = CultureEnglish });
            Thread.Sleep(
                1000); //Added delay to ensure timestamp difference as sometimes they seem to have the same timestamp
            repository.Save(new RedirectUrl { ContentKey = _secondSubPage.Key, Url = Url, Culture = CultureGerman });
            Thread.Sleep(1000);
            repository.Save(new RedirectUrl { ContentKey = _thirdSubPage.Key, Url = UrlAlt, Culture = string.Empty });

            scope.Complete();
        }
    }

    [TearDown]
    public void ResetNotificationHandler()
    {
        RedirectUrlNotificationHandler.Creating = null;
        RedirectUrlNotificationHandler.Created = null;
        RedirectUrlNotificationHandler.Deleting = null;
        RedirectUrlNotificationHandler.Deleted = null;
    }

    [Test]
    [LongRunning]
    public void Can_Get_Most_Recent_RedirectUrl()
    {
        var redirect = RedirectUrlService.GetMostRecentRedirectUrl(Url);
        Assert.AreEqual(redirect.ContentId, _secondSubPage.Id);
    }

    [Test]
    [LongRunning]
    public void Can_Get_Most_Recent_RedirectUrl_With_Culture()
    {
        var redirect = RedirectUrlService.GetMostRecentRedirectUrl(Url, CultureEnglish);
        Assert.AreEqual(redirect.ContentId, _firstSubPage.Id);
    }

    [Test]
    [LongRunning]
    public void Can_Get_Most_Recent_RedirectUrl_With_Culture_When_No_CultureVariant_Exists()
    {
        var redirect = RedirectUrlService.GetMostRecentRedirectUrl(UrlAlt, UnusedCulture);
        Assert.AreEqual(redirect.ContentId, _thirdSubPage.Id);
    }

    [Test]
    public void Can_Register_Redirect()
    {
        const string TestUrl = "testUrl";

        RedirectUrlService.Register(TestUrl, _firstSubPage.Key);

        var redirect = RedirectUrlService.GetMostRecentRedirectUrl(TestUrl, CultureEnglish);

        Assert.AreEqual(redirect.ContentId, _firstSubPage.Id);
    }

    [Test]
    public void Register_Publishes_Creating_And_Created_Notifications_With_New_Url()
    {
        const string OldUrl = "/old/url";
        const string NewUrl = "/new/url";

        RedirectUrlCreatingNotification? creatingCaptured = null;
        RedirectUrlCreatedNotification? createdCaptured = null;
        RedirectUrlNotificationHandler.Creating = n => creatingCaptured = n;
        RedirectUrlNotificationHandler.Created = n => createdCaptured = n;

        var result = RedirectUrlService.Register(OldUrl, NewUrl, _firstSubPage.Key, CultureEnglish);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(RedirectUrlOperationStatus.Success, result.Status);

            Assert.IsNotNull(creatingCaptured);
            Assert.AreEqual(_firstSubPage.Key, creatingCaptured!.ContentKey);
            Assert.AreEqual(OldUrl, creatingCaptured.OldUrl);
            Assert.AreEqual(NewUrl, creatingCaptured.NewUrl);

            Assert.IsNotNull(createdCaptured);
            Assert.AreEqual(_firstSubPage.Key, createdCaptured!.ContentKey);
            Assert.AreEqual(OldUrl, createdCaptured.OldUrl);
            Assert.AreEqual(NewUrl, createdCaptured.NewUrl);
        });
    }

    [Test]
    public void Register_Returns_CancelledByNotification_When_Handler_Cancels_Creating()
    {
        const string OldUrl = "/cancelled/old";
        const string NewUrl = "/cancelled/new";

        var createdWasCalled = false;
        RedirectUrlNotificationHandler.Creating = n => n.Cancel = true;
        RedirectUrlNotificationHandler.Created = _ => createdWasCalled = true;

        var result = RedirectUrlService.Register(OldUrl, NewUrl, _firstSubPage.Key, CultureEnglish);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(RedirectUrlOperationStatus.CancelledByNotification, result.Status);
            Assert.IsFalse(createdWasCalled, "Created notification must not fire when Creating was cancelled.");
        });

        // Verify nothing was persisted.
        var persisted = RedirectUrlService.GetMostRecentRedirectUrl(OldUrl, CultureEnglish);
        Assert.IsNull(persisted);
    }

    [Test]
    public void DeleteWithStatus_By_Entity_Returns_CancelledByNotification_When_Handler_Cancels()
    {
        IRedirectUrl existing = RedirectUrlService.GetContentRedirectUrls(_firstSubPage.Key).First();

        var deletedWasCalled = false;
        RedirectUrlNotificationHandler.Deleting = n => n.Cancel = true;
        RedirectUrlNotificationHandler.Deleted = _ => deletedWasCalled = true;

        var status = RedirectUrlService.DeleteWithStatus(existing);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(RedirectUrlOperationStatus.CancelledByNotification, status);
            Assert.IsFalse(deletedWasCalled);
        });

        // Verify the redirect still exists.
        Assert.IsNotNull(RedirectUrlService.GetContentRedirectUrls(_firstSubPage.Key).FirstOrDefault(r => r.Key == existing.Key));
    }

    [Test]
    public void DeleteWithStatus_By_Id_Returns_NotFound_When_Missing()
    {
        var status = RedirectUrlService.DeleteWithStatus(Guid.NewGuid());

        Assert.AreEqual(RedirectUrlOperationStatus.NotFound, status);
    }

    [Test]
    public void DeleteContentRedirectUrlsWithStatus_Cancels_All_When_Handler_Cancels()
    {
        RedirectUrlNotificationHandler.Deleting = n => n.Cancel = true;

        var status = RedirectUrlService.DeleteContentRedirectUrlsWithStatus(_firstSubPage.Key);

        Assert.AreEqual(RedirectUrlOperationStatus.CancelledByNotification, status);
        Assert.IsNotEmpty(RedirectUrlService.GetContentRedirectUrls(_firstSubPage.Key));
    }

    internal sealed class RedirectUrlNotificationHandler :
        INotificationHandler<RedirectUrlCreatingNotification>,
        INotificationHandler<RedirectUrlCreatedNotification>,
        INotificationHandler<RedirectUrlDeletingNotification>,
        INotificationHandler<RedirectUrlDeletedNotification>
    {
        public static Action<RedirectUrlCreatingNotification>? Creating { get; set; }

        public static Action<RedirectUrlCreatedNotification>? Created { get; set; }

        public static Action<RedirectUrlDeletingNotification>? Deleting { get; set; }

        public static Action<RedirectUrlDeletedNotification>? Deleted { get; set; }

        public void Handle(RedirectUrlCreatingNotification notification) => Creating?.Invoke(notification);

        public void Handle(RedirectUrlCreatedNotification notification) => Created?.Invoke(notification);

        public void Handle(RedirectUrlDeletingNotification notification) => Deleting?.Invoke(notification);

        public void Handle(RedirectUrlDeletedNotification notification) => Deleted?.Invoke(notification);
    }
}
