// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class SupressNotificationsTests : UmbracoIntegrationTest
    {
        private IContentService ContentService => GetRequiredService<IContentService>();

        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

        protected override void CustomTestSetup(IUmbracoBuilder builder)
        {
            base.CustomTestSetup(builder);

            builder.AddNotificationHandler<ContentSavingNotification, TestContentNotificationHandler>();
            builder.AddNotificationHandler<ContentTypeSavingNotification, TestContentTypeNotificationHandler>();
        }

        [Test]
        public void GivenScope_WhenNotificationsSuppressed_ThenNotificationsDoNotExecute()
        {
            using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
            using IDisposable _ = scope.Notifications.Supress();

            ContentType contentType = ContentTypeBuilder.CreateBasicContentType();
            ContentTypeService.Save(contentType);
            Content content = ContentBuilder.CreateBasicContent(contentType);
            ContentService.Save(content);

            Assert.Pass();
        }

        [Test]
        public void GivenNestedScope_WhenOuterNotificationsSuppressed_ThenNotificationsDoNotExecute()
        {
            using (IScope parentScope = ScopeProvider.CreateScope(autoComplete: true))
            {
                using IDisposable _ = parentScope.Notifications.Supress();

                using (IScope childScope = ScopeProvider.CreateScope(autoComplete: true))
                {
                    ContentType contentType = ContentTypeBuilder.CreateBasicContentType();
                    ContentTypeService.Save(contentType);
                    Content content = ContentBuilder.CreateBasicContent(contentType);
                    ContentService.Save(content);

                    Assert.Pass();
                }
            }
        }

        private class TestContentNotificationHandler : INotificationHandler<ContentSavingNotification>
        {
            public void Handle(ContentSavingNotification notification)
                => Assert.Fail("Notification was sent");
        }

        private class TestContentTypeNotificationHandler : INotificationHandler<ContentTypeSavingNotification>
        {
            public void Handle(ContentTypeSavingNotification notification)
                => Assert.Fail("Notification was sent");
        }
    }
}
