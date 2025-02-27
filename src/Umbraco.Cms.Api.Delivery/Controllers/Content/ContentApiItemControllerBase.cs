using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Delivery.Controllers.Content;

public abstract class ContentApiItemControllerBase : ContentApiControllerBase
{
    protected ContentApiItemControllerBase(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder)
        : base(apiPublishedContentCache, apiContentResponseBuilder)
    {
    }

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
