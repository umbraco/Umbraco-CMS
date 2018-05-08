﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Web.Security;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using RenderingEngine = Umbraco.Core.RenderingEngine;

namespace Umbraco.Web.Routing
{
    // fixme - make this public
    // fixme - making sense to have an interface?
    internal class PublishedRouter
    {
        private readonly IWebRoutingSection _webRoutingSection;
        private readonly ContentFinderCollection _contentFinders;
        private readonly IContentLastChanceFinder _contentLastChanceFinder;
        private readonly ServiceContext _services;
        private readonly ProfilingLogger _profilingLogger;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedRouter"/> class.
        /// </summary>
        public PublishedRouter(
            IWebRoutingSection webRoutingSection,
            ContentFinderCollection contentFinders,
            IContentLastChanceFinder contentLastChanceFinder,
            ServiceContext services,
            ProfilingLogger proflog,
            Func<string, IEnumerable<string>> getRolesForLogin = null)
        {
            _webRoutingSection = webRoutingSection ?? throw new ArgumentNullException(nameof(webRoutingSection)); // fixme usage?
            _contentFinders = contentFinders ?? throw new ArgumentNullException(nameof(contentFinders));
            _contentLastChanceFinder = contentLastChanceFinder ?? throw new ArgumentNullException(nameof(contentLastChanceFinder));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _profilingLogger = proflog ?? throw new ArgumentNullException(nameof(proflog));
            _logger = proflog.Logger;

            GetRolesForLogin = getRolesForLogin ?? (s => Roles.Provider.GetRolesForUser(s));
        }

        // fixme
        // in 7.7 this is cached in the PublishedContentRequest, which ... makes little sense
        // killing it entirely, if we need cache, just implement it properly !!
        // this is all soooo weird
        public Func<string, IEnumerable<string>> GetRolesForLogin { get; }

        public PublishedRequest CreateRequest(UmbracoContext umbracoContext, Uri uri = null)
        {
            return new PublishedRequest(this, umbracoContext, uri ?? umbracoContext.CleanedUmbracoUrl);
        }

        #region Request

        /// <summary>
        /// Tries to route the request.
        /// </summary>
        internal bool TryRouteRequest(PublishedRequest request)
        {
            // disabled - is it going to change the routing?
            //_pcr.OnPreparing();

            FindDomain(request);
            if (request.IsRedirect) return false;
            if (request.HasPublishedContent) return true;
            FindPublishedContent(request);
            if (request.IsRedirect) return false;

            // don't handle anything - we just want to ensure that we find the content
            //HandlePublishedContent();
            //FindTemplate();
            //FollowExternalRedirect();
            //HandleWildcardDomains();

            // disabled - we just want to ensure that we find the content
            //_pcr.OnPrepared();

            return request.HasPublishedContent;
        }

