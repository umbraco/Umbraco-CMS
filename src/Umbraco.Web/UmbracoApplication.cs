using System;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;

namespace Umbraco.Web
{
	/// <summary>
	/// Represents the Umbraco global.asax class.
	/// </summary>
    public class UmbracoApplication : UmbracoApplicationBase
	{
        // if configured and in debug mode, a ManifestWatcher watches App_Plugins folders for
        // package.manifest chances and restarts the application on any change
	    private ManifestWatcher _mw;

	    protected override void OnApplicationStarted(object sender, EventArgs e)
	    {
	        base.OnApplicationStarted(sender, e);

	        if (ApplicationContext.Current.IsConfigured == false || GlobalSettings.DebugMode == false)
                return;

	        var appPlugins = IOHelper.MapPath("~/App_Plugins/");
	        if (Directory.Exists(appPlugins) == false) return;

	        _mw = new ManifestWatcher(Current.Logger);
	        _mw.Start(Directory.GetDirectories(appPlugins));
	    }

	    protected override void OnApplicationEnd(object sender, EventArgs e)
	    {
	        base.OnApplicationEnd(sender, e);
	        _mw?.Dispose();
	    }

	    protected override IBootManager GetBootManager()
	    {
            return new WebBootManager(this);
	    }
	}
}
