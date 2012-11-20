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
	/// Looks up the document using ILookup's and sets any additional properties required on the PublishedContentRequest object
	/// </summary>
	internal class PublishedContentRequestBuilder
	{
		private readonly PublishedContentRequest _publishedContentRequest;
		private readonly UmbracoContext _umbracoContext;
		private readonly RoutingContext _routingContext;

		public PublishedContentRequestBuilder(PublishedContentRequest publishedContentRequest)
		{
			if (publishedContentRequest == null) throw new ArgumentNullException("publishedContentRequest");
			_publishedContentRequest = publishedContentRequest;
			_umbracoContext = publishedContentRequest.RoutingContext.UmbracoContext;
			_routingContext = publishedContentRequest.RoutingContext;
		}

		/// <summary>
		/// Determines the rendering engine to use and sets the flag on the PublishedContentRequest
		/// </summary>
		internal void DetermineRenderingEngine()
		{
			//First, if there is no template, we will default to use MVC because MVC supports Hijacking routes which
			//sometimes don't require a template since the developer may want full control over the rendering. 
			//Webforms doesn't support this so MVC it is. MVC will also handle what to do if no template or hijacked route
			//is there (i.e. blank page)
			if (!_publishedContentRequest.HasTemplate)
			{
				_publishedContentRequest.RenderingEngine = RenderingEngine.Mvc;
				return;
			}

			//NOTE: Not sure how the alias is actually saved with a space as this shouldn't ever be the case? 
			// but apparently this happens. I think what should actually be done always is the template alias 
			// should be saved using the ToUmbracoAlias method and then we can use this here too, that way it
			// it 100% consistent. I'll leave this here for now until further invenstigation.
			var templateAlias = _publishedContentRequest.Template.Alias.Replace(" ", string.Empty);
			//var templateAlias = _publishedContentRequest.Template.Alias.ToUmbracoAlias(StringAliasCaseType.PascalCase);

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
							_publishedContentRequest.RenderingEngine = renderingEngine;
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

			LogHelper.Debug<PublishedContentRequest>("{0}Uri=\"{1}\"", () => tracePrefix, () => _publishedContentRequest.Uri);

			// try to find a domain matching the current request
			var domainAndUri = DomainHelper.DomainMatch(Domain.GetDomains(), _umbracoContext.CleanedUmbracoUrl, false);

			// handle domain
			if (domainAndUri != null)
			{
				// matching an existing domain
				LogHelper.Debug<PublishedContentRequest>("{0}Matches domain=\"{1}\", rootId={2}, culture=\"{3}\"",
												 () => tracePrefix,
												 () => domainAndUri.Domain.Name,
												 () => domainAndUri.Domain.RootNodeId,
												 () => domainAndUri.Domain.Language.CultureAlias);

				_publishedContentRequest.Domain = domainAndUri.Domain;
				_publishedContentRequest.DomainUri = domainAndUri.Uri;
				_publishedContentRequest.Culture = new CultureInfo(domainAndUri.Domain.Language.CultureAlias);

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
				LogHelper.Debug<PublishedContentRequest>("{0}Matches no domain", () => tracePrefix);

				var defaultLanguage = Language.GetAllAsList().FirstOrDefault();
				_publishedContentRequest.Culture = defaultLanguage == null ? CultureInfo.CurrentUICulture : new CultureInfo(defaultLanguage.CultureAlias);
			}

			LogHelper.Debug<PublishedContentRequest>("{0}Culture=\"{1}\"", () => tracePrefix, () => _publishedContentRequest.Culture.Name);

			return _publishedContentRequest.Domain != null;
		}

		/// <summary>
		/// Determines the Umbraco document (if any) matching the http request.
		/// </summary>
		/// <returns>A value indicating whether a document and template nave been found.</returns>
		internal bool LookupDocument()
		{
			const string tracePrefix = "LookupDocument: ";
			LogHelper.Debug<PublishedContentRequest>("{0}Path=\"{1}\"", () => tracePrefix, () => _publishedContentRequest.Uri.AbsolutePath);

			// run the document lookups
			LookupDocument1();

			// not handling umbracoRedirect here but after LookupDocument2
			// so internal redirect, 404, etc has precedence over redirect

			// handle not-found, redirects, access, template
			LookupDocument2();

			// handle umbracoRedirect (moved from umbraco.page)
			FollowRedirect();

			// handle wildcard domains
			HandleWildcardDomains();

			bool resolved = _publishedContentRequest.HasNode && _publishedContentRequest.HasTemplate;
			return resolved;
		}

		/// <summary>
		/// Performs the document resolution first pass.
		/// </summary>
		/// <remarks>The first past consists in running the document lookups.</remarks>
		internal void LookupDocument1()
		{
			const string tracePrefix = "LookupDocument: ";

			// look for the document
			// the first successful resolver, if any, will set this.Node, and may also set this.Template
			// some lookups may implement caching

			using (DisposableTimer.DebugDuration<PluginManager>(
				() => string.Format("{0}Begin resolvers", tracePrefix),
				() => string.Format("{0}End resolvers, {1}", tracePrefix, (_publishedContentRequest.HasNode ? "a document was found" : "no document was found"))))
			{
				_routingContext.DocumentLookups.Any(lookup => lookup.TrySetDocument(_publishedContentRequest));
			}
		}

		/// <summary>
		/// Performs the document resolution second pass.
		/// </summary>
		/// <remarks>
		/// The second pass consists in handling "not found", internal redirects, access validation, and template.
		/// TODO: Rename this method accordingly .... but to what?
		/// </remarks>
		internal void LookupDocument2()
		{
			const string tracePrefix = "LookupDocument2: ";

			// handle "not found", follow internal redirects, validate access, template
			// because these might loop, we have to have some sort of infinite loop detection 
			int i = 0, j = 0;
			const int maxLoop = 12;
			do
			{
				LogHelper.Debug<PublishedContentRequest>("{0}{1}", () => tracePrefix, () => (i == 0 ? "Begin" : "Loop"));

				// handle not found
				if (!_publishedContentRequest.HasNode)
				{
					_publishedContentRequest.Is404 = true;
					LogHelper.Debug<PublishedContentRequest>("{0}No document, try last chance lookup", () => tracePrefix);

					// if it fails then give up, there isn't much more that we can do
					var lastChance = _routingContext.DocumentLastChanceLookup;
					if (lastChance == null || !lastChance.TrySetDocument(_publishedContentRequest))
					{
						LogHelper.Debug<PublishedContentRequest>("{0}Failed to find a document, give up", () => tracePrefix);
						break;
					}

					LogHelper.Debug<PublishedContentRequest>("{0}Found a document", () => tracePrefix);
				}

				// follow internal redirects as long as it's not running out of control ie infinite loop of some sort
				j = 0;
				while (FollowInternalRedirects() && j++ < maxLoop) ;
				if (j == maxLoop) // we're running out of control
					break;

				// ensure access
				if (_publishedContentRequest.HasNode)
					EnsureNodeAccess();

				// loop while we don't have page, ie the redirect or access
				// got us to nowhere and now we need to run the notFoundLookup again
				// as long as it's not running out of control ie infinite loop of some sort

			} while (!_publishedContentRequest.HasNode && i++ < maxLoop);

			if (i == maxLoop || j == maxLoop)
			{
				LogHelper.Debug<PublishedContentRequest>("{0}Looks like we're running into an infinite loop, abort", () => tracePrefix);
				_publishedContentRequest.PublishedContent = null;
			}

			// resolve template - will do nothing if a template is already set
			// moved out of the loop because LookupTemplate does set .PublishedContent to null anymore
			// (see node in LookupTemplate)
			if (_publishedContentRequest.HasNode)
				LookupTemplate();
			
			LogHelper.Debug<PublishedContentRequest>("{0}End", () => tracePrefix);
		}

		/// <summary>
		/// Follows internal redirections through the <c>umbracoInternalRedirectId</c> document property.
		/// </summary>
		/// <returns>A value indicating whether redirection took place and led to a new published document.</returns>
		/// <remarks>
		/// <para>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</para>
		/// <para>As per legacy, if the redirect does not work, we just ignore it.</para>
		/// </remarks>
		private bool FollowInternalRedirects()
		{
			const string tracePrefix = "FollowInternalRedirects: ";

			if (_publishedContentRequest.PublishedContent == null)
				throw new InvalidOperationException("There is no node.");

			bool redirect = false;
			var internalRedirect = _publishedContentRequest.PublishedContent.GetPropertyValue<string>("umbracoInternalRedirectId");
			
			if (!string.IsNullOrWhiteSpace(internalRedirect))
			{
				LogHelper.Debug<PublishedContentRequest>("{0}Found umbracoInternalRedirectId={1}", () => tracePrefix, () => internalRedirect);

				int internalRedirectId;
				if (!int.TryParse(internalRedirect, out internalRedirectId))
					internalRedirectId = -1;

				if (internalRedirectId <= 0)
				{
					// bad redirect - log and display the current page (legacy behavior)
					//_publishedContentRequest.Document = null; // no! that would be to force a 404
					LogHelper.Debug<PublishedContentRequest>("{0}Failed to redirect to id={1}: invalid value", () => tracePrefix, () => internalRedirect);
				}
				else if (internalRedirectId == _publishedContentRequest.DocumentId)
				{
					// redirect to self
					LogHelper.Debug<PublishedContentRequest>("{0}Redirecting to self, ignore", () => tracePrefix);
				}
				else
				{
					// redirect to another page
					var node = _routingContext.PublishedContentStore.GetDocumentById(
						_umbracoContext,
						internalRedirectId);

					_publishedContentRequest.PublishedContent = node;
					if (node != null)
					{
						redirect = true;
						LogHelper.Debug<PublishedContentRequest>("{0}Redirecting to id={1}", () => tracePrefix, () => internalRedirectId);
					}
					else
					{
						LogHelper.Debug<PublishedContentRequest>("{0}Failed to redirect to id={1}: no such published document", () => tracePrefix, () => internalRedirectId);
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

			if (_publishedContentRequest.PublishedContent == null)
				throw new InvalidOperationException("There is no node.");

			var path = _publishedContentRequest.PublishedContent.Path;

			if (Access.IsProtected(_publishedContentRequest.DocumentId, path))
			{
				LogHelper.Debug<PublishedContentRequest>("{0}Page is protected, check for access", () => tracePrefix);

                System.Web.Security.MembershipUser user = null;
                try
                {
                    user = System.Web.Security.Membership.GetUser();
                }
                catch (ArgumentException)
                {
                    LogHelper.Debug<PublishedContentRequest>("{0}Membership.GetUser returned ArgumentException", () => tracePrefix);
                }

				if (user == null || !Member.IsLoggedOn())
				{
					LogHelper.Debug<PublishedContentRequest>("{0}Not logged in, redirect to login page", () => tracePrefix);
					var loginPageId = Access.GetLoginPage(path);
					if (loginPageId != _publishedContentRequest.DocumentId)
						_publishedContentRequest.PublishedContent = _routingContext.PublishedContentStore.GetDocumentById(
							_umbracoContext,
							loginPageId);
				}
				else if (!Access.HasAccces(_publishedContentRequest.DocumentId, user.ProviderUserKey))
				{
					LogHelper.Debug<PublishedContentRequest>("{0}Current member has not access, redirect to error page", () => tracePrefix);
					var errorPageId = Access.GetErrorPage(path);
					if (errorPageId != _publishedContentRequest.DocumentId)
						_publishedContentRequest.PublishedContent = _routingContext.PublishedContentStore.GetDocumentById(
							_umbracoContext,
							errorPageId);
				}
				else
				{
					LogHelper.Debug<PublishedContentRequest>("{0}Current member has access", () => tracePrefix);
				}
			}
			else
			{
				LogHelper.Debug<PublishedContentRequest>("{0}Page is not protected", () => tracePrefix);
			}
		}

		/// <summary>
		/// Resolves a template for the current node.
		/// </summary>
		private void LookupTemplate()
		{
			// HERE we should let people register their own way of finding a template, same as with documents!!!!
			// do we?

			//return if the request already has a template assigned, this can be possible if an ILookup assigns one
			if (_publishedContentRequest.HasTemplate) return;

			const string tracePrefix = "LookupTemplate: ";

			if (_publishedContentRequest.PublishedContent == null)
				throw new InvalidOperationException("There is no node.");

			//gets item from query string, form, cookie or server vars
			var templateAlias = _umbracoContext.HttpContext.Request["altTemplate"];

			if (templateAlias.IsNullOrWhiteSpace())
			{
				//we don't have an alt template specified, so lookup the template id on the document and then lookup the template
				// associated with it.
				//TODO: When we remove the need for a database for templates, then this id should be irrelavent, not sure how were going to do this nicely.

				var templateId = _publishedContentRequest.PublishedContent.TemplateId;
				LogHelper.Debug<PublishedContentRequest>("{0}Look for template id={1}", () => tracePrefix, () => templateId);
				
				if (templateId > 0)
				{					
					//NOTE: don't use the Template ctor as the result is not cached... instead use this static method
					var template = Template.GetTemplate(templateId);
					if (template == null)
						throw new InvalidOperationException("The template with Id " + templateId + " does not exist, the page cannot render");
					_publishedContentRequest.Template = template;
				}
			}
			else
			{
				LogHelper.Debug<PublishedContentRequest>("{0}Look for template alias=\"{1}\" (altTemplate)", () => tracePrefix, () => templateAlias);

				var template = Template.GetByAlias(templateAlias, true);
				_publishedContentRequest.Template = template;
			}

			if (!_publishedContentRequest.HasTemplate)
			{
				LogHelper.Debug<PublishedContentRequest>("{0}No template was found.");

				// initial idea was: if we're not already 404 and UmbracoSettings.HandleMissingTemplateAs404 is true
				// then reset _publishedContentRequest.Document to null to force a 404.
				//
				// but: because we want to let MVC hijack routes even though no template is defined, we decide that
				// a missing template is OK but the request will then be forwarded to MVC, which will need to take
				// care of everything.
				//
				// so, don't set _publishedContentRequest.Document to null here
			}
		}

		/// <summary>
		/// Follows external redirection through <c>umbracoRedirect</c> document property.
		/// </summary>
		/// <remarks>As per legacy, if the redirect does not work, we just ignore it.</remarks>
		private void FollowRedirect()
		{
			if (_publishedContentRequest.HasNode)
			{
				var redirectId = _publishedContentRequest.PublishedContent.GetPropertyValue<int>("umbracoRedirect", -1);
				
				string redirectUrl = "#";
				if (redirectId > 0)
					redirectUrl = _routingContext.NiceUrlProvider.GetNiceUrl(redirectId);
				if (redirectUrl != "#")
					_publishedContentRequest.RedirectUrl = redirectUrl;
			}
		}

		/// <summary>
		/// Looks for wildcard domains in the path and updates <c>Culture</c> accordingly.
		/// </summary>
		private void HandleWildcardDomains()
		{
			const string tracePrefix = "HandleWildcardDomains: ";

			if (!_publishedContentRequest.HasNode)
				return;

			var nodePath = _publishedContentRequest.PublishedContent.Path;
			LogHelper.Debug<PublishedContentRequest>("{0}Path=\"{1}\"", () => tracePrefix, () => nodePath);
			var rootNodeId = _publishedContentRequest.HasDomain ? _publishedContentRequest.Domain.RootNodeId : (int?)null;
			var domain = DomainHelper.LookForWildcardDomain(Domain.GetDomains(), nodePath, rootNodeId);

			if (domain != null)
			{
				_publishedContentRequest.Culture = new CultureInfo(domain.Language.CultureAlias);
				LogHelper.Debug<PublishedContentRequest>("{0}Got domain on node {1}, set culture to \"{2}\".", () => tracePrefix,
					() => domain.RootNodeId, () => _publishedContentRequest.Culture.Name);
			}
			else
			{
				LogHelper.Debug<PublishedContentRequest>("{0}No match.", () => tracePrefix);
			}
		}
	}
}