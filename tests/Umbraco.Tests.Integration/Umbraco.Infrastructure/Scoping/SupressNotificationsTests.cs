// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class SuppressNotificationsTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();
    private IMediaService MediaService => GetRequiredService<IMediaService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.AddNotificationHandler<ContentSavingNotification, TestContentNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeSavingNotification, TestContentTypeNotificationHandler>();
        builder.AddNotificationHandler<MediaSavedNotification, TestMediaNotificationHandler>();
    }

    [Test]
    public void GivenScope_WhenNotificationsSuppressed_ThenNotificationsDoNotExecute()
    {
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        using var _ = scope.Notifications.Suppress();

        var contentType = ContentTypeBuilder.CreateBasicContentType();
        ContentTypeService.Save(contentType);
        var content = ContentBuilder.CreateBasicContent(contentType);
        ContentService.Save(content);
    }

    [Test]
    public void GivenNestedScope_WhenOuterNotificationsSuppressed_ThenNotificationsDoNotExecute()
    {
        using (var parentScope = ScopeProvider.CreateScope(autoComplete: true))
        {
            using var _ = parentScope.Notifications.Suppress();

            using (var childScope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var contentType = ContentTypeBuilder.CreateBasicContentType();
                ContentTypeService.Save(contentType);
                var content = ContentBuilder.CreateBasicContent(contentType);
                ContentService.Save(content);
            }
        }
    }

    [Test]
    public void GivenSuppressedNotifications_WhenDisposed_ThenNotificationsExecute()
    {
        var asserted = 0;
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            using var suppressed = scope.Notifications.Suppress();

            var mediaType = MediaTypeBuilder.CreateImageMediaType("test");
            MediaTypeService.Save(mediaType);

            suppressed.Dispose();

            asserted = TestContext.CurrentContext.AssertCount;
            var media = MediaBuilder.CreateMediaImage(mediaType, -1);
            MediaService.Save(media);
        }

        Assert.AreEqual(asserted + 1, TestContext.CurrentContext.AssertCount);
    }

    [Test]
    public void GivenSuppressedNotificationsOnParent_WhenChildSuppresses_ThenExceptionIsThrown()
    {
        using (var parentScope = ScopeProvider.CreateScope(autoComplete: true))
        using (var parentSuppressed = parentScope.Notifications.Suppress())
        {
            using (var childScope = ScopeProvider.CreateScope(autoComplete: true))
            {
                Assert.Throws<InvalidOperationException>(() => childScope.Notifications.Suppress());
            }
        }
    }

    private class TestContentNotificationHandler : INotificationHandler<ContentSavingNotification>
    {
        public void Handle(ContentSavingNotification notification)
            => Assert.Fail("Notification was sent");
    }

    private class TestMediaNotificationHandler : INotificationHandler<MediaSavedNotification>
    {
        public void Handle(MediaSavedNotification notification)
            => Assert.IsNotNull(notification);
    }

    private class TestContentTypeNotificationHandler : INotificationHandler<ContentTypeSavingNotification>
    {
        public void Handle(ContentTypeSavingNotification notification)
            => Assert.Fail("Notification was sent");
    }
}
