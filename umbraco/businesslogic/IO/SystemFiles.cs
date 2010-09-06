using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace umbraco.IO
{
    public class SystemFiles
    {

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

        public static string XsltextensionsConfig
        {
            get
            {
                return SystemDirectories.Config + "/xsltextensions.config";
            }
        }

        public static string RestextensionsConfig
        {
            get
            {
                return SystemDirectories.Config + "/restextensions.config";
            }
        }


        public static string SkinningXml
        {
            get
            {
                return SystemDirectories.Data + "/skinning.config";
            }
        }

        public static string NotFoundhandlersConfig
        {
            get
            {
                return SystemDirectories.Config + "/404handlers.config";
            }
        }

        public static string ContentCacheXml
        {
            get
            {
                if (ContentCacheXmlIsEphemeral)
                {
                    return Path.Combine(HttpRuntime.CodegenDir, @"UmbracoData\umbraco.config");
                }
                return IOHelper.returnPath("umbracoContentXML", "~/App_Data/umbraco.config");
            }
        }

        public static bool ContentCacheXmlIsEphemeral
        {
            get
            {
                bool returnValue = false;
                string configSetting = ConfigurationManager.AppSettings["umbracoContentXMLUseLocalTemp"];

                if (!string.IsNullOrEmpty(configSetting))
                    if(bool.TryParse(configSetting, out returnValue))
                        return returnValue;

                return false;
            }
        }
    }
}
