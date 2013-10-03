using System;
using System.Web;
using System.Xml.Linq;
using Umbraco.Core.IO;
using umbraco.presentation.preview;
using umbraco.BusinessLogic;
using System.Xml;

namespace umbraco.presentation
{

    /// <summary>
    /// A custom HttpServerUtilityBase object which exposes some additional methods and also overrides
    /// some such as MapPath to use the IOHelper.
    /// </summary>
    /// <remarks>
    /// Unforunately when this class was created it 
    /// inherited from HttpServerUtilityWrapper intead of HttpServerUtilityBase which meant that this is not unit testable
    /// and still has a dependency on a real HttpRequest. So now it inherits from HttpRequestBase which 
    /// means we need to override all of the methods and just wrap them with the _server object passed in.
    /// 
    /// This now needs to exist only because of backwards compatibility
    /// </remarks>
    [Obsolete("This class is no longer used, for the overridden MapPath methods and custom methods use IOHelper")]
    public class UmbracoServerUtility : HttpServerUtilityBase
    {
        private readonly HttpServerUtilityBase _server;

        #region Constructor

        public UmbracoServerUtility(HttpServerUtilityBase server)
        {
            _server = server;
        }

        [Obsolete("Use the alternate constructor which accepts a HttpServerUtilityBase object instead")]
        public UmbracoServerUtility(HttpServerUtility server)
        {
            _server = new HttpServerUtilityWrapper(server);
        } 

        #endregion

        #region Wrapped methods

        public override object CreateObject(string progID)
        {
            return _server.CreateObject(progID);
        }

        public override object CreateObject(Type type)
        {
            return _server.CreateObject(type);
        }

        public override object CreateObjectFromClsid(string clsid)
        {
            return _server.CreateObjectFromClsid(clsid);
        }

        public override bool Equals(object obj)
        {
            return _server.Equals(obj);
        }

        public override void Execute(IHttpHandler handler, System.IO.TextWriter writer, bool preserveForm)
        {
            _server.Execute(handler, writer, preserveForm);
        }

        public override void Execute(string path)
        {
            _server.Execute(path);
        }

        public override void Execute(string path, bool preserveForm)
        {
            _server.Execute(path, preserveForm);
        }

        public override void Execute(string path, System.IO.TextWriter writer)
        {
            _server.Execute(path, writer);
        }

        public override void Execute(string path, System.IO.TextWriter writer, bool preserveForm)
        {
            _server.Execute(path, writer, preserveForm);
        }

        public override int GetHashCode()
        {
            return _server.GetHashCode();
        }

        public override Exception GetLastError()
        {
            return _server.GetLastError();
        }

        public override string HtmlDecode(string s)
        {
            return _server.HtmlDecode(s);
        }

        public override void HtmlDecode(string s, System.IO.TextWriter output)
        {
            _server.HtmlDecode(s, output);
        }

        public override string HtmlEncode(string s)
        {
            return _server.HtmlEncode(s);
        }

        public override void ClearError()
        {
            _server.ClearError();
        }

        public override void HtmlEncode(string s, System.IO.TextWriter output)
        {
            _server.HtmlEncode(s, output);
        }

        public override string MachineName
        {
            get
            {
                return _server.MachineName;
            }
        }

        public override int ScriptTimeout
        {
            get
            {
                return _server.ScriptTimeout;
            }
            set
            {
                _server.ScriptTimeout = value;
            }
        }

        public override string ToString()
        {
            return _server.ToString();
        }

        public override void Transfer(IHttpHandler handler, bool preserveForm)
        {
            _server.Transfer(handler, preserveForm);
        }

        public override void Transfer(string path)
        {
            _server.Transfer(path);
        }

        public override void Transfer(string path, bool preserveForm)
        {
            _server.Transfer(path, preserveForm);
        }

        public override void TransferRequest(string path)
        {
            _server.TransferRequest(path);
        }

        public override void TransferRequest(string path, bool preserveForm)
        {
            _server.TransferRequest(path, preserveForm);
        }

        public override void TransferRequest(string path, bool preserveForm, string method, System.Collections.Specialized.NameValueCollection headers)
        {
            _server.TransferRequest(path, preserveForm, method, headers);
        }

        public override string UrlDecode(string s)
        {
            return _server.UrlDecode(s);
        }

        public override void UrlDecode(string s, System.IO.TextWriter output)
        {
            _server.UrlDecode(s, output);
        }

        public override string UrlEncode(string s)
        {
            return _server.UrlEncode(s);
        }

        public override void UrlEncode(string s, System.IO.TextWriter output)
        {
            _server.UrlEncode(s, output);
        }

        public override string UrlPathEncode(string s)
        {
            return _server.UrlPathEncode(s);
        }

        public override byte[] UrlTokenDecode(string input)
        {
            return _server.UrlTokenDecode(input);
        }

        public override string UrlTokenEncode(byte[] input)
        {
            return _server.UrlTokenEncode(input);
        }

        #endregion

        #region Overridden methods

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
       
        #endregion

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

		[Obsolete("This is no longer used in the codebase and will be removed. ")]
        private const string XDocumentCacheKey = "XDocumentCache";

        /// <summary>
        /// Gets the Umbraco XML cache
        /// </summary>
        /// <value>The content XML.</value>
        [Obsolete("This is no longer used in the codebase and will be removed. If you need to access the current XML cache document you can use the Umbraco.Web.Umbraco.Context.GetXml() method.")]
        public XDocument ContentXml
        {
            get
            {
                if (UmbracoContext.Current.InPreviewMode)
                {
                    var pc = new PreviewContent(new Guid(StateHelper.Cookies.Preview.GetValue()));
                    pc.LoadPreviewset();
                    return XmlDocumentToXDocument(pc.XmlContent);
                }
                else
                {
                    if (HttpContext.Current == null)
                        return XDocument.Load(ContentXmlPath);
                    var xml = HttpContext.Current.Items[XDocumentCacheKey] as XDocument;
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
