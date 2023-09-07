using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.Controllers;

public abstract class ContentApiItemControllerBase : ContentApiControllerBase
{
    // please remove this in V14 when the obsolete constructors have been removed
    private readonly IPublicAccessService _publicAccessService;

    [Obsolete($"Please use the parameterless constructor does not accept {nameof(IPublicAccessService)}. Will be removed in V14.")]
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

    protected async Task<IActionResult?> HandleMemberAccessAsync(IPublishedContent content, IRequestMemberAccessService requestMemberAccessService)
    {
        PublicAccessStatus accessStatus = await requestMemberAccessService.MemberHasAccessToAsync(content);
        return accessStatus is PublicAccessStatus.AccessAccepted
            ? null
            : accessStatus is PublicAccessStatus.AccessDenied
                ? Forbidden()
                : Unauthorized();
    }
}
