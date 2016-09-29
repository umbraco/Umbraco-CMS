using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using System.Linq;
using System.Xml;
using Umbraco.Core.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http;
using System;
using System.Net;
using Umbraco.Core.Cache;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using System.Text;

namespace Umbraco.Web.Editors
{
  
    //we need to fire up the controller like this to enable loading of remote css directly from this controller
    [PluginController("UmbracoApi")]
    [ValidationFilter]
    [AngularJsonOnlyConfiguration]
    [IsBackOffice]
    public class DashboardController : UmbracoApiController
    {
        //we have baseurl as a param to make previewing easier, so we can test with a dev domain from client side
        [WebApi.UmbracoAuthorize]
        [ValidateAngularAntiForgeryToken]
        public async Task<JObject> GetRemoteDashboardContent(string section, string baseUrl = "https://dashboard.umbraco.org/")
        {
            var ctx = UmbracoContext.Current;
            if (ctx == null)
                throw new HttpResponseException(System.Net.HttpStatusCode.InternalServerError);
            
            var user = Security.CurrentUser;
            var userType = user.UserType.Alias;
            var allowedSections = string.Join(",", user.AllowedSections);
            var lang = user.Language;
            var version = UmbracoVersion.GetSemanticVersion().ToSemanticString();

            var url = string.Format(baseUrl+"{0}?section={0}&type={1}&allowed={2}&lang={3}&version={4}", section, userType, allowedSections, lang, version);
            var key = "umb-dyn-dash-" + userType + lang + allowedSections + section;

            //look to see if we already have a requested result in the cache.
            var content = ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<JObject>(key);
            if (content != null)
                return content;
            try
            {
                var web = new HttpClient();

                //fetch dashboard json and parse to JObject
                var json = await web.GetStringAsync(url);
                content = JObject.Parse(json);
                
                //save server content for 30 mins
                ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem<JObject>(key, () => content, timeout: new TimeSpan(0, 30, 0));
            }
            catch (Exception ex)
            {
                //set the content to an empty result and cache for 5 mins
                //we return it like this, we avoid error codes which triggers UI warnings

                content = new JObject();
                ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem<JObject>(key, () => content, timeout: new TimeSpan(0, 5, 0));
            }

            return content;
        }

        [WebApi.UmbracoAuthorize]
        public async Task<HttpResponseMessage> GetRemoteDashboardCss(string section, string baseUrl = "https://dashboard.umbraco.org/")
        {
            var cssUrl = string.Format(baseUrl + "css/dashboard.css?section={0}", section);
            var key = "umb-dyn-dash-css-" + section;

            //look to see if we already have a requested result in the cache.
            var content = ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<string>(key);

            //else fetch remotely
            if (content == null)
            {
                try
                {
                    var web = new HttpClient();

                    //fetch remote css
                    content = await web.GetStringAsync(cssUrl);

                    //save server content for 30 mins
                    ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem<string>(key, () => content, timeout: new TimeSpan(0, 30, 0));
                }
                catch (Exception ex)
                {
                    //set the content to an empty result and cache for 5 mins
                    //we return it like this, we avoid error codes which triggers UI warnings

                    content = string.Empty;
                    ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem<string>(key, () => content, timeout: new TimeSpan(0, 5, 0));
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "text/css")
            };
            
        }


        [WebApi.UmbracoAuthorize]
        [ValidateAngularAntiForgeryToken]
        public IEnumerable<Tab<DashboardControl>> GetDashboard(string section)
        {
            var tabs = new List<Tab<DashboardControl>>();
            var i = 1;

            // The dashboard config can contain more than one area inserted by a package.
            foreach( var dashboardSection in UmbracoConfig.For.DashboardSettings().Sections.Where(x => x.Areas.Contains(section)))
            {
                //we need to validate access to this section
                if (DashboardSecurity.AuthorizeAccess(dashboardSection, Security.CurrentUser, Services.SectionService))
                {
                    //User is authorized
                    foreach (var dashTab in dashboardSection.Tabs)
                    {
                        //we need to validate access to this tab
                        if (DashboardSecurity.AuthorizeAccess(dashTab, Security.CurrentUser, Services.SectionService))
                        {
                            var props = new List<DashboardControl>();

                            foreach (var dashCtrl in dashTab.Controls)
                            {
                                if (DashboardSecurity.AuthorizeAccess(dashCtrl, Security.CurrentUser,
                                                                      Services.SectionService))
                                {
                                    var ctrl = new DashboardControl();
                                    var controlPath = dashCtrl.ControlPath.Trim(' ', '\r', '\n');
                                    ctrl.Path = IOHelper.FindFile(controlPath);
                                    if (controlPath.ToLower().EndsWith(".ascx"))
                                    {
                                        ctrl.ServerSide = true;
                                    }
                                    props.Add(ctrl);
                                }
                            }

                            tabs.Add(new Tab<DashboardControl>
                                {
                                    Id = i,
                                    Alias = dashTab.Caption.ToSafeAlias(),
                                    IsActive = i == 1,
                                    Label = dashTab.Caption,
                                    Properties = props
                                });
                            i++;
                        }
                    }
                }
            }

            //In case there are no tabs or a user doesn't have access the empty tabs list is returned
            return tabs;

        }

    }
}
