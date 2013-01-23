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
using Template = umbraco.cms.businesslogic.template.Template;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Represents a request for one specified Umbraco IPublishedContent to be rendered
	/// by one specified template, using one specified Culture and RenderingEngine.
	/// </summary>
	internal class PublishedContentRequest
    {
		/// <summary>
		/// Triggers once the published content request has been prepared, but before it is processed.
		/// </summary>
		/// <remarks>When the event triggers, preparation is done ie domain, culture, document, template,
		/// rendering engine, etc. have been setup. It is then possible to change anything, before
		/// the request is actually processed and rendered by Umbraco.</remarks>
		public static event EventHandler<EventArgs> Prepared;

		// the engine that does all the processing
		// because in order to keep things clean and separated,
		// the content request is just a data holder
		private PublishedContentRequestEngine _engine;

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

			_engine = new PublishedContentRequestEngine(this);

			this.RenderingEngine = RenderingEngine.Mvc; // default
		}

		/// <summary>
		/// Gets the engine associated to the request.
		/// </summary>
		internal PublishedContentRequestEngine Engine { get { return _engine; } }

		/// <summary>
		/// Prepares the request.
		/// </summary>
		internal void Prepare()
		{
			_engine.PrepareRequest();
		}

		/// <summary>
		/// Updates the request when there is no template to render the content.
		/// </summary>
		internal void UpdateOnMissingTemplate()
		{
			_engine.UpdateRequestOnMissingTemplate();
		}

		/// <summary>
		/// Triggers the Prepared event.
		/// </summary>
		internal void OnPrepared()
		{
			if (Prepared != null)
				Prepared(this, EventArgs.Empty);
		}

		/// <summary>
		/// Gets or sets the cleaned up Uri used for routing.
		/// </summary>
		/// <remarks>The cleaned up Uri has no virtual directory, no trailing slash, no .aspx extension, etc.</remarks>
		public Uri Uri { get; private set; }

		#region PublishedContent

		/// <summary>
		/// The identifier of the requested IPublishedContent, if any, else zero.
		/// </summary>
		private int _publishedContentId = 0;

		/// <summary>
		/// The requested IPublishedContent, if any, else <c>null</c>.
		/// </summary>
		private IPublishedContent _publishedContent = null;

		/// <summary>
		/// The initial requested IPublishedContent, if any, else <c>null</c>.
		/// </summary>
		/// <remarks>The initial requested content is the content that was found by the finders,
		/// before anything such as 404, redirect... took place.</remarks>
		private IPublishedContent _initialPublishedContent = null;

		/// <summary>
		/// Gets or sets the requested content.
		/// </summary>
		/// <remarks>Setting the requested content clears <c>Template</c>.</remarks>
		public IPublishedContent PublishedContent
		{			
			get { return _publishedContent; }
			set
			{
				_publishedContent = value;
				this.Template = null;
				_publishedContentId = _publishedContent != null ? _publishedContent.Id : 0;
			}
		}

		/// <summary>
		/// Gets the initial requested content.
		/// </summary>
		/// <remarks>The initial requested content is the content that was found by the finders,
		/// before anything such as 404, redirect... took place.</remarks>
		public IPublishedContent InitialPublishedContent { get { return _initialPublishedContent; } }

		/// <summary>
		/// Gets or sets a value indicating whether the current published content is the initial one.
		/// </summary>
		public bool IsInitialPublishedContent 
		{
			get { return _initialPublishedContent != null && _initialPublishedContent == _publishedContent; }
			set { _initialPublishedContent = _publishedContent; }
		}

        /// <summary>
        /// Gets a value indicating whether the content request has a content.
        /// </summary>
        public bool HasPublishedContent
        {
            get { return this.PublishedContent != null; }
        }

		#endregion

		#region Template

	    /// <summary>
        /// Gets or sets the template to use to display the requested content.
        /// </summary>
		public Template Template { get; set; }

        /// <summary>
        /// Gets a value indicating whether the content request has a template.
        /// </summary>
        public bool HasTemplate
        {
            get { return this.Template != null; }
        }

		#endregion

		#region Domain and Culture

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

		// TODO: fixme - do we want to have an ordered list of alternate cultures,
        //         to allow for fallbacks when doing dictionnary lookup and such?

		#endregion

		#region Rendering

		/// <summary>
		/// Gets or sets whether the rendering engine is MVC or WebForms.
		/// </summary>
		public RenderingEngine RenderingEngine { get; internal set; }

		#endregion

		/// <summary>
		/// Gets or sets the current RoutingContext.
		/// </summary>
		public RoutingContext RoutingContext { get; private set; }

		/// <summary>
		/// The "umbraco page" object.
		/// </summary>
		private page _umbracoPage;

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

		#region Status

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