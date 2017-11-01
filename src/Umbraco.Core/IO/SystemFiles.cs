using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.IO
{
    public class SystemFiles
    {
        [Obsolete("This file is no longer used and should not be accessed!")]
        public static string AccessXml
        {
            get
            {
                return SystemDirectories.Data + "/access.config";
            }
        }

        public static string CreateUiXml
        {
            get
            {
                return SystemDirectories.Umbraco + "/config/create/UI.xml";
            }
        }

        public static string TinyMceConfig
        {
            get
            {
                return SystemDirectories.Config + "/tinyMceConfig.config";
            }
        }

        public static string MetablogConfig
        {
            get
            {
                return SystemDirectories.Config + "/metablogConfig.config";
            }
        }

        public static string DashboardConfig
        {
            get
            {
                return SystemDirectories.Config + "/dashboard.config";
            }
        }

        public static string NotFoundhandlersConfig
        {
            get
            {
                return SystemDirectories.Config + "/404handlers.config";
            }
        }

        public static string FeedProxyConfig
        {
            get
            {
                return string.Concat(SystemDirectories.Config, "/feedProxy.config");
            }
        }

        public static string ContentCacheXml
        {
            get
            {
                switch (GlobalSettings.LocalTempStorageLocation)
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

        [Obsolete("Use GlobalSettings.ContentCacheXmlStoredInCodeGen instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static bool ContentCacheXmlStoredInCodeGen
        {
            get { return GlobalSettings.ContentCacheXmlStoredInCodeGen; }
        }
    }
}
