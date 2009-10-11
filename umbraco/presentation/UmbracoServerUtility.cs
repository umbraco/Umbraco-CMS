using System;
using System.Web;
using umbraco.presentation.LiveEditing;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using System.Xml.Linq;

namespace umbraco.presentation
{
    public class UmbracoServerUtility : HttpServerUtilityWrapper
    {
        private HttpServerUtility m_Server;

        public UmbracoServerUtility(HttpServerUtility server)
            : base(server)
        {
            m_Server = server;
        }

        public string UmbracoPath
        {
            get
            {
                return GlobalSettings.Path;
            }
        }

        public string ContentXmlPath
        {
            get
            {
                return GlobalSettings.ContentXML;
            }
        }

        public XDocument ContentXml
        {
            get
            {
                return XDocument.Load(this.ContentXmlPath);
            }
        }

        public string DataFolder
        {
            get
            {
                return GlobalSettings.StorageDirectory;
            }
        }
    }
}
