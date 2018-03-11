using System;
using System.Web.Http;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using umbraco.businesslogic.Exceptions;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using File = System.IO.File;
using Umbraco.Core;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class RedirectUrlManagementController : UmbracoAuthorizedApiController
    {

        /// <summary>
        /// Returns true/false of whether redirect tracking is enabled or not
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetEnableState()
        {
            var enabled = UmbracoConfig.For.UmbracoSettings().WebRouting.DisableRedirectUrlTracking == false;
            var userIsAdmin = Umbraco.UmbracoContext.Security.CurrentUser.IsAdmin();
            return Ok(new { enabled, userIsAdmin });
        }

        //add paging
        [HttpGet]
        public RedirectUrlSearchResult SearchRedirectUrls(string searchTerm, int page = 0, int pageSize = 10)
        {
            var searchResult = new RedirectUrlSearchResult();
            var redirectUrlService = Services.RedirectUrlService;
            long resultCount;

            var redirects = string.IsNullOrWhiteSpace(searchTerm) 
                ? redirectUrlService.GetAllRedirectUrls(page, pageSize, out resultCount)
                : redirectUrlService.SearchRedirectUrls(searchTerm, page, pageSize, out resultCount);

            searchResult.SearchResults = Mapper.Map<IEnumerable<ContentRedirectUrl>>(redirects).ToArray();
            //now map the Content/published url
            foreach (var result in searchResult.SearchResults)
            {
                result.DestinationUrl = result.ContentId > 0 ? Umbraco.Url(result.ContentId) : "#";
            }

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
            GuidUdi guidIdi;
            var contentKey = GuidUdi.TryParse(contentUdi, out guidIdi) ? guidIdi.Guid : default(Guid);
            //hmm what to do if this doesn't parse as a Guid?
            var redirectUrlService = Services.RedirectUrlService;
            var redirects = redirectUrlService.GetContentRedirectUrls(contentKey);
            redirectsResult.SearchResults = Mapper.Map<IEnumerable<ContentRedirectUrl>>(redirects).ToArray();
            //now map the Content/published - don't need to do this? - not displaying the target Url on the info tab, no need to retrieve in the same was as on the dashboard
            //not doing paging 'yet'
            redirectsResult.TotalCount = redirects.Count();
            redirectsResult.CurrentPage = 1;
            redirectsResult.PageCount = 1;
            return redirectsResult;
        }
        [HttpPost]
        public IHttpActionResult DeleteRedirectUrl(Guid id)
        {
            var redirectUrlService = Services.RedirectUrlService;
            redirectUrlService.Delete(id);
            return Ok();
        }
        /// <summary>
        /// Creates a redirect in the Redirect Url Tracking table for a particular relative Url to a specific content item identified by Udi
        /// </summary>
        /// <param name="url">Relative url to redirect from</param>
        /// <param name="id">Udi of content item to redirect to</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CreateRedirectUrl(string url, string id)
        {
            if (String.IsNullOrEmpty(url)){
                //should we be validating this url here, to make sure people not passing http or /oldsystem.php style urls, this is just for nice vanity redirects, or Url changes not tracked
                return BadRequest("Missing RedirectFromUrl");
            }
            var redirectUrlService = Services.RedirectUrlService;
            Guid contentKey = default(Guid);
            if (!String.IsNullOrEmpty(id))
            {
                GuidUdi guidIdi;
                contentKey = GuidUdi.TryParse(id, out guidIdi) ? guidIdi.Guid : default(Guid);
            }
            try
            {
                if (contentKey != Guid.Empty)
                {
                    redirectUrlService.Register(url, contentKey);
                    return Ok();
                }
                else
                {
                    return BadRequest(id + " not a valid Guid Udi");
                }
             
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost]
        public IHttpActionResult ToggleUrlTracker(bool disable)
        {
            var userIsAdmin = Umbraco.UmbracoContext.Security.CurrentUser.IsAdmin();
            if (userIsAdmin == false)
            {
                var errorMessage = "User is not a member of the administrators group and so is not allowed to toggle the URL tracker";
                LogHelper.Debug<RedirectUrlManagementController>(errorMessage);
                throw new UserAuthorizationException(errorMessage);
            }

            var httpContext = TryGetHttpContext();
            if (httpContext.Success == false) throw new InvalidOperationException("Cannot acquire HttpContext");
            var configFilePath = httpContext.Result.Server.MapPath("~/config/umbracoSettings.config");

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
