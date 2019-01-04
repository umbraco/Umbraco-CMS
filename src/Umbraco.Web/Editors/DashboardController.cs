using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http;
using System;
using System.Net;
using System.Text;
using Umbraco.Core.Cache;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Logging;

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
        private readonly Dashboards _dashboards;

        public DashboardController(Dashboards dashboards)
        {
            _dashboards = dashboards;
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

            var content = ApplicationCache.RuntimeCache.GetCacheItem<JObject>(key);
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

                    ApplicationCache.RuntimeCache.InsertCacheItem<JObject>(key, () => result, new TimeSpan(0, 30, 0));
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error<DashboardController>(ex.InnerException ?? ex, "Error getting dashboard content from '{Url}'", url);

                    //it's still new JObject() - we return it like this to avoid error codes which triggers UI warnings
                    ApplicationCache.RuntimeCache.InsertCacheItem<JObject>(key, () => result, new TimeSpan(0, 5, 0));
                }
            }

            return result;
        }

        public async Task<HttpResponseMessage> GetRemoteDashboardCss(string section, string baseUrl = "https://dashboard.umbraco.org/")
        {
            var url = string.Format(baseUrl + "css/dashboard.css?section={0}", section);
            var key = "umbraco-dynamic-dashboard-css-" + section;

            var content = ApplicationCache.RuntimeCache.GetCacheItem<string>(key);
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
                        ApplicationCache.RuntimeCache.InsertCacheItem<string>(key, () => result, new TimeSpan(0, 30, 0));
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error<DashboardController>(ex.InnerException ?? ex, "Error getting dashboard CSS from '{Url}'", url);

                    //it's still string.Empty - we return it like this to avoid error codes which triggers UI warnings
                    ApplicationCache.RuntimeCache.InsertCacheItem<string>(key, () => result, new TimeSpan(0, 5, 0));
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result, Encoding.UTF8, "text/css")
            };
        }

        [ValidateAngularAntiForgeryToken]
        [OutgoingEditorModelEvent]
        public IEnumerable<Tab<DashboardControl>> GetDashboard(string section)
        {
            return _dashboards.GetDashboards(section, Security.CurrentUser);
        }
    }
}
