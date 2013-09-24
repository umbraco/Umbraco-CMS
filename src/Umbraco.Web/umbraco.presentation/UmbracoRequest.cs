using System;
using System.Web;

namespace umbraco.presentation
{

    /// <summary>
    /// A custom HttpRequestBase object which exposes some additional methods
    /// </summary>
    /// <remarks>
    /// The only reason this class exists is because somebody decided that it would be a good idea to piggy
    /// back the IsDebug method on top of the HttpRequest object. Unforunately when this was created it 
    /// inherited from HttpRequestWrapper intead of HttpRequestBase which meant that this is not unit testable
    /// and still has a dependency on a real HttpRequest. So now it inherits from HttpRequestBase which 
    /// means we need to override all of the methods and just wrap them with the _request object passed in.
    /// 
    /// This now needs to exist only because of backwards compatibility
    /// </remarks>
    [Obsolete("This class is no longer used, for the IsDebug method use Umbraco.Web.UmbracoContext.IsDebug")]
    public class UmbracoRequest : HttpRequestBase
    {
        private readonly HttpRequestBase _request;

        #region Constructor
        public UmbracoRequest(HttpRequestBase request)
        {
            _request = request;
        }

        [Obsolete("Use the alternative contructor that accepts a HttpRequestBase object instead")]
        public UmbracoRequest(HttpRequest request)
        {
            _request = new HttpRequestWrapper(request);
        } 
        #endregion

        #region Wrapped Methods

        public override string[] AcceptTypes
        {
            get
            {
                return _request.AcceptTypes;
            }
        }

        public override string AnonymousID
        {
            get
            {
                return _request.AnonymousID;
            }
        }

        public override string ApplicationPath
        {
            get
            {
                return _request.ApplicationPath;
            }
        }

        public override string AppRelativeCurrentExecutionFilePath
        {
            get
            {
                return _request.AppRelativeCurrentExecutionFilePath;
            }
        }

        public override byte[] BinaryRead(int count)
        {
            return _request.BinaryRead(count);
        }

        public override HttpBrowserCapabilitiesBase Browser
        {
            get
            {
                return _request.Browser;
            }
        }

        public override HttpClientCertificate ClientCertificate
        {
            get
            {
                return _request.ClientCertificate;
            }
        }

        public override System.Text.Encoding ContentEncoding
        {
            get
            {
                return _request.ContentEncoding;
            }
            set
            {
                _request.ContentEncoding = value;
            }
        }

        public override int ContentLength
        {
            get
            {
                return _request.ContentLength;
            }
        }

        public override string ContentType
        {
            get
            {
                return _request.ContentType;
            }
            set
            {
                _request.ContentType = value;
            }
        }

        public override HttpCookieCollection Cookies
        {
            get
            {
                return _request.Cookies;
            }
        }

        public override string CurrentExecutionFilePath
        {
            get
            {
                return _request.CurrentExecutionFilePath;
            }
        }

        public override bool Equals(object obj)
        {
            return _request.Equals(obj);
        }

        public override string FilePath
        {
            get
            {
                return _request.FilePath;
            }
        }

        public override HttpFileCollectionBase Files
        {
            get
            {
                return _request.Files;
            }
        }

        public override System.IO.Stream Filter
        {
            get
            {
                return _request.Filter;
            }
            set
            {
                _request.Filter = value;
            }
        }

        public override System.Collections.Specialized.NameValueCollection Form
        {
            get
            {
                return _request.Form;
            }
        }

        public override int GetHashCode()
        {
            return _request.GetHashCode();
        }

        public override System.Collections.Specialized.NameValueCollection Headers
        {
            get
            {
                return _request.Headers;
            }
        }

        public override System.Security.Authentication.ExtendedProtection.ChannelBinding HttpChannelBinding
        {
            get
            {
                return _request.HttpChannelBinding;
            }
        }

        public override string HttpMethod
        {
            get
            {
                return _request.HttpMethod;
            }
        }

        public override System.IO.Stream InputStream
        {
            get
            {
                return _request.InputStream;
            }
        }

        public override bool IsAuthenticated
        {
            get
            {
                return _request.IsAuthenticated;
            }
        }

        public override bool IsLocal
        {
            get
            {
                return _request.IsLocal;
            }
        }

        public override bool IsSecureConnection
        {
            get
            {
                return _request.IsSecureConnection;
            }
        }

        public override System.Security.Principal.WindowsIdentity LogonUserIdentity
        {
            get
            {
                return _request.LogonUserIdentity;
            }
        }

        public override int[] MapImageCoordinates(string imageFieldName)
        {
            return _request.MapImageCoordinates(imageFieldName);
        }

        public override string MapPath(string virtualPath)
        {
            return _request.MapPath(virtualPath);
        }

        public override string MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping)
        {
            return _request.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping);
        }

        public override System.Collections.Specialized.NameValueCollection Params
        {
            get
            {
                return _request.Params;
            }
        }

        public override string Path
        {
            get
            {
                return _request.Path;
            }
        }

        public override string PathInfo
        {
            get
            {
                return _request.PathInfo;
            }
        }

        public override string PhysicalApplicationPath
        {
            get
            {
                return _request.PhysicalApplicationPath;
            }
        }

        public override string PhysicalPath
        {
            get
            {
                return _request.PhysicalPath;
            }
        }

        public override System.Collections.Specialized.NameValueCollection QueryString
        {
            get
            {
                return _request.QueryString;
            }
        }

        public override string RawUrl
        {
            get
            {
                return _request.RawUrl;
            }
        }

        public override System.Web.Routing.RequestContext RequestContext
        {
            get
            {
                return _request.RequestContext;
            }
        }

        public override string RequestType
        {
            get
            {
                return _request.RequestType;
            }
            set
            {
                _request.RequestType = value;
            }
        }

        public override void SaveAs(string filename, bool includeHeaders)
        {
            _request.SaveAs(filename, includeHeaders);
        }

        public override System.Collections.Specialized.NameValueCollection ServerVariables
        {
            get
            {
                return _request.ServerVariables;
            }
        }

        public override string this[string key]
        {
            get
            {
                return _request[key];
            }
        }

        public override string ToString()
        {
            return _request.ToString();
        }

        public override int TotalBytes
        {
            get
            {
                return _request.TotalBytes;
            }
        }

        public override Uri Url
        {
            get
            {
                return _request.Url;
            }
        }

        public override Uri UrlReferrer
        {
            get
            {
                return _request.UrlReferrer;
            }
        }

        public override string UserAgent
        {
            get
            {
                return _request.UserAgent;
            }
        }

        public override string UserHostAddress
        {
            get
            {
                return _request.UserHostAddress;
            }
        }

        public override string UserHostName
        {
            get
            {
                return _request.UserHostName;
            }
        }

        public override string[] UserLanguages
        {
            get
            {
                return _request.UserLanguages;
            }
        }

        public override void ValidateInput()
        {
            _request.ValidateInput();
        }


        #endregion

        /// <summary>
        /// Gets a value indicating whether the request has debugging enabled
        /// </summary>
        /// <value><c>true</c> if this instance is debug; otherwise, <c>false</c>.</value>
        public bool IsDebug
        {
            get
            {
                return GlobalSettings.DebugMode && (!string.IsNullOrEmpty(this["umbdebugshowtrace"]) || !string.IsNullOrEmpty(this["umbdebug"]));
            }
        }
    }
}
