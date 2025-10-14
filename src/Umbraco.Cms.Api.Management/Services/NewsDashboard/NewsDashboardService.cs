using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services.NewsDashboard;

/// <inheritdoc />
public class NewsDashboardService : INewsDashboardService
{
    private readonly AppCaches _appCaches;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly ISiteIdentifierService _siteIdentifierService;
    private readonly ILogger<NewsDashboardService> _logger;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly GlobalSettings _globalSettings;

    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsDashboardService"/> class.
    /// </summary>
    public NewsDashboardService(
        AppCaches appCaches,
        IUmbracoVersion umbracoVersion,
        ISiteIdentifierService siteIdentifierService,
        ILogger<NewsDashboardService> logger,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IOptions<GlobalSettings> globalSettings)
    {
        _appCaches = appCaches;
        _umbracoVersion = umbracoVersion;
        _siteIdentifierService = siteIdentifierService;
        _logger = logger;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _globalSettings = globalSettings.Value;
    }

    /// <inheritdoc />
    public async Task<NewsDashboardResponseModel> GetItemsAsync()
    {
        const string BaseUrl = "https://umbraco-dashboard-news.euwest01.umbraco.io";
        const string Path = "/api/News";

        var version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild();
        _siteIdentifierService.TryGetOrCreateSiteIdentifier(out Guid siteIdentifier);

        var language = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Language ?? _globalSettings.DefaultUILanguage;

        var url = $"{BaseUrl}/{Path}?version={version}&siteId={siteIdentifier}&language={language}";

        const string CacheKey = "umbraco-dashboard-news";
        NewsDashboardResponseModel? content = _appCaches.RuntimeCache.GetCacheItem<NewsDashboardResponseModel>(CacheKey);
        if (content is not null)
        {
            return content;
        }

        try
        {
            var json = await _httpClient.GetStringAsync(url);

            if (TryMapModel(json, out NewsDashboardResponseModel? model))
            {
                _appCaches.RuntimeCache.InsertCacheItem(CacheKey, () => model, new TimeSpan(0, 30, 0));
                content = model;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.InnerException ?? ex, "Error getting dashboard content from {Url}", url);
        }

        return content ?? new NewsDashboardResponseModel { Items = [] };
    }

    private bool TryMapModel(string json, [MaybeNullWhen(false)] out NewsDashboardResponseModel newsDashboardResponseModel)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };

            List<NewsDashboardItemResponseModel>? items = JsonSerializer.Deserialize<List<NewsDashboardItemResponseModel>>(json, options);
            newsDashboardResponseModel = new NewsDashboardResponseModel { Items = items ?? [] };

            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex.InnerException ?? ex, "Error deserializing dashboard news items");
            newsDashboardResponseModel = null;
            return false;
        }
    }
}
