using System;
using System.Web;
using System.Threading;

using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.datatype.controls;
using umbraco.cms.businesslogic.stat;

namespace umbraco
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : HttpApplication
	{
		protected Timer publishingTimer;
		protected Timer pingTimer;

		public Global()
		{
			InitializeComponent();
		}

        //protected void Application_Start(Object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Log.Add(LogTypes.System, BusinessLogic.User.GetUser(0), -1, "Application started at " + DateTime.Now);
        //    }
        //    catch
        //    {
        //    }

        //    // Check for configured key
        //    if(!GlobalSettings.Configured)
        //    {
        //        Application["umbracoNeedConfiguration"] = "true";
        //    }
        //    else
        //    {
        //        // add current default url
        //        Application["umbracoUrl"] = string.Format("{0}:{1}{2}",
        //            this.Context.Request.ServerVariables["SERVER_NAME"],
        //            HttpContext.Current.Request.ServerVariables["SERVER_PORT"], GlobalSettings.Path);

        //        /* This section is needed on start-up because timer objects
        //         * might initialize before these are initialized without a traditional
        //         * request, and therefore lacks information on application paths */

        //        // initialize datatype factory
        //        new Factory();

        //        // initialize action handlers
        //        Action.registerActionHandlers();

        //        /* Initialize SECTION END */

        //        // Start ping / keepalive timer
        //        pingTimer = new Timer(new TimerCallback(keepAliveService.PingUmbraco), this.Context, 1000, 300000);

        //        // Start publishingservice
        //        publishingTimer = new Timer(new TimerCallback(publishingService.CheckPublishing), this.Context, 1000, 60000);
        //    }
        //}

		protected void Session_Start(Object sender, EventArgs e)
		{
			if(GlobalSettings.EnableStat)
			{
				try
				{
					new Session();
				}
				catch(Exception state)
				{
                    Log.Add(LogTypes.Error, BusinessLogic.User.GetUser(0), -1, "Error initializing stat: " + state);
				}
			}
		}

        //protected void Application_BeginRequest(Object sender, EventArgs e)
        //{
        //    if(HttpContext.Current.Request.Path.ToLower().IndexOf(".aspx") > -1 ||
        //        HttpContext.Current.Request.Path.ToLower().IndexOf(".") == -1)
        //    {
        //        // Check if path or script is reserved!
        //        bool urlIsReserved = false;
        //        if(GlobalSettings.ReservedUrls.ToLower().IndexOf(string.Format(", {0}, ",
        //            HttpContext.Current.Request.Path.ToLower())) > -1)
        //            urlIsReserved = true;

        //        string[] reservedPaths = GlobalSettings.ReservedPaths.Split(',');
        //        for(int i = 0; i < reservedPaths.Length; i++)
        //        {
        //            if((HttpContext.Current.Request.Path).ToLower().StartsWith(reservedPaths[i].Trim().ToLower()))
        //            {
        //                urlIsReserved = true;
        //            }
        //        }

        //        if(!urlIsReserved)
        //        {
        //            if(Application["umbracoNeedConfiguration"] != null)
        //                HttpContext.Current.Response.Redirect(string.Format("{0}/../install/default.aspx?redir=true",
        //                    GlobalSettings.Path), true);
        //            else if(content.Instance.isInitializing)
        //                HttpContext.Current.RewritePath(string.Format("{0}/../config/splashes/booting.htm",
        //                    GlobalSettings.Path));
        //            else
        //            {
        //                string path = HttpContext.Current.Request.Path;
        //                string query = HttpContext.Current.Request.Url.Query;
        //                if(query != null && query != "")
        //                {
        //                    // Clean umbPage from querystring, caused by .NET 2.0 default Auth Controls
        //                    if(query.IndexOf("umbPage") > 0)
        //                    {
        //                        query += "&";
        //                        path = query.Substring(9, query.IndexOf("&") - 9);
        //                        query = query.Substring(query.IndexOf("&") + 1, query.Length - query.IndexOf("&") - 1);
        //                    }
        //                    else if(query.Length > 0)
        //                        query = query.Substring(1, query.Length - 1);

        //                    if(query.Length > 0)
        //                    {
        //                        HttpContext.Current.Items["VirtualUrl"] = path + "?" + query;
        //                        HttpContext.Current.RewritePath(string.Format("{0}/../default.aspx?umbPage={1}&{2}",
        //                            GlobalSettings.Path, path, query));
        //                    }
        //                }
        //                if(HttpContext.Current.Items["VirtualUrl"] == null)
        //                {
        //                    HttpContext.Current.Items["VirtualUrl"] = path;
        //                    HttpContext.Current.RewritePath(string.Format("{0}/../default.aspx?umbPage={1}",
        //                        GlobalSettings.Path, path));
        //                }
        //            }
        //        }
        //    }
        //}

		protected void Application_EndRequest(Object sender, EventArgs e)
		{
		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{
		}

        //protected void Application_Error(Object sender, EventArgs e)
        //{
        //    if(Request != null)
        //        Log.Add(LogTypes.Error, BusinessLogic.User.GetUser(0), -1,
        //            string.Format("At {0} (Refered by: {1}): {2}",
        //                this.Request.RawUrl, this.Request.UrlReferrer, this.Server.GetLastError().InnerException));
        //    else
        //        Log.Add(LogTypes.Error, BusinessLogic.User.GetUser(0), -1,
        //            "No Context available -> " + Server.GetLastError().InnerException);
        //}

		protected void Session_End(Object sender, EventArgs e)
		{
			//if(GlobalSettings.EnableStat)
			//    cms.businesslogic.stat.Session.EndSession(new Guid(Request.Cookies["umbracoSessionId"].Value));
		}

		protected void Application_End(Object sender, EventArgs e)
		{
            Log.Add(LogTypes.System, BusinessLogic.User.GetUser(0), -1, "Application shutted down at " + DateTime.Now);
		}

		#region Web Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			
		}

		#endregion
	}
}

