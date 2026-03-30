using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

/// <summary>
/// API controller responsible for managing member types that are allowed to be created at the root level of the member section.
/// </summary>
[ApiVersion("1.0")]
public class AllowedAtRootMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedAtRootMemberTypeController"/> class, which manages allowed member types at the root level.
    /// </summary>
    /// <param name="memberTypeService">The service used to manage member types.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco objects to API models.</param>
    public AllowedAtRootMemberTypeController(IMemberTypeService memberTypeService, IUmbracoMapper umbracoMapper)
    {
        _memberTypeService = memberTypeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paged list of member types that can be created at the root level.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for paging).</param>
    /// <param name="take">The maximum number of items to return (used for paging).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{AllowedMemberType}"/> representing the allowed member types.</returns>
    [HttpGet("allowed-at-root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AllowedMemberType>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets member types allowed at root.")]
    [EndpointDescription("Gets a collection of member types that are allowed to be created at the root level.")]
    public async Task<IActionResult> AllowedAtRoot(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        PagedModel<IMemberType> result = await _memberTypeService.GetAllAllowedAsRootAsync(skip, take);

        List<AllowedMemberType> viewModels = _umbracoMapper.MapEnumerable<IMemberType, AllowedMemberType>(result.Items);

        var pagedViewModel = new PagedViewModel<AllowedMemberType>
        {
            Total = result.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
