using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Routing;

/// <summary>
///     Default implementation of <see cref="IPublicAccessRequestHandler" />, which checks the member's access to
///     protected content and re-routes the request to the login or error page configured for the protecting rule.
/// </summary>
public class PublicAccessRequestHandler : IPublicAccessRequestHandler
{
    private readonly ILogger<PublicAccessRequestHandler> _logger;
    private readonly IPublicAccessChecker _publicAccessChecker;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IPublishedRouter _publishedRouter;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IUmbracoRouteValuesFactory _umbracoRouteValuesFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessRequestHandler" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="publicAccessService">The service used to find the public access rule protecting the content.</param>
    /// <param name="publicAccessChecker">The checker used to determine whether the current member has access to the content.</param>
    /// <param name="umbracoContextAccessor">The accessor used to resolve the published login and error pages.</param>
    /// <param name="umbracoRouteValuesFactory">The factory used to create route values for a re-routed request.</param>
    /// <param name="publishedRouter">The router used to re-route the request to another page.</param>
    public PublicAccessRequestHandler(
        ILogger<PublicAccessRequestHandler> logger,
        IPublicAccessService publicAccessService,
        IPublicAccessChecker publicAccessChecker,
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoRouteValuesFactory umbracoRouteValuesFactory,
        IPublishedRouter publishedRouter)
    {
        _logger = logger;
        _publicAccessService = publicAccessService;
        _publicAccessChecker = publicAccessChecker;
        _umbracoContextAccessor = umbracoContextAccessor;
        _umbracoRouteValuesFactory = umbracoRouteValuesFactory;
        _publishedRouter = publishedRouter;
    }

    /// <inheritdoc />
    public async Task<UmbracoRouteValues?> RewriteForPublishedContentAccessAsync(
        HttpContext httpContext,
        UmbracoRouteValues routeValues)
    {
        // because these might loop, we have to have some sort of infinite loop detection
        var i = 0;
        const int maxLoop = 8;
        PublicAccessStatus publicAccessStatus;
        do
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                _logger.LogDebug(nameof(RewriteForPublishedContentAccessAsync) + ": Loop {LoopCounter}", i);
            }

            IPublishedContent? publishedContent = routeValues.PublishedRequest?.PublishedContent;
            if (publishedContent == null)
            {
                return routeValues;
            }

            var path = publishedContent.Path;

            Attempt<PublicAccessEntry?> publicAccessAttempt = _publicAccessService.IsProtected(path);

            if (publicAccessAttempt.Success)
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                {
                    _logger.LogDebug("EnsurePublishedContentAccess: Page is protected, check for access");
                }

                // manually authenticate the request
                AuthenticateResult authResult =
                    await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
                if (authResult.Succeeded)
                {
                    // set the user to the auth result. we need to do this here because this occurs
                    // before the authentication middleware.
                    // NOTE: It would be possible to just pass the authResult to the HasMemberAccessToContentAsync method
                    // instead of relying directly on the user assigned to the http context, and then the auth middleware
                    // will run anyways and assign the user. Perhaps that is a little cleaner, but would require more code
                    // changes right now, and really it's not any different in the end result.
                    httpContext.SetPrincipalForRequest(authResult.Principal);
                }

