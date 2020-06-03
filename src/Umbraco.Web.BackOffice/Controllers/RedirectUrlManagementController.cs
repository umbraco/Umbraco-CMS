using System;
using System.Xml;
using System.Security;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.Mapping;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Attributes;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController("UmbracoApi")]
    public class RedirectUrlManagementController : UmbracoAuthorizedApiController
    {
        private readonly ILogger _logger;
        private readonly IWebRoutingSettings _webRoutingSettings;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IRedirectUrlService _redirectUrlService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IHostingEnvironment _hostingEnvironment;

        public RedirectUrlManagementController(ILogger logger,
            IWebRoutingSettings webRoutingSettings,
            IUmbracoContextAccessor umbracoContextAccessor,
            IRedirectUrlService redirectUrlService,
            UmbracoMapper umbracoMapper,
            IHostingEnvironment hostingEnvironment)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webRoutingSettings = webRoutingSettings ?? throw new ArgumentNullException(nameof(webRoutingSettings));
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _redirectUrlService = redirectUrlService ?? throw new ArgumentNullException(nameof(redirectUrlService));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        /// <summary>
        /// Returns true/false of whether redirect tracking is enabled or not
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetEnableState()
        {
            var enabled = _webRoutingSettings.DisableRedirectUrlTracking == false;
            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            var userIsAdmin = umbracoContext.Security.CurrentUser.IsAdmin();
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
            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            var userIsAdmin = umbracoContext.Security.CurrentUser.IsAdmin();
            if (userIsAdmin == false)
            {
                var errorMessage = "User is not a member of the administrators group and so is not allowed to toggle the URL tracker";
                _logger.Debug<RedirectUrlManagementController>(errorMessage);
                throw new SecurityException(errorMessage);
            }
            var configFilePath =_hostingEnvironment.MapPathContentRoot("~/config/umbracoSettings.config");

            var action = disable ? "disable" : "enable";

            if (System.IO.File.Exists(configFilePath) == false)
                return BadRequest($"Couldn't {action} URL Tracker, the umbracoSettings.config file does not exist.");

            var umbracoConfig = new XmlDocument { PreserveWhitespace = true };
            umbracoConfig.Load(configFilePath);

            var webRoutingElement = umbracoConfig.SelectSingleNode("//web.routing") as XmlElement;
            if (webRoutingElement == null)
                return BadRequest($"Couldn't {action} URL Tracker, the web.routing element was not found in umbracoSettings.config.");

            // note: this adds the attribute if it does not exist
            webRoutingElement.SetAttribute("disableRedirectUrlTracking", disable.ToString().ToLowerInvariant());
            umbracoConfig.Save(configFilePath);

            return Ok($"URL tracker is now {action}d.");
        }
    }
}
