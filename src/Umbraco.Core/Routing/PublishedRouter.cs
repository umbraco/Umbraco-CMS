using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Umbraco.Cms.Core.Routing
{

    /// <summary>
    /// Provides the default <see cref="IPublishedRouter"/> implementation.
    /// </summary>
    public partial class PublishedRouter : IPublishedRouter
    {
        private WebRoutingSettings _webRoutingSettings;
        private readonly ContentFinderCollection _contentFinders;
        private readonly IContentLastChanceFinder _contentLastChanceFinder;
        private readonly IProfilingLogger _profilingLogger;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ILogger<PublishedRouter> _logger;
        private readonly IPublishedUrlProvider _publishedUrlProvider;
        private readonly IRequestAccessor _requestAccessor;
        private readonly IPublishedValueFallback _publishedValueFallback;
        private readonly IFileService _fileService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IEventAggregator _eventAggregator;

        private static readonly Action<ILogger, int?, Exception> s_logRetrievingTemplate
            = LoggerMessage.Define<int?>(LogLevel.Debug, new EventId(44), "GetTemplateModel: Get template id={TemplateId}");

        private static readonly Action<ILogger, string, Exception> s_logFindPublishedContentAndTemplate
            = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(23), "FindPublishedContentAndTemplate: Path={UriAbsolutePath}");

        private static readonly Action<ILogger, string, Uri, Exception> s_logFindDomain
          = LoggerMessage.Define<string, Uri>(LogLevel.Debug, new EventId(24), "{TracePrefix}Uri={RequestUri}");

        private static readonly Action<ILogger, string, string, int, string, Exception> s_logDomainFound
          = LoggerMessage.Define<string, string, int, string>(LogLevel.Debug, new EventId(25), "{TracePrefix}Matches domain={Domain}, rootId={RootContentId}, culture={Culture}");

        private static readonly Action<ILogger, string, Exception> s_logDomainNotFound
         = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(26), "{TracePrefix}Matches no domain");

        private static readonly Action<ILogger, string, string, Exception> s_logDomainCulture
         = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(27), "{TracePrefix}Culture={CultureName}");

        private static readonly Action<ILogger, string, string, Exception> s_logHandleWildcardDomains
           = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(28), "{TracePrefix}Path={NodePath}");

        private static readonly Action<ILogger, string, int, string, Exception> s_logFoundWildcardDomain
          = LoggerMessage.Define<string, int, string>(LogLevel.Debug, new EventId(29), "{TracePrefix}Got domain on node {DomainContentId}, set culture to {CultureName}");

        private static readonly Action<ILogger, string, Exception> s_logNotFoundWildcardDomain
           = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(30), "{TracePrefix}No match.");

        private static readonly Action<ILogger, string, Exception> s_logExecutingContentFinder
            = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(31), "Finder {ContentFinderType}");

        private static readonly Action<ILogger, bool, string, string, string, string, int?, Exception> s_logExecutedContentFinder
            = LoggerMessage.Define<bool, string, string, string, string, int?>(LogLevel.Debug, new EventId(32),
                "Found? {Found}, Content: {PublishedContentId}, Template: {TemplateAlias}, Domain: {Domain}, Culture: {Culture}, StatusCode: {StatusCode}");

        private static readonly Action<ILogger, object, Exception> s_logFailedToInternalRedirectInvalidId
            = LoggerMessage.Define<object>(LogLevel.Debug, new EventId(33),
                "FollowInternalRedirects: Failed to redirect to id={InternalRedirectId}: value is not an int nor a GuidUdi.");

        private static readonly Action<ILogger, object, Exception> s_logFailedToInternalRedirectNoDocumentById
            = LoggerMessage.Define<object>(LogLevel.Debug, new EventId(34),
                "FollowInternalRedirects: Failed to redirect to id={InternalRedirectId}: no such published document.");

        private static readonly Action<ILogger, int, string, Exception> s_logTemplateFound
            = LoggerMessage.Define<int, string>(LogLevel.Debug, new EventId(36), "FindTemplate: Running with template id={TemplateId} alias={TemplateAlias}");

        private static readonly Action<ILogger, int?, Exception> s_logTemplateNotFound
            = LoggerMessage.Define<int?>(LogLevel.Debug, new EventId(37), "FindTemplate: Could not find template with id {TemplateId}");

        private static readonly Action<ILogger, string, Exception> s_logFindingAlternativeTemplateByAlias
            = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(39), "FindTemplate: Look for alternative template alias={AltTemplate}");

        private static readonly Action<ILogger, int, string, Exception> s_logFoundAlternativeTemplateByAlias
            = LoggerMessage.Define<int, string>(LogLevel.Debug, new EventId(40), "FindTemplate: Got alternative template id={TemplateId} alias={TemplateAlias}");

        private static readonly Action<ILogger, string, Exception> s_logNotFoundAlternativeTemplateByAlias
           = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(41), "FindTemplate: The alternative template with alias={AltTemplate} does not exist, ignoring.");

        private static readonly Action<ILogger, string, int, Exception> s_logNotAllowedAlternativeTemplateByAlias
          = LoggerMessage.Define<string, int>(LogLevel.Warning, new EventId(42), "FindTemplate: Alternative template {TemplateAlias} is not allowed on node {NodeId}, ignoring.");

        private static readonly Action<ILogger, int, string, Exception> s_logFallbackToDefaultTempate
        = LoggerMessage.Define<int, string>(LogLevel.Debug, new EventId(43), "FindTemplate: Running with template id={TemplateId} alias={TemplateAlias}");

        private static readonly Action<ILogger, int, string, Exception> s_logRetrievedTemplate
            = LoggerMessage.Define<int, string>(LogLevel.Debug, new EventId(45), "GetTemplateModel: Got template id={TemplateId} alias={TemplateAlias}");

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedRouter"/> class.
        /// </summary>
        public PublishedRouter(
            IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
            ContentFinderCollection contentFinders,
            IContentLastChanceFinder contentLastChanceFinder,
            IVariationContextAccessor variationContextAccessor,
            IProfilingLogger proflog,
            ILogger<PublishedRouter> logger,
            IPublishedUrlProvider publishedUrlProvider,
            IRequestAccessor requestAccessor,
            IPublishedValueFallback publishedValueFallback,
            IFileService fileService,
            IContentTypeService contentTypeService,
            IUmbracoContextAccessor umbracoContextAccessor,
            IEventAggregator eventAggregator)
        {
            _webRoutingSettings = webRoutingSettings.CurrentValue ?? throw new ArgumentNullException(nameof(webRoutingSettings));
            _contentFinders = contentFinders ?? throw new ArgumentNullException(nameof(contentFinders));
            _contentLastChanceFinder = contentLastChanceFinder ?? throw new ArgumentNullException(nameof(contentLastChanceFinder));
            _profilingLogger = proflog ?? throw new ArgumentNullException(nameof(proflog));
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            _logger = logger;
            _publishedUrlProvider = publishedUrlProvider;
            _requestAccessor = requestAccessor;
            _publishedValueFallback = publishedValueFallback;
            _fileService = fileService;
            _contentTypeService = contentTypeService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _eventAggregator = eventAggregator;
            webRoutingSettings.OnChange(x => _webRoutingSettings = x);
        }

        /// <inheritdoc />
        public async Task<IPublishedRequestBuilder> CreateRequestAsync(Uri uri)
        {
            // trigger the Creating event - at that point the URL can be changed
            // this is based on this old task here: https://issues.umbraco.org/issue/U4-7914 which was fulfiled by
            // this PR https://github.com/umbraco/Umbraco-CMS/pull/1137
            // It's to do with proxies, quote:

            /*
                "Thinking about another solution.
                We already have an event, PublishedContentRequest.Prepared, which triggers once the request has been prepared and domain, content, template have been figured out -- but before it renders -- so ppl can change things before rendering.
                Wondering whether we could have a event, PublishedContentRequest.Preparing, which would trigger before the request is prepared, and would let ppl change the value of the request's URI (which by default derives from the HttpContext request).
                That way, if an in-between equipement changes the URI, you could replace it with the original, public-facing URI before we process the request, meaning you could register your HTTPS domain and it would work. And you would have to supply code for each equipment. Less magic in Core."
             */

            // but now we'll just have one event for creating so if people wish to change the URL here they can but nothing else
            var creatingRequest = new CreatingRequestNotification(uri);
            await _eventAggregator.PublishAsync(creatingRequest);

            var publishedRequestBuilder = new PublishedRequestBuilder(creatingRequest.Url, _fileService);
            return publishedRequestBuilder;
        }

        private IPublishedRequest TryRouteRequest(IPublishedRequestBuilder request)
        {
            FindDomain(request);

            if (request.IsRedirect())
            {
                return request.Build();
            }

            if (request.HasPublishedContent())
            {
                return request.Build();
            }

            FindPublishedContent(request);

            return request.Build();
        }

        private void SetVariationContext(string? culture)
        {
            VariationContext? variationContext = _variationContextAccessor.VariationContext;
            if (variationContext != null && variationContext.Culture == culture)
            {
                return;
            }

            _variationContextAccessor.VariationContext = new VariationContext(culture);
        }

        /// <inheritdoc />
        public async Task<IPublishedRequest> RouteRequestAsync(IPublishedRequestBuilder builder, RouteRequestOptions options)
        {
            // outbound routing performs different/simpler logic
            if (options.RouteDirection == RouteDirection.Outbound)
            {
                return TryRouteRequest(builder);
            }

            // find domain
            if (builder.Domain == null)
            {
                FindDomain(builder);
            }

            await RouteRequestInternalAsync(builder);

            // complete the PCR and assign the remaining values
            return BuildRequest(builder);
        }

        private async Task RouteRequestInternalAsync(IPublishedRequestBuilder builder)
        {
            // if request builder was already flagged to redirect then return
            // whoever called us is in charge of actually redirecting
            if (builder.IsRedirect())
            {
                return;
            }

            // set the culture
            SetVariationContext(builder.Culture);

            var foundContentByFinders = false;

            // Find the published content if it's not assigned.
            // This could be manually assigned with a custom route handler, etc...
            // which in turn could call this method
            // to setup the rest of the pipeline but we don't want to run the finders since there's one assigned.
            if (!builder.HasPublishedContent())
            {
                LogFindPublishedContentAndTemplate(builder.Uri.AbsolutePath);

                // run the document finders
                foundContentByFinders = FindPublishedContent(builder);
            }

            // if we are not a redirect
            if (!builder.IsRedirect())
            {
                // handle not-found, redirects, access...
                HandlePublishedContent(builder);

                // find a template
                FindTemplate(builder, foundContentByFinders);

                // handle umbracoRedirect
                FollowExternalRedirect(builder);

                // handle wildcard domains
                HandleWildcardDomains(builder);

                // set the culture  -- again, 'cos it might have changed due to a finder or wildcard domain
                SetVariationContext(builder.Culture);
            }

            // trigger the routing request (used to be called Prepared) event - at that point it is still possible to change about anything
            // even though the request might be flagged for redirection - we'll redirect _after_ the event
            var routingRequest = new RoutingRequestNotification(builder);
            await _eventAggregator.PublishAsync(routingRequest);

            // we don't take care of anything so if the content has changed, it's up to the user
            // to find out the appropriate template
        }

        private void LogFindPublishedContentAndTemplate(string uriAbsolutePath)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logFindPublishedContentAndTemplate.Invoke(_logger, uriAbsolutePath, null);
            }
        }

        /// <summary>
        /// This method finalizes/builds the PCR with the values assigned.
        /// </summary>
        /// <returns>
        /// Returns false if the request was not successfully configured
        /// </returns>
        /// <remarks>
        /// This method logic has been put into it's own method in case developers have created a custom PCR or are assigning their own values
        /// but need to finalize it themselves.
        /// </remarks>
        internal IPublishedRequest BuildRequest(IPublishedRequestBuilder builder)
        {
            IPublishedRequest result = builder.Build();

            if (!builder.HasPublishedContent())
            {
                return result;
            }

            // set the culture -- again, 'cos it might have changed in the event handler
            SetVariationContext(result.Culture);

            return result;
        }

        /// <inheritdoc />
        public async Task<IPublishedRequest> UpdateRequestAsync(IPublishedRequest request, IPublishedContent? publishedContent)
        {
            // store the original (if any)
            IPublishedContent? content = request.PublishedContent;

            IPublishedRequestBuilder builder = new PublishedRequestBuilder(request.Uri, _fileService);

            // set to the new content (or null if specified)
            builder.SetPublishedContent(publishedContent);

            // re-route
            await RouteRequestInternalAsync(builder);

            // return if we are redirect
            if (builder.IsRedirect())
            {
                return BuildRequest(builder);
            }

            // this will occur if publishedContent is null and the last chance finders also don't assign content
            if (!builder.HasPublishedContent())
            {
                // means the engine could not find a proper document to handle 404
                // restore the saved content so we know it exists
                builder.SetPublishedContent(content);
            }

            if (!builder.HasDomain())
            {
                FindDomain(builder);
            }

            return BuildRequest(builder);
        }

        /// <summary>
        /// Finds the site root (if any) matching the http request, and updates the PublishedRequest accordingly.
        /// </summary>
        /// <returns>A value indicating whether a domain was found.</returns>
        internal bool FindDomain(IPublishedRequestBuilder request)
        {
            const string tracePrefix = "FindDomain: ";

            // note - we are not handling schemes nor ports here.
            LogFindDomain(tracePrefix, request.Uri);
            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            IDomainCache? domainsCache = umbracoContext.PublishedSnapshot.Domains;
            var domains = domainsCache?.GetAll(includeWildcards: false).ToList();

            // determines whether a domain corresponds to a published document, since some
            // domains may exist but on a document that has been unpublished - as a whole - or
            // that is not published for the domain's culture - in which case the domain does
            // not apply
            bool IsPublishedContentDomain(Domain domain)
            {
                // just get it from content cache - optimize there, not here
                IPublishedContent? domainDocument = umbracoContext.PublishedSnapshot.Content?.GetById(domain.ContentId);

                // not published - at all
                if (domainDocument == null)
                {
                    return false;
                }

                // invariant - always published
                if (!domainDocument.ContentType.VariesByCulture())
                {
                    return true;
                }

                // variant, ensure that the culture corresponding to the domain's language is published
                return domain.Culture is not null && domainDocument.Cultures.ContainsKey(domain.Culture);
            }

            domains = domains?.Where(IsPublishedContentDomain).ToList();

            var defaultCulture = domainsCache?.DefaultCulture;

            // try to find a domain matching the current request
            DomainAndUri? domainAndUri = DomainUtilities.SelectDomain(domains, request.Uri, defaultCulture: defaultCulture);

            // handle domain - always has a contentId and a culture
            if (domainAndUri != null)
            {
                // matching an existing domain
                LogDomainFound(tracePrefix, domainAndUri.Name, domainAndUri.ContentId, domainAndUri.Culture);

                request.SetDomain(domainAndUri);

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
                LogDomainNotFound(tracePrefix);

                request.SetCulture(defaultCulture ?? CultureInfo.CurrentUICulture.Name);
            }

            LogDomainCulture(tracePrefix, request.Culture);

            return request.Domain != null;
        }

        private void LogFindDomain(string tracePrefix, Uri requestUri)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logFindDomain.Invoke(_logger, tracePrefix, requestUri, null);
            }
        }

        private void LogDomainFound(string tracePrefix, string domain, int rootContentId, string culture)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logDomainFound.Invoke(_logger, tracePrefix, domain, rootContentId, culture, null);
            }
        }

        private void LogDomainNotFound(string tracePrefix)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logDomainNotFound.Invoke(_logger, tracePrefix, null);
            }
        }

        private void LogDomainCulture(string tracePrefix, string cultureName)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logDomainCulture.Invoke(_logger, tracePrefix, cultureName, null);
            }
        }

        /// <summary>
        /// Looks for wildcard domains in the path and updates <c>Culture</c> accordingly.
        /// </summary>
        internal void HandleWildcardDomains(IPublishedRequestBuilder request)
        {
            const string tracePrefix = "HandleWildcardDomains: ";

            if (request.PublishedContent == null)
            {
                return;
            }

            var nodePath = request.PublishedContent.Path;
            LogHandleWildcardDomains(tracePrefix, nodePath);
            var rootNodeId = request.Domain != null ? request.Domain.ContentId : (int?)null;
            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            Domain? domain = DomainUtilities.FindWildcardDomainInPath(umbracoContext.PublishedSnapshot.Domains?.GetAll(true), nodePath, rootNodeId);

            // always has a contentId and a culture
            if (domain != null)
            {
                request.SetCulture(domain.Culture);
                LogFoundWildcardDomain(tracePrefix, domain.ContentId, request.Culture);
            }
            else
            {
                LogNotFoundWildcardDomain(tracePrefix);
            }
        }

        private void LogHandleWildcardDomains(string tracePrefix, string nodePath)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logHandleWildcardDomains.Invoke(_logger, tracePrefix, nodePath, null);
            }
        }

        private void LogFoundWildcardDomain(string tracePrefix, int domainContentId, string cultureName)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logFoundWildcardDomain.Invoke(_logger, tracePrefix, domainContentId, cultureName, null);
            }
        }

        private void LogNotFoundWildcardDomain(string tracePrefix)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logNotFoundWildcardDomain.Invoke(_logger, tracePrefix, null);
            }
        }

        internal bool FindTemplateRenderingEngineInDirectory(DirectoryInfo directory, string alias, string[] extensions)
        {
            if (directory == null || directory.Exists == false)
            {
                return false;
            }

            var pos = alias.IndexOf('/');
            if (pos > 0)
            {
                // recurse
                DirectoryInfo? subdir = directory.GetDirectories(alias.Substring(0, pos)).FirstOrDefault();
                alias = alias.Substring(pos + 1);
                return subdir != null && FindTemplateRenderingEngineInDirectory(subdir, alias, extensions);
            }

            // look here
            return directory.GetFiles().Any(f => extensions.Any(e => f.Name.InvariantEquals(alias + e)));
        }

        /// <summary>
        /// Tries to find the document matching the request, by running the IPublishedContentFinder instances.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no finder collection.</exception>
        internal bool FindPublishedContent(IPublishedRequestBuilder request)
        {
            const string tracePrefix = "FindPublishedContent: ";

            // look for the document
            // the first successful finder, if any, will set this.PublishedContent, and may also set this.Template
            // some finders may implement caching
            using (_profilingLogger.DebugDuration<PublishedRouter>(
                $"{tracePrefix}Begin finders",
                $"{tracePrefix}End finders"))
            {
                // iterate but return on first one that finds it
                var found = _contentFinders.Any(finder =>
                {
                    LogExecutingContentFinder(finder.GetType().FullName);
                    return finder.TryFindContent(request);
                });
                LogExecutedContentFinder(
                    found,
                    request.HasPublishedContent() ? request.PublishedContent?.Id.ToString() : "NULL",
                    request.HasTemplate() ? request.Template?.Alias : "NULL",
                    request.HasDomain() ? request.Domain?.ToString() : "NULL",
                    request.Culture ?? "NULL",
                    request.ResponseStatusCode);

                return found;
            }
        }

        private void LogExecutingContentFinder(string contentFinderType)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logExecutingContentFinder.Invoke(_logger, contentFinderType, null);
            }
        }

        private void LogExecutedContentFinder(bool found, string publishedContentId, string templateAlias, string domain, string culture, int? statusCode)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logExecutedContentFinder.Invoke(_logger, found, publishedContentId, templateAlias, domain, culture, statusCode, null);
            }
        }

        /// <summary>
        /// Handles the published content (if any).
        /// </summary>
        /// <param name="request">The request builder.</param>
        /// <remarks>
        /// Handles "not found", internal redirects ...
        /// things that must be handled in one place because they can create loops
        /// </remarks>
        private void HandlePublishedContent(IPublishedRequestBuilder request)
        {
            // because these might loop, we have to have some sort of infinite loop detection
            int i = 0, j = 0;
            const int maxLoop = 8;
            do
            {
                _logger.LogDebug("HandlePublishedContent: Loop {LoopCounter}", i);

                // handle not found
                if (request.PublishedContent == null)
                {
                    request.SetIs404();
                    _logger.LogDebug("HandlePublishedContent: No document, try last chance lookup");

                    // if it fails then give up, there isn't much more that we can do
                    if (_contentLastChanceFinder.TryFindContent(request) == false)
                    {
                        _logger.LogDebug("HandlePublishedContent: Failed to find a document, give up");
                        break;
                    }

                    _logger.LogDebug("HandlePublishedContent: Found a document");
                }

                // follow internal redirects as long as it's not running out of control ie infinite loop of some sort
                j = 0;
                while (FollowInternalRedirects(request) && j++ < maxLoop)
                { }

                // we're running out of control
                if (j == maxLoop)
                {
                    break;
                }

                // loop while we don't have page, ie the redirect or access
                // got us to nowhere and now we need to run the notFoundLookup again
                // as long as it's not running out of control ie infinite loop of some sort
            } while (request.PublishedContent == null && i++ < maxLoop);

            if (i == maxLoop || j == maxLoop)
            {
                _logger.LogDebug("HandlePublishedContent: Looks like we are running into an infinite loop, abort");
                request.SetPublishedContent(null);
            }

            _logger.LogDebug("HandlePublishedContent: End");
        }

        /// <summary>
        /// Follows internal redirections through the <c>umbracoInternalRedirectId</c> document property.
        /// </summary>
        /// <param name="request">The request builder.</param>
        /// <returns>A value indicating whether redirection took place and led to a new published document.</returns>
        /// <remarks>
        /// <para>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</para>
        /// <para>As per legacy, if the redirect does not work, we just ignore it.</para>
        /// </remarks>
        private bool FollowInternalRedirects(IPublishedRequestBuilder request)
        {
            if (request.PublishedContent == null)
            {
                throw new InvalidOperationException("There is no PublishedContent.");
            }

            // don't try to find a redirect if the property doesn't exist
            if (request.PublishedContent.HasProperty(Constants.Conventions.Content.InternalRedirectId) == false)
            {
                return false;
            }

            var redirect = false;
            var valid = false;
            IPublishedContent? internalRedirectNode = null;
            var internalRedirectId = request.PublishedContent.Value(_publishedValueFallback, Constants.Conventions.Content.InternalRedirectId, defaultValue: -1);
            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();

            if (internalRedirectId > 0)
            {
                // try and get the redirect node from a legacy integer ID
                valid = true;
                internalRedirectNode = umbracoContext.Content?.GetById(internalRedirectId);
            }
            else
            {
                GuidUdi? udiInternalRedirectId = request.PublishedContent.Value<GuidUdi>(_publishedValueFallback, Constants.Conventions.Content.InternalRedirectId);
                if (udiInternalRedirectId is not null)
                {
                    // try and get the redirect node from a UDI Guid
                    valid = true;
                    internalRedirectNode = umbracoContext.Content?.GetById(udiInternalRedirectId.Guid);
                }
            }

            if (valid == false)
            {
                // bad redirect - log and display the current page (legacy behavior)
                LogFailedToInternalRedirectInvalidId(request.PublishedContent.GetProperty(Constants.Conventions.Content.InternalRedirectId)?.GetSourceValue());
            }

            if (internalRedirectNode == null)
            {
                LogFailedToInternalRedirectNoDocumentById(request.PublishedContent.GetProperty(Constants.Conventions.Content.InternalRedirectId)?.GetSourceValue());
            }
            else if (internalRedirectId == request.PublishedContent.Id)
            {
                // redirect to self
                _logger.LogDebug("FollowInternalRedirects: Redirecting to self, ignore");
            }
            else
            {
                // save since it will be cleared
                ITemplate? template = request.Template;

                request.SetInternalRedirect(internalRedirectNode); // don't use .PublishedContent here

                // must restore the template if it's an internal redirect & the config option is set
                if (request.IsInternalRedirect && _webRoutingSettings.InternalRedirectPreservesTemplate)
                {
                    // restore
                    request.SetTemplate(template);
                }

                redirect = true;
                LogInternalRedirecting(internalRedirectId);
            }

            return redirect;
        }

        private void LogFailedToInternalRedirectInvalidId(object internalRedirectId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logFailedToInternalRedirectInvalidId.Invoke(_logger, internalRedirectId, null);
            }
        }
        
        private void LogFailedToInternalRedirectNoDocumentById(object internalRedirectId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logFailedToInternalRedirectNoDocumentById.Invoke(_logger, internalRedirectId, null);
            }
        }
        private static readonly Action<ILogger, int, Exception> _logInternalRedirecting
         = LoggerMessage.Define<int>(LogLevel.Debug, new EventId(35), "FollowInternalRedirects: Redirecting to id ={InternalRedirectId}");

        private void LogInternalRedirecting(int internalRedirectId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logInternalRedirecting.Invoke(_logger, internalRedirectId, null);
            }
        }

        /// <summary>
        /// Finds a template for the current node, if any.
        /// </summary>
        /// <param name="request">The request builder.</param>
        /// <param name="contentFoundByFinders">If the content was found by the finders, before anything such as 404, redirect... took place.</param>
        private void FindTemplate(IPublishedRequestBuilder request, bool contentFoundByFinders)
        {
            // TODO: We've removed the event, might need to re-add?
            // NOTE: at the moment there is only 1 way to find a template, and then ppl must
            // use the Prepared event to change the template if they wish. Should we also
            // implement an ITemplateFinder logic?
            if (request.PublishedContent == null)
            {
                request.SetTemplate(null);
                return;
            }

            // read the alternate template alias, from querystring, form, cookie or server vars,
            // only if the published content is the initial once, else the alternate template
            // does not apply
            // + optionally, apply the alternate template on internal redirects
            var useAltTemplate = contentFoundByFinders
                || (_webRoutingSettings.InternalRedirectPreservesTemplate && request.IsInternalRedirect);

            var altTemplate = useAltTemplate
                ? _requestAccessor.GetRequestValue(Constants.Conventions.Url.AltTemplate)
                : null;

            if (string.IsNullOrWhiteSpace(altTemplate))
            {
                // we don't have an alternate template specified. use the current one if there's one already,
                // which can happen if a content lookup also set the template (LookupByNiceUrlAndTemplate...),
                // else lookup the template id on the document then lookup the template with that id.
                if (request.HasTemplate())
                {
                    _logger.LogDebug("FindTemplate: Has a template already, and no alternate template.");
                    return;
                }

                // TODO: We need to limit altTemplate to only allow templates that are assigned to the current document type!
                // if the template isn't assigned to the document type we should log a warning and return 404
                var templateId = request.PublishedContent.TemplateId;
                ITemplate? template = GetTemplate(templateId);
                request.SetTemplate(template);
                if (template != null)
                {
                    LogTemplateFound(template.Id, template.Alias);
                }
                else
                {
                    LogTemplateNotFound(templateId);
                }
            }
            else
            {
                // we have an alternate template specified. lookup the template with that alias
                // this means the we override any template that a content lookup might have set
                // so /path/to/page/template1?altTemplate=template2 will use template2

                // ignore if the alias does not match - just trace
                if (request.HasTemplate())
                {
                    _logger.LogDebug("FindTemplate: Has a template already, but also an alternative template.");
                }

                LogFindingAlternativeTemplateByAlias(altTemplate);

                // IsAllowedTemplate deals both with DisableAlternativeTemplates and ValidateAlternativeTemplates settings
                if (request.PublishedContent.IsAllowedTemplate(
                    _fileService,
                    _contentTypeService,
                    _webRoutingSettings.DisableAlternativeTemplates,
                    _webRoutingSettings.ValidateAlternativeTemplates,
                    altTemplate))
                {
                    // allowed, use
                    ITemplate? template = _fileService.GetTemplate(altTemplate);

                    if (template != null)
                    {
                        request.SetTemplate(template);
                        LogFoundAlternativeTemplateByAlias(template.Id, template.Alias);
                    }
                    else
                    {
                        LogNotFoundAlternativeTemplateByAlias(altTemplate);
                    }
                }
                else
                {
                    LogNotAllowedAlternativeTemplateByAlias(altTemplate, request.PublishedContent.Id);

                    // no allowed, back to default
                    var templateId = request.PublishedContent.TemplateId;
                    ITemplate? template = GetTemplate(templateId);
                    request.SetTemplate(template);
                    LogFallbackToDefaultTempate(template?.Id, template?.Alias);
                }
            }

            if (!request.HasTemplate())
            {
                _logger.LogDebug("FindTemplate: No template was found.");

                // initial idea was: if we're not already 404 and UmbracoSettings.HandleMissingTemplateAs404 is true
                // then reset _pcr.Document to null to force a 404.
                //
                // but: because we want to let MVC hijack routes even though no template is defined, we decide that
                // a missing template is OK but the request will then be forwarded to MVC, which will need to take
                // care of everything.
                //
                // so, don't set _pcr.Document to null here
            }
        }

        private void LogTemplateFound(int templateId, string templateAlias)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logTemplateFound.Invoke(_logger, templateId, templateAlias, null);
            }
        }

        private void LogTemplateNotFound(int? templateId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logTemplateNotFound.Invoke(_logger, templateId, null);
            }
        }

        private void LogFindingAlternativeTemplateByAlias(string altTemplate)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logFindingAlternativeTemplateByAlias.Invoke(_logger, altTemplate, null);
            }
        }

        private void LogFoundAlternativeTemplateByAlias(int templateId, string templateAlias)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logFoundAlternativeTemplateByAlias.Invoke(_logger, templateId, templateAlias, null);
            }
        }

        private void LogNotFoundAlternativeTemplateByAlias(string altTemplate)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logNotFoundAlternativeTemplateByAlias.Invoke(_logger, altTemplate, null);
            }
        }

        private void LogNotAllowedAlternativeTemplateByAlias(string templateAlias, int nodeId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logNotAllowedAlternativeTemplateByAlias.Invoke(_logger, templateAlias, nodeId, null);
            }
        }

        private void LogFallbackToDefaultTempate(int templateId, string templateAlias)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logFallbackToDefaultTempate.Invoke(_logger, templateId, templateAlias, null);
            }
        }


        private ITemplate? GetTemplate(int? templateId)
        {
            if (templateId.HasValue == false || templateId.Value == default)
            {
                _logger.LogDebug("GetTemplateModel: No template.");
                return null;
            }

            LogRetrievingTemplate(templateId);

            if (templateId == null)
            {
                throw new InvalidOperationException("The template is not set, the page cannot render.");
            }

            ITemplate? template = _fileService.GetTemplate(templateId.Value);
            if (template == null)
            {
                throw new InvalidOperationException("The template with Id " + templateId + " does not exist, the page cannot render.");
            }

            LogRetrievedTemplate(template.Id, template.Alias);
            return template;
        }


        private void LogRetrievingTemplate(int? templateId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logRetrievingTemplate.Invoke(_logger, templateId, null);
            }
        }

        private void LogRetrievedTemplate(int templateId, string templateAlias)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logRetrievedTemplate.Invoke(_logger, templateId, templateAlias, null);
            }
        }

        /// <summary>
        /// Follows external redirection through <c>umbracoRedirect</c> document property.
        /// </summary>
        /// <remarks>As per legacy, if the redirect does not work, we just ignore it.</remarks>
        private void FollowExternalRedirect(IPublishedRequestBuilder request)
        {
            if (request.PublishedContent == null)
            {
                return;
            }

            // don't try to find a redirect if the property doesn't exist
            if (request.PublishedContent.HasProperty(Constants.Conventions.Content.Redirect) == false)
            {
                return;
            }

            var redirectId = request.PublishedContent.Value(_publishedValueFallback, Constants.Conventions.Content.Redirect, defaultValue: -1);
            var redirectUrl = "#";
            if (redirectId > 0)
            {
                redirectUrl = _publishedUrlProvider.GetUrl(redirectId);
            }
            else
            {
                // might be a UDI instead of an int Id
                GuidUdi? redirectUdi = request.PublishedContent.Value<GuidUdi>(_publishedValueFallback, Constants.Conventions.Content.Redirect);
                if (redirectUdi is not null)
                {
                    redirectUrl = _publishedUrlProvider.GetUrl(redirectUdi.Guid);
                }
            }

            if (redirectUrl != "#")
            {
                request.SetRedirect(redirectUrl);
            }
        }
    }
}
