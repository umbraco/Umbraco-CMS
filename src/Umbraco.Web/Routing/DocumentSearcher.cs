using System;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Routing
{
	internal class DocumentSearcher
	{
		private readonly DocumentRequest _documentRequest;
		private readonly UmbracoContext _umbracoContext;
		private readonly RoutingContext _routingContext;

		public DocumentSearcher(DocumentRequest documentRequest)
		{
			_documentRequest = documentRequest;
			_umbracoContext = documentRequest.RoutingContext.UmbracoContext;
			_routingContext = documentRequest.RoutingContext;
		}

		/// <summary>
		/// Determines the site root (if any) matching the http request.
		/// </summary>        
		/// <returns>A value indicating whether a domain was found.</returns>
		internal bool LookupDomain()
		{
			const string tracePrefix = "LookupDomain: ";

			// note - we are not handling schemes nor ports here.

			LogHelper.Debug<DocumentRequest>("{0}Uri=\"{1}\"", () => tracePrefix, () => _documentRequest.Uri);

			// try to find a domain matching the current request
			var domainAndUri = DomainHelper.DomainMatch(Domain.GetDomains(), _umbracoContext.UmbracoUrl, false);

			// handle domain
			if (domainAndUri != null)
			{
				// matching an existing domain
				LogHelper.Debug<DocumentRequest>("{0}Matches domain=\"{1}\", rootId={2}, culture=\"{3}\"",
				                                 () => tracePrefix,
				                                 () => domainAndUri.Domain.Name,
				                                 () => domainAndUri.Domain.RootNodeId,
				                                 () => domainAndUri.Domain.Language.CultureAlias);

				_documentRequest.Domain = domainAndUri.Domain;
				_documentRequest.DomainUri = domainAndUri.Uri;
				_documentRequest.Culture = new CultureInfo(domainAndUri.Domain.Language.CultureAlias);

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
				_documentRequest.Culture = defaultLanguage == null ? CultureInfo.CurrentUICulture : new CultureInfo(defaultLanguage.CultureAlias);
			}

			LogHelper.Debug<DocumentRequest>("{0}Culture=\"{1}\"", () => tracePrefix, () => _documentRequest.Culture.Name);

			return _documentRequest.Domain != null;
		}

		/// <summary>
		/// Determines the Umbraco document (if any) matching the http request.
		/// </summary>
		/// <returns>A value indicating whether a document and template nave been found.</returns>
		internal bool LookupDocument()
		{
			const string tracePrefix = "LookupDocument: ";
			LogHelper.Debug<DocumentRequest>("{0}Path=\"{1}\"", () => tracePrefix, () => _documentRequest.Uri.AbsolutePath);

			// look for the document
			// the first successful resolver, if any, will set this.Node, and may also set this.Template
			// some lookups may implement caching

			using (DisposableTimer.DebugDuration<PluginManager>(
				string.Format("{0}Begin resolvers", tracePrefix),
				string.Format("{0}End resolvers, {1}", tracePrefix, (_documentRequest.HasNode ? "a document was found" : "no document was found"))))
			{
				_routingContext.DocumentLookups.Any(lookup => lookup.TrySetDocument(_documentRequest));
			}

			// fixme - not handling umbracoRedirect
			// should come after internal redirects
			// so after ResolveDocument2() => docreq.IsRedirect => handled by the module!

			// handle not-found, redirects, access, template
			LookupDocument2();

			// handle umbracoRedirect (moved from umbraco.page)
			FollowRedirect();

			bool resolved = _documentRequest.HasNode && _documentRequest.HasTemplate;
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
				if (!_documentRequest.HasNode)
				{
					_documentRequest.Is404 = true;
					LogHelper.Debug<DocumentRequest>("{0}No document, try last chance lookup", () => tracePrefix);

					// if it fails then give up, there isn't much more that we can do
					var lastChance = _routingContext.DocumentLastChanceLookup;
					if (lastChance == null || !lastChance.TrySetDocument(_documentRequest))
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
				if (_documentRequest.HasNode)
					EnsureNodeAccess();

				// resolve template
				if (_documentRequest.HasNode)
					LookupTemplate();

				// loop while we don't have page, ie the redirect or access
				// got us to nowhere and now we need to run the notFoundLookup again
				// as long as it's not running out of control ie infinite loop of some sort

			} while (!_documentRequest.HasNode && i++ < maxLoop);

			if (i == maxLoop || j == maxLoop)
			{
				LogHelper.Debug<DocumentRequest>("{0}Looks like we're running into an infinite loop, abort", () => tracePrefix);
				_documentRequest.Node = null;
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

			if (_documentRequest.Node == null)
				throw new InvalidOperationException("There is no node.");

			bool redirect = false;
			string internalRedirect = _routingContext.ContentStore.GetDocumentProperty(_umbracoContext, _documentRequest.Node, "umbracoInternalRedirectId");

			if (!string.IsNullOrWhiteSpace(internalRedirect))
			{
				LogHelper.Debug<DocumentRequest>("{0}Found umbracoInternalRedirectId={1}", () => tracePrefix, () => internalRedirect);

				int internalRedirectId;
				if (!int.TryParse(internalRedirect, out internalRedirectId))
					internalRedirectId = -1;

				if (internalRedirectId <= 0)
				{
					// bad redirect
					_documentRequest.Node = null;
					LogHelper.Debug<DocumentRequest>("{0}Failed to redirect to id={1}: invalid value", () => tracePrefix, () => internalRedirect);
				}
				else if (internalRedirectId == _documentRequest.NodeId)
				{
					// redirect to self
					LogHelper.Debug<DocumentRequest>("{0}Redirecting to self, ignore", () => tracePrefix);
				}
				else
				{
					// redirect to another page
					var node = _routingContext.ContentStore.GetDocumentById(
						_umbracoContext,
						internalRedirectId);
					
					_documentRequest.Node = node;
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

			if (_documentRequest.Node == null)
				throw new InvalidOperationException("There is no node.");

			var path = _routingContext.ContentStore.GetDocumentProperty(_umbracoContext, _documentRequest.Node, "@path");

			if (Access.IsProtected(_documentRequest.NodeId, path))
			{
				LogHelper.Debug<DocumentRequest>("{0}Page is protected, check for access", () => tracePrefix);

				var user = System.Web.Security.Membership.GetUser();

				if (user == null || !Member.IsLoggedOn())
				{
					LogHelper.Debug<DocumentRequest>("{0}Not logged in, redirect to login page", () => tracePrefix);
					var loginPageId = Access.GetLoginPage(path);
					if (loginPageId != _documentRequest.NodeId)
						_documentRequest.Node = _routingContext.ContentStore.GetDocumentById(
							_umbracoContext,
							loginPageId);
				}
				else if (!Access.HasAccces(_documentRequest.NodeId, user.ProviderUserKey))
				{
					LogHelper.Debug<DocumentRequest>("{0}Current member has not access, redirect to error page", () => tracePrefix);
					var errorPageId = Access.GetErrorPage(path);
					if (errorPageId != _documentRequest.NodeId)
						_documentRequest.Node = _routingContext.ContentStore.GetDocumentById(
							_umbracoContext,
							errorPageId);
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

			if (_documentRequest.Node == null)
				throw new InvalidOperationException("There is no node.");

			var templateAlias = _umbracoContext.HttpContext.Request.QueryString["altTemplate"];
			if (string.IsNullOrWhiteSpace(templateAlias))
				templateAlias = _umbracoContext.HttpContext.Request.Form["altTemplate"];

			// fixme - we might want to support cookies?!? NO but provide a hook to change the template

			if (!_documentRequest.HasTemplate || !string.IsNullOrWhiteSpace(templateAlias))
			{
				if (string.IsNullOrWhiteSpace(templateAlias))
				{
					templateAlias = _routingContext.ContentStore.GetDocumentProperty(_umbracoContext, _documentRequest.Node, "@TemplateId");
					LogHelper.Debug<DocumentRequest>("{0}Look for template id={1}", () => tracePrefix, () => templateAlias);
					int templateId;
					if (!int.TryParse(templateAlias, out templateId))
						templateId = 0;
					_documentRequest.Template = templateId > 0 ? new Template(templateId) : null;
				}
				else
				{
					LogHelper.Debug<DocumentRequest>("{0}Look for template alias=\"{1}\" (altTemplate)", () => tracePrefix, () => templateAlias);
					_documentRequest.Template = Template.GetByAlias(templateAlias);
				}

				if (!_documentRequest.HasTemplate)
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
			if (_documentRequest.HasNode)
			{
				int redirectId;
				if (!int.TryParse(_routingContext.ContentStore.GetDocumentProperty(_umbracoContext, _documentRequest.Node, "umbracoRedirect"), out redirectId))
					redirectId = -1;
				string redirectUrl = "#";
				if (redirectId > 0)
					redirectUrl = _routingContext.NiceUrlProvider.GetNiceUrl(redirectId);
				if (redirectUrl != "#")
					_documentRequest.RedirectUrl = redirectUrl;
			}
		}
	}
}