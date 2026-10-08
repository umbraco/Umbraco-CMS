// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Routing;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Mock)]
internal sealed class ContentFinderByUrlAliasTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUmbracoContextAccessor UmbracoContextAccessor => GetRequiredService<IUmbracoContextAccessor>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
    }

    [SetUp]
    public async Task SetUpTest()
    {
        await GetRequiredService<IDocumentUrlService>().InitAsync(false, CancellationToken.None);
        await GetRequiredService<IDocumentUrlAliasService>().InitAsync(false, CancellationToken.None);
    }

    [Test]
    public async Task TryFindContent_Resolves_Published_Document_When_Unpublished_Document_Held_The_Same_Alias()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("pageWithAlias")
            .AddPropertyType()
                .WithAlias(Constants.Conventions.Content.UrlAlias)
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .WithValueStorageType(ValueStorageType.Nvarchar)
                .Done()
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var pageA = new ContentBuilder().WithContentType(contentType).WithName("Page A").Build();
        pageA.SetValue(Constants.Conventions.Content.UrlAlias, "shared-alias");
        ContentService.Save(pageA);
        ContentService.Publish(pageA, ["*"]);

        // Unpublishing retains the last published property values, including the alias.
        ContentService.Unpublish(pageA);
        IContent reloadedPageA = ContentService.GetById(pageA.Key)!;
        reloadedPageA.SetValue(Constants.Conventions.Content.UrlAlias, "archived-alias");
        ContentService.Save(reloadedPageA);

        var pageB = new ContentBuilder().WithContentType(contentType).WithName("Page B").Build();
        pageB.SetValue(Constants.Conventions.Content.UrlAlias, "shared-alias");
        ContentService.Save(pageB);
        ContentService.Publish(pageB, ["*"]);

        UmbracoContextAccessor.Clear();
        UmbracoContextFactory.EnsureUmbracoContext();

        var finder = ActivatorUtilities.CreateInstance<ContentFinderByUrlAlias>(Services);
        var request = new PublishedRequestBuilder(new Uri("https://localhost/shared-alias"), GetRequiredService<ITemplateService>());

        var found = await finder.TryFindContent(request);

        Assert.That(found, Is.True);
        Assert.That(request.PublishedContent?.Key, Is.EqualTo(pageB.Key));
    }
}