        /// <summary>
        /// Prepares the request.
        /// </summary>
        /// <returns>
        /// Returns false if the request was not successfully prepared
        /// </returns>
        public bool PrepareRequest(PublishedRequest request)
        {
            // note - at that point the original legacy module did something do handle IIS custom 404 errors
            //   ie pages looking like /anything.aspx?404;/path/to/document - I guess the reason was to support
            //   "directory urls" without having to do wildcard mapping to ASP.NET on old IIS. This is a pain
            //   to maintain and probably not used anymore - removed as of 06/2012. @zpqrtbnk.
            //
            //   to trigger Umbraco's not-found, one should configure IIS and/or ASP.NET custom 404 errors
            //   so that they point to a non-existing page eg /redirect-404.aspx
            //   TODO: SD: We need more information on this for when we release 4.10.0 as I'm not sure what this means.

            // trigger the Preparing event - at that point anything can still be changed
            // the idea is that it is possible to change the uri
            //
            request.OnPreparing();

            //find domain
            FindDomain(request);

            // if request has been flagged to redirect then return
            // whoever called us is in charge of actually redirecting
            if (request.IsRedirect)
            {
                return false;
            }

            // set the culture on the thread - once, so it's set when running document lookups
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = request.Culture;

            //find the published content if it's not assigned. This could be manually assigned with a custom route handler, or
            // with something like EnsurePublishedContentRequestAttribute or UmbracoVirtualNodeRouteHandler. Those in turn call this method
            // to setup the rest of the pipeline but we don't want to run the finders since there's one assigned.
            if (request.PublishedContent == null)
            {
                // find the document & template
                FindPublishedContentAndTemplate(request);
            }

            // handle wildcard domains
            HandleWildcardDomains(request);

            // set the culture on the thread -- again, 'cos it might have changed due to a finder or wildcard domain
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = request.Culture;

            // trigger the Prepared event - at that point it is still possible to change about anything
            // even though the request might be flagged for redirection - we'll redirect _after_ the event
            //
            // also, OnPrepared() will make the PublishedContentRequest readonly, so nothing can change
            //
            request.OnPrepared();

            // we don't take care of anything so if the content has changed, it's up to the user
            // to find out the appropriate template

            //complete the PCR and assign the remaining values
            return ConfigureRequest(request);
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
        public bool ConfigureRequest(PublishedRequest frequest)
        {
            if (frequest.HasPublishedContent == false)
            {
                return false;
            }

            // set the culture on the thread -- again, 'cos it might have changed in the event handler
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = frequest.Culture;

            // if request has been flagged to redirect, or has no content to display,
            // then return - whoever called us is in charge of actually redirecting
            if (frequest.IsRedirect || frequest.HasPublishedContent == false)
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

            // assign the legacy page back to the request
            // handlers like default.aspx will want it and most macros currently need it
            frequest.UmbracoPage = new page(frequest);

            // used by many legacy objects
            frequest.UmbracoContext.HttpContext.Items["pageID"] = frequest.PublishedContent.Id;
            frequest.UmbracoContext.HttpContext.Items["pageElements"] = frequest.UmbracoPage.Elements;

            return true;
        }

        /// <summary>
        /// Updates the request when there is no template to render the content.
        /// </summary>
        /// <remarks>This is called from Mvc when there's a document to render but no template.</remarks>
        public void UpdateRequestOnMissingTemplate(PublishedRequest request)
        {
            // clear content
            var content = request.PublishedContent;
            request.PublishedContent = null;

            HandlePublishedContent(request); // will go 404
            FindTemplate(request);

            // if request has been flagged to redirect then return
            // whoever called us is in charge of redirecting
            if (request.IsRedirect)
                return;

            if (request.HasPublishedContent == false)
            {
                // means the engine could not find a proper document to handle 404
                // restore the saved content so we know it exists
                request.PublishedContent = content;
                return;
            }

            if (request.HasTemplate == false)
            {
                // means we may have a document, but we have no template
                // at that point there isn't much we can do and there is no point returning
                // to Mvc since Mvc can't do much either
                return;
            }

            // see note in PrepareRequest()

            // assign the legacy page back to the docrequest
            // handlers like default.aspx will want it and most macros currently need it
            request.UmbracoPage = new page(request);

            // these two are used by many legacy objects
            request.UmbracoContext.HttpContext.Items["pageID"] = request.PublishedContent.Id;
            request.UmbracoContext.HttpContext.Items["pageElements"] = request.UmbracoPage.Elements;
        }

        #endregion

        #region Domain

        /// <summary>
        /// Finds the site root (if any) matching the http request, and updates the PublishedContentRequest accordingly.
        /// </summary>
        /// <returns>A value indicating whether a domain was found.</returns>
        internal bool FindDomain(PublishedRequest request)
        {
            const string tracePrefix = "FindDomain: ";

            // note - we are not handling schemes nor ports here.

            _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Uri=\"{request.Uri}\"");

            var domainsCache = request.UmbracoContext.PublishedSnapshot.Domains;

            //get the domains but filter to ensure that any referenced content is actually published
            var domains = domainsCache.GetAll(includeWildcards: false)
                .Where(x => request.UmbracoContext.PublishedSnapshot.Content.GetById(x.ContentId) != null);

            var defaultCulture = domainsCache.DefaultCulture;

            // try to find a domain matching the current request
            var domainAndUri = DomainHelper.SelectDomain(domains, request.Uri, defaultCulture: defaultCulture);

            // handle domain - always has a contentId and a culture
            if (domainAndUri != null)
            {
                // matching an existing domain
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Matches domain=\"{domainAndUri.Name}\", rootId={domainAndUri.ContentId}, culture=\"{domainAndUri.Culture}\"");

                request.Domain = domainAndUri;
                request.Culture = domainAndUri.Culture;

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
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Matches no domain");

                request.Culture = defaultCulture == null ? CultureInfo.CurrentUICulture : new CultureInfo(defaultCulture);
            }

            _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Culture=\"{request.Culture.Name}\"");

