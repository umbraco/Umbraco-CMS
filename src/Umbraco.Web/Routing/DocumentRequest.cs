using System;
using System.Linq;
using System.Text;
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

	internal enum RenderingEngine
	{
		Mvc,
		WebForms
	}
	
	/// <summary>
	/// represents a request for one specified Umbraco document to be rendered
	/// by one specified template, using one particular culture.
	/// </summary>
	internal class DocumentRequest
    {
		public DocumentRequest(Uri uri, RoutingContext routingContext)
        {
			this.Uri = uri;
			RoutingContext = routingContext;
			RenderingEngine = RenderingEngine.Mvc;			
        }

		/// <summary>
		/// the id of the requested node, if any, else zero.
		/// </summary>
		int _nodeId = 0;
		
		private IDocument _document = null;

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

		public IDocument Document
		{			
			get { return _document; }
			set
			{
				_document = value;
				this.Template = null;
				_nodeId = _document != null ? _document.Id : 0;
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
                if (this.Document == null)
                    throw new InvalidOperationException("DocumentRequest has no document.");
                return _nodeId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the document request has a document.
        /// </summary>
        public bool HasNode
        {
            get { return this.Document != null; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the requested document could not be found.
        /// </summary>
        public bool Is404 { get; internal set; }

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