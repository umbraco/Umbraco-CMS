using System;
using Umbraco.Core;

namespace Umbraco.Web.Install.UpgradeScripts
{
    internal class UpgradeScriptRegistrar : IApplicationEventHandler
    {
        public void OnApplicationInitialized(UmbracoApplication httpApplication, ApplicationContext applicationContext)
        {
            //Add contnet path fixup for any version from 4.10 up to 4.11.4
            UpgradeScriptManager.AddUpgradeScript(
                () => new ContentPathFix(), 
                new VersionRange(
                    new Version(4, 10),
                    new Version(4, 11, 4)));
        }

        public void OnApplicationStarting(UmbracoApplication httpApplication, ApplicationContext applicationContext)
        {
            
        }

        public void OnApplicationStarted(UmbracoApplication httpApplication, ApplicationContext applicationContext)
        {
            
        }
    }
}