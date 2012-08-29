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
using umbraco.BusinessLogic;
using umbraco.NodeFactory;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.member;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
	
	/// <summary>
	/// Represents a found template that is resolved by the ILookups.
	/// The TemplateObject is the business logic object that represents a template, this will be different for 
	/// web forms and MVC. 
	/// </summary>
	/// <remarks>
	/// NOTE: This is not the prettiest thing in the world and we cannot use generics but we need to avoid looking up 
	/// template objects more than once which would occur if we were only storing the alias.
	/// Once we take templates out of the db this becomes even more interesting because the templateId on the XML
	/// will probably not be an integer Id anymore but more like an alias so the reprecussions will be big.
	/// </remarks>
	internal class TemplateLookup
	{
		/// <summary>
		/// Static method to return an empty template lookup
		/// </summary>
		/// <returns></returns>
		internal static TemplateLookup NoTemplate()
		{
			return new TemplateLookup();
		}

		private TemplateLookup()
		{
			
		}

		internal TemplateLookup(string alias, object templateObject)
		{
			TemplateAlias = alias;
			TemplateObject = templateObject;
		}

		internal bool FoundTemplate
		{
			get { return TemplateObject != null; }
		}

		/// <summary>
		/// The alias of the template found
		/// </summary>
		internal string TemplateAlias { get; private set; }

		/// <summary>
		/// The business logic template object that has been found, null if not found
		/// </summary>
		internal object TemplateObject { get; private set; }
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
        }

		/// <summary>
		/// the id of the requested node, if any, else zero.
		/// </summary>
		int _nodeId = 0;

		/// <summary>
		/// the requested node, if any, else null.
		/// </summary>
		XmlNode _xmlNode = null;

		private IDocument _node = null;

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
		public bool IsMvc { get; internal set; }

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

        // TODO: fixme - do we want to have an ordered list of alternate cultures,
        //         to allow for fallbacks when doing dictionnary lookup and such?

		public IDocument Node
		{			
			get { return _node; }
			set
			{
				_node = value;
				this.TemplateLookup = null;
				_nodeId = _node != null ? _node.Id : 0;
			}
		}

        /// <summary>
        /// Gets or sets the document request's template lookup
        /// </summary>
		public TemplateLookup TemplateLookup { get; set; }

        /// <summary>
        /// Gets a value indicating whether the document request has a template.
        /// </summary>
        public bool HasTemplate
        {
            get { return this.TemplateLookup != null && TemplateLookup.FoundTemplate; }
        }

        /// <summary>
        /// Gets the id of the document.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the document request has no document.</exception>
        public int NodeId
        {
            get
            {
                if (this.Node == null)
                    throw new InvalidOperationException("DocumentRequest has no document.");
                return _nodeId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the document request has a document.
        /// </summary>
        public bool HasNode
        {
            get { return this.Node != null; }
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