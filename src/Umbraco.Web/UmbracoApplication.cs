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
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using umbraco.businesslogic;

namespace Umbraco.Web
{
    public class CustomRenderController : RenderMvcController
    {
        public override ActionResult Index(Models.RenderModel model)
        {
            HttpContext.Response.ContentType = "text/plain";
            return base.Index(model);
        }
    }

    public class CustomApplicationEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            DefaultRenderMvcControllerResolver.Current.SetDefaultControllerType(typeof(CustomRenderController));
            base.ApplicationStarting(umbracoApplication, applicationContext);
        }
    }

	/// <summary>
	/// The Umbraco global.asax class
	/// </summary>
    public class UmbracoApplication : UmbracoApplicationBase
	{
           
	    protected override IBootManager GetBootManager()
	    {
            return new WebBootManager(this);	        
	    }
	}
}