                publicAccessStatus = await _publicAccessChecker.HasMemberAccessToContentAsync(publishedContent.Id);
                switch (publicAccessStatus)
                {
                    case PublicAccessStatus.NotLoggedIn:
                        // redirect if this is not the login page
                        if (publicAccessAttempt.Result!.LoginNodeId != publishedContent.Id)
                        {
                            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                            {
                                _logger.LogDebug("EnsurePublishedContentAccess: Not logged in, redirect to login page");
                            }
                            routeValues = await SetPublishedContentAsOtherPageAsync(
                                httpContext, routeValues.PublishedRequest, publicAccessAttempt.Result!.LoginNodeId);
                        }

                        break;
                    case PublicAccessStatus.AccessDenied:
                        // Redirect if this is not the access denied page
                        if (publicAccessAttempt.Result!.NoAccessNodeId != publishedContent.Id)
                        {
                            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                            {
                                _logger.LogDebug(
                                "EnsurePublishedContentAccess: Current member has not access, redirect to error page");
                            }
                            routeValues = await SetPublishedContentAsOtherPageAsync(
                                httpContext, routeValues.PublishedRequest, publicAccessAttempt.Result!.NoAccessNodeId);
                        }

                        break;
                    case PublicAccessStatus.LockedOut:
                        if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                        {
                            _logger.LogDebug("Current member is locked out, redirect to error page");
                        }
                        routeValues = await SetPublishedContentAsOtherPageAsync(
                            httpContext, routeValues.PublishedRequest, publicAccessAttempt.Result!.NoAccessNodeId);
                        break;
                    case PublicAccessStatus.NotApproved:
                        if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                        {
                            _logger.LogDebug("Current member is unapproved, redirect to error page");
                        }
                        routeValues = await SetPublishedContentAsOtherPageAsync(
                            httpContext, routeValues.PublishedRequest, publicAccessAttempt.Result!.NoAccessNodeId);
                        break;
                    case PublicAccessStatus.AccessAccepted:
                        if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                        {
                            _logger.LogDebug("Current member has access");
                        }
                        break;
                }
            }
            else
            {
                publicAccessStatus = PublicAccessStatus.AccessAccepted;
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                {
                    _logger.LogDebug("EnsurePublishedContentAccess: Page is not protected");
                }
            }
        }

        // loop until we have access or reached max loops
        while (publicAccessStatus != PublicAccessStatus.AccessAccepted && i++ < maxLoop);

        if (i == maxLoop)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                _logger.LogDebug(nameof(RewriteForPublishedContentAccessAsync) +
                             ": Looks like we are running into an infinite loop, abort");
            }
        }

        return routeValues;
    }

    private async Task<UmbracoRouteValues> SetPublishedContentAsOtherPageAsync(
        HttpContext httpContext, IPublishedRequest? publishedRequest, int pageId)
    {
        if (pageId != publishedRequest?.PublishedContent?.Id)
        {
            IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            if (publishedRequest is null)
            {
                throw new InvalidOperationException("No published request to re-route to the page with id " + pageId);
            }

            IPublishedContent? publishedContent = umbracoContext.Content?.GetById(pageId);
            if (publishedContent is null)
            {
                _logger.LogWarning(
                    "The public access page with id {PageId} has no published content, so the request for the protected page with id {ProtectedPageId} is handled as not found.",
                    pageId,
                    publishedRequest.PublishedContent?.Id);

                IPublishedRequest notFound = await CreateNotFoundRequestAsync(publishedRequest);
                return await _umbracoRouteValuesFactory.CreateAsync(httpContext, notFound);
            }

            IPublishedRequest reRouted = await _publishedRouter.UpdateRequestAsync(publishedRequest, publishedContent);

            // we need to change the content item that is getting rendered so we have to re-create UmbracoRouteValues.
            UmbracoRouteValues updatedRouteValues = await _umbracoRouteValuesFactory.CreateAsync(httpContext, reRouted);

            return updatedRouteValues;
        }

        throw new InvalidOperationException(
            "Public Access rule has a redirect node set to itself, nothing can be routed.");
    }

    private async Task<IPublishedRequest> CreateNotFoundRequestAsync(IPublishedRequest publishedRequest)
    {
        IPublishedRequest notFound = await _publishedRouter.UpdateRequestAsync(publishedRequest, null);

        // When no not-found content is resolved, the router restores the content it was given, which here is
        // the protected page. That must never be served, so fall back to a request without content.
        if (notFound.PublishedContent is null || notFound.PublishedContent.Id != publishedRequest.PublishedContent?.Id)
        {
            return notFound;
        }

        IPublishedRequestBuilder builder = await _publishedRouter.CreateRequestAsync(publishedRequest.Uri);
        if (publishedRequest.Domain is not null)
        {
            builder.SetDomain(publishedRequest.Domain);
        }

        builder.SetCulture(publishedRequest.Culture);
        builder.SetSegment(publishedRequest.Segment);
        builder.SetIs404();
        return builder.Build();
    }
}
