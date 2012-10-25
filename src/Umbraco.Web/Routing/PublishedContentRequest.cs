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
		/// This creates a PublishedContentRequest and assigns it to the current HttpContext and then proceeds to 
		/// process the request using the PublishedContentRequestBuilder. If everything is successful, the callback 
		/// method will be called.
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="umbracoContext"></param>
		/// <param name="uri"></param>
		/// <param name="onSuccess"></param>
		internal static void ProcessRequest(HttpContextBase httpContext, UmbracoContext umbracoContext, Uri uri, Action<PublishedContentRequest> onSuccess)
		{
			if (umbracoContext == null)
				throw new NullReferenceException("The UmbracoContext.Current is null, ProcessRequest cannot proceed unless there is a current UmbracoContext");
			if (uri == null) throw new ArgumentNullException("uri");
			if (umbracoContext.RoutingContext == null)
				throw new NullReferenceException("The UmbracoContext.RoutingContext has not been assigned, ProcessRequest cannot proceed unless there is a RoutingContext assigned to the UmbracoContext");

			var docreq = new PublishedContentRequest(uri, umbracoContext.RoutingContext);
			//assign back since this is a front-end request
			umbracoContext.PublishedContentRequest = docreq;

			// note - at that point the original legacy module did something do handle IIS custom 404 errors
			//   ie pages looking like /anything.aspx?404;/path/to/document - I guess the reason was to support
			//   "directory urls" without having to do wildcard mapping to ASP.NET on old IIS. This is a pain
			//   to maintain and probably not used anymore - removed as of 06/2012. @zpqrtbnk.
			//
			//   to trigger Umbraco's not-found, one should configure IIS and/or ASP.NET custom 404 errors
			//   so that they point to a non-existing page eg /redirect-404.aspx
			//   TODO: SD: We need more information on this for when we release 4.10.0 as I'm not sure what this means.

			//create the searcher
			var searcher = new PublishedContentRequestBuilder(docreq);
			//find domain
			searcher.LookupDomain();
			// redirect if it has been flagged
			if (docreq.IsRedirect)
				httpContext.Response.Redirect(docreq.RedirectUrl, true);
			//set the culture on the thread
			Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = docreq.Culture;
			//find the document, found will be true if the doc request has found BOTH a node and a template
			// though currently we don't use this value.
			var found = searcher.LookupDocument();
			//this could be called in the LookupDocument method, but I've just put it here for clarity.
			searcher.DetermineRenderingEngine();

			//TODO: here we should launch an event so that people can modify the doc request to do whatever they want.

			// redirect if it has been flagged
			if (docreq.IsRedirect)
				httpContext.Response.Redirect(docreq.RedirectUrl, true);

			// handle 404
			if (docreq.Is404)
			{
				httpContext.Response.StatusCode = 404;

				if (!docreq.HasNode)
				{
					httpContext.RemapHandler(new PublishedContentNotFoundHandler());
					return;
				}

				// else we have a document to render
				// not having a template is ok here, MVC will take care of it
			}

			// just be safe - should never ever happen
			if (!docreq.HasNode)
				throw new Exception("No document to render.");

			// render even though we might have no template
			// to give MVC a chance to hijack routes
			// pass off to our handlers (mvc or webforms)

			// assign the legacy page back to the docrequest
			// handlers like default.aspx will want it and most macros currently need it
			docreq.UmbracoPage = new page(docreq);

			// these two are used by many legacy objects
			httpContext.Items["pageID"] = docreq.DocumentId;
			httpContext.Items["pageElements"] = docreq.UmbracoPage.Elements;

			if (onSuccess != null)
				onSuccess(docreq);
		}


		/// <summary>
		/// Create a content request for a specific URL
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="routingContext"></param>
		public PublishedContentRequest(Uri uri, RoutingContext routingContext)
        {
			if (uri == null) throw new ArgumentNullException("uri");
			if (routingContext == null) throw new ArgumentNullException("routingContext");

			this.Uri = uri;
			RoutingContext = routingContext;
			
			//set default
			RenderingEngine = RenderingEngine.Mvc;			
        }

		/// <summary>
		/// the id of the requested node, if any, else zero.
		/// </summary>
		int _nodeId = 0;
		
		private IPublishedContent _publishedContent = null;

        #region Properties

		/// <summary>
		/// Returns the current RoutingContext
		/// </summary>
		public RoutingContext RoutingContext { get; private set; }
		
		/// <summary>
		/// The cleaned up Uri used for routing
		/// </summary>
		public Uri Uri { get; private set; }

        /// <summary>
        /// Gets or sets the document request's domain.
        /// </summary>
        public Domain Domain { get; internal set; }

		public Uri DomainUri { get; internal set; }

		/// <summary>
		/// Gets or sets whether the rendering engine is MVC or WebForms
		/// </summary>
		public RenderingEngine RenderingEngine { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the document request has a domain.
        /// </summary>
        public bool HasDomain
        {
            get { return this.Domain != null; }
        }

        /// <summary>
        /// Gets or sets the document request's culture
        /// </summary>
        public CultureInfo Culture { get; set; }

		private page _umbracoPage;

		/// <summary>
		/// Returns the Umbraco page object
		/// </summary>
		/// <remarks>
		/// This value is only used for legacy/webforms code.
		/// </remarks>
		internal page UmbracoPage
		{
			get
			{
				if (_umbracoPage == null)
				{
					throw new InvalidOperationException("The umbraco page object is only available once Finalize()");
				}
				return _umbracoPage;
			}
			set { _umbracoPage = value; }
		}
		
		// TODO: fixme - do we want to have an ordered list of alternate cultures,
        //         to allow for fallbacks when doing dictionnary lookup and such?

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
        /// Gets or sets the document request's template lookup
        /// </summary>
		public Template Template { get; set; }

        /// <summary>
        /// Gets a value indicating whether the document request has a template.
        /// </summary>
        public bool HasTemplate
        {
            get { return this.Template != null ; }
        }

        /// <summary>
        /// Gets the id of the document.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the document request has no document.</exception>
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
        /// Gets a value indicating whether the document request has a document.
        /// </summary>
        public bool HasNode
        {
            get { return this.PublishedContent != null; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the requested document could not be found. This is set in the PublishedContentRequestBuilder.
        /// </summary>
        internal bool Is404 { get; set; }

        /// <summary>
        /// Gets a value indicating whether the document request triggers a redirect.
        /// </summary>
        public bool IsRedirect { get { return !string.IsNullOrWhiteSpace(this.RedirectUrl); } }

        /// <summary>
        /// Gets the url to redirect to, when the document request triggers a redirect.
        /// </summary>
        public string RedirectUrl { get; set; }

        #endregion
		
    }
}