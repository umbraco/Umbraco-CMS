using System;
using System.Web;
using umbraco.presentation.LiveEditing;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using System.Xml.Linq;
using umbraco.IO;
using umbraco.presentation.preview;
using umbraco.BusinessLogic;
using System.Xml;

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

        /// <summary>
        /// Returns the physical file path that corresponds to the specified virtual path on the Web server.
        /// </summary>
        /// <param name="path">The virtual path of the Web server.</param>
        /// <returns>
        /// The physical file path that corresponds to <paramref name="path"/>.
        /// </returns>
        /// <exception cref="T:System.Web.HttpException">
        /// The current <see cref="T:System.Web.HttpContext"/> is null.
        /// </exception>
        public override string MapPath(string path)
        {
            return IOHelper.MapPath(path);
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

        private readonly string XDocumentCacheKey = "XDocumentCache";
        /// <summary>
        /// Gets the Umbraco XML cache
        /// </summary>
        /// <value>The content XML.</value>
        public XDocument ContentXml
        {
            get
            {
                if (UmbracoContext.Current.InPreviewMode)
                {
                    PreviewContent pc = new PreviewContent(new Guid(StateHelper.Cookies.Preview.GetValue()));
                    pc.LoadPreviewset();
                    return XmlDocumentToXDocument(pc.XmlContent);
                }
                else
                {
                    if (HttpContext.Current == null)
                        return XDocument.Load(ContentXmlPath);
                    XDocument xml = HttpContext.Current.Items[XDocumentCacheKey] as XDocument;
                    if (xml == null)
                    {
                        xml = XmlDocumentToXDocument(content.Instance.XmlContent);
                        HttpContext.Current.Items[XDocumentCacheKey] = xml;
                    }
                    return xml;
                }
            }
        }

        private XDocument XmlDocumentToXDocument(XmlDocument xml)
        {
            using (var nodeReader = new XmlNodeReader(xml))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
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
