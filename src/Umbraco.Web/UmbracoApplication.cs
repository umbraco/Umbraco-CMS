using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;
using umbraco.businesslogic;

namespace Umbraco.Web
{
	/// <summary>
	/// The Umbraco global.asax class
	/// </summary>
    public class UmbracoApplication : UmbracoApplicationBase
	{
            //don't output the MVC version header (security)
            MvcHandler.DisableMvcResponseHeader = true;

		
	    protected override IBootManager GetBootManager()
	    {
            return new WebBootManager(this);	        
	    }
	}
}
