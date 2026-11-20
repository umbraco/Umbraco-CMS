// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
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
internal sealed class ContentFinderByRedirectUrlTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IDomainService DomainService => GetRequiredService<IDomainService>();

    private IUmbracoContextAccessor UmbracoContextAccessor => GetRequiredService<IUmbracoContextAccessor>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<DomainSavedNotification, DomainSavedDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentPublishingNotification, RedirectTrackingHandler>();
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, RedirectTrackingHandler>();
    }

    [SetUp]
    public async Task SetUpTest()
    {
        await GetRequiredService<IDocumentUrlService>().InitAsync(false, CancellationToken.None);

        // The redirect tracker resolves URLs through the URL provider, which reads the current request.
        GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext
        {
            Request = { Scheme = "https", Host = new HostString("localhost"), Path = "/" },
        };
    }

    /// <summary>
    /// Verifies that a redirect registered for a page below a culture-only (wildcard) domain is found for a request
    /// to the old URL on the hostname domain above it, as the request is routed by the hostname domain.
    /// </summary>
    [Test]
    public async Task TryFindContent_Redirects_Renamed_Page_Below_Culture_Only_Domain_Under_Hostname_Domain()
    {
        (IContent root, IContent section, IContent page) = await CreateSiteAsync();
        await AssignDomainsAsync(root, new DomainsUpdateModel { Domains = [new DomainModel { DomainName = "example.com", IsoCode = "en-US" }] });
        await AssignDomainsAsync(section, new DomainsUpdateModel { DefaultIsoCode = "en-US", Domains = [] });

        await RenameAndPublishAsync(page, "Renamed Page");

        await AssertRedirectsAsync(new Uri("https://example.com/section/page"), page, "https://example.com/section/renamed-page/");
    }

    /// <summary>
    /// Verifies that a redirect registered for a page below a culture-only (wildcard) domain, with no hostname domain
    /// above it, is found for a request to the old URL, which no domain matches.
    /// </summary>
    [Test]
    public async Task TryFindContent_Redirects_Renamed_Page_Below_Culture_Only_Domain_Without_Hostname_Domain()
    {
        (_, IContent section, IContent page) = await CreateSiteAsync();
        await AssignDomainsAsync(section, new DomainsUpdateModel { DefaultIsoCode = "en-US", Domains = [] });

        await RenameAndPublishAsync(page, "Renamed Page");

        await AssertRedirectsAsync(new Uri("https://localhost/section/page"), page, "/section/renamed-page/");
    }

    private async Task<(IContent Root, IContent Section, IContent Page)> CreateSiteAsync()
    {
        var contentType = new ContentTypeBuilder().WithAlias("page").Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContent root = await CreateAndPublishAsync(contentType, "Site", Constants.System.Root);
        IContent section = await CreateAndPublishAsync(contentType, "Section", root.Id);
        IContent page = await CreateAndPublishAsync(contentType, "Page", section.Id);
        return (root, section, page);
    }

    private async Task<IContent> CreateAndPublishAsync(IContentType contentType, string name, int parentId)
    {
        IContent content = new ContentBuilder().WithContentType(contentType).WithName(name).WithParentId(parentId).Build();
        await ContentService.SaveAsync(content, Constants.Security.SuperUserKey, null, CancellationToken.None);
        await ContentService.PublishAsync(content, ["*"], Constants.Security.SuperUserKey, CancellationToken.None);
        return content;
    }

    private async Task AssignDomainsAsync(IContent content, DomainsUpdateModel updateModel)
    {
        var result = await DomainService.UpdateDomainsAsync(content.Key, updateModel);
        Assert.That(result.Success, Is.True, $"Assigning domains to '{content.Name}' failed with {result.Status}.");
    }

    private async Task RenameAndPublishAsync(IContent content, string name)
    {
        UmbracoContextAccessor.Clear();
        UmbracoContextFactory.EnsureUmbracoContext();

        IContent reloaded = (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!;
        reloaded.Name = name;
        await ContentService.SaveAsync(reloaded, Constants.Security.SuperUserKey, null, CancellationToken.None);
        await ContentService.PublishAsync(reloaded, ["*"], Constants.Security.SuperUserKey, CancellationToken.None);
    }

    private async Task AssertRedirectsAsync(Uri oldUrl, IContent expectedContent, string expectedRedirectUrl)
    {
        UmbracoContextAccessor.Clear();
        UmbracoContextFactory.EnsureUmbracoContext();

        var request = new PublishedRequestBuilder(oldUrl, GetRequiredService<ITemplateService>());
        GetRequiredService<IPublishedRouter>().RouteDomain(request);

        var finder = ActivatorUtilities.CreateInstance<ContentFinderByRedirectUrl>(Services);
        var found = await finder.TryFindContent(request);
        IEnumerable<IRedirectUrl> redirectUrls = await GetRequiredService<IRedirectUrlService>().GetContentRedirectUrlsAsync(expectedContent.Key);

        Assert.Multiple(() =>
        {
            Assert.That(
                redirectUrls,
                Is.Not.Empty,
                "Renaming the page should have registered a redirect.");
            Assert.That(found, Is.True, $"No redirect was found for {oldUrl}.");
            Assert.That(request.Build().RedirectUrl, Is.EqualTo(expectedRedirectUrl));
        });
    }
}
