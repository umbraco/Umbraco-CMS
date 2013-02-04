using System;
using System.Configuration;
using System.Security.Authentication;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Install;
using umbraco;
using umbraco.businesslogic.Exceptions;

namespace Umbraco.Web.UI.Install
{
    public partial class InstallerRestService : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LogHelper.Info<InstallerRestService>(string.Format("Hitting Page_Load on InstallerRestService.aspx for the requested '{0}' feed", Request.QueryString["feed"]));

            // Stop Caching in IE
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);

            // Stop Caching in Firefox
            Response.Cache.SetNoStore();

            string feed = Request.QueryString["feed"];
            string url = "http://our.umbraco.org/html/twitter";

            if (feed == "progress")
            {
                Response.ContentType = "application/json";
                Response.Write(InstallHelper.GetProgress());
            }
            else
            {
                if (feed == "blogs")
                    url = "http://our.umbraco.org/html/blogs";

                if (feed == "sitebuildervids")
                    url = "http://umbraco.org/feeds/videos/site-builder-foundation-html";

                if (feed == "developervids")
                    url = "http://umbraco.org/feeds/videos/developer-foundation-html";

                string xmlResponse = library.GetXmlDocumentByUrl(url).Current.OuterXml;

                if (!xmlResponse.Contains("System.Net.WebException"))
                {
                    Response.Write(library.GetXmlDocumentByUrl(url).Current.OuterXml);
                }
                else
                {
                    Response.Write("We can't connect to umbraco.tv right now.  Click <strong>Set up your new website</strong> above to continue.");
                }
            }
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static string InstallOrUpgrade()
        {
            //if its not configured then we can continue
            if (ApplicationContext.Current == null || ApplicationContext.Current.IsConfigured)
            {
                throw new AuthenticationException("The application is already configured");
            }

            LogHelper.Info<InstallerRestService>("Running 'InstallOrUpgrade' service");

            var result = ApplicationContext.Current.DatabaseContext.CreateDatabaseSchemaAndDataOrUpgrade();

            // Remove legacy umbracoDbDsn configuration setting if it exists and connectionstring also exists
            if (ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName] != null)
            {
                Core.Configuration.GlobalSettings.RemoveSetting(Core.Configuration.GlobalSettings.UmbracoConnectionName);
            }
            else
            {
                var ex = new ArgumentNullException(string.Format("ConfigurationManager.ConnectionStrings[{0}]", Core.Configuration.GlobalSettings.UmbracoConnectionName), "Install / upgrade did not complete successfully, umbracoDbDSN was not set in the connectionStrings section");
                LogHelper.Error<InstallerRestService>("", ex);
                throw ex;
            }

            var js = new JavaScriptSerializer();
            var jsonResult = js.Serialize(result);
            return jsonResult;
        }
    }
}