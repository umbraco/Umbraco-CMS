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
internal sealed class ElementServiceNotificationWithCacheTests : UmbracoIntegrationTest
{
    private IContentType _contentType;

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    protected override void ConfigureTestServices(IServiceCollection services)
        => services.AddSingleton(AppCaches.Create(Mock.Of<IRequestCache>()));

    [SetUp]
    public async Task SetupTest()
    {
        ContentRepositoryBase.ThrowOnWarning = true;

        _contentType = ContentTypeBuilder.CreateBasicElementType();
        _contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);
    }

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ElementSavingNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementSavedNotification, ElementNotificationHandler>();

    [Test]
    public async Task Saving_Saved_Get_Value()
    {
        var createAttempt = await ElementEditingService.CreateAsync(
            new ElementCreateModel
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

        ElementNotificationHandler.SavingElement = notification =>
        {
            savingWasCalled = true;

            var saved = notification.SavedEntities.First();
            var element = ElementService.GetById(saved.Key)!;

            Assert.Multiple(() =>
            {
                Assert.AreEqual("Updated name", saved.Name);
                Assert.AreEqual("Initial name", element.Name);
            });
        };

        ElementNotificationHandler.SavedElement = notification =>
        {
            savedWasCalled = true;

            var saved = notification.SavedEntities.First();
            var element = ElementService.GetById(saved.Key)!;

            Assert.Multiple(() =>
            {
                Assert.AreEqual("Updated name", saved.Name);
                Assert.AreEqual("Updated name", element.Name);
            });
        };

        try
        {
            var updateAttempt = await ElementEditingService.UpdateAsync(
                createAttempt.Result.Content!.Key,
                new ElementUpdateModel
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
            ElementNotificationHandler.SavingElement = null;
            ElementNotificationHandler.SavedElement = null;
        }
    }

    internal sealed class ElementNotificationHandler :
        INotificationHandler<ElementSavingNotification>,
        INotificationHandler<ElementSavedNotification>
    {
        public static Action<ElementSavingNotification>? SavingElement { get; set; }

        public static Action<ElementSavedNotification>? SavedElement { get; set; }

        public void Handle(ElementSavedNotification notification) => SavedElement?.Invoke(notification);

        public void Handle(ElementSavingNotification notification) => SavingElement?.Invoke(notification);
    }
}
