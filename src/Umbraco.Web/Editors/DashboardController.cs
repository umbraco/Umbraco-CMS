using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Linq;
using System.Net;
using System.Text;
using Umbraco.Core.Cache;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Models;
using Umbraco.Web.Services;
using System.Web.Http;

namespace Umbraco.Web.Editors
{
    //we need to fire up the controller like this to enable loading of remote css directly from this controller
    [PluginController("UmbracoApi")]
    [ValidationFilter]
    [AngularJsonOnlyConfiguration]
    [IsBackOffice]
    [WebApi.UmbracoAuthorize]

    public class DashboardController : UmbracoApiController
    {
        private readonly IDashboardService _dashboardService;
        private readonly IContentDashboardSettings _dashboardSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> with all its dependencies.
        /// </summary>
        public DashboardController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger,
            IRuntimeState runtimeState, IDashboardService dashboardService, UmbracoHelper umbracoHelper,
            IContentDashboardSettings dashboardSettings)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _dashboardService = dashboardService;
            _dashboardSettings = dashboardSettings;
        }

        //we have just one instance of HttpClient shared for the entire application
        private static readonly HttpClient HttpClient = new HttpClient();

        //we have baseurl as a param to make previewing easier, so we can test with a dev domain from client side
        [ValidateAngularAntiForgeryToken]
        public async Task<JObject> GetRemoteDashboardContent(string section, string baseUrl = "https://dashboard.umbraco.com/")
        {
            var user = Security.CurrentUser;
            var allowedSections = string.Join(",", user.AllowedSections);
            var language = user.Language;
            var version = UmbracoVersion.SemanticVersion.ToSemanticString();
            var isAdmin = user.IsAdmin();

            VerifyDashboardSource(baseUrl);

            var url = string.Format("{0}{1}?section={2}&allowed={3}&lang={4}&version={5}&admin={6}",
                baseUrl,
                _dashboardSettings.ContentDashboardPath,
                section,
                allowedSections,
                language,
                version,
                isAdmin);
            var key = "umbraco-dynamic-dashboard-" + language + allowedSections.Replace(",", "-") + section;

            var content = AppCaches.RuntimeCache.GetCacheItem<JObject>(key);
            var result = new JObject();
            if (content != null)
            {
                result = content;
            }
            else
            {
                //content is null, go get it
                try
                {
                    //fetch dashboard json and parse to JObject
                    var json = await HttpClient.GetStringAsync(url);
                    content = JObject.Parse(json);
                    result = content;

                    AppCaches.RuntimeCache.InsertCacheItem<JObject>(key, () => result, new TimeSpan(0, 30, 0));
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error<DashboardController, string>(ex.InnerException ?? ex, "Error getting dashboard content from {Url}", url);

                    //it's still new JObject() - we return it like this to avoid error codes which triggers UI warnings
                    AppCaches.RuntimeCache.InsertCacheItem<JObject>(key, () => result, new TimeSpan(0, 5, 0));
                }
            }

            return result;
        }

        public async Task<HttpResponseMessage> GetRemoteDashboardCss(string section, string baseUrl = "https://dashboard.umbraco.org/")
        {
            VerifyDashboardSource(baseUrl);

            var url = string.Format(baseUrl + "css/dashboard.css?section={0}", section);
            var key = "umbraco-dynamic-dashboard-css-" + section;

            var content = AppCaches.RuntimeCache.GetCacheItem<string>(key);
            var result = string.Empty;

            if (content != null)
            {
                result = content;
            }
            else
            {
                //content is null, go get it
                try
                {
                    //fetch remote css
                    content = await HttpClient.GetStringAsync(url);

                    //can't use content directly, modified closure problem
                    result = content;

                    //save server content for 30 mins
                    AppCaches.RuntimeCache.InsertCacheItem<string>(key, () => result, new TimeSpan(0, 30, 0));
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error<DashboardController, string>(ex.InnerException ?? ex, "Error getting dashboard CSS from {Url}", url);

                    //it's still string.Empty - we return it like this to avoid error codes which triggers UI warnings
                    AppCaches.RuntimeCache.InsertCacheItem<string>(key, () => result, new TimeSpan(0, 5, 0));
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result, Encoding.UTF8, "text/css")
            };
        }

        public async Task<HttpResponseMessage> GetRemoteXml(string site, string url)
        {
            VerifyDashboardSource(url);

            // This is used in place of the old feedproxy.config
            // Which was used to grab data from our.umbraco.com, umbraco.com or umbraco.tv
            // for certain dashboards or the help drawer
            var urlPrefix = string.Empty;
            switch (site.ToUpper())
            {
                case "TV":
                    urlPrefix = "https://umbraco.tv/";
                    break;

                case "OUR":
                    urlPrefix = "https://our.umbraco.com/";
                    break;

                case "COM":
                    urlPrefix = "https://umbraco.com/";
                    break;

                default:
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
            }


            //Make remote call to fetch videos or remote dashboard feed data
            var key = $"umbraco-XML-feed-{site}-{url.ToCleanString(Core.Strings.CleanStringType.UrlSegment)}";

            var content = AppCaches.RuntimeCache.GetCacheItem<string>(key);
            var result = string.Empty;

            if (content != null)
            {
                result = content;
            }
            else
            {
                //content is null, go get it
                try
                {
                    //fetch remote css
                    content = await HttpClient.GetStringAsync($"{urlPrefix}{url}");

                    //can't use content directly, modified closure problem
                    result = content;

                    //save server content for 30 mins
                    AppCaches.RuntimeCache.InsertCacheItem<string>(key, () => result, new TimeSpan(0, 30, 0));
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error<DashboardController, string, string>(ex.InnerException ?? ex, "Error getting remote dashboard data from {UrlPrefix}{Url}", urlPrefix, url);

                    //it's still string.Empty - we return it like this to avoid error codes which triggers UI warnings
                    AppCaches.RuntimeCache.InsertCacheItem<string>(key, () => result, new TimeSpan(0, 5, 0));
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result, Encoding.UTF8, "text/xml")
            };

        }

        // return IDashboardSlim - we don't need sections nor access rules
        [ValidateAngularAntiForgeryToken]
        [OutgoingEditorModelEvent]
        public IEnumerable<Tab<IDashboardSlim>> GetDashboard(string section)
        {
            return _dashboardService.GetDashboards(section, Security.CurrentUser).Select(x => new Tab<IDashboardSlim>
            {
                Id = x.Id,
                Key = x.Key,
                Type = x.Type,
                Alias = x.Alias,
                Label = x.Label,
                Expanded = x.Expanded,
                IsActive = x.IsActive,
                Properties = x.Properties.Select(y => new DashboardSlim
                {
                    Alias = y.Alias,
                    View = y.View
                })
            }).ToList();
        }

        // Checks if the passed URL is part of the configured allowlist of addresses
        private bool IsAllowedUrl(string url)
        {
            // No addresses specified indicates that any URL is allowed
            if (string.IsNullOrEmpty(_dashboardSettings.ContentDashboardUrlAllowlist) || _dashboardSettings.ContentDashboardUrlAllowlist.Contains(url))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void VerifyDashboardSource(string url)
        {
            if(!IsAllowedUrl(url))
            {
                Logger.Error<DashboardController>($"The following URL is not listed in the allowlist for ContentDashboardUrl in the Web.config: {url}");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Dashboard source not permitted"));
            }
        }
    }
}
