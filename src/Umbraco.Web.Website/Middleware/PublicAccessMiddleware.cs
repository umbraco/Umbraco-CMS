using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Routing;

namespace Umbraco.Cms.Web.Website.Middleware
{
    public class PublicAccessMiddleware : IMiddleware
    {
        private readonly ILogger<PublicAccessMiddleware> _logger;
        private readonly IPublicAccessService _publicAccessService;
        private readonly IPublicAccessChecker _publicAccessChecker;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IUmbracoRouteValuesFactory _umbracoRouteValuesFactory;
        private readonly IPublishedRouter _publishedRouter;

        public PublicAccessMiddleware(
            ILogger<PublicAccessMiddleware> logger,
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

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            UmbracoRouteValues umbracoRouteValues = context.Features.Get<UmbracoRouteValues>();

            if (umbracoRouteValues != null)
            {
                await EnsurePublishedContentAccess(context, umbracoRouteValues);
            }

            await next(context);
        }

        /// <summary>
        /// Ensures that access to current node is permitted.
        /// </summary>
        /// <remarks>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</remarks>
        private async Task EnsurePublishedContentAccess(HttpContext httpContext, UmbracoRouteValues routeValues)
        {
            // because these might loop, we have to have some sort of infinite loop detection
            int i = 0;
            const int maxLoop = 8;
            PublicAccessStatus publicAccessStatus = PublicAccessStatus.AccessAccepted;
            do
            {
                _logger.LogDebug(nameof(EnsurePublishedContentAccess) + ": Loop {LoopCounter}", i);


                IPublishedContent publishedContent = routeValues.PublishedRequest?.PublishedContent;
                if (publishedContent == null)
                {
                    return;
                }

                var path = publishedContent.Path;

                Attempt<PublicAccessEntry> publicAccessAttempt = _publicAccessService.IsProtected(path);

                if (publicAccessAttempt)
                {
                    _logger.LogDebug("EnsurePublishedContentAccess: Page is protected, check for access");

                    publicAccessStatus = await _publicAccessChecker.HasMemberAccessToContentAsync(publishedContent.Id);
                    switch (publicAccessStatus)
                    {
                        case PublicAccessStatus.NotLoggedIn:
                            _logger.LogDebug("EnsurePublishedContentAccess: Not logged in, redirect to login page");
                            routeValues = await SetPublishedContentAsOtherPageAsync(httpContext, routeValues.PublishedRequest, publicAccessAttempt.Result.LoginNodeId);
                            break;
                        case PublicAccessStatus.AccessDenied:
                            _logger.LogDebug("EnsurePublishedContentAccess: Current member has not access, redirect to error page");
                            routeValues = await SetPublishedContentAsOtherPageAsync(httpContext, routeValues.PublishedRequest, publicAccessAttempt.Result.NoAccessNodeId);
                            break;
                        case PublicAccessStatus.LockedOut:
                            _logger.LogDebug("Current member is locked out, redirect to error page");
                            routeValues = await SetPublishedContentAsOtherPageAsync(httpContext, routeValues.PublishedRequest, publicAccessAttempt.Result.NoAccessNodeId);
                            break;
                        case PublicAccessStatus.NotApproved:
                            _logger.LogDebug("Current member is unapproved, redirect to error page");
                            routeValues = await SetPublishedContentAsOtherPageAsync(httpContext, routeValues.PublishedRequest, publicAccessAttempt.Result.NoAccessNodeId);
                            break;
                        case PublicAccessStatus.AccessAccepted:
                            _logger.LogDebug("Current member has access");
                            break;
                    }
                }
                else
                {
                    publicAccessStatus = PublicAccessStatus.AccessAccepted;
                    _logger.LogDebug("EnsurePublishedContentAccess: Page is not protected");
                }


                //loop until we have access or reached max loops
            } while (routeValues != null && publicAccessStatus != PublicAccessStatus.AccessAccepted && i++ < maxLoop);

            if (i == maxLoop)
            {
                _logger.LogDebug(nameof(EnsurePublishedContentAccess) + ": Looks like we are running into an infinite loop, abort");                
            }
        }



        private async Task<UmbracoRouteValues> SetPublishedContentAsOtherPageAsync(HttpContext httpContext, IPublishedRequest publishedRequest, int pageId)
        {
            if (pageId != publishedRequest.PublishedContent.Id)
            {
                IPublishedContent publishedContent = _umbracoContextAccessor.UmbracoContext.PublishedSnapshot.Content.GetById(pageId);
                if (publishedContent == null)
                {
                    throw new InvalidOperationException("No content found by id " + pageId);
                }

                IPublishedRequest reRouted = await _publishedRouter.UpdateRequestAsync(publishedRequest, publishedContent);

                // we need to change the content item that is getting rendered so we have to re-create UmbracoRouteValues.
                UmbracoRouteValues updatedRouteValues = await _umbracoRouteValuesFactory.CreateAsync(httpContext, reRouted);

                // Update the feature
                httpContext.Features.Set(updatedRouteValues);

                return updatedRouteValues;
            }
            else
            {
                _logger.LogWarning("Public Access rule has a redirect node set to itself, nothing can be routed.");
                // Update the feature to nothing - cannot continue
                httpContext.Features.Set<UmbracoRouteValues>(null);
                return null;
            }
        }
    }
}
