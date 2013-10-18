using System;
using System.Web;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// A legacy class for old style handling of URL requests
    /// </summary>
    internal class LegacyRequestInitializer
    {
    	private readonly Uri _requestUrl;
    	private readonly HttpContextBase _httpContext;

        public LegacyRequestInitializer(Uri requestUrl, HttpContextBase httpContext)
        {
        	_requestUrl = requestUrl;
        	_httpContext = httpContext;
        }

    	public void InitializeRequest()
        {
            var uri = _requestUrl;

			global::umbraco.presentation.UmbracoContext.Current = new global::umbraco.presentation.UmbracoContext(_httpContext);

            // legacy - umbOriginalUrl used by default.aspx to rewritepath so forms are happy
            // legacy - umbOriginalUrl used by presentation/umbraco/urlRewriter/UrlRewriterFormWriter which handles <form action="..."
            // legacy - umbOriginalUrl also in Umbraco's back-end!
            _httpContext.Items["umbOriginalUrl"] = uri.AbsolutePath;
            // legacy - umbPage used by default.aspx to get the "clean url"... whatever...
            _httpContext.Items["UmbPage"] = uri.AbsolutePath;
            // legacy - virtualUrl used by presentation/template.cs to handle <form action="..."
            // legacy - virtualUrl used by presentation/umbraco/urlRewriter/UrlRewriterFormWriter which handles <form action="..." too
            // but, what if we RewritePath as soon as default.aspx begins, shouldn't it clear the form action?
            _httpContext.Items["VirtualUrl"] = uri.PathAndQuery; //String.Format("{0}{1}{2}", uri.AbsolutePath, string.IsNullOrWhiteSpace(docreq.QueryString) ? "" : "?", docreq.QueryString);
        }
    }
}