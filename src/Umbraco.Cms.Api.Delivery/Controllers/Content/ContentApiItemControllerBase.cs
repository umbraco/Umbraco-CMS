using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.Controllers.Content;

public abstract class ContentApiItemControllerBase : ContentApiControllerBase
{
    // TODO: Remove this in V14 when the obsolete constructors have been removed
    private readonly IPublicAccessService _publicAccessService;

    [Obsolete($"Please use the constructor that does not accept {nameof(IPublicAccessService)}. Will be removed in V14.")]
    protected ContentApiItemControllerBase(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IPublicAccessService publicAccessService)
        : this(apiPublishedContentCache, apiContentResponseBuilder)
    {
    }

    protected ContentApiItemControllerBase(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder)
        : base(apiPublishedContentCache, apiContentResponseBuilder)
        => _publicAccessService = StaticServiceProvider.Instance.GetRequiredService<IPublicAccessService>();

    [Obsolete($"Please use {nameof(IPublicAccessService)} to test for content protection. Will be removed in V14.")]
    protected bool IsProtected(IPublishedContent content) => _publicAccessService.IsProtected(content.Path);

    protected async Task<IActionResult?> HandleMemberAccessAsync(IPublishedContent contentItem, IRequestMemberAccessService requestMemberAccessService)
    {
        PublicAccessStatus accessStatus = await requestMemberAccessService.MemberHasAccessToAsync(contentItem);
        return accessStatus is PublicAccessStatus.AccessAccepted
            ? null
            : accessStatus is PublicAccessStatus.AccessDenied
                ? Forbidden()
                : Unauthorized();
    }

    protected async Task<IActionResult?> HandleMemberAccessAsync(IEnumerable<IPublishedContent> contentItems, IRequestMemberAccessService requestMemberAccessService)
    {
        foreach (IPublishedContent content in contentItems)
        {
            IActionResult? result = await HandleMemberAccessAsync(content, requestMemberAccessService);
            // if any of the content items yield an error based on the current member access, return that error
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }
}
