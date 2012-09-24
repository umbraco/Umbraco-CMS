using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Looks up the document using ILookup's and sets any additional properties required on the DocumentRequest object
	/// </summary>
	internal class DocumentRequestBuilder
	{
		private readonly DocumentRequest _documentRequest;
		private readonly UmbracoContext _umbracoContext;
		private readonly RoutingContext _routingContext;

		public DocumentRequestBuilder(DocumentRequest documentRequest)
		{
			_documentRequest = documentRequest;
			_umbracoContext = documentRequest.RoutingContext.UmbracoContext;
			_routingContext = documentRequest.RoutingContext;
		}

		/// <summary>
		/// Determines the rendering engine to use and sets the flag on the DocumentRequest
		/// </summary>
		internal void DetermineRenderingEngine()
		{
			//First, if there is no template, we will default to use MVC because MVC supports Hijacking routes which
			//sometimes don't require a template since the developer may want full control over the rendering. 
			//Webforms doesn't support this so MVC it is. MVC will also handle what to do if no template or hijacked route
			//is there (i.e. blank page)
			if (!_documentRequest.HasTemplate)
			{
				_documentRequest.RenderingEngine = RenderingEngine.Mvc;
				return;
			}

			var templateAlias = _documentRequest.Template.Alias;

			Func<DirectoryInfo, string, string[], RenderingEngine, bool> determineEngine =
				(directory, alias, extensions, renderingEngine) =>
					{
						//so we have a template, now we need to figure out where the template is, this is done just by the Alias field					
						//ensure it exists
						if (!directory.Exists) Directory.CreateDirectory(directory.FullName);
						var file = directory.GetFiles()
							.FirstOrDefault(x => extensions.Any(e => x.Name.InvariantEquals(alias + e)));
											
						if (file != null)
						{
							//it is mvc since we have a template there that exists with this alias
							_documentRequest.RenderingEngine = renderingEngine;
							return true;
						}
						return false;
					};

			//first determine if it is MVC, we will favor mvc if there is a template with the same name in both 
			// folders, if it is then MVC will be selected
			if (!determineEngine(
				new DirectoryInfo(IOHelper.MapPath(SystemDirectories.MvcViews)),
				templateAlias,
				new[]{".cshtml", ".vbhtml"},
				RenderingEngine.Mvc))
			{
				//if not, then determine if it is webforms (this should def match if a template is assigned and its not in the MVC folder)
				// if it doesn't match, then MVC will be used by default anyways.
				determineEngine(
					new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Masterpages)),
					templateAlias,
					new[] {".master"},
					RenderingEngine.WebForms);
			}

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

					LogHelper.Debug<DocumentRequest>("{0}Found a document", () => tracePrefix);
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
				_documentRequest.Document = null;
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

			if (_documentRequest.Document == null)
				throw new InvalidOperationException("There is no node.");

			bool redirect = false;
			var internalRedirect = _documentRequest.Document.GetPropertyValue<string>("umbracoInternalRedirectId");
			
			if (!string.IsNullOrWhiteSpace(internalRedirect))
			{
				LogHelper.Debug<DocumentRequest>("{0}Found umbracoInternalRedirectId={1}", () => tracePrefix, () => internalRedirect);

				int internalRedirectId;
				if (!int.TryParse(internalRedirect, out internalRedirectId))
					internalRedirectId = -1;

				if (internalRedirectId <= 0)
				{
					// bad redirect
					_documentRequest.Document = null;
					LogHelper.Debug<DocumentRequest>("{0}Failed to redirect to id={1}: invalid value", () => tracePrefix, () => internalRedirect);
				}
				else if (internalRedirectId == _documentRequest.DocumentId)
				{
					// redirect to self
					LogHelper.Debug<DocumentRequest>("{0}Redirecting to self, ignore", () => tracePrefix);
				}
				else
				{
					// redirect to another page
					var node = _routingContext.PublishedContentStore.GetDocumentById(
						_umbracoContext,
						internalRedirectId);

					_documentRequest.Document = node;
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

			if (_documentRequest.Document == null)
				throw new InvalidOperationException("There is no node.");

			var path = _documentRequest.Document.Path;

			if (Access.IsProtected(_documentRequest.DocumentId, path))
			{
				LogHelper.Debug<DocumentRequest>("{0}Page is protected, check for access", () => tracePrefix);

				var user = System.Web.Security.Membership.GetUser();

				if (user == null || !Member.IsLoggedOn())
				{
					LogHelper.Debug<DocumentRequest>("{0}Not logged in, redirect to login page", () => tracePrefix);
					var loginPageId = Access.GetLoginPage(path);
					if (loginPageId != _documentRequest.DocumentId)
						_documentRequest.Document = _routingContext.PublishedContentStore.GetDocumentById(
							_umbracoContext,
							loginPageId);
				}
				else if (!Access.HasAccces(_documentRequest.DocumentId, user.ProviderUserKey))
				{
					LogHelper.Debug<DocumentRequest>("{0}Current member has not access, redirect to error page", () => tracePrefix);
					var errorPageId = Access.GetErrorPage(path);
					if (errorPageId != _documentRequest.DocumentId)
						_documentRequest.Document = _routingContext.PublishedContentStore.GetDocumentById(
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
			//return if the request already has a template assigned, this can be possible if an ILookup assigns one
			if (_documentRequest.HasTemplate) return;

			const string tracePrefix = "LookupTemplate: ";

			if (_documentRequest.Document == null)
				throw new InvalidOperationException("There is no node.");

			//gets item from query string, form, cookie or server vars
			var templateAlias = _umbracoContext.HttpContext.Request["altTemplate"];

			if (templateAlias.IsNullOrWhiteSpace())
			{
				//we don't have an alt template specified, so lookup the template id on the document and then lookup the template
				// associated with it.
				//TODO: When we remove the need for a database for templates, then this id should be irrelavent, not sure how were going to do this nicely.

				var templateId = _documentRequest.Document.TemplateId;
				LogHelper.Debug<DocumentRequest>("{0}Look for template id={1}", () => tracePrefix, () => templateId);
				
				if (templateId > 0)
				{
					//NOTE: This will throw an exception if the template id doesn't exist, but that is ok to inform the front end.
					var template = new Template(templateId);
					_documentRequest.Template = template;
				}
			}
			else
			{
				LogHelper.Debug<DocumentRequest>("{0}Look for template alias=\"{1}\" (altTemplate)", () => tracePrefix, () => templateAlias);
				//TODO: Need to figure out if this is web forms or MVC based on template name somehow!!
				var template = Template.GetByAlias(templateAlias);
				_documentRequest.Template = template;
			}

			if (!_documentRequest.HasTemplate)
			{
				LogHelper.Debug<DocumentRequest>("{0}No template was found.");
				// do not do it if we're already 404 else it creates an infinite loop
				if (Umbraco.Core.Configuration.UmbracoSettings.HandleMissingTemplateAs404 && !_documentRequest.Is404)
				{
					LogHelper.Debug<DocumentRequest>("{0}Assume page not found (404).");
					_documentRequest.Document = null;
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
				var redirectId = _documentRequest.Document.GetPropertyValue<int>("umbracoRedirect", -1);
				
				string redirectUrl = "#";
				if (redirectId > 0)
					redirectUrl = _routingContext.NiceUrlProvider.GetNiceUrl(redirectId);
				if (redirectUrl != "#")
					_documentRequest.RedirectUrl = redirectUrl;
			}
		}
	}
}