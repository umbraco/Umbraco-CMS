using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Diagnostics;

// legacy
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.NodeFactory;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.language;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{



	/// <summary>
	/// represents a request for one specified Umbraco document to be rendered
	/// by one specified template, using one particular culture.
	/// </summary>
    internal class DocumentRequest
    {
		public DocumentRequest(Uri uri, UmbracoContext umbracoContext)
        {
			this.Uri = uri;
			RoutingContext = umbracoContext.RoutingContext;
			UmbracoContext = umbracoContext;
        }

		/// <summary>
		/// the id of the requested node, if any, else zero.
		/// </summary>
		int _nodeId = 0;

		/// <summary>
		/// the requested node, if any, else null.
		/// </summary>
		XmlNode _xmlNode = null;

		private INode _node = null;

        #region Properties

		/// <summary>
		/// Returns the current RoutingContext
		/// </summary>
		public RoutingContext RoutingContext { get; private set; }

		public UmbracoContext UmbracoContext { get; private set; }

		public Uri Uri { get; private set; }

        /// <summary>
        /// Gets or sets the document request's domain.
        /// </summary>
        public Domain Domain { get; private set; }

		public Uri DomainUri { get; private set; }

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

		public INode Node
		{
			get
			{
				if (!HasNode)
					return null;
				if (_node == null)
				{
					//TODO: See the note below, if we don't allow for a get/set INode then how would someone implement
					// their own INode? it would not be possible since we're instantiating a specific Node object here.
					_node = new Node(XmlNode);
				}
				return _node;
			}
		}

		//TODO: Should we remove this somehow in place of an INode getter/setter? we are really bound to the xml structure here
        /// <summary>
        /// Gets or sets the document request's document xml node.
        /// </summary>
        internal XmlNode XmlNode
        {
            get
            {
                return _xmlNode;
            }
            set
            {
                _xmlNode = value;
                this.Template = null;
                if (_xmlNode != null)
					_nodeId = int.Parse(RoutingContext.ContentStore.GetNodeProperty(_xmlNode, "@id"));
                else
                    _nodeId = 0;
            }
        }

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
                if (this.XmlNode == null)
                    throw new InvalidOperationException("DocumentRequest has no document.");
                return _nodeId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the document request has a document.
        /// </summary>
        public bool HasNode
        {
            get { return this.XmlNode != null; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the requested document could not be found.
        /// </summary>
        public bool Is404 { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the document request triggers a redirect.
        /// </summary>
        public bool IsRedirect { get { return !string.IsNullOrWhiteSpace(this.RedirectUrl); } }

        /// <summary>
        /// Gets the url to redirect to, when the document request triggers a redirect.
        /// </summary>
        public string RedirectUrl { get; set; }

        #endregion

        #region Lookup

        /// <summary>
        /// Determines the site root (if any) matching the http request.
        /// </summary>        
        /// <returns>A value indicating whether a domain was found.</returns>
		internal bool LookupDomain()
        {
			const string tracePrefix = "LookupDomain: ";

            // note - we are not handling schemes nor ports here.

			LogHelper.Debug<DocumentRequest>("{0}Uri=\"{1}\"", () => tracePrefix, () => this.Uri);

            // try to find a domain matching the current request
			var domainAndUri = DomainHelper.DomainMatch(Domain.GetDomains(), UmbracoContext.UmbracoUrl, false);

            // handle domain
			if (domainAndUri != null)
            {
                // matching an existing domain
				LogHelper.Debug<DocumentRequest>("{0}Matches domain=\"{1}\", rootId={2}, culture=\"{3}\"",
					() => tracePrefix,
					() => domainAndUri.Domain.Name,
					() => domainAndUri.Domain.RootNodeId,
					() => domainAndUri.Domain.Language.CultureAlias);

				this.Domain = domainAndUri.Domain;
				this.DomainUri = domainAndUri.Uri;
				this.Culture = new CultureInfo(domainAndUri.Domain.Language.CultureAlias);

				// canonical? not implemented at the moment
				// if (...)
				// {
				//  this.RedirectUrl = "...";
				//  return true;
				// }
            }
            else
            {
                // not matching any existing domain
            	LogHelper.Debug<DocumentRequest>("{0}Matches no domain", () => tracePrefix);

                var defaultLanguage = Language.GetAllAsList().FirstOrDefault();
                this.Culture = defaultLanguage == null ? CultureInfo.CurrentUICulture : new CultureInfo(defaultLanguage.CultureAlias);
            }

			LogHelper.Debug<DocumentRequest>("{0}Culture=\"{1}\"", () => tracePrefix, () => this.Culture.Name);

            return this.Domain != null;
        }

        /// <summary>
        /// Determines the Umbraco document (if any) matching the http request.
        /// </summary>
        /// <returns>A value indicating whether a document and template nave been found.</returns>
        internal bool LookupDocument()
        {
			const string tracePrefix = "LookupDocument: ";
			LogHelper.Debug<DocumentRequest>("{0}Path=\"{1}\"", () => tracePrefix, () => this.Uri.AbsolutePath);

            // look for the document
            // the first successful resolver, if any, will set this.Node, and may also set this.Template
            // some lookups may implement caching
            
			using (DisposableTimer.DebugDuration<PluginManager>(
				string.Format("{0}Begin resolvers", tracePrefix),
				string.Format("{0}End resolvers, {1}", tracePrefix, (this.HasNode ? "a document was found" : "no document was found"))))
			{				
				RoutingContext.DocumentLookups.Any(lookup => lookup.TrySetDocument(this));	
			}			

            // fixme - not handling umbracoRedirect
            // should come after internal redirects
            // so after ResolveDocument2() => docreq.IsRedirect => handled by the module!

            // handle not-found, redirects, access, template
            LookupDocument2();

            // handle umbracoRedirect (moved from umbraco.page)
            FollowRedirect();

            bool resolved = this.HasNode && this.HasTemplate;
            return resolved;
        }

        /// <summary>
        /// Performs the document resolution second pass.
        /// </summary>
        /// <remarks>The second pass consists in handling "not found", internal redirects, access validation, and template.</remarks>
		private void LookupDocument2()
        {
			const string tracePrefix = "LookupDocument2: ";

            // handle "not found", follow internal redirects, validate access, template
            // because these might loop, we have to have some sort of infinite loop detection 
            int i = 0, j = 0;
            const int maxLoop = 12;
            do
            {
				LogHelper.Debug<DocumentRequest>("{0}{1}", () => tracePrefix, () => (i == 0 ? "Begin" : "Loop"));                

                // handle not found
                if (!this.HasNode)
                {
                    this.Is404 = true;
					LogHelper.Debug<DocumentRequest>("{0}No document, try last chance lookup", () => tracePrefix);                    

                    // if it fails then give up, there isn't much more that we can do
					var lastChance = RoutingContext.DocumentLastChanceLookup;
					if (lastChance == null || !lastChance.TrySetDocument(this))
                    {
						LogHelper.Debug<DocumentRequest>("{0}Failed to find a document, give up", () => tracePrefix);
                        break;
                    }
                    else
                    {
						LogHelper.Debug<DocumentRequest>("{0}Found a document", () => tracePrefix);
                    }
                }

                // follow internal redirects as long as it's not running out of control ie infinite loop of some sort
                j = 0;
                while (FollowInternalRedirects() && j++ < maxLoop) ;
                if (j == maxLoop) // we're running out of control
                    break;

                // ensure access
                if (this.HasNode)
                    EnsureNodeAccess();

                // resolve template
                if (this.HasNode)
                    LookupTemplate();

                // loop while we don't have page, ie the redirect or access
                // got us to nowhere and now we need to run the notFoundLookup again
                // as long as it's not running out of control ie infinite loop of some sort

            } while (!this.HasNode && i++ < maxLoop);

            if (i == maxLoop || j == maxLoop)
            {
				LogHelper.Debug<DocumentRequest>("{0}Looks like we're running into an infinite loop, abort", () => tracePrefix);
                this.XmlNode = null;
            }
			LogHelper.Debug<DocumentRequest>("{0}End", () => tracePrefix);
        }

        /// <summary>
        /// Follows internal redirections through the <c>umbracoInternalRedirectId</c> document property.
        /// </summary>
        /// <returns>A value indicating whether redirection took place and led to a new published document.</returns>
        /// <remarks>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</remarks>
        private bool FollowInternalRedirects()
        {
            const string tracePrefix = "FollowInternalRedirects: ";

            if (this.XmlNode == null)
                throw new InvalidOperationException("There is no node.");

            bool redirect = false;
			string internalRedirect = RoutingContext.ContentStore.GetNodeProperty(this.XmlNode, "umbracoInternalRedirectId");

            if (!string.IsNullOrWhiteSpace(internalRedirect))
            {
				LogHelper.Debug<DocumentRequest>("{0}Found umbracoInternalRedirectId={1}", () => tracePrefix, () => internalRedirect);

                int internalRedirectId;
                if (!int.TryParse(internalRedirect, out internalRedirectId))
                    internalRedirectId = -1;

                if (internalRedirectId <= 0)
                {
                    // bad redirect
                    this.XmlNode = null;
					LogHelper.Debug<DocumentRequest>("{0}Failed to redirect to id={1}: invalid value", () => tracePrefix, () => internalRedirect);
                }
                else if (internalRedirectId == this.NodeId)
                {
                    // redirect to self
					LogHelper.Debug<DocumentRequest>("{0}Redirecting to self, ignore", () => tracePrefix);
                }
                else
                {
                    // redirect to another page
					var node = RoutingContext.ContentStore.GetNodeById(internalRedirectId);
                    this.XmlNode = node;
                    if (node != null)
                    {
                        redirect = true;
						LogHelper.Debug<DocumentRequest>("{0}Redirecting to id={1}", () => tracePrefix, () => internalRedirectId);
                    }
                    else
                    {
						LogHelper.Debug<DocumentRequest>("{0}Failed to redirect to id={1}: no such published document", () => tracePrefix, () => internalRedirectId);
                    }
                }
            }

            return redirect;
        }

        /// <summary>
        /// Ensures that access to current node is permitted.
        /// </summary>
        /// <remarks>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</remarks>
        private void EnsureNodeAccess()
        {
            const string tracePrefix = "EnsurePageAccess: ";

            if (this.XmlNode == null)
                throw new InvalidOperationException("There is no node.");

			var path = RoutingContext.ContentStore.GetNodeProperty(this.XmlNode, "@path");

            if (Access.IsProtected(this.NodeId, path))
            {
				LogHelper.Debug<DocumentRequest>("{0}Page is protected, check for access", () => tracePrefix);

                var user = System.Web.Security.Membership.GetUser();

                if (user == null || !Member.IsLoggedOn())
                {
					LogHelper.Debug<DocumentRequest>("{0}Not logged in, redirect to login page", () => tracePrefix);
                    var loginPageId = Access.GetLoginPage(path);
                    if (loginPageId != this.NodeId)
						this.XmlNode = RoutingContext.ContentStore.GetNodeById(loginPageId);
                }
                else if (!Access.HasAccces(this.NodeId, user.ProviderUserKey))
                {
					LogHelper.Debug<DocumentRequest>("{0}Current member has not access, redirect to error page", () => tracePrefix);
                    var errorPageId = Access.GetErrorPage(path);
                    if (errorPageId != this.NodeId)
						this.XmlNode = RoutingContext.ContentStore.GetNodeById(errorPageId);
                }
                else
                {
					LogHelper.Debug<DocumentRequest>("{0}Current member has access", () => tracePrefix);
                }
            }
            else
            {
				LogHelper.Debug<DocumentRequest>("{0}Page is not protected", () => tracePrefix);
            }
        }

        /// <summary>
        /// Resolves a template for the current node.
        /// </summary>
        private void LookupTemplate()
        {
			const string tracePrefix = "LookupTemplate: ";

            if (this.XmlNode == null)
                throw new InvalidOperationException("There is no node.");

			var templateAlias = UmbracoContext.HttpContext.Request.QueryString["altTemplate"];
            if (string.IsNullOrWhiteSpace(templateAlias))
				templateAlias = UmbracoContext.HttpContext.Request.Form["altTemplate"];

            // fixme - we might want to support cookies?!? NO but provide a hook to change the template

            if (!this.HasTemplate || !string.IsNullOrWhiteSpace(templateAlias))
            {
                if (string.IsNullOrWhiteSpace(templateAlias))
                {
					templateAlias = RoutingContext.ContentStore.GetNodeProperty(this.XmlNode, "@template");
					LogHelper.Debug<DocumentRequest>("{0}Look for template id={1}", () => tracePrefix, () => templateAlias);
                    int templateId;
                    if (!int.TryParse(templateAlias, out templateId))
                        templateId = 0;
                    this.Template = templateId > 0 ? new Template(templateId) : null;
                }
                else
                {
					LogHelper.Debug<DocumentRequest>("{0}Look for template alias=\"{1}\" (altTemplate)", () => tracePrefix, () => templateAlias);
                    this.Template = Template.GetByAlias(templateAlias);
                }

                if (!this.HasTemplate)
                {
					LogHelper.Debug<DocumentRequest>("{0}No template was found", () => tracePrefix);

                    //TODO: I like the idea of this new setting, but lets get this in to the core at a later time, for now lets just get the basics working.
                    //if (Settings.HandleMissingTemplateAs404)
                    //{
                    //    this.Node = null;
                    //    LogHelper.Debug<DocumentRequest>("{0}Assume page not found (404)", tracePrefix);
                    //}

                    // else we have no template
                    // and there isn't much more we can do about it
                }
                else
                {
                    LogHelper.Debug<DocumentRequest>("{0}Found", () => tracePrefix);
                }
            }
        }

        /// <summary>
        /// Follows external redirection through <c>umbracoRedirect</c> document property.
        /// </summary>
        private void FollowRedirect()
        {
            if (this.HasNode)
            {
                int redirectId;
				if (!int.TryParse(RoutingContext.ContentStore.GetNodeProperty(this.XmlNode, "umbracoRedirect"), out redirectId))
                    redirectId = -1;
                string redirectUrl = "#";
                if (redirectId > 0)
					redirectUrl = RoutingContext.NiceUrlProvider.GetNiceUrl(redirectId);
                if (redirectUrl != "#")
                    this.RedirectUrl = redirectUrl;
            }
        }

        #endregion
    }
}