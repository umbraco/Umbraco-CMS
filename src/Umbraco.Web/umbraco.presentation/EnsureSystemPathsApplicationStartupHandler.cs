using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.presentation
{
    public class EnsureSystemPathsApplicationStartupHandler : ApplicationEventHandler
    {
        protected override void ApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationInitialized(umbracoApplication, applicationContext);

            IOHelper.EnsurePathExists("~/App_Data");
            IOHelper.EnsurePathExists(SystemDirectories.Media);
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews);
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/Partials");
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/MacroPartials");
        }
    }
}