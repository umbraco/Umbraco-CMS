// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.Web.Common.Attributes;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    public class RedirectUrlManagementController : UmbracoAuthorizedApiController
    {
        private readonly ILogger<RedirectUrlManagementController> _logger;
        private readonly WebRoutingSettings _webRoutingSettings;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IRedirectUrlService _redirectUrlService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfigManipulator _configManipulator;

        public RedirectUrlManagementController(
            ILogger<RedirectUrlManagementController> logger,
            IOptions<WebRoutingSettings> webRoutingSettings,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IRedirectUrlService redirectUrlService,
            UmbracoMapper umbracoMapper,
            IHostingEnvironment hostingEnvironment,
            IConfigManipulator configManipulator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webRoutingSettings = webRoutingSettings.Value ?? throw new ArgumentNullException(nameof(webRoutingSettings));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _redirectUrlService = redirectUrlService ?? throw new ArgumentNullException(nameof(redirectUrlService));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _configManipulator = configManipulator ?? throw new ArgumentNullException(nameof(configManipulator));
        }

        /// <summary>
        /// Returns true/false of whether redirect tracking is enabled or not
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetEnableState()
        {
            var enabled = _webRoutingSettings.DisableRedirectUrlTracking == false;
            var userIsAdmin = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.IsAdmin();
            return Ok(new { enabled, userIsAdmin });
        }

        //add paging
        [HttpGet]
        public RedirectUrlSearchResult SearchRedirectUrls(string searchTerm, int page = 0, int pageSize = 10)
        {
            var searchResult = new RedirectUrlSearchResult();
            long resultCount;

            var redirects = string.IsNullOrWhiteSpace(searchTerm)
                ? _redirectUrlService.GetAllRedirectUrls(page, pageSize, out resultCount)
                : _redirectUrlService.SearchRedirectUrls(searchTerm, page, pageSize, out resultCount);

            searchResult.SearchResults = _umbracoMapper.MapEnumerable<IRedirectUrl, ContentRedirectUrl>(redirects);
            searchResult.TotalCount = resultCount;
            searchResult.CurrentPage = page;
            searchResult.PageCount = ((int)resultCount + pageSize - 1) / pageSize;

            return searchResult;

        }
        /// <summary>
        /// This lists the RedirectUrls for a particular content item
        /// Do we need to consider paging here?
        /// </summary>
        /// <param name="contentUdi">Udi of content item to retrieve RedirectUrls for</param>
        /// <returns></returns>
        [HttpGet]
        public RedirectUrlSearchResult RedirectUrlsForContentItem(string contentUdi)
        {
            var redirectsResult = new RedirectUrlSearchResult();
            if (UdiParser.TryParse(contentUdi, out GuidUdi guidIdi))
            {

                var redirects = _redirectUrlService.GetContentRedirectUrls(guidIdi.Guid);
                var mapped = _umbracoMapper.MapEnumerable<IRedirectUrl, ContentRedirectUrl>(redirects);
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
            var userIsAdmin = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.IsAdmin();
            if (userIsAdmin == false)
            {
                var errorMessage = "User is not a member of the administrators group and so is not allowed to toggle the URL tracker";
                _logger.LogDebug(errorMessage);
                throw new SecurityException(errorMessage);
            }

            var action = disable ? "disable" : "enable";

            _configManipulator.SaveDisableRedirectUrlTracking(disable);

            return Ok($"URL tracker is now {action}d.");
        }
    }
}
