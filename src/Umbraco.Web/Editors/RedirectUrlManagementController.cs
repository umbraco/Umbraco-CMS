﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Xml;
using umbraco.businesslogic.Exceptions;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
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
            if (GuidUdi.TryParse(contentUdi, out var guidIdi))
            {
                var redirectUrlService = Services.RedirectUrlService;
                var redirects = redirectUrlService.GetContentRedirectUrls(guidIdi.Guid);
                redirectsResult.SearchResults = Mapper.Map<IEnumerable<ContentRedirectUrl>>(redirects).ToArray();
                //not doing paging 'yet'
                redirectsResult.TotalCount = redirects.Count();
                redirectsResult.CurrentPage = 1;
                redirectsResult.PageCount = 1;
            }
            return redirectsResult;
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


        /// <summary>
        /// Returns true/false of whether redirect tracking is enabled and the user has permission to add redirects
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetEnableStateForContent(int contentId)
        {
            var enabled = UmbracoConfig.For.UmbracoSettings().WebRouting.DisableRedirectUrlTracking == false;
            var userIsAllowed = IsUserRedirectEditEnabled(contentId);
            return Ok(new { enabled, userIsAllowed });
        }

        private bool IsUserRedirectEditEnabled(int contentId)
        {
            var user = Umbraco.UmbracoContext.Security.CurrentUser;
            //Validate user
            var permissions = Services.UserService.GetPermissions(user, contentId);
            return permissions.Any(p => p.AssignedPermissions.Contains(ActionPublish.Instance.Letter.ToString()) == true && p.AssignedPermissions.Contains(ActionUpdate.Instance.Letter.ToString()) == true);
        }

        [HttpPost]
        public IHttpActionResult AddRedirect(string url, int contentId)
        {
            
            if(IsUserRedirectEditEnabled(contentId) == false)
            {
                return Unauthorized();
            }
            //Validate url            
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("Url is required");
            }

            url = url.Trim().TrimEnd("/");
            var validUrlRegx = new Regex(@"^([\/]{1})[^\/].*(?<!\/)$");
            if (!validUrlRegx.IsMatch(url))
            {
                return BadRequest("Url is not valid");
            }

            //ensure url does not point to a file as redirector adds a trailing slash
            if (url.IndexOf('.') > -1)
            {
                return BadRequest("Url cannot contain a dot.");
            }

            //ensure url does not have a query string            
            if (url.IndexOf("?") > -1)
            {
                return BadRequest("Url cannot contain a querystring.");
            }

            var target = Services.ContentService.GetById(contentId);
            Services.RedirectUrlService.Register(url, target.Key);
            return Ok(string.Format("Url {0} now redirects to {1}", url, target.Name));                  
        }
    }
}
