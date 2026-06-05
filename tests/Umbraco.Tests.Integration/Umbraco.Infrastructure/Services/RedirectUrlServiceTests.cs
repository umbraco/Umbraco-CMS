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
        .AddNotificationHandler<RedirectUrlSavingNotification, RedirectUrlNotificationHandler>()
        .AddNotificationHandler<RedirectUrlSavedNotification, RedirectUrlNotificationHandler>()
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
        RedirectUrlNotificationHandler.Saving = null;
        RedirectUrlNotificationHandler.Saved = null;
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
    public void Can_RegisterWithStatus_Redirect()
    {
        const string TestUrl = "testUrl";

        var status = RedirectUrlService.RegisterWithStatus(TestUrl, _firstSubPage.Key);

        var redirect = RedirectUrlService.GetMostRecentRedirectUrl(TestUrl, CultureEnglish);

        Assert.AreEqual(redirect.ContentId, _firstSubPage.Id);
        Assert.AreEqual(status.Status, RedirectUrlOperationStatus.Success);
    }

    [Test]
    public void RegisterWithStatus_Publishes_Saving_And_Saved_Notifications()
    {
        const string OldUrl = "/old/url";

        RedirectUrlSavingNotification? savingCaptured = null;
        RedirectUrlSavedNotification? savedCaptured = null;
        RedirectUrlNotificationHandler.Saving = n => savingCaptured = n;
        RedirectUrlNotificationHandler.Saved = n => savedCaptured = n;

        var result = RedirectUrlService.RegisterWithStatus(OldUrl, _firstSubPage.Key, CultureEnglish);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(RedirectUrlOperationStatus.Success, result.Status);

            Assert.IsNotNull(savingCaptured);
            IRedirectUrl savingEntity = savingCaptured!.SavedEntities.Single();
            Assert.AreEqual(_firstSubPage.Key, savingEntity.ContentKey);
            Assert.AreEqual(OldUrl, savingEntity.Url);

            Assert.IsNotNull(savedCaptured);
            IRedirectUrl savedEntity = savedCaptured!.SavedEntities.Single();
            Assert.AreEqual(_firstSubPage.Key, savedEntity.ContentKey);
            Assert.AreEqual(OldUrl, savedEntity.Url);
        });
    }

    [Test]
    public void RegisterWithStatus_Returns_CancelledByNotification_When_Handler_Cancels_Saving()
    {
        const string OldUrl = "/cancelled/old";

        var savedWasCalled = false;
        RedirectUrlNotificationHandler.Saving = n => n.Cancel = true;
        RedirectUrlNotificationHandler.Saved = _ => savedWasCalled = true;

        var result = RedirectUrlService.RegisterWithStatus(OldUrl, _firstSubPage.Key, CultureEnglish);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(RedirectUrlOperationStatus.CancelledByNotification, result.Status);
            Assert.IsFalse(savedWasCalled, "Saved notification must not fire when Saving was cancelled.");
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
        INotificationHandler<RedirectUrlSavingNotification>,
        INotificationHandler<RedirectUrlSavedNotification>,
        INotificationHandler<RedirectUrlDeletingNotification>,
        INotificationHandler<RedirectUrlDeletedNotification>
    {
        public static Action<RedirectUrlSavingNotification>? Saving { get; set; }

        public static Action<RedirectUrlSavedNotification>? Saved { get; set; }

        public static Action<RedirectUrlDeletingNotification>? Deleting { get; set; }

        public static Action<RedirectUrlDeletedNotification>? Deleted { get; set; }

        public void Handle(RedirectUrlSavingNotification notification) => Saving?.Invoke(notification);

        public void Handle(RedirectUrlSavedNotification notification) => Saved?.Invoke(notification);

        public void Handle(RedirectUrlDeletingNotification notification) => Deleting?.Invoke(notification);

        public void Handle(RedirectUrlDeletedNotification notification) => Deleted?.Invoke(notification);
    }
}
