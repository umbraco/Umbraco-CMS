using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.Request;

public abstract class ApiContentRequestTestBase : UmbracoIntegrationTest
{
    protected IContentService ContentService => GetRequiredService<IContentService>();

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IApiContentRouteBuilder ApiContentRouteBuilder => GetRequiredService<IApiContentRouteBuilder>();

    protected IVariationContextAccessor VariationContextAccessor => GetRequiredService<IVariationContextAccessor>();

    protected IUmbracoContextAccessor UmbracoContextAccessor => GetRequiredService<IUmbracoContextAccessor>();

    protected IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();

        builder.AddDeliveryApi();
    }

    [TearDown]
    public async Task CleanUpAfterTest()
    {
        var domainService = GetRequiredService<IDomainService>();
        foreach (var content in ContentService.GetRootContent())
        {
            await domainService.UpdateDomainsAsync(content.Key, new DomainsUpdateModel { Domains = [] });
        }

        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext?.Request.Headers.Clear();
    }

    protected void SetVariationContext(string? culture)
        => VariationContextAccessor.VariationContext = new VariationContext(culture: culture);

    protected async Task SetContentHost(IContent content, string host, string culture)
        => await GetRequiredService<IDomainService>().UpdateDomainsAsync(
            content.Key,
            new DomainsUpdateModel { Domains = [new DomainModel { DomainName = host, IsoCode = culture }] });

    protected void SetRequestHost(string host)
    {
        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();

        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString(host),
                Path = "/",
                QueryString = new QueryString(string.Empty)
            },
            RequestServices = Services
        };
    }

    protected void SetRequestStartItem(string startItem)
    {
        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();
        if (httpContextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException("HTTP context is null");
        }

        httpContextAccessor.HttpContext.Request.Headers["Start-Item"] = startItem;
    }

    protected void RefreshContentCache()
        => Services.GetRequiredService<ContentCacheRefresher>().Refresh([new ContentCacheRefresher.JsonPayload { ChangeTypes = TreeChangeTypes.RefreshAll }]);
}
