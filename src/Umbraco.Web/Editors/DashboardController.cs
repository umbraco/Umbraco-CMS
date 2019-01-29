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
using Umbraco.Web.Services;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> with auto dependencies.
        /// </summary>
        public DashboardController()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> with all its dependencies.
        /// </summary>
        public DashboardController(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, IDashboardService dashboardService)
            : base(globalSettings, umbracoContext, sqlContext, services, appCaches, logger, runtimeState)
        {
            _dashboardService = dashboardService;
        }

        //we have just one instance of HttpClient shared for the entire application
        private static readonly HttpClient HttpClient = new HttpClient();

        //we have baseurl as a param to make previewing easier, so we can test with a dev domain from client side
        [ValidateAngularAntiForgeryToken]
        public async Task<JObject> GetRemoteDashboardContent(string section, string baseUrl = "https://dashboard.umbraco.org/")
        {
            var user = Security.CurrentUser;
            var allowedSections = string.Join(",", user.AllowedSections);
            var language = user.Language;
            var version = UmbracoVersion.SemanticVersion.ToSemanticString();

            var url = string.Format(baseUrl + "{0}?section={0}&allowed={1}&lang={2}&version={3}", section, allowedSections, language, version);
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
                    Logger.Error<DashboardController>(ex.InnerException ?? ex, "Error getting dashboard content from {Url}", url);

                    //it's still new JObject() - we return it like this to avoid error codes which triggers UI warnings
                    AppCaches.RuntimeCache.InsertCacheItem<JObject>(key, () => result, new TimeSpan(0, 5, 0));
                }
            }

            return result;
        }

        public async Task<HttpResponseMessage> GetRemoteDashboardCss(string section, string baseUrl = "https://dashboard.umbraco.org/")
        {
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
                    Logger.Error<DashboardController>(ex.InnerException ?? ex, "Error getting dashboard CSS from {Url}", url);

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
                    urlPrefix = "https://our.umbraco.org/";
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
                    Logger.Error<DashboardController>(ex.InnerException ?? ex, "Error getting remote dashboard data from {UrlPrefix}{Url}", urlPrefix, url);

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
                Alias = x.Alias,
                Label = x.Label,
                Expanded = x.Expanded,
                IsActive = x.IsActive,
                Properties = x.Properties.Select(y => new DashboardSlim
                {
                    Alias = y.Alias,
                    View = y.View
                })
            });
        }
    }
}
