using System;
using System.IO;
using System.Web;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.IO
{
    public class SystemFiles
    {
        public static string TinyMceConfig => SystemDirectories.Config + "/tinyMceConfig.config";
        
        public static string GetContentCacheXml(IGlobalSettings globalSettings)
        {
            switch (globalSettings.LocalTempStorageLocation)
            {
                case LocalTempStorage.AspNetTemp:
                    return Path.Combine(HttpRuntime.CodegenDir, @"UmbracoData\umbraco.config");
                case LocalTempStorage.EnvironmentTemp:
                    var appDomainHash = HttpRuntime.AppDomainAppId.ToSHA1();
                    var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "UmbracoData",
                        //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                        // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                        // utilizing an old path
                        appDomainHash);
                    return Path.Combine(cachePath, "umbraco.config");
                case LocalTempStorage.Default:
                    return IOHelper.ReturnPath("umbracoContentXML", "~/App_Data/umbraco.config");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
