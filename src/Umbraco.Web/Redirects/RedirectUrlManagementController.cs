using System;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;
using File = System.IO.File;

namespace Umbraco.Web.Redirects
{
    public class RedirectUrlManagementController : UmbracoAuthorizedApiController
    {
        //add paging
        [HttpGet]
        public RedirectUrlSearchResult SearchRedirectUrls(string searchTerm, int page = 0, int pageSize = 10)
        {
            var searchResult = new RedirectUrlSearchResult { UrlTrackerDisabled = UmbracoConfig.For.UmbracoSettings().WebRouting.DisableRedirectUrlTracking };
            var redirectUrlService = Services.RedirectUrlService;
            var resultCount = 0L;

            var redirects = string.IsNullOrWhiteSpace(searchTerm) 
                ? redirectUrlService.GetAllRedirectUrls(page, pageSize, out resultCount)
                : redirectUrlService.SearchRedirectUrls(searchTerm, page, pageSize, out resultCount);

            searchResult.SearchResults = redirects;
            searchResult.TotalCount = resultCount;
            searchResult.CurrentPage = page;
            searchResult.PageCount = ((int)resultCount + pageSize - 1) / pageSize;

            searchResult.HasSearchResults = resultCount > 0;
            searchResult.HasExactMatch = (resultCount == 1);
            return searchResult;

        }

        [HttpGet]
        public HttpResponseMessage GetPublishedUrl(int id)
        {
            var publishedUrl = "#";
            if (id > 0)
                publishedUrl = Umbraco.Url(id);

            return new HttpResponseMessage { Content = new StringContent(publishedUrl, Encoding.UTF8, "text/html") };

        }

        [HttpPost]
        public IHttpActionResult DeleteRedirectUrl(Guid id)
        {
            var redirectUrlService = Services.RedirectUrlService;
            redirectUrlService.Delete(id);
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult ToggleUrlTracker(bool disable)
        {
            var configFilePath = HostingEnvironment.MapPath("~/config/umbracoSettings.config");

            var action = disable ? "disable" : "enable";

            if (File.Exists(configFilePath) == false)
                return BadRequest(string.Format("Couldn't {0} URL Tracker, the umbracoSettings.config file does not exist.", action));

            var umbracoConfig = new XmlDocument { PreserveWhitespace = true };
            umbracoConfig.Load(configFilePath);

            var webRoutingElement = umbracoConfig.SelectSingleNode("//web.routing") as XmlElement;
            if (webRoutingElement == null)
                return BadRequest(string.Format("Couldn't {0} URL Tracker, the web.routing element was not found in umbracoSettings.config.", action));

            // note: this adds the attribute if it does not exist
            webRoutingElement.SetAttribute("disableRedirectUrlTracking", disable.ToString().ToLowerInvariant());
            umbracoConfig.Save(configFilePath);

            return Ok(string.Format("URL tracker is now {0}d", action));
        }
    }
}