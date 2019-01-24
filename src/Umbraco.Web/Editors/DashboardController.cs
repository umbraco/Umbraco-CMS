using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using System.Linq;
using Umbraco.Core.IO;
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
        //we have just one instance of HttpClient shared for the entire application
        private static readonly HttpClient HttpClient = new HttpClient();
        //we have baseurl as a param to make previewing easier, so we can test with a dev domain from client side
        [ValidateAngularAntiForgeryToken]
        public async Task<JObject> GetRemoteDashboardContent(string section, string baseUrl = "https://dashboard.umbraco.org/")
        {
            var context = UmbracoContext.Current;
            if (context == null)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            var user = Security.CurrentUser;
            var allowedSections = string.Join(",", user.AllowedSections);
            var language = user.Language;
            var version = UmbracoVersion.GetSemanticVersion().ToSemanticString();

            var url = string.Format(baseUrl + "{0}?section={0}&allowed={1}&lang={2}&version={3}", section, allowedSections, language, version);
            var key = "umbraco-dynamic-dashboard-" + language + allowedSections.Replace(",", "-") + section;

            var content = ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<JObject>(key);
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

                    ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem<JObject>(key, () => result, new TimeSpan(0, 30, 0));
                }
                catch (HttpRequestException ex)
                {
                    LogHelper.Debug<DashboardController>(string.Format("Error getting dashboard content from '{0}': {1}\n{2}", url, ex.Message, ex.InnerException));

                    //it's still new JObject() - we return it like this to avoid error codes which triggers UI warnings
                    ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem<JObject>(key, () => result, new TimeSpan(0, 5, 0));
                }
            }

            return result;
        }

        public async Task<HttpResponseMessage> GetRemoteDashboardCss(string section, string baseUrl = "https://dashboard.umbraco.org/")
        {
            var url = string.Format(baseUrl + "css/dashboard.css?section={0}", section);
            var key = "umbraco-dynamic-dashboard-css-" + section;

            var content = ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<string>(key);
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
                    ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem<string>(key, () => result, new TimeSpan(0, 30, 0));
                }
                catch (HttpRequestException ex)
                {
                    LogHelper.Debug<DashboardController>(string.Format("Error getting dashboard CSS from '{0}': {1}\n{2}", url, ex.Message, ex.InnerException));

                    //it's still string.Empty - we return it like this to avoid error codes which triggers UI warnings
                    ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem<string>(key, () => result, new TimeSpan(0, 5, 0));
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result, Encoding.UTF8, "text/css")
            };
        }

        [ValidateAngularAntiForgeryToken]
        public IEnumerable<Tab<DashboardControl>> GetDashboard(string section)
        {
            var dashboardHelper = new DashboardHelper(Services.SectionService);
            return dashboardHelper.GetDashboard(section, Security.CurrentUser);
        }
    }
}
