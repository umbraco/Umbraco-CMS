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
            var userIsAdmin = Umbraco.UmbracoContext.Security.CurrentUser.IsAdmin();
            if (userIsAdmin == false)
            {
                var errorMessage = string.Format("User of type {0} is not allowed to toggle the URL tracker", 
                    Umbraco.UmbracoContext.Security.CurrentUser.UserType.Alias);
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