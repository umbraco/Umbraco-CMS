namespace Umbraco.Web
{
    /// <summary>
    /// A legacy class for old style handling of URL requests
    /// </summary>
    internal class LegacyRequestInitializer
    {
        private readonly UmbracoContext _umbracoContext;

        public LegacyRequestInitializer(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }

        public void InitializeRequest()
        {
            var uri = _umbracoContext.OriginalUrl;

            // legacy - umbOriginalUrl used by default.aspx to rewritepath so forms are happy
            // legacy - umbOriginalUrl used by presentation/umbraco/urlRewriter/UrlRewriterFormWriter which handles <form action="..."
            // legacy - umbOriginalUrl also in Umbraco's back-end!
            _umbracoContext.HttpContext.Items["umbOriginalUrl"] = uri.AbsolutePath;
            // legacy - umbPage used by default.aspx to get the "clean url"... whatever... fixme - we prob. don't want it anymore
            _umbracoContext.HttpContext.Items["UmbPage"] = uri.AbsolutePath;
            // legacy - virtualUrl used by presentation/template.cs to handle <form action="..."
            // legacy - virtualUrl used by presentation/umbraco/urlRewriter/UrlRewriterFormWriter which handles <form action="..." too
            // but, what if we RewritePath as soon as default.aspx begins, shouldn't it clear the form action?
            _umbracoContext.HttpContext.Items["VirtualUrl"] = uri.PathAndQuery; //String.Format("{0}{1}{2}", uri.AbsolutePath, string.IsNullOrWhiteSpace(docreq.QueryString) ? "" : "?", docreq.QueryString);
        }
    }
}