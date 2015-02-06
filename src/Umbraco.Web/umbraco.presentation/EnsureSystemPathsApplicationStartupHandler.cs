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

            EnsurePathExists("~/App_Code");
            EnsurePathExists("~/App_Data");
            EnsurePathExists(SystemDirectories.AppPlugins);
            EnsurePathExists(SystemDirectories.Css);            
            EnsurePathExists(SystemDirectories.Masterpages);
            EnsurePathExists(SystemDirectories.Media);
            EnsurePathExists(SystemDirectories.Scripts);
            EnsurePathExists(SystemDirectories.UserControls);
            EnsurePathExists(SystemDirectories.Xslt);
            EnsurePathExists(SystemDirectories.MvcViews);
            EnsurePathExists(SystemDirectories.MvcViews + "/Partials");
            EnsurePathExists(SystemDirectories.MvcViews + "/MacroPartials");
        }
        
        public void EnsurePathExists(string path)
        {
            var absolutePath = IOHelper.MapPath(path);
            if (!Directory.Exists(absolutePath))
                Directory.CreateDirectory(absolutePath);
        }
    }
}