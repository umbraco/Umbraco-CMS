using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services.NewsDashboard;

public class NewsDashboardService : INewsDashboardService
{
    private readonly AppCaches _appCaches;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly ISiteIdentifierService _siteIdentifierService;
    private readonly ILogger<NewsDashboardService> _logger;
    private static readonly HttpClient HttpClient = new();


    public NewsDashboardService(AppCaches appCaches, IUmbracoVersion umbracoVersion, ISiteIdentifierService siteIdentifierService, ILogger<NewsDashboardService> logger)
    {
        _appCaches = appCaches;
        _umbracoVersion = umbracoVersion;
        _siteIdentifierService = siteIdentifierService;
        _logger = logger;
    }

    public async Task<NewsDashboardResponseModel> GetArticlesAsync()
    {
        var baseUrl = "https://umbraco-dashboard-news.euwest01.umbraco.io";
        var path = "/api/News";
        var key = "umbraco-dashboard-news";
        var version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild();
        _siteIdentifierService.TryGetOrCreateSiteIdentifier(out Guid siteIdentifier);

        var url = $"{baseUrl}/{path}?version={version}&siteId={siteIdentifier}";

        NewsDashboardResponseModel? content = _appCaches.RuntimeCache.GetCacheItem<NewsDashboardResponseModel>(key);
        if (content is not null)
        {
            return content;
        }

        try
        {
            var json = await HttpClient.GetStringAsync(url);

            if (TryMapModel(json, out NewsDashboardResponseModel? model))
            {
                _appCaches.RuntimeCache.InsertCacheItem(key, () => model, new TimeSpan(0, 30, 0));
                content = model;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.InnerException ?? ex, "Error getting dashboard content from {Url}", url);
        }

        return content ?? new NewsDashboardResponseModel { Articles = [] };
    }

    private bool TryMapModel(string json, [MaybeNullWhen(false)] out NewsDashboardResponseModel newsDashboardResponseModel)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };

            List<NewsDashboardArticleResponseModel>? articles = JsonSerializer.Deserialize<List<NewsDashboardArticleResponseModel>>(json, options);
            newsDashboardResponseModel = new NewsDashboardResponseModel { Articles = articles ?? [] };

            return true;
        }
        catch (JsonException)
        {
            newsDashboardResponseModel = null;
            return false;
        }
    }
}
