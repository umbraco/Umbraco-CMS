using NUnit.Framework;
using Umbraco.Cms.Core.Handlers;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class DocumentUrlServiceTest : UmbracoIntegrationTestWithContent
{
    protected IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder
            .AddNotificationAsyncHandler<ContentPublishedNotification, RoutingNotificationHandler>();

    }

    [Test]
    [LongRunning]
    public async Task RebuildAllUrlsAsync()
    {
        ContentService.PublishBranch(Textpage, true, []);

        for (int i = 3; i < 10; i++)
        {
            var unusedSubPage = ContentBuilder.CreateSimpleContent(ContentType, "Text Page " + i, Textpage.Id);
            unusedSubPage.Key = Guid.NewGuid();
            ContentService.Save(unusedSubPage);
            ContentService.Publish(unusedSubPage, new string[0]);
        }

        await DocumentUrlService.RebuildAllUrlsAsync();

    }
}
