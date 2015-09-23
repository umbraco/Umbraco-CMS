using System;
using System.Collections.Generic;
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
                if (GlobalSettings.ContentCacheXmlStoredInCodeGen && SystemUtilities.GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted)
                {
                    return Path.Combine(HttpRuntime.CodegenDir, @"UmbracoData\umbraco.config");
                }
                return IOHelper.ReturnPath("umbracoContentXML", "~/App_Data/umbraco.config");
            }
        }

        [Obsolete("Use GlobalSettings.ContentCacheXmlStoredInCodeGen instead")]
        internal static bool ContentCacheXmlStoredInCodeGen
        {
            get { return GlobalSettings.ContentCacheXmlStoredInCodeGen; }
        }
    }
}
