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
        //we have baseurl as a param to make previewing easier, so we can test with a dev domain from client side
        [ValidateAngularAntiForgeryToken]
        public async Task<JObject> GetRemoteDashboardContent(string section, string baseUrl = "https://dashboard.umbraco.org/")
        {
            var context = UmbracoContext.Current;
            if (context == null)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            var user = Security.CurrentUser;
            var userType = user.UserType.Alias;
            var allowedSections = string.Join(",", user.AllowedSections);
            var language = user.Language;
            var version = UmbracoVersion.SemanticVersion.ToSemanticString();

            var url = string.Format(baseUrl + "{0}?section={0}&type={1}&allowed={2}&lang={3}&version={4}", section, userType, allowedSections, language, version);
            var key = "umbraco-dynamic-dashboard-" + userType + language + allowedSections.Replace(",", "-") + section;

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
                    using (var web = new HttpClient())
                    {
                        //fetch dashboard json and parse to JObject
                        var json = await web.GetStringAsync(url);
                        content = JObject.Parse(json);
                        result = content;
                    }

                    ApplicationCache.RuntimeCache.InsertCacheItem<JObject>(key, () => result, new TimeSpan(0, 30, 0));
                }
                catch (HttpRequestException ex)
                {
                    Logger.Debug<DashboardController>($"Error getting dashboard content from '{url}': {ex.Message}\n{ex.InnerException}");

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
                    using (var web = new HttpClient())
                    {
                        //fetch remote css
                        content = await web.GetStringAsync(url);

                        //can't use content directly, modified closure problem
                        result = content;

                        //save server content for 30 mins
                        ApplicationCache.RuntimeCache.InsertCacheItem<string>(key, () => result, new TimeSpan(0, 30, 0));
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.Debug<DashboardController>(string.Format("Error getting dashboard CSS from '{0}': {1}\n{2}", url, ex.Message, ex.InnerException));

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
        public IEnumerable<Tab<DashboardControl>> GetDashboard(string section)
        {
            var tabs = new List<Tab<DashboardControl>>();
            var i = 1;

            // The dashboard config can contain more than one area inserted by a package.
            foreach( var dashboardSection in UmbracoConfig.For.DashboardSettings().Sections.Where(x => x.Areas.Contains(section)))
            {
                //we need to validate access to this section
                if (DashboardSecurity.AuthorizeAccess(dashboardSection, Security.CurrentUser, Services.SectionService) == false)
                    continue;

                //User is authorized
                foreach (var tab in dashboardSection.Tabs)
                {
                    //we need to validate access to this tab
                    if (DashboardSecurity.AuthorizeAccess(tab, Security.CurrentUser, Services.SectionService) == false)
                        continue;

                    var dashboardControls = new List<DashboardControl>();

                    foreach (var control in tab.Controls)
                    {
                        if (DashboardSecurity.AuthorizeAccess(control, Security.CurrentUser, Services.SectionService) == false)
                            continue;

                        var dashboardControl = new DashboardControl();
                        var controlPath = control.ControlPath.Trim();
                        dashboardControl.Path = IOHelper.FindFile(controlPath);
                        if (controlPath.ToLowerInvariant().EndsWith(".ascx".ToLowerInvariant()))
                            dashboardControl.ServerSide = true;

                        dashboardControls.Add(dashboardControl);
                    }

                    tabs.Add(new Tab<DashboardControl>
                    {
                        Id = i,
                        Alias = tab.Caption.ToSafeAlias(),
                        IsActive = i == 1,
                        Label = tab.Caption,
                        Properties = dashboardControls
                    });

                    i++;
                }
            }

            //In case there are no tabs or a user doesn't have access the empty tabs list is returned
            return tabs;
        }
    }
}
