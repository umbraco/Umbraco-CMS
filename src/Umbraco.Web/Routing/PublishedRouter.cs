using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Macros;
using Umbraco.Web.Security;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides the default <see cref="IPublishedRouter"/> implementation.
    /// </summary>
    public class PublishedRouter : IPublishedRouter
    {
        private readonly IWebRoutingSection _webRoutingSection;
        private readonly ContentFinderCollection _contentFinders;
        private readonly IContentLastChanceFinder _contentLastChanceFinder;
        private readonly ServiceContext _services;
        private readonly IProfilingLogger _profilingLogger;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedRouter"/> class.
        /// </summary>
        public PublishedRouter(
            IWebRoutingSection webRoutingSection,
            ContentFinderCollection contentFinders,
            IContentLastChanceFinder contentLastChanceFinder,
            IVariationContextAccessor variationContextAccessor,
            ServiceContext services,
            IProfilingLogger proflog)
        {
            _webRoutingSection = webRoutingSection ?? throw new ArgumentNullException(nameof(webRoutingSection));
            _contentFinders = contentFinders ?? throw new ArgumentNullException(nameof(contentFinders));
            _contentLastChanceFinder = contentLastChanceFinder ?? throw new ArgumentNullException(nameof(contentLastChanceFinder));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _profilingLogger = proflog ?? throw new ArgumentNullException(nameof(proflog));
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            _logger = proflog;
        }

        /// <inheritdoc />
        public PublishedRequest CreateRequest(UmbracoContext umbracoContext, Uri uri = null)
        {
            return new PublishedRequest(this, umbracoContext, uri ?? umbracoContext.CleanedUmbracoUrl);
        }

        #region Request

        /// <inheritdoc />
        public bool TryRouteRequest(PublishedRequest request)
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

        private void SetVariationContext(string culture)
        {
            var variationContext = _variationContextAccessor.VariationContext;
            if (variationContext != null && variationContext.Culture == culture) return;
            _variationContextAccessor.VariationContext = new VariationContext(culture);
        }

        /// <inheritdoc />
        public bool PrepareRequest(PublishedRequest request)
        {
            // note - at that point the original legacy module did something do handle IIS custom 404 errors
            //   ie pages looking like /anything.aspx?404;/path/to/document - I guess the reason was to support
            //   "directory URLs" without having to do wildcard mapping to ASP.NET on old IIS. This is a pain
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
            SetVariationContext(request.Culture.Name);

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
            SetVariationContext(request.Culture.Name);

            // trigger the Prepared event - at that point it is still possible to change about anything
            // even though the request might be flagged for redirection - we'll redirect _after_ the event
            //
            // also, OnPrepared() will make the PublishedRequest readonly, so nothing can change
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
            SetVariationContext(frequest.Culture.Name);

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
            frequest.LegacyContentHashTable = new PublishedContentHashtableConverter(frequest);

            return true;
        }

        /// <inheritdoc />
        public void UpdateRequestToNotFound(PublishedRequest request)
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
            request.LegacyContentHashTable = new PublishedContentHashtableConverter(request);
            
        }

        #endregion

        #region Domain

        /// <summary>
        /// Finds the site root (if any) matching the http request, and updates the PublishedRequest accordingly.
        /// </summary>
        /// <returns>A value indicating whether a domain was found.</returns>
        internal bool FindDomain(PublishedRequest request)
        {
            const string tracePrefix = "FindDomain: ";

            // note - we are not handling schemes nor ports here.

            _logger.Debug<PublishedRouter, string, Uri>("{TracePrefix}Uri={RequestUri}", tracePrefix, request.Uri);

            var domainsCache = request.UmbracoContext.PublishedSnapshot.Domains;
            var domains = domainsCache.GetAll(includeWildcards: false).ToList();

            // determines whether a domain corresponds to a published document, since some
            // domains may exist but on a document that has been unpublished - as a whole - or
            // that is not published for the domain's culture - in which case the domain does
            // not apply
            bool IsPublishedContentDomain(Domain domain)
            {
                // just get it from content cache - optimize there, not here
                var domainDocument = request.UmbracoContext.PublishedSnapshot.Content.GetById(domain.ContentId);

                // not published - at all
                if (domainDocument == null)
                    return false;

                // invariant - always published
                if (!domainDocument.ContentType.VariesByCulture())
                    return true;

                // variant, ensure that the culture corresponding to the domain's language is published
                return domainDocument.Cultures.ContainsKey(domain.Culture.Name);
            }

            domains = domains.Where(IsPublishedContentDomain).ToList();

            var defaultCulture = domainsCache.DefaultCulture;

            // try to find a domain matching the current request
            var domainAndUri = DomainUtilities.SelectDomain(domains, request.Uri, defaultCulture: defaultCulture);

            // handle domain - always has a contentId and a culture
            if (domainAndUri != null)
            {
                // matching an existing domain
                _logger.Debug<PublishedRouter>("{TracePrefix}Matches domain={Domain}, rootId={RootContentId}, culture={Culture}", tracePrefix, domainAndUri.Name, domainAndUri.ContentId, domainAndUri.Culture);

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
                _logger.Debug<PublishedRouter, string>("{TracePrefix}Matches no domain", tracePrefix);

                request.Culture = defaultCulture == null ? CultureInfo.CurrentUICulture : new CultureInfo(defaultCulture);
            }

            _logger.Debug<PublishedRouter, string, string>("{TracePrefix}Culture={CultureName}", tracePrefix, request.Culture.Name);

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
            _logger.Debug<PublishedRouter, string, string>("{TracePrefix}Path={NodePath}", tracePrefix, nodePath);
            var rootNodeId = request.HasDomain ? request.Domain.ContentId : (int?)null;
            var domain = DomainUtilities.FindWildcardDomainInPath(request.UmbracoContext.PublishedSnapshot.Domains.GetAll(true), nodePath, rootNodeId);

            // always has a contentId and a culture
            if (domain != null)
            {
                request.Culture = domain.Culture;
                _logger.Debug<PublishedRouter, string, int, string>("{TracePrefix}Got domain on node {DomainContentId}, set culture to {CultureName}", tracePrefix, domain.ContentId, request.Culture.Name);
            }
            else
            {
                _logger.Debug<PublishedRouter, string>("{TracePrefix}No match.", tracePrefix);
            }
        }

        #endregion

        #region Rendering engine

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

        /// <inheritdoc />
        public ITemplate GetTemplate(string alias)
        {
            return _services.FileService.GetTemplate(alias);
        }

        /// <summary>
        /// Finds the Umbraco document (if any) matching the request, and updates the PublishedRequest accordingly.
        /// </summary>
        /// <returns>A value indicating whether a document and template were found.</returns>
        private void FindPublishedContentAndTemplate(PublishedRequest request)
        {
            _logger.Debug<PublishedRouter, string>("FindPublishedContentAndTemplate: Path={UriAbsolutePath}", request.Uri.AbsolutePath);

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
                $"{tracePrefix}Executing finders...",
                $"{tracePrefix}Completed executing finders"))
            {
                //iterate but return on first one that finds it
                var found = _contentFinders.Any(finder =>
                {
                    _logger.Debug<PublishedRouter, string>("Finder {ContentFinderType}", finder.GetType().FullName);
                    return finder.TryFindContent(request);
                });

                _logger.Debug<PublishedRouter>(
                    "Found? {Found} Content: {PublishedContentId}, Template: {TemplateAlias}, Domain: {Domain}, Culture: {Culture}, Is404: {Is404}, StatusCode: {StatusCode}",
                    found,
                    request.HasPublishedContent ? request.PublishedContent.Id : "NULL",
                    request.HasTemplate ? request.TemplateAlias : "NULL",
                    request.HasDomain ? request.Domain.ToString() : "NULL",
                    request.Culture?.Name ?? "NULL",
                    request.Is404,
                    request.ResponseStatusCode);
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
            // because these might loop, we have to have some sort of infinite loop detection
            int i = 0, j = 0;
            const int maxLoop = 8;
            do
            {
                _logger.Debug<PublishedRouter,int>("HandlePublishedContent: Loop {LoopCounter}", i);

                // handle not found
                if (request.HasPublishedContent == false)
                {
                    request.Is404 = true;
                    _logger.Debug<PublishedRouter>("HandlePublishedContent: No document, try last chance lookup");

                    // if it fails then give up, there isn't much more that we can do
                    if (_contentLastChanceFinder.TryFindContent(request) == false)
                    {
                        _logger.Debug<PublishedRouter>("HandlePublishedContent: Failed to find a document, give up");
                        break;
                    }

                    _logger.Debug<PublishedRouter>("HandlePublishedContent: Found a document");
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
                _logger.Debug<PublishedRouter>("HandlePublishedContent: Looks like we are running into an infinite loop, abort");
                request.PublishedContent = null;
            }

            _logger.Debug<PublishedRouter>("HandlePublishedContent: End");
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
            if (request.PublishedContent == null)
                throw new InvalidOperationException("There is no PublishedContent.");

            // don't try to find a redirect if the property doesn't exist
            if (request.PublishedContent.HasProperty(Constants.Conventions.Content.InternalRedirectId) == false)
            {
                return false;
            }

            var internalRedirectId = request.PublishedContent.Value(Constants.Conventions.Content.InternalRedirectId)?.ToString();

            if (internalRedirectId == null)
            {
                // no value stored, just return, no need to log
                return false;
            }

            if (int.TryParse(internalRedirectId, out var internalRedirectIdAsInt) && internalRedirectIdAsInt == request.PublishedContent.Id)
            {
                // redirect to self
                _logger.Debug<PublishedRouter>("FollowInternalRedirects: Redirecting to self, ignore");
                return false;
            }

            IPublishedContent internalRedirectNode = null;
            if (internalRedirectIdAsInt > 0)
            {
                // try and get the redirect node from a legacy integer ID
                internalRedirectNode = request.UmbracoContext.Content.GetById(internalRedirectIdAsInt);
            }
            else if (GuidUdi.TryParse(internalRedirectId, out var internalRedirectIdAsUdi))
            {
                // try and get the redirect node from a UDI Guid
                internalRedirectNode = request.UmbracoContext.Content.GetById(internalRedirectIdAsUdi.Guid);
            }

            if (internalRedirectNode == null)
            {
                _logger.Debug<PublishedRouter,object>("FollowInternalRedirects: Failed to redirect to id={InternalRedirectId}: no such published document.",
                    request.PublishedContent.GetProperty(Constants.Conventions.Content.InternalRedirectId).GetSourceValue());
                return false;
            }

            request.SetInternalRedirectPublishedContent(internalRedirectNode); // don't use .PublishedContent here
            _logger.Debug<PublishedRouter,int>("FollowInternalRedirects: Redirecting to id={InternalRedirectId}", internalRedirectIdAsInt);
            return true;
        }

        /// <summary>
        /// Ensures that access to current node is permitted.
        /// </summary>
        /// <remarks>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</remarks>
        private void EnsurePublishedContentAccess(PublishedRequest request)
        {
            if (request.PublishedContent == null)
                throw new InvalidOperationException("There is no PublishedContent.");

            var path = request.PublishedContent.Path;

            var publicAccessAttempt = _services.PublicAccessService.IsProtected(path);

            if (publicAccessAttempt)
            {
                _logger.Debug<PublishedRouter>("EnsurePublishedContentAccess: Page is protected, check for access");

                var membershipHelper = Current.Factory.GetInstance<MembershipHelper>();

                if (membershipHelper.IsLoggedIn() == false)
                {
                    _logger.Debug<PublishedRouter>("EnsurePublishedContentAccess: Not logged in, redirect to login page");

                    var loginPageId = publicAccessAttempt.Result.LoginNodeId;

                    if (loginPageId != request.PublishedContent.Id)
                        request.PublishedContent = request.UmbracoContext.PublishedSnapshot.Content.GetById(loginPageId);
                }
                else if (_services.PublicAccessService.HasAccess(request.PublishedContent.Id, _services.ContentService, membershipHelper.CurrentUserName, membershipHelper.GetCurrentUserRoles()) == false)
                {
                    _logger.Debug<PublishedRouter>("EnsurePublishedContentAccess: Current member has not access, redirect to error page");
                    var errorPageId = publicAccessAttempt.Result.NoAccessNodeId;
                    if (errorPageId != request.PublishedContent.Id)
                        request.PublishedContent = request.UmbracoContext.PublishedSnapshot.Content.GetById(errorPageId);
                }
                else
                {
                    if (membershipHelper.IsUmbracoMembershipProviderActive())
                    {
                        // grab the current member
                        var member = membershipHelper.GetCurrentMember();
                        // if the member has the "approved" and/or "locked out" properties, make sure they're correctly set before allowing access
                        var memberIsActive = true;
                        if (member != null)
                        {
                            if (member.HasProperty(Constants.Conventions.Member.IsApproved) == false)
                                memberIsActive = member.Value<bool>(Constants.Conventions.Member.IsApproved);

                            if (member.HasProperty(Constants.Conventions.Member.IsLockedOut) == false)
                                memberIsActive = member.Value<bool>(Constants.Conventions.Member.IsLockedOut) == false;
                        }

                        if (memberIsActive == false)
                        {
                            _logger.Debug<PublishedRouter>(
                                "Current member is either unapproved or locked out, redirect to error page");
                            var errorPageId = publicAccessAttempt.Result.NoAccessNodeId;
                            if (errorPageId != request.PublishedContent.Id)
                                request.PublishedContent =
                                    request.UmbracoContext.PublishedSnapshot.Content.GetById(errorPageId);
                        }
                        else
                        {
                            _logger.Debug<PublishedRouter>("Current member has access");
                        }
                    }
                    else
                    {
                        _logger.Debug<PublishedRouter>("Current custom MembershipProvider member has access");
                    }
                }
            }
            else
            {
                _logger.Debug<PublishedRouter>("EnsurePublishedContentAccess: Page is not protected");
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

            if (request.PublishedContent == null)
            {
                request.TemplateModel = null;
                return;
            }

            // read the alternate template alias, from querystring, form, cookie or server vars,
            // only if the published content is the initial once, else the alternate template
            // does not apply
            // + optionally, apply the alternate template on internal redirects
            var useAltTemplate = request.IsInitialPublishedContent
                || (_webRoutingSection.InternalRedirectPreservesTemplate && request.IsInternalRedirectPublishedContent);
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
                    _logger.Debug<PublishedRequest>("FindTemplate: Has a template already, and no alternate template.");
                    return;
                }

                // TODO: When we remove the need for a database for templates, then this id should be irrelevant,
                // not sure how were going to do this nicely.

                // TODO: We need to limit altTemplate to only allow templates that are assigned to the current document type!
                // if the template isn't assigned to the document type we should log a warning and return 404

                var templateId = request.PublishedContent.TemplateId;
                request.TemplateModel = GetTemplateModel(templateId);
            }
            else
            {
                // we have an alternate template specified. lookup the template with that alias
                // this means the we override any template that a content lookup might have set
                // so /path/to/page/template1?altTemplate=template2 will use template2

                // ignore if the alias does not match - just trace

                if (request.HasTemplate)
                    _logger.Debug<PublishedRouter>("FindTemplate: Has a template already, but also an alternative template.");
                _logger.Debug<PublishedRouter, string>("FindTemplate: Look for alternative template alias={AltTemplate}", altTemplate);

                // IsAllowedTemplate deals both with DisableAlternativeTemplates and ValidateAlternativeTemplates settings
                if (request.PublishedContent.IsAllowedTemplate(altTemplate))
                {
                    // allowed, use
                    var template = _services.FileService.GetTemplate(altTemplate);

                    if (template != null)
                    {
                        request.TemplateModel = template;
                        _logger.Debug<PublishedRouter, int, string>("FindTemplate: Got alternative template id={TemplateId} alias={TemplateAlias}", template.Id, template.Alias);
                    }
                    else
                    {
                        _logger.Debug<PublishedRouter, string>("FindTemplate: The alternative template with alias={AltTemplate} does not exist, ignoring.", altTemplate);
                    }
                }
                else
                {
                    _logger.Warn<PublishedRouter, string,int>("FindTemplate: Alternative template {TemplateAlias} is not allowed on node {NodeId}, ignoring.", altTemplate, request.PublishedContent.Id);

                    // no allowed, back to default
                    var templateId = request.PublishedContent.TemplateId;
                    request.TemplateModel = GetTemplateModel(templateId);
                }
            }

            if (request.HasTemplate == false)
            {
                _logger.Debug<PublishedRouter>("FindTemplate: No template was found.");

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
                _logger.Debug<PublishedRouter, int, string>("FindTemplate: Running with template id={TemplateId} alias={TemplateAlias}", request.TemplateModel.Id, request.TemplateModel.Alias);
            }
        }

        private ITemplate GetTemplateModel(int? templateId)
        {
            if (templateId.HasValue == false || templateId.Value == default)
            {
                _logger.Debug<PublishedRouter>("GetTemplateModel: No template.");
                return null;
            }

            _logger.Debug<PublishedRouter, int?>("GetTemplateModel: Get template id={TemplateId}", templateId);

            if (templateId == null)
                throw new InvalidOperationException("The template is not set, the page cannot render.");

            var template = _services.FileService.GetTemplate(templateId.Value);
            if (template == null)
                throw new InvalidOperationException("The template with Id " + templateId + " does not exist, the page cannot render.");
            _logger.Debug<PublishedRouter, int, string>("GetTemplateModel: Got template id={TemplateId} alias={TemplateAlias}", template.Id, template.Alias);
            return template;
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
