using System;
using System.Web;
using System.Threading;

using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.datatype.controls;

namespace umbraco
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : HttpApplication
	{
        protected virtual void Application_Start(object sender, EventArgs e)
        {

        }

        protected virtual void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected virtual void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected virtual void Application_Error(object sender, EventArgs e)
        {

        }

        protected virtual void Session_End(object sender, EventArgs e)
        {

        }

		protected virtual void Application_End(Object sender, EventArgs e)
		{
            Log.Add(LogTypes.System, BusinessLogic.User.GetUser(0), -1, "Application shut down at " + DateTime.Now);
		}
	}
}

