using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;

using umbraco;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.member;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using RenderingEngine = Umbraco.Core.RenderingEngine;

namespace Umbraco.Web.Routing
{
	internal class PublishedContentRequestEngine
	{	    
	    private readonly PublishedContentRequest _pcr;
	    private readonly RoutingContext _routingContext;
	    private readonly IWebRoutingSection _webRoutingSection;

	    /// <summary>
	    /// Initializes a new instance of the <see cref="PublishedContentRequestEngine"/> class with a content request.
	    /// </summary>
	    /// <param name="webRoutingSection"></param>
	    /// <param name="pcr">The content request.</param>
	    public PublishedContentRequestEngine(            
            IWebRoutingSection webRoutingSection,
            PublishedContentRequest pcr)
		{
	        if (pcr == null) throw new ArgumentException("pcr is null.");
	        if (webRoutingSection == null) throw new ArgumentNullException("webRoutingSection");
	       
	        _pcr = pcr;
	        _webRoutingSection = webRoutingSection;

	        _routingContext = pcr.RoutingContext;
			if (_routingContext == null) throw new ArgumentException("pcr.RoutingContext is null.");
			
			var umbracoContext = _routingContext.UmbracoContext;
			if (umbracoContext == null) throw new ArgumentException("pcr.RoutingContext.UmbracoContext is null.");
			if (umbracoContext.RoutingContext != _routingContext) throw new ArgumentException("RoutingContext confusion.");
			// no! not set yet.
			//if (umbracoContext.PublishedContentRequest != _pcr) throw new ArgumentException("PublishedContentRequest confusion.");
		}

        protected ProfilingLogger ProfilingLogger
	    {
            get { return _routingContext.UmbracoContext.Application.ProfilingLogger; }
	    }

	    protected ServiceContext Services
	    {
            get { return _routingContext.UmbracoContext.Application.Services; }

	    }

		#region Public

		/// <summary>
		/// Prepares the request.
		/// </summary>
		/// <returns>
		/// Returns false if the request was not successfully prepared
		/// </returns>
		public bool PrepareRequest()
		{
			// note - at that point the original legacy module did something do handle IIS custom 404 errors
			//   ie pages looking like /anything.aspx?404;/path/to/document - I guess the reason was to support
			//   "directory urls" without having to do wildcard mapping to ASP.NET on old IIS. This is a pain
			//   to maintain and probably not used anymore - removed as of 06/2012. @zpqrtbnk.
			//
			//   to trigger Umbraco's not-found, one should configure IIS and/or ASP.NET custom 404 errors
			//   so that they point to a non-existing page eg /redirect-404.aspx
			//   TODO: SD: We need more information on this for when we release 4.10.0 as I'm not sure what this means.

			//find domain
			FindDomain();

			// if request has been flagged to redirect then return
			// whoever called us is in charge of actually redirecting
		    if (_pcr.IsRedirect)
		    {
		        return false;
		    }

		    // set the culture on the thread - once, so it's set when running document lookups
			Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = _pcr.Culture;

            //find the published content if it's not assigned. This could be manually assigned with a custom route handler, or
            // with something like EnsurePublishedContentRequestAttribute or UmbracoVirtualNodeRouteHandler. Those in turn call this method
            // to setup the rest of the pipeline but we don't want to run the finders since there's one assigned.
		    if (_pcr.PublishedContent == null)
		    {
                // find the document & template
                FindPublishedContentAndTemplate();

                // set the culture on the thread -- again, 'cos it might have changed due to a wildcard domain
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = _pcr.Culture;
		    }

			// trigger the Prepared event - at that point it is still possible to change about anything
            // even though the request might be flagged for redirection - we'll redirect _after_ the event
            //
            // also, OnPrepared() will make the PublishedContentRequest readonly, so nothing can change
            //
			_pcr.OnPrepared();

            // we don't take care of anything so if the content has changed, it's up to the user
            // to find out the appropriate template

            //complete the PCR and assign the remaining values
		    return ConfigureRequest();
		}

