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
        protected void Application_Start(Object sender, EventArgs e)
        {

        }

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

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {

        }

		protected void Application_EndRequest(Object sender, EventArgs e)
		{
		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{
		}

        protected void Application_Error(Object sender, EventArgs e)
        {

        }

		protected void Session_End(Object sender, EventArgs e)
		{
			
		}

		protected void Application_End(Object sender, EventArgs e)
		{
            Log.Add(LogTypes.System, BusinessLogic.User.GetUser(0), -1, "Application shutted down at " + DateTime.Now);
		}
	}
}

