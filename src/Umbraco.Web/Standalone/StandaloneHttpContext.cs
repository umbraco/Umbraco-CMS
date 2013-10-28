using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Umbraco.Web.Standalone
{
    /// <summary>
    /// An Http context for use in standalone applications.
    /// </summary>
    internal class StandaloneHttpContext : HttpContextBase
    {
        private readonly string _url;
        private readonly HttpSessionStateBase _session = new StandaloneHttpSessionState();
        private readonly HttpResponseBase _response;
        private readonly HttpRequestBase _request = new StandaloneHttpRequest();
        private readonly TextWriter _writer = new StringWriter();
        private readonly IDictionary _items = new Dictionary<string, object>();

        public StandaloneHttpContext()
        {
            _response = new HttpResponseWrapper(new HttpResponse(_writer));
        }

        public StandaloneHttpContext(string url)
            : this()
        {
            if (url == null) throw new ArgumentNullException("url");
            _url = url;
            _request = new HttpRequestWrapper(new HttpRequest("", _url, ""));
        }


        // what else should we implement here?
        
        public override IDictionary Items
        {
            get { return _items; }
        }

        public override HttpSessionStateBase Session
        {
            get { return _session; }
        }

        public override HttpRequestBase Request
        {
            get { return _request; }
        }

        public override HttpResponseBase Response
        {
            get { return _response; }
        }

    }

    internal class StandaloneHttpSessionState : HttpSessionStateBase
    {
        
    }

    internal class StandaloneHttpRequest : HttpRequestBase
    {
        public override Uri Url
        {
            get { return new Uri("http://localhost"); }
        }
    }
}
