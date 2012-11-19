using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Globalization;
using System.Diagnostics;

// legacy
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.NodeFactory;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.member;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// represents a request for one specified Umbraco document to be rendered
	/// by one specified template, using one particular culture.
	/// </summary>
	internal class PublishedContentRequest
    {
		/// <summary>
		/// Assigns the request to the http context and proceeds to process the request. If everything is successful, invoke the callback.
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="umbracoContext"></param>
		/// <param name="onSuccess"></param>
		internal void ProcessRequest(HttpContextBase httpContext, UmbracoContext umbracoContext, Action<PublishedContentRequest> onSuccess)
		{
			if (umbracoContext == null)
				throw new NullReferenceException("The UmbracoContext.Current is null, ProcessRequest cannot proceed unless there is a current UmbracoContext");
			if (umbracoContext.RoutingContext == null)
				throw new NullReferenceException("The UmbracoContext.RoutingContext has not been assigned, ProcessRequest cannot proceed unless there is a RoutingContext assigned to the UmbracoContext");

			//assign back since this is a front-end request
			umbracoContext.PublishedContentRequest = this;

			// note - at that point the original legacy module did something do handle IIS custom 404 errors
			//   ie pages looking like /anything.aspx?404;/path/to/document - I guess the reason was to support
			//   "directory urls" without having to do wildcard mapping to ASP.NET on old IIS. This is a pain
			//   to maintain and probably not used anymore - removed as of 06/2012. @zpqrtbnk.
			//
			//   to trigger Umbraco's not-found, one should configure IIS and/or ASP.NET custom 404 errors
			//   so that they point to a non-existing page eg /redirect-404.aspx
			//   TODO: SD: We need more information on this for when we release 4.10.0 as I'm not sure what this means.

			//find domain
			_builder.LookupDomain();
			// redirect if it has been flagged
			if (this.IsRedirect)
				httpContext.Response.Redirect(this.RedirectUrl, true);
			//set the culture on the thread - once, so it's set when running document lookups
			Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = this.Culture;
			//find the document, found will be true if the doc request has found BOTH a node and a template
			// though currently we don't use this value.
			var found = _builder.LookupDocument();
			//set the culture on the thread -- again, 'cos it might have changed due to a wildcard domain
			Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = this.Culture;
			//this could be called in the LookupDocument method, but I've just put it here for clarity.
			_builder.DetermineRenderingEngine();

			//TODO: here we should launch an event so that people can modify the doc request to do whatever they want.

			// redirect if it has been flagged
			if (this.IsRedirect)
				httpContext.Response.Redirect(this.RedirectUrl, true);

			// handle 404
			if (this.Is404)
			{
				httpContext.Response.StatusCode = 404;

				if (!this.HasNode)
				{
					httpContext.RemapHandler(new PublishedContentNotFoundHandler());
					return;
				}

				// else we have a document to render
				// not having a template is ok here, MVC will take care of it
			}

			// just be safe - should never ever happen
			if (!this.HasNode)
				throw new Exception("No document to render.");

			// trigger PublishedContentRequest.Rendering event?
			// with complete access to the content request?

			// render even though we might have no template
			// to give MVC a chance to hijack routes
			// pass off to our handlers (mvc or webforms)

			// assign the legacy page back to the docrequest
			// handlers like default.aspx will want it and most macros currently need it
			this.UmbracoPage = new page(this);

			// these two are used by many legacy objects
			httpContext.Items["pageID"] = this.DocumentId;
			httpContext.Items["pageElements"] = this.UmbracoPage.Elements;

			if (onSuccess != null)
				onSuccess(this);
		}

		/// <summary>
		/// After execution is handed off to MVC, we can finally check if the request has: No Template assigned and also the 
		/// route is not hijacked. When this occurs, we need to send the routing back through the builder to check for 
		/// not found handlers.
		/// </summary>
		/// <param name="httpContext"></param>
		/// <returns></returns>
		internal IHttpHandler ProcessNoTemplateInMvc(HttpContextBase httpContext)
		{
			var content = this.PublishedContent;
			this.PublishedContent = null;

			_builder.LookupDocument2();
			_builder.DetermineRenderingEngine();

			// redirect if it has been flagged
			if (this.IsRedirect)
			{
				httpContext.Response.Redirect(this.RedirectUrl, true);
			}
				

			// here .Is404 _has_ to be true
			httpContext.Response.StatusCode = 404;

			if (!this.HasNode)
			{
				// means the builder could not find a proper document to handle 404
				// restore the saved content so we know it exists
				this.PublishedContent = content;
				return new PublishedContentNotFoundHandler();
			}

			if (!this.HasTemplate)
			{
				// means the builder could find a proper document, but the document has no template
				// at that point there isn't much we can do and there is no point returning
				// to Mvc since Mvc can't do much
				return new PublishedContentNotFoundHandler("In addition, no template exists to render the custom 404.");
			}

			// render even though we might have no template
			// to give MVC a chance to hijack routes
			// pass off to our handlers (mvc or webforms)

			// assign the legacy page back to the docrequest
			// handlers like default.aspx will want it and most macros currently need it
			this.UmbracoPage = new page(this);

			// these two are used by many legacy objects
			httpContext.Items["pageID"] = this.DocumentId;
			httpContext.Items["pageElements"] = this.UmbracoPage.Elements;

			switch (this.RenderingEngine)
			{
				case Core.RenderingEngine.Mvc:
					return null;
				case Core.RenderingEngine.WebForms:
				default:
					return (global::umbraco.UmbracoDefault)System.Web.Compilation.BuildManager.CreateInstanceFromVirtualPath("~/default.aspx", typeof(global::umbraco.UmbracoDefault));
			}
		}

		private PublishedContentRequestBuilder _builder;

		/// <summary>
		/// Initializes a new instance of the <see cref="PublishedContentRequest"/> class with a specific Uri and routing context.
		/// </summary>
		/// <param name="uri">The request <c>Uri</c>.</param>
		/// <param name="routingContext">A routing context.</param>
		public PublishedContentRequest(Uri uri, RoutingContext routingContext)
        {
			if (uri == null) throw new ArgumentNullException("uri");
			if (routingContext == null) throw new ArgumentNullException("routingContext");

			this.Uri = uri;
			this.RoutingContext = routingContext;

			_builder = new PublishedContentRequestBuilder(this);
			
			// set default
			this.RenderingEngine = RenderingEngine.Mvc;			
        }

        #region Properties

		/// <summary>
		/// The identifier of the requested node, if any, else zero.
		/// </summary>
		int _nodeId = 0;

		/// <summary>
		/// The requested node, if any, else <c>null</c>.
		/// </summary>
		private IPublishedContent _publishedContent = null;

		/// <summary>
		/// The "umbraco page" object.
		/// </summary>
		private page _umbracoPage;

		/// <summary>
		/// Gets or sets the current RoutingContext.
		/// </summary>
		public RoutingContext RoutingContext { get; private set; }
		
		/// <summary>
		/// Gets or sets the cleaned up Uri used for routing.
		/// </summary>
		public Uri Uri { get; private set; }

        /// <summary>
        /// Gets or sets the content request's domain.
        /// </summary>
        public Domain Domain { get; internal set; }

		/// <summary>
		/// Gets or sets the content request's domain Uri.
		/// </summary>
		/// <remarks>The <c>Domain</c> may contain "example.com" whereas the <c>Uri</c> will be fully qualified eg "http://example.com/".</remarks>
		public Uri DomainUri { get; internal set; }

		/// <summary>
		/// Gets or sets whether the rendering engine is MVC or WebForms.
		/// </summary>
		public RenderingEngine RenderingEngine { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the content request has a domain.
        /// </summary>
        public bool HasDomain
        {
            get { return this.Domain != null; }
        }

        /// <summary>
        /// Gets or sets the content request's culture.
        /// </summary>
        public CultureInfo Culture { get; set; }

		/// <summary>
		/// Gets or sets the "umbraco page" object.
		/// </summary>
		/// <remarks>
		/// This value is only used for legacy/webforms code.
		/// </remarks>
		internal page UmbracoPage
		{
			get
			{
				if (_umbracoPage == null)
					throw new InvalidOperationException("The umbraco page object is only available once Finalize()");

				return _umbracoPage;
			}
			set { _umbracoPage = value; }
		}
		
		// TODO: fixme - do we want to have an ordered list of alternate cultures,
        //         to allow for fallbacks when doing dictionnary lookup and such?

		/// <summary>
		/// Gets or sets the requested content.
		/// </summary>
		public IPublishedContent PublishedContent
		{			
			get { return _publishedContent; }
			set
			{
				_publishedContent = value;
				this.Template = null;
				_nodeId = _publishedContent != null ? _publishedContent.Id : 0;
			}
		}

        /// <summary>
        /// Gets or sets the template to use to display the requested content.
        /// </summary>
		public Template Template { get; set; }

        /// <summary>
        /// Gets a value indicating whether the content request has a template.
        /// </summary>
        public bool HasTemplate
        {
            get { return this.Template != null ; }
        }

        /// <summary>
        /// Gets the identifier of the requested content.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the content request has no content.</exception>
        public int DocumentId
        {
            get
            {
                if (this.PublishedContent == null)
                    throw new InvalidOperationException("PublishedContentRequest has no document.");
                return _nodeId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the content request has a content.
        /// </summary>
        public bool HasNode
        {
            get { return this.PublishedContent != null; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the requested content could not be found.
        /// </summary>
		/// <remarks>This is set in the <c>PublishedContentRequestBuilder</c>.</remarks>
        internal bool Is404 { get; set; }

        /// <summary>
        /// Gets a value indicating whether the content request triggers a redirect.
        /// </summary>
        public bool IsRedirect { get { return !string.IsNullOrWhiteSpace(this.RedirectUrl); } }

        /// <summary>
        /// Gets or sets the url to redirect to, when the content request triggers a redirect.
        /// </summary>
        public string RedirectUrl { get; set; }

        #endregion		
    }
}