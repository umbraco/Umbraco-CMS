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
			//get
			//{
			//    if (!HasNode)
			//        return null;
			//    if (_node == null)
			//    {
			//        //TODO: See the note below, if we don't allow for a get/set INode then how would someone implement
			//        // their own INode? it would not be possible since we're instantiating a specific Node object here.
			//        _node = new Node(XmlNode);
			//    }
			//    return _node;
			//}
			get { return _node; }
			set
			{
				_node = value;
				this.Template = null;
				_nodeId = _node != null ? _node.Id : 0;
			}
		}

		////TODO: Should we remove this somehow in place of an INode getter/setter? we are really bound to the xml structure here
		///// <summary>
		///// Gets or sets the document request's document xml node.
		///// </summary>
		//internal XmlNode XmlNode
		//{
		//    get
		//    {
		//        return _xmlNode;
		//    }
		//    set
		//    {
		//        _xmlNode = value;
		//        this.Template = null;
		//        if (_xmlNode != null)
		//            _nodeId = int.Parse(RoutingContext.ContentStore.GetNodeProperty(_xmlNode, "@id"));
		//        else
		//            _nodeId = 0;
		//    }
		//}

        /// <summary>
        /// Gets or sets the document request's template.
        /// </summary>
        public Template Template { get; set; }

        /// <summary>
        /// Gets a value indicating whether the document request has a template.
        /// </summary>
        public bool HasTemplate
        {
            get { return this.Template != null; }
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