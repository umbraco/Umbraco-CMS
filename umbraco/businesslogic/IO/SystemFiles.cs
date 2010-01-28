using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.IO
{
    public class SystemFiles
    {

        public static string AccessXml
        {
            get
            {
                return SystemDirectories.Data + "/access.xml";
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
                return IOHelper.returnPath("umbracoContentXML", "~/data/umbraco.config");
            }
        }
    }
}