        /// <summary>
        /// Called by PrepareRequest once everything has been discovered, resolved and assigned to the PCR. This method
        /// finalizes the PCR with the values assigned.
        /// </summary>
        /// <returns>
        /// Returns false if the request was not successfully configured
        /// </returns>
        /// <remarks>
        /// This method logic has been put into it's own method in case developers have created a custom PCR or are assigning their own values
        /// but need to finalize it themselves.
        /// </remarks>
        public bool ConfigureRequest()
        {
            if (_pcr.HasPublishedContent == false)
            {
                return false;
            }

            // set the culture on the thread -- again, 'cos it might have changed in the event handler
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = _pcr.Culture;

            // if request has been flagged to redirect, or has no content to display,
            // then return - whoever called us is in charge of actually redirecting
            if (_pcr.IsRedirect || _pcr.HasPublishedContent == false)
            {
                return false;
            }

            // we may be 404 _and_ have a content

            // can't go beyond that point without a PublishedContent to render
            // it's ok not to have a template, in order to give MVC a chance to hijack routes

            // note - the page() ctor below will cause the "page" to get the value of all its
            // "elements" ie of all the IPublishedContent property. If we use the object value,
            // that will trigger macro execution - which can't happen because macro execution
            // requires that _pcr.UmbracoPage is already initialized = catch-22. The "legacy"
            // pipeline did _not_ evaluate the macros, ie it is using the data value, and we
            // have to keep doing it because of that catch-22.

            // assign the legacy page back to the docrequest
            // handlers like default.aspx will want it and most macros currently need it
            _pcr.UmbracoPage = new page(_pcr);

            // used by many legacy objects
            _routingContext.UmbracoContext.HttpContext.Items["pageID"] = _pcr.PublishedContent.Id;
            _routingContext.UmbracoContext.HttpContext.Items["pageElements"] = _pcr.UmbracoPage.Elements;

            return true;
        }

		/// <summary>
		/// Updates the request when there is no template to render the content.
		/// </summary>
		/// <remarks>This is called from Mvc when there's a document to render but no template.</remarks>
		public void UpdateRequestOnMissingTemplate()
		{
			// clear content
			var content = _pcr.PublishedContent;
			_pcr.PublishedContent = null;

			HandlePublishedContent(); // will go 404
			FindTemplate();

			// if request has been flagged to redirect then return
			// whoever called us is in charge of redirecting
			if (_pcr.IsRedirect)
				return;

			if (_pcr.HasPublishedContent == false)
			{
				// means the engine could not find a proper document to handle 404
				// restore the saved content so we know it exists
				_pcr.PublishedContent = content;
				return;
			}

			if (_pcr.HasTemplate == false)
			{
				// means we may have a document, but we have no template
				// at that point there isn't much we can do and there is no point returning
				// to Mvc since Mvc can't do much either
				return;
			}

            // see note in PrepareRequest()

			// assign the legacy page back to the docrequest
			// handlers like default.aspx will want it and most macros currently need it
			_pcr.UmbracoPage = new page(_pcr);

			// these two are used by many legacy objects
			_routingContext.UmbracoContext.HttpContext.Items["pageID"] = _pcr.PublishedContent.Id;
			_routingContext.UmbracoContext.HttpContext.Items["pageElements"] = _pcr.UmbracoPage.Elements;
		}

		#endregion

		#region Domain

		/// <summary>
		/// Finds the site root (if any) matching the http request, and updates the PublishedContentRequest accordingly.
		/// </summary>        
		/// <returns>A value indicating whether a domain was found.</returns>
		internal bool FindDomain()
		{
			const string tracePrefix = "FindDomain: ";

			// note - we are not handling schemes nor ports here.

			ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Uri=\"{1}\"", () => tracePrefix, () => _pcr.Uri);

			// try to find a domain matching the current request
            var domainAndUri = DomainHelper.DomainForUri(Services.DomainService.GetAll(false), _pcr.Uri);

