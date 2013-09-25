using System;
using System.Web;

namespace umbraco.presentation
{
    /// <summary>
    /// A custom HttpResponseBase class
    /// 
    /// The only reason this class exists is for backwards compatibility. Previously this used to 
    /// inherit from HttpResponseWrapper which is incorrect since it is not unit testable and means that
    /// it still has a reliance on the real HttpResponse object which means that the UmbracoContext still has 
    /// a reliance on the real HttpContext.
    /// </summary>
    [Obsolete("This class is no longer used")]
    public class UmbracoResponse : HttpResponseBase
    {
        private readonly HttpResponseBase _response;

        #region Contructor
        public UmbracoResponse(HttpResponseBase response)
        {
            _response = response;
        }

        public UmbracoResponse(HttpResponse response)
        {
            _response = new HttpResponseWrapper(response);
        } 
        #endregion

        #region Wrapped methods

        public override void AddCacheDependency(params System.Web.Caching.CacheDependency[] dependencies)
        {
            base.AddCacheDependency(dependencies);
        }

        public override void AddCacheItemDependencies(string[] cacheKeys)
        {
            base.AddCacheItemDependencies(cacheKeys);
        }

        public override void AddCacheItemDependencies(System.Collections.ArrayList cacheKeys)
        {
            base.AddCacheItemDependencies(cacheKeys);
        }

        public override void AddCacheItemDependency(string cacheKey)
        {
            base.AddCacheItemDependency(cacheKey);
        }

        public override void AddFileDependencies(string[] filenames)
        {
            base.AddFileDependencies(filenames);
        }

        public override void AddFileDependencies(System.Collections.ArrayList filenames)
        {
            base.AddFileDependencies(filenames);
        }

        public override void AddFileDependency(string filename)
        {
            base.AddFileDependency(filename);
        }

        public override void AddHeader(string name, string value)
        {
            base.AddHeader(name, value);
        }

        public override void AppendCookie(HttpCookie cookie)
        {
            base.AppendCookie(cookie);
        }

        public override void AppendHeader(string name, string value)
        {
            base.AppendHeader(name, value);
        }

        public override void AppendToLog(string param)
        {
            base.AppendToLog(param);
        }

        public override string ApplyAppPathModifier(string virtualPath)
        {
            return base.ApplyAppPathModifier(virtualPath);
        }

        public override void BinaryWrite(byte[] buffer)
        {
            base.BinaryWrite(buffer);
        }

        public override bool Buffer
        {
            get
            {
                return base.Buffer;
            }
            set
            {
                base.Buffer = value;
            }
        }

        public override bool BufferOutput
        {
            get
            {
                return base.BufferOutput;
            }
            set
            {
                base.BufferOutput = value;
            }
        }
        public override HttpCachePolicyBase Cache
        {
            get
            {
                return base.Cache;
            }
        }

        public override string CacheControl
        {
            get
            {
                return base.CacheControl;
            }
            set
            {
                base.CacheControl = value;
            }
        }

        public override string Charset
        {
            get
            {
                return base.Charset;
            }
            set
            {
                base.Charset = value;
            }
        }


        public override void Clear()
        {
            base.Clear();
        }

        public override void ClearContent()
        {
            base.ClearContent();
        }

        public override void ClearHeaders()
        {
            base.ClearHeaders();
        }

        public override void Close()
        {
            base.Close();
        }

        public override System.Text.Encoding ContentEncoding
        {
            get
            {
                return base.ContentEncoding;
            }
            set
            {
                base.ContentEncoding = value;
            }
        }

        public override string ContentType
        {
            get
            {
                return base.ContentType;
            }
            set
            {
                base.ContentType = value;
            }
        }

        public override HttpCookieCollection Cookies
        {
            get
            {
                return base.Cookies;
            }
        }

        public override void DisableKernelCache()
        {
            base.DisableKernelCache();
        }

        public override void End()
        {
            base.End();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int Expires
        {
            get
            {
                return base.Expires;
            }
            set
            {
                base.Expires = value;
            }
        }

        public override DateTime ExpiresAbsolute
        {
            get
            {
                return base.ExpiresAbsolute;
            }
            set
            {
                base.ExpiresAbsolute = value;
            }
        }

        public override System.IO.Stream Filter
        {
            get
            {
                return base.Filter;
            }
            set
            {
                base.Filter = value;
            }
        }

        public override void Flush()
        {
            base.Flush();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override System.Text.Encoding HeaderEncoding
        {
            get
            {
                return base.HeaderEncoding;
            }
            set
            {
                base.HeaderEncoding = value;
            }
        }

        public override System.Collections.Specialized.NameValueCollection Headers
        {
            get
            {
                return base.Headers;
            }
        }

        public override bool IsClientConnected
        {
            get
            {
                return base.IsClientConnected;
            }
        }

        public override bool IsRequestBeingRedirected
        {
            get
            {
                return base.IsRequestBeingRedirected;
            }
        }


        public override System.IO.TextWriter Output
        {
            get
            {
                return base.Output;
            }
            set
            {
                base.Output = value;
            }
        }
        public override System.IO.Stream OutputStream
        {
            get
            {
                return base.OutputStream;
            }
        }
        public override void Pics(string value)
        {
            base.Pics(value);
        }
        public override void Redirect(string url)
        {
            base.Redirect(url);
        }
        public override void Redirect(string url, bool endResponse)
        {
            base.Redirect(url, endResponse);
        }
        public override string RedirectLocation
        {
            get
            {
                return base.RedirectLocation;
            }
            set
            {
                base.RedirectLocation = value;
            }
        }
        public override void RedirectPermanent(string url)
        {
            base.RedirectPermanent(url);
        }
        public override void RedirectPermanent(string url, bool endResponse)
        {
            base.RedirectPermanent(url, endResponse);
        }
        public override void RedirectToRoute(object routeValues)
        {
            base.RedirectToRoute(routeValues);
        }
        public override void RedirectToRoute(string routeName)
        {
            base.RedirectToRoute(routeName);
        }
        public override void RedirectToRoute(string routeName, object routeValues)
        {
            base.RedirectToRoute(routeName, routeValues);
        }
        public override void RedirectToRoute(string routeName, System.Web.Routing.RouteValueDictionary routeValues)
        {
            base.RedirectToRoute(routeName, routeValues);
        }
        public override void RedirectToRoute(System.Web.Routing.RouteValueDictionary routeValues)
        {
            base.RedirectToRoute(routeValues);
        }
        public override void RedirectToRoutePermanent(object routeValues)
        {
            base.RedirectToRoutePermanent(routeValues);
        }
        public override void RedirectToRoutePermanent(string routeName)
        {
            base.RedirectToRoutePermanent(routeName);
        }
        public override void RedirectToRoutePermanent(string routeName, object routeValues)
        {
            base.RedirectToRoutePermanent(routeName, routeValues);
        }
        public override void RedirectToRoutePermanent(string routeName, System.Web.Routing.RouteValueDictionary routeValues)
        {
            base.RedirectToRoutePermanent(routeName, routeValues);
        }
        public override void RedirectToRoutePermanent(System.Web.Routing.RouteValueDictionary routeValues)
        {
            base.RedirectToRoutePermanent(routeValues);
        }
        public override void RemoveOutputCacheItem(string path)
        {
            base.RemoveOutputCacheItem(path);
        }
        public override void RemoveOutputCacheItem(string path, string providerName)
        {
            base.RemoveOutputCacheItem(path, providerName);
        }
        public override void SetCookie(HttpCookie cookie)
        {
            base.SetCookie(cookie);
        }
        public override string Status
        {
            get
            {
                return base.Status;
            }
            set
            {
                base.Status = value;
            }
        }
        public override int StatusCode
        {
            get
            {
                return base.StatusCode;
            }
            set
            {
                base.StatusCode = value;
            }
        }
        public override string StatusDescription
        {
            get
            {
                return base.StatusDescription;
            }
            set
            {
                base.StatusDescription = value;
            }
        }
        public override int SubStatusCode
        {
            get
            {
                return base.SubStatusCode;
            }
            set
            {
                base.SubStatusCode = value;
            }
        }
        public override bool SuppressContent
        {
            get
            {
                return base.SuppressContent;
            }
            set
            {
                base.SuppressContent = value;
            }
        }
        public override string ToString()
        {
            return base.ToString();
        }
        public override void TransmitFile(string filename)
        {
            base.TransmitFile(filename);
        }
        public override void TransmitFile(string filename, long offset, long length)
        {
            base.TransmitFile(filename, offset, length);
        }
        public override bool TrySkipIisCustomErrors
        {
            get
            {
                return base.TrySkipIisCustomErrors;
            }
            set
            {
                base.TrySkipIisCustomErrors = value;
            }
        }
        public override void Write(char ch)
        {
            base.Write(ch);
        }
        public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
        }
        public override void Write(object obj)
        {
            base.Write(obj);
        }
        public override void Write(string s)
        {
            base.Write(s);
        }
        public override void WriteFile(IntPtr fileHandle, long offset, long size)
        {
            base.WriteFile(fileHandle, offset, size);
        }
        public override void WriteFile(string filename)
        {
            base.WriteFile(filename);
        }
        public override void WriteFile(string filename, bool readIntoMemory)
        {
            base.WriteFile(filename, readIntoMemory);
        }
        public override void WriteFile(string filename, long offset, long size)
        {
            base.WriteFile(filename, offset, size);
        }
        public override void WriteSubstitution(HttpResponseSubstitutionCallback callback)
        {
            base.WriteSubstitution(callback);
        }
        #endregion
    }
}
