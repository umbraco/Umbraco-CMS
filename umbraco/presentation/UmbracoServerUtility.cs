using System;
using System.Web;
using umbraco.presentation.LiveEditing;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using System.Xml.Linq;
using umbraco.IO;

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
                return IOHelper.ResolveUrl( SystemDirectories.Umbraco );
            }
        }

        public string ContentXmlPath
        {
            get
            {
                return IOHelper.ResolveUrl( SystemFiles.ContentCacheXml );
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
                return IOHelper.ResolveUrl( SystemDirectories.Data );
            }
        }
    }
}