			// handle domain
			if (domainAndUri != null)
			{
				// matching an existing domain
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Matches domain=\"{1}\", rootId={2}, culture=\"{3}\"",
												 () => tracePrefix,
												 () => domainAndUri.UmbracoDomain.DomainName,
												 () => domainAndUri.UmbracoDomain.RootContent.Id,
                                                 () => domainAndUri.UmbracoDomain.Language.IsoCode);

                _pcr.UmbracoDomain = domainAndUri.UmbracoDomain;
				_pcr.DomainUri = domainAndUri.Uri;
                _pcr.Culture = new CultureInfo(domainAndUri.UmbracoDomain.Language.IsoCode);

				// canonical? not implemented at the moment
				// if (...)
				// {
				//  _pcr.RedirectUrl = "...";
				//  return true;
				// }
			}
			else
			{
				// not matching any existing domain
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Matches no domain", () => tracePrefix);

                var defaultLanguage = Services.LocalizationService.GetAllLanguages().FirstOrDefault();
				_pcr.Culture = defaultLanguage == null ? CultureInfo.CurrentUICulture : new CultureInfo(defaultLanguage.IsoCode);
			}

			ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Culture=\"{1}\"", () => tracePrefix, () => _pcr.Culture.Name);

			return _pcr.UmbracoDomain != null;
		}

		/// <summary>
		/// Looks for wildcard domains in the path and updates <c>Culture</c> accordingly.
		/// </summary>
		internal void HandleWildcardDomains()
		{
			const string tracePrefix = "HandleWildcardDomains: ";

			if (_pcr.HasPublishedContent == false)
				return;

			var nodePath = _pcr.PublishedContent.Path;
			ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Path=\"{1}\"", () => tracePrefix, () => nodePath);
            var rootNodeId = _pcr.HasDomain ? _pcr.UmbracoDomain.RootContent.Id : (int?)null;
            var domain = DomainHelper.FindWildcardDomainInPath(Services.DomainService.GetAll(true), nodePath, rootNodeId);

			if (domain != null)
			{
				_pcr.Culture = new CultureInfo(domain.Language.IsoCode);
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Got domain on node {1}, set culture to \"{2}\".", () => tracePrefix,
                    () => domain.RootContent.Id, () => _pcr.Culture.Name);
			}
			else
			{
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}No match.", () => tracePrefix);
			}
		}

		#endregion

		#region Rendering engine

        /// <summary>
        /// Finds the rendering engine to use to render a template specified by its alias.
        /// </summary>
        /// <param name="alias">The alias of the template.</param>
        /// <returns>The rendering engine, or Unknown if the template was not found.</returns>
        internal RenderingEngine FindTemplateRenderingEngine(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
                return RenderingEngine.Unknown;

            alias = alias.Replace('\\', '/'); // forward slashes only

            // NOTE: we could start with what's the current default?

            if (FindTemplateRenderingEngineInDirectory(new DirectoryInfo(IOHelper.MapPath(SystemDirectories.MvcViews)),
                    alias, new[] { ".cshtml", ".vbhtml" }))
                return RenderingEngine.Mvc;

            if (FindTemplateRenderingEngineInDirectory(new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Masterpages)),
                    alias, new[] { ".master" }))
                return RenderingEngine.WebForms;

            return RenderingEngine.Unknown;
        }

        internal bool FindTemplateRenderingEngineInDirectory(DirectoryInfo directory, string alias, string[] extensions)
        {
            if (directory == null || directory.Exists == false)
                return false;

            var pos = alias.IndexOf('/');
            if (pos > 0)
            {
                // recurse
                var subdir = directory.GetDirectories(alias.Substring(0, pos)).FirstOrDefault();
                alias = alias.Substring(pos + 1);
                return subdir != null && FindTemplateRenderingEngineInDirectory(subdir, alias, extensions);
            }

            // look here
            return directory.GetFiles().Any(f => extensions.Any(e => f.Name.InvariantEquals(alias + e)));
        }

		#endregion

		#region Document and template

	    /// <summary>
	    /// Finds the Umbraco document (if any) matching the request, and updates the PublishedContentRequest accordingly.
	    /// </summary>
	    /// <returns>A value indicating whether a document and template were found.</returns>
	    private void FindPublishedContentAndTemplate()
		{
			const string tracePrefix = "FindPublishedContentAndTemplate: ";
			ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Path=\"{1}\"", () => tracePrefix, () => _pcr.Uri.AbsolutePath);

			// run the document finders
			FindPublishedContent();

            // if request has been flagged to redirect then return
            // whoever called us is in charge of actually redirecting
            // -- do not process anything any further --
            if (_pcr.IsRedirect)
	            return;

			// not handling umbracoRedirect here but after LookupDocument2
			// so internal redirect, 404, etc has precedence over redirect

			// handle not-found, redirects, access...
			HandlePublishedContent();

			// find a template
			FindTemplate();

			// handle umbracoRedirect
			FollowExternalRedirect();

			// handle wildcard domains
			HandleWildcardDomains();
		}

	    /// <summary>
	    /// Tries to find the document matching the request, by running the IPublishedContentFinder instances.
	    /// </summary>
        /// <exception cref="InvalidOperationException">There is no finder collection.</exception>
	    internal void FindPublishedContent()
		{
			const string tracePrefix = "FindPublishedContent: ";

			// look for the document
			// the first successful finder, if any, will set this.PublishedContent, and may also set this.Template
			// some finders may implement caching

            using (ProfilingLogger.DebugDuration<PublishedContentRequestEngine>(
				string.Format("{0}Begin finders", tracePrefix),
				string.Format("{0}End finders, {1}", tracePrefix, (_pcr.HasPublishedContent ? "a document was found" : "no document was found"))))
			{
			    if (_routingContext.PublishedContentFinders == null)
                    throw new InvalidOperationException("There is no finder collection.");

                //iterate but return on first one that finds it
			    var found = _routingContext.PublishedContentFinders.Any(finder => finder.TryFindContent(_pcr));
			}

		    // indicate that the published content (if any) we have at the moment is the
			// one that was found by the standard finders before anything else took place.
		    _pcr.SetIsInitialPublishedContent();
		}

		/// <summary>
		/// Handles the published content (if any).
		/// </summary>
		/// <remarks>
		/// Handles "not found", internal redirects, access validation...
		/// things that must be handled in one place because they can create loops
		/// </remarks>
		private void HandlePublishedContent()
		{
			const string tracePrefix = "HandlePublishedContent: ";

			// because these might loop, we have to have some sort of infinite loop detection 
			int i = 0, j = 0;
			const int maxLoop = 8;
			do
			{
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}{1}", () => tracePrefix, () => (i == 0 ? "Begin" : "Loop"));

				// handle not found
				if (_pcr.HasPublishedContent == false)
				{
					_pcr.Is404 = true;
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}No document, try last chance lookup", () => tracePrefix);

					// if it fails then give up, there isn't much more that we can do
					var lastChance = _routingContext.PublishedContentLastChanceFinder;
					if (lastChance == null || lastChance.TryFindContent(_pcr) == false)
					{
						ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Failed to find a document, give up", () => tracePrefix);
						break;
					}

					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Found a document", () => tracePrefix);
				}

				// follow internal redirects as long as it's not running out of control ie infinite loop of some sort
				j = 0;
				while (FollowInternalRedirects() && j++ < maxLoop)
				{ }
			    if (j == maxLoop) // we're running out of control
					break;

				// ensure access
				if (_pcr.HasPublishedContent)
					EnsurePublishedContentAccess();

				// loop while we don't have page, ie the redirect or access
				// got us to nowhere and now we need to run the notFoundLookup again
				// as long as it's not running out of control ie infinite loop of some sort

			} while (_pcr.HasPublishedContent == false && i++ < maxLoop);

			if (i == maxLoop || j == maxLoop)
			{
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Looks like we're running into an infinite loop, abort", () => tracePrefix);
				_pcr.PublishedContent = null;
			}

			ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}End", () => tracePrefix);
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

			if (_pcr.PublishedContent == null)
				throw new InvalidOperationException("There is no PublishedContent.");

			bool redirect = false;
			var internalRedirect = _pcr.PublishedContent.GetPropertyValue<string>(Constants.Conventions.Content.InternalRedirectId);

			if (string.IsNullOrWhiteSpace(internalRedirect) == false)
			{
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Found umbracoInternalRedirectId={1}", () => tracePrefix, () => internalRedirect);

				int internalRedirectId;
				if (int.TryParse(internalRedirect, out internalRedirectId) == false)
					internalRedirectId = -1;

				if (internalRedirectId <= 0)
				{
					// bad redirect - log and display the current page (legacy behavior)
					//_pcr.Document = null; // no! that would be to force a 404
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Failed to redirect to id={1}: invalid value", () => tracePrefix, () => internalRedirect);
				}
				else if (internalRedirectId == _pcr.PublishedContent.Id)
				{
					// redirect to self
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Redirecting to self, ignore", () => tracePrefix);
				}
				else
				{
					// redirect to another page
                    var node = _routingContext.UmbracoContext.ContentCache.GetById(internalRedirectId);

                    _pcr.SetInternalRedirectPublishedContent(node); // don't use .PublishedContent here
                    if (node != null)
					{
						redirect = true;
						ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Redirecting to id={1}", () => tracePrefix, () => internalRedirectId);
					}
					else
					{
						ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Failed to redirect to id={1}: no such published document", () => tracePrefix, () => internalRedirectId);
					}
				}
			}

			return redirect;
		}
        
		/// <summary>
		/// Ensures that access to current node is permitted.
		/// </summary>
		/// <remarks>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</remarks>
		private void EnsurePublishedContentAccess()
		{
			const string tracePrefix = "EnsurePublishedContentAccess: ";

			if (_pcr.PublishedContent == null)
				throw new InvalidOperationException("There is no PublishedContent.");

			var path = _pcr.PublishedContent.Path;

		    var publicAccessAttempt = Services.PublicAccessService.IsProtected(path);

            if (publicAccessAttempt)
			{
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Page is protected, check for access", () => tracePrefix);

			    var membershipHelper = new MembershipHelper(_routingContext.UmbracoContext);

				if (membershipHelper.IsLoggedIn() == false)
				{
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Not logged in, redirect to login page", () => tracePrefix);

                    var loginPageId = publicAccessAttempt.Result.LoginNodeId;

					if (loginPageId != _pcr.PublishedContent.Id)
                        _pcr.PublishedContent = _routingContext.UmbracoContext.ContentCache.GetById(loginPageId);
				}
                else if (Services.PublicAccessService.HasAccess(_pcr.PublishedContent.Id, Services.ContentService, _pcr.GetRolesForLogin(membershipHelper.CurrentUserName)) == false)
				{
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Current member has not access, redirect to error page", () => tracePrefix);
				    var errorPageId = publicAccessAttempt.Result.NoAccessNodeId;
					if (errorPageId != _pcr.PublishedContent.Id)
                        _pcr.PublishedContent = _routingContext.UmbracoContext.ContentCache.GetById(errorPageId);
				}
				else
				{
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Current member has access", () => tracePrefix);
				}
			}
			else
			{
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Page is not protected", () => tracePrefix);
			}
		}

		/// <summary>
		/// Finds a template for the current node, if any.
		/// </summary>
		private void FindTemplate()
		{
			// NOTE: at the moment there is only 1 way to find a template, and then ppl must
			// use the Prepared event to change the template if they wish. Should we also
			// implement an ITemplateFinder logic?

			const string tracePrefix = "FindTemplate: ";

            if (_pcr.PublishedContent == null)
            {
                _pcr.TemplateModel = null;
                return;
            }

			// read the alternate template alias, from querystring, form, cookie or server vars,
			// only if the published content is the initial once, else the alternate template
			// does not apply
            // + optionnally, apply the alternate template on internal redirects
            var useAltTemplate = _pcr.IsInitialPublishedContent
                || (_webRoutingSection.InternalRedirectPreservesTemplate && _pcr.IsInternalRedirectPublishedContent);
            string altTemplate = useAltTemplate
                ? _routingContext.UmbracoContext.HttpContext.Request[Constants.Conventions.Url.AltTemplate]
				: null;

			if (string.IsNullOrWhiteSpace(altTemplate))
			{
				// we don't have an alternate template specified. use the current one if there's one already,
				// which can happen if a content lookup also set the template (LookupByNiceUrlAndTemplate...),
				// else lookup the template id on the document then lookup the template with that id.

				if (_pcr.HasTemplate)
				{
					ProfilingLogger.Logger.Debug<PublishedContentRequest>("{0}Has a template already, and no alternate template.", () => tracePrefix);
					return;
				}

				// TODO: When we remove the need for a database for templates, then this id should be irrelavent,
				// not sure how were going to do this nicely.

				var templateId = _pcr.PublishedContent.TemplateId;

				if (templateId > 0)
				{
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Look for template id={1}", () => tracePrefix, () => templateId);
					var template = ApplicationContext.Current.Services.FileService.GetTemplate(templateId);
					if (template == null)
						throw new InvalidOperationException("The template with Id " + templateId + " does not exist, the page cannot render");
					_pcr.TemplateModel = template;
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Got template id={1} alias=\"{2}\"", () => tracePrefix, () => template.Id, () => template.Alias);
				}
				else
				{
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}No specified template.", () => tracePrefix);
				}
			}
			else
			{
				// we have an alternate template specified. lookup the template with that alias
				// this means the we override any template that a content lookup might have set
				// so /path/to/page/template1?altTemplate=template2 will use template2

				// ignore if the alias does not match - just trace

				if (_pcr.HasTemplate)
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Has a template already, but also an alternate template.", () => tracePrefix);
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Look for alternate template alias=\"{1}\"", () => tracePrefix, () => altTemplate);

				var template = ApplicationContext.Current.Services.FileService.GetTemplate(altTemplate);
				if (template != null)
				{
					_pcr.TemplateModel = template;
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Got template id={1} alias=\"{2}\"", () => tracePrefix, () => template.Id, () => template.Alias);
				}
				else
				{
					ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}The template with alias=\"{1}\" does not exist, ignoring.", () => tracePrefix, () => altTemplate);
				}
			}

			if (_pcr.HasTemplate == false)
			{
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}No template was found.", () => tracePrefix);

				// initial idea was: if we're not already 404 and UmbracoSettings.HandleMissingTemplateAs404 is true
				// then reset _pcr.Document to null to force a 404.
				//
				// but: because we want to let MVC hijack routes even though no template is defined, we decide that
				// a missing template is OK but the request will then be forwarded to MVC, which will need to take
				// care of everything.
				//
				// so, don't set _pcr.Document to null here
			}
			else
			{
				ProfilingLogger.Logger.Debug<PublishedContentRequestEngine>("{0}Running with template id={1} alias=\"{2}\"", () => tracePrefix, () => _pcr.TemplateModel.Id, () => _pcr.TemplateModel.Alias);
			}
		}

		/// <summary>
		/// Follows external redirection through <c>umbracoRedirect</c> document property.
		/// </summary>
		/// <remarks>As per legacy, if the redirect does not work, we just ignore it.</remarks>
		private void FollowExternalRedirect()
		{
		    if (_pcr.HasPublishedContent == false) return;

		    var redirectId = _pcr.PublishedContent.GetPropertyValue(Constants.Conventions.Content.Redirect, -1);
		    var redirectUrl = "#";
		    if (redirectId > 0)
				redirectUrl = _routingContext.UrlProvider.GetUrl(redirectId);
		    if (redirectUrl != "#")
		        _pcr.SetRedirect(redirectUrl);
		}
	
		#endregion
	}
}