            return request.Domain != null;
        }

        /// <summary>
        /// Looks for wildcard domains in the path and updates <c>Culture</c> accordingly.
        /// </summary>
        internal void HandleWildcardDomains(PublishedRequest request)
        {
            const string tracePrefix = "HandleWildcardDomains: ";

            if (request.HasPublishedContent == false)
                return;

            var nodePath = request.PublishedContent.Path;
            _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Path=\"{nodePath}\"");
            var rootNodeId = request.HasDomain ? request.Domain.ContentId : (int?)null;
            var domain = DomainHelper.FindWildcardDomainInPath(request.UmbracoContext.PublishedSnapshot.Domains.GetAll(true), nodePath, rootNodeId);

            // always has a contentId and a culture
            if (domain != null)
            {
                request.Culture = domain.Culture;
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Got domain on node {domain.ContentId}, set culture to \"{request.Culture.Name}\".");
            }
            else
            {
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}No match.");
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

            // fixme - bad - we probably should be using the appropriate filesystems!

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
        /// Gets a template.
        /// </summary>
        /// <param name="alias">The template alias</param>
        /// <returns>The template.</returns>
        public ITemplate GetTemplate(string alias)
        {
            return _services.FileService.GetTemplate(alias);
        }

        /// <summary>
        /// Finds the Umbraco document (if any) matching the request, and updates the PublishedContentRequest accordingly.
        /// </summary>
        /// <returns>A value indicating whether a document and template were found.</returns>
        private void FindPublishedContentAndTemplate(PublishedRequest request)
        {
            const string tracePrefix = "FindPublishedContentAndTemplate: ";
            _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Path=\"{request.Uri.AbsolutePath}\"");

            // run the document finders
            FindPublishedContent(request);

            // if request has been flagged to redirect then return
            // whoever called us is in charge of actually redirecting
            // -- do not process anything any further --
            if (request.IsRedirect)
                return;

            // not handling umbracoRedirect here but after LookupDocument2
            // so internal redirect, 404, etc has precedence over redirect

            // handle not-found, redirects, access...
            HandlePublishedContent(request);

            // find a template
            FindTemplate(request);

            // handle umbracoRedirect
            FollowExternalRedirect(request);
        }

        /// <summary>
        /// Tries to find the document matching the request, by running the IPublishedContentFinder instances.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no finder collection.</exception>
        internal void FindPublishedContent(PublishedRequest request)
        {
            const string tracePrefix = "FindPublishedContent: ";

            // look for the document
            // the first successful finder, if any, will set this.PublishedContent, and may also set this.Template
            // some finders may implement caching

            using (_profilingLogger.DebugDuration<PublishedRouter>(
                $"{tracePrefix}Begin finders",
                $"{tracePrefix}End finders, {(request.HasPublishedContent ? "a document was found" : "no document was found")}"))
            {
                //iterate but return on first one that finds it
                var found = _contentFinders.Any(finder =>
                {
                    _logger.Debug<PublishedRouter>("Finder " + finder.GetType().FullName);
                    return finder.TryFindContent(request);
                });
            }

            // indicate that the published content (if any) we have at the moment is the
            // one that was found by the standard finders before anything else took place.
            request.SetIsInitialPublishedContent();
        }

        /// <summary>
        /// Handles the published content (if any).
        /// </summary>
        /// <remarks>
        /// Handles "not found", internal redirects, access validation...
        /// things that must be handled in one place because they can create loops
        /// </remarks>
        private void HandlePublishedContent(PublishedRequest request)
        {
            const string tracePrefix = "HandlePublishedContent: ";

            // because these might loop, we have to have some sort of infinite loop detection
            int i = 0, j = 0;
            const int maxLoop = 8;
            do
            {
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}{(i == 0 ? "Begin" : "Loop")}");

                // handle not found
                if (request.HasPublishedContent == false)
                {
                    request.Is404 = true;
                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}No document, try last chance lookup");

                    // if it fails then give up, there isn't much more that we can do
                    if (_contentLastChanceFinder.TryFindContent(request) == false)
                    {
                        _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Failed to find a document, give up");
                        break;
                    }

                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Found a document");
                }

                // follow internal redirects as long as it's not running out of control ie infinite loop of some sort
                j = 0;
                while (FollowInternalRedirects(request) && j++ < maxLoop)
                { }
                if (j == maxLoop) // we're running out of control
                    break;

                // ensure access
                if (request.HasPublishedContent)
                    EnsurePublishedContentAccess(request);

                // loop while we don't have page, ie the redirect or access
                // got us to nowhere and now we need to run the notFoundLookup again
                // as long as it's not running out of control ie infinite loop of some sort

            } while (request.HasPublishedContent == false && i++ < maxLoop);

            if (i == maxLoop || j == maxLoop)
            {
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Looks like we're running into an infinite loop, abort");
                request.PublishedContent = null;
            }

            _logger.Debug<PublishedRouter>(() => $"{tracePrefix}End");
        }

        /// <summary>
        /// Follows internal redirections through the <c>umbracoInternalRedirectId</c> document property.
        /// </summary>
        /// <returns>A value indicating whether redirection took place and led to a new published document.</returns>
        /// <remarks>
        /// <para>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</para>
        /// <para>As per legacy, if the redirect does not work, we just ignore it.</para>
        /// </remarks>
        private bool FollowInternalRedirects(PublishedRequest request)
        {
            const string tracePrefix = "FollowInternalRedirects: ";

            if (request.PublishedContent == null)
                throw new InvalidOperationException("There is no PublishedContent.");

            // don't try to find a redirect if the property doesn't exist
            if (request.PublishedContent.HasProperty(Constants.Conventions.Content.InternalRedirectId) == false)
                return false;

            var redirect = false;
            var valid = false;
            IPublishedContent internalRedirectNode = null;
            var internalRedirectId = request.PublishedContent.Value(Constants.Conventions.Content.InternalRedirectId, defaultValue: -1);

            if (internalRedirectId > 0)
            {
                // try and get the redirect node from a legacy integer ID
                valid = true;
                internalRedirectNode = request.UmbracoContext.ContentCache.GetById(internalRedirectId);
            }
            else
            {
                var udiInternalRedirectId = request.PublishedContent.Value<GuidUdi>(Constants.Conventions.Content.InternalRedirectId);
                if (udiInternalRedirectId != null)
                {
                    // try and get the redirect node from a UDI Guid
                    valid = true;
                    internalRedirectNode = request.UmbracoContext.ContentCache.GetById(udiInternalRedirectId.Guid);
                }
            }

            if (valid == false)
            {
                // bad redirect - log and display the current page (legacy behavior)
                _logger.Debug<PublishedRouter>($"{tracePrefix}Failed to redirect to id={request.PublishedContent.GetProperty(Constants.Conventions.Content.InternalRedirectId).GetSourceValue()}: value is not an int nor a GuidUdi.");
            }

            if (internalRedirectNode == null)
            {
                _logger.Debug<PublishedRouter>($"{tracePrefix}Failed to redirect to id={request.PublishedContent.GetProperty(Constants.Conventions.Content.InternalRedirectId).GetSourceValue()}: no such published document.");
            }
            else if (internalRedirectId == request.PublishedContent.Id)
            {
                // redirect to self
                _logger.Debug<PublishedRouter>($"{tracePrefix}Redirecting to self, ignore");
            }
            else
            {
                request.SetInternalRedirectPublishedContent(internalRedirectNode); // don't use .PublishedContent here
                redirect = true;
                _logger.Debug<PublishedRouter>($"{tracePrefix}Redirecting to id={internalRedirectId}");
            }

            return redirect;
        }

        /// <summary>
        /// Ensures that access to current node is permitted.
        /// </summary>
        /// <remarks>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</remarks>
        private void EnsurePublishedContentAccess(PublishedRequest request)
        {
            const string tracePrefix = "EnsurePublishedContentAccess: ";

            if (request.PublishedContent == null)
                throw new InvalidOperationException("There is no PublishedContent.");

            var path = request.PublishedContent.Path;

            var publicAccessAttempt = _services.PublicAccessService.IsProtected(path);

            if (publicAccessAttempt)
            {
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Page is protected, check for access");

                var membershipHelper = new MembershipHelper(request.UmbracoContext);

                if (membershipHelper.IsLoggedIn() == false)
                {
                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Not logged in, redirect to login page");

                    var loginPageId = publicAccessAttempt.Result.LoginNodeId;

                    if (loginPageId != request.PublishedContent.Id)
                        request.PublishedContent = request.UmbracoContext.PublishedSnapshot.Content.GetById(loginPageId);
                }
                else if (_services.PublicAccessService.HasAccess(request.PublishedContent.Id, _services.ContentService, GetRolesForLogin(membershipHelper.CurrentUserName)) == false)
                {
                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Current member has not access, redirect to error page");
                    var errorPageId = publicAccessAttempt.Result.NoAccessNodeId;
                    if (errorPageId != request.PublishedContent.Id)
                        request.PublishedContent = request.UmbracoContext.PublishedSnapshot.Content.GetById(errorPageId);
                }
                else
                {
                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Current member has access");
                }
            }
            else
            {
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Page is not protected");
            }
        }

        /// <summary>
        /// Finds a template for the current node, if any.
        /// </summary>
        private void FindTemplate(PublishedRequest request)
        {
            // NOTE: at the moment there is only 1 way to find a template, and then ppl must
            // use the Prepared event to change the template if they wish. Should we also
            // implement an ITemplateFinder logic?

            const string tracePrefix = "FindTemplate: ";

            if (request.PublishedContent == null)
            {
                request.TemplateModel = null;
                return;
            }

            // read the alternate template alias, from querystring, form, cookie or server vars,
            // only if the published content is the initial once, else the alternate template
            // does not apply
            // + optionnally, apply the alternate template on internal redirects
            var useAltTemplate = _webRoutingSection.DisableAlternativeTemplates == false
                && (request.IsInitialPublishedContent
                || (_webRoutingSection.InternalRedirectPreservesTemplate && request.IsInternalRedirectPublishedContent));
            var altTemplate = useAltTemplate
                ? request.UmbracoContext.HttpContext.Request[Constants.Conventions.Url.AltTemplate]
                : null;

            if (string.IsNullOrWhiteSpace(altTemplate))
            {
                // we don't have an alternate template specified. use the current one if there's one already,
                // which can happen if a content lookup also set the template (LookupByNiceUrlAndTemplate...),
                // else lookup the template id on the document then lookup the template with that id.

                if (request.HasTemplate)
                {
                    _logger.Debug<PublishedRequest>("{0}Has a template already, and no alternate template.");
                    return;
                }

                // TODO: When we remove the need for a database for templates, then this id should be irrelavent,
                // not sure how were going to do this nicely.

                var templateId = request.PublishedContent.TemplateId;

                if (templateId > 0)
                {
                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Look for template id={templateId}");
                    var template = _services.FileService.GetTemplate(templateId);
                    if (template == null)
                        throw new InvalidOperationException("The template with Id " + templateId + " does not exist, the page cannot render");
                    request.TemplateModel = template;
                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Got template id={template.Id} alias=\"{template.Alias}\"");
                }
                else
                {
                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}No specified template.");
                }
            }
            else
            {
                // we have an alternate template specified. lookup the template with that alias
                // this means the we override any template that a content lookup might have set
                // so /path/to/page/template1?altTemplate=template2 will use template2

                // ignore if the alias does not match - just trace

                if (request.HasTemplate)
                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Has a template already, but also an alternate template.");
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Look for alternate template alias=\"{altTemplate}\"");

                var template = _services.FileService.GetTemplate(altTemplate);
                if (template != null)
                {
                    request.TemplateModel = template;
                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Got template id={template.Id} alias=\"{template.Alias}\"");
                }
                else
                {
                    _logger.Debug<PublishedRouter>(() => $"{tracePrefix}The template with alias=\"{altTemplate}\" does not exist, ignoring.");
                }
            }

            if (request.HasTemplate == false)
            {
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}No template was found.");

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
                _logger.Debug<PublishedRouter>(() => $"{tracePrefix}Running with template id={request.TemplateModel.Id} alias=\"{request.TemplateModel.Alias}\"");
            }
        }

        /// <summary>
        /// Follows external redirection through <c>umbracoRedirect</c> document property.
        /// </summary>
        /// <remarks>As per legacy, if the redirect does not work, we just ignore it.</remarks>
        private void FollowExternalRedirect(PublishedRequest request)
        {
            if (request.HasPublishedContent == false) return;

            // don't try to find a redirect if the property doesn't exist
            if (request.PublishedContent.HasProperty(Constants.Conventions.Content.Redirect) == false)
                return;

            var redirectId = request.PublishedContent.Value(Constants.Conventions.Content.Redirect, defaultValue: -1);
            var redirectUrl = "#";
            if (redirectId > 0)
            {
                redirectUrl = request.UmbracoContext.UrlProvider.GetUrl(redirectId);
            }
            else
            {
                // might be a UDI instead of an int Id
                var redirectUdi = request.PublishedContent.Value<GuidUdi>(Constants.Conventions.Content.Redirect);
                if (redirectUdi != null)
                    redirectUrl = request.UmbracoContext.UrlProvider.GetUrl(redirectUdi.Guid);
            }
            if (redirectUrl != "#")
                request.SetRedirect(redirectUrl);
        }

        #endregion
    }
}
