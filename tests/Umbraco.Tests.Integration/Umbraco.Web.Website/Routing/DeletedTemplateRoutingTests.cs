using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.Website.Routing;

/// <summary>
///     Verifies that routing handles published content whose template has since been deleted.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class DeletedTemplateRoutingTests : UmbracoIntegrationTest
{
    private readonly TestVariationContextAccessor _variationContextAccessor = new();

    private ITemplate _template = null!;
    private Content _page = null!;

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IVariationContextAccessor>(_variationContextAccessor);
        builder.AddUmbracoHybridCache();
        builder.Services.Configure<WebRoutingSettings>(settings => settings.ValidateAlternativeTemplates = true);

        // Use real caches and run the cache refreshers, so the caches behave as they do when a template is deleted in a
        // running site.
        builder.Services.AddUnique(_ => new AppCaches(
            new DeepCloneAppCache(new ObjectCacheAppCache()),
            NoAppCache.Instance,
            new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache()))));
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<TemplateDeletedNotification, TemplateDeletedDistributedCacheNotificationHandler>();
    }

    [SetUp]
    public async Task SetUp()
    {
        DeleteAllTemplateViewFiles();

        _template = await CreateTemplateAsync("Routed Page", "routedPage");

        ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("routedPageType", "Routed Page Type");
        contentType.AllowedTemplates = [_template];
        contentType.SetDefaultTemplate(_template);
        var contentTypeResult = await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        Assert.That(contentTypeResult.Success, Is.True, $"Creating the content type failed with {contentTypeResult.Result}.");

        _page = ContentBuilder.CreateSimpleContent(contentType, "Routed Page");
        _page.TemplateId = _template.Id;
        ContentService.Save(_page);
        ContentService.Publish(_page, ["*"]);

        _variationContextAccessor.VariationContext = new VariationContext(
            await GetRequiredService<ILanguageService>().GetDefaultIsoCodeAsync());
    }

    [TearDown]
    public void TearDownTemplateFiles() => DeleteAllTemplateViewFiles();

    [Test]
    public async Task Routing_Resolves_No_Template_When_The_Template_Of_The_Published_Content_Was_Deleted()
    {
        await DeleteTemplateAsync();

        IPublishedRequest request = await RouteAsync(string.Empty);

        Assert.That(request.PublishedContent?.Key, Is.EqualTo(_page.Key));
        Assert.That(request.Template, Is.Null);
    }

    [Test]
    public async Task Routing_Resolves_No_Template_When_Falling_Back_From_A_Disallowed_Alternative_Template_To_A_Deleted_Template()
    {
        await CreateTemplateAsync("Not Allowed", "notAllowed");
        await DeleteTemplateAsync();

        IPublishedRequest request = await RouteAsync("?altTemplate=notAllowed");

        Assert.That(request.PublishedContent?.Key, Is.EqualTo(_page.Key));
        Assert.That(request.Template, Is.Null);
    }

    private async Task<ITemplate> CreateTemplateAsync(string name, string alias)
    {
        var result = await TemplateService.CreateAsync(name, alias, "@{ Layout = null; }", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True, $"Creating template {alias} failed with {result.Status}.");
        return result.Result;
    }

    private async Task DeleteTemplateAsync()
    {
        var result = await TemplateService.DeleteAsync(_template.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // The published content still references the deleted template until the page is republished.
        IPublishedContent? publishedPage = GetRequiredService<IPublishedContentCache>().GetById(_page.Key);
        Assert.That(publishedPage?.TemplateId, Is.EqualTo(_template.Id));
    }

    private async Task<IPublishedRequest> RouteAsync(string queryString)
    {
        // The HTTP and Umbraco contexts are held in AsyncLocals, so they are established here, in the same
        // execution context as the routing calls, rather than in an awaited SetUp whose values would not flow back.
        GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString("localhost"),
                Path = "/",
                QueryString = new QueryString(queryString),
            },
        };
        GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();

        var router = GetRequiredService<IPublishedRouter>();
        IPublishedRequestBuilder builder = await router.CreateRequestAsync(new Uri("https://localhost/" + queryString));
        return await router.RouteRequestAsync(builder, new RouteRequestOptions(RouteDirection.Inbound));
    }
}
