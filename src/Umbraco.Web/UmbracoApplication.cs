using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Web.Routing;
using umbraco.businesslogic;

namespace Umbraco.Web
{
	/// <summary>
	/// The Umbraco global.asax class
	/// </summary>
    public class UmbracoApplication : UmbracoApplicationBase
	{
        private readonly ManifestWatcher _mw = new ManifestWatcher();

	    protected override void OnApplicationStarted(object sender, EventArgs e)
	    {
	        base.OnApplicationStarted(sender, e);

	        if (ApplicationContext.Current.IsConfigured && GlobalSettings.DebugMode)
	        {   
	            _mw.Start(Directory.GetDirectories(IOHelper.MapPath("~/App_Plugins/")));
	        }
	    }

	    protected override void OnApplicationEnd(object sender, EventArgs e)
	    {
	        base.OnApplicationEnd(sender, e);

            _mw.Dispose();
	    }

	    protected override IBootManager GetBootManager()
	    {
            return new WebBootManager(this);	        
	    }
	}
}
