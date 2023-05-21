// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class RedirectUrlManagementController : UmbracoAuthorizedApiController
{
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IConfigManipulator _configManipulator;
    private readonly ILogger<RedirectUrlManagementController> _logger;
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;

    public RedirectUrlManagementController(
        ILogger<RedirectUrlManagementController> logger,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IRedirectUrlService redirectUrlService,
        IUmbracoMapper umbracoMapper,
        IConfigManipulator configManipulator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webRoutingSettings = webRoutingSettings ?? throw new ArgumentNullException(nameof(webRoutingSettings));
        _backofficeSecurityAccessor = backofficeSecurityAccessor ??
                                      throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
        _redirectUrlService = redirectUrlService ?? throw new ArgumentNullException(nameof(redirectUrlService));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        _configManipulator = configManipulator ?? throw new ArgumentNullException(nameof(configManipulator));
    }

    /// <summary>
    ///     Returns true/false of whether redirect tracking is enabled or not
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetEnableState()
    {
        var enabled = _webRoutingSettings.CurrentValue.DisableRedirectUrlTracking == false;
        var userIsAdmin = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.IsAdmin() ?? false;
        return Ok(new { enabled, userIsAdmin });
    }

    //add paging
    [HttpGet]
    public RedirectUrlSearchResult SearchRedirectUrls(string searchTerm, int page = 0, int pageSize = 10)
    {
        var searchResult = new RedirectUrlSearchResult();

        IEnumerable<IRedirectUrl> redirects = string.IsNullOrWhiteSpace(searchTerm)
            ? _redirectUrlService.GetAllRedirectUrls(page, pageSize, out long resultCount)
            : _redirectUrlService.SearchRedirectUrls(searchTerm, page, pageSize, out resultCount);

        searchResult.SearchResults =
            _umbracoMapper.MapEnumerable<IRedirectUrl, ContentRedirectUrl>(redirects).WhereNotNull();
        searchResult.TotalCount = resultCount;
        searchResult.CurrentPage = page;
        searchResult.PageCount = ((int)resultCount + pageSize - 1) / pageSize;

        return searchResult;
    }

    /// <summary>
    ///     This lists the RedirectUrls for a particular content item
    ///     Do we need to consider paging here?
    /// </summary>
    /// <param name="contentUdi">Udi of content item to retrieve RedirectUrls for</param>
    /// <returns></returns>
    [HttpGet]
    public RedirectUrlSearchResult RedirectUrlsForContentItem(string contentUdi)
    {
        var redirectsResult = new RedirectUrlSearchResult();
        if (UdiParser.TryParse(contentUdi, out GuidUdi? guidIdi))
        {
            IEnumerable<IRedirectUrl> redirects = _redirectUrlService.GetContentRedirectUrls(guidIdi!.Guid);
            var mapped = _umbracoMapper.MapEnumerable<IRedirectUrl, ContentRedirectUrl>(redirects).WhereNotNull()
                .ToList();
            redirectsResult.SearchResults = mapped;
            //not doing paging 'yet'
            redirectsResult.TotalCount = mapped.Count;
            redirectsResult.CurrentPage = 1;
            redirectsResult.PageCount = 1;
        }

        return redirectsResult;
    }

    [HttpPost]
    public IActionResult DeleteRedirectUrl(Guid id)
    {
        _redirectUrlService.Delete(id);
        return Ok();
    }

    [HttpPost]
    public IActionResult ToggleUrlTracker(bool disable)
    {
        var userIsAdmin = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.IsAdmin();
        if (userIsAdmin == false)
        {
            var errorMessage =
                "User is not a member of the administrators group and so is not allowed to toggle the URL tracker";
            _logger.LogDebug(errorMessage);
            throw new SecurityException(errorMessage);
        }

        var action = disable ? "disable" : "enable";

        _configManipulator.SaveDisableRedirectUrlTracking(disable);

        // TODO this is ridiculous, but we need to ensure the configuration is reloaded, before this request is ended.
        // otherwise we can read the old value in GetEnableState.
        // The value is equal to JsonConfigurationSource.ReloadDelay
        Thread.Sleep(250);

        return Ok($"URL tracker is now {action}d.");
    }
}
