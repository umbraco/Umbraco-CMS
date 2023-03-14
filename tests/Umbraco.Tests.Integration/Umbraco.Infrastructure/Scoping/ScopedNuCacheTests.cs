using System;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Tests.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ScopedNuCacheTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();
    private Mock<IHttpContextAccessor> MockHttpContextAccessor { get; } = CreateMockHttpContextAccessor();
    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private IVariationContextAccessor VariationContextAccessor => GetRequiredService<IVariationContextAccessor>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        NotificationHandler.PublishedContent = notification => { };
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddCoreNotifications();
        builder.AddNotificationHandler<ContentPublishedNotification, NotificationHandler>();
        builder.Services.AddUnique<IUmbracoContextAccessor, TestUmbracoContextAccessor>();
        builder.Services.AddUnique(MockHttpContextAccessor.Object);
        builder.AddNuCache();
    }

    public class NotificationHandler : INotificationHandler<ContentPublishedNotification>
    {
        public static Action<ContentPublishedNotification> PublishedContent { get; set; }
        public void Handle(ContentPublishedNotification notification) => PublishedContent?.Invoke(notification);
    }

    private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor()
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");

        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);
        return mockHttpContextAccessor;
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestScope(bool complete)
    {
        var umbracoContext = UmbracoContextFactory.EnsureUmbracoContext().UmbracoContext;

        // create document type, document
        var contentType = new ContentType(ShortStringHelper, -1) { Alias = "CustomDocument", Name = "Custom Document" };
        ContentTypeService.Save(contentType);
        var item = new Content("name", -1, contentType);

        using (var scope = ScopeProvider.CreateScope())
        {
            ContentService.SaveAndPublish(item);
            scope.Complete();
        }

        // event handler
        var evented = 0;
        NotificationHandler.PublishedContent = notification =>
        {
            evented++;

            var e = umbracoContext.Content.GetById(item.Id);

            // during events, due to LiveSnapshot, we see the changes
            Assert.IsNotNull(e);
            Assert.AreEqual("changed", e.Name(VariationContextAccessor));
        };

        // been created
        var x = umbracoContext.Content.GetById(item.Id);
        Assert.IsNotNull(x);
        Assert.AreEqual("name", x.Name(VariationContextAccessor));

        using (var scope = ScopeProvider.CreateScope())
        {
            item.Name = "changed";
            ContentService.SaveAndPublish(item);

            if (complete)
            {
                scope.Complete();
            }
        }

        // only 1 event occuring because we are publishing twice for the same event for
        // the same object and the scope deduplicates the events (uses the latest)
        Assert.AreEqual(complete ? 1 : 0, evented);

        // after the scope,
        // if completed, we see the changes
        // else changes have been rolled back
        x = umbracoContext.Content.GetById(item.Id);
        Assert.IsNotNull(x);
        Assert.AreEqual(complete ? "changed" : "name", x.Name(VariationContextAccessor));
    }
}
