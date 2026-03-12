using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

/// <summary>
/// Provides API endpoints for managing composition references of member types in Umbraco.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class CompositionReferenceMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositionReferenceMemberTypeController"/> class, which manages composition references for member types.
    /// </summary>
    /// <param name="memberTypeService">Service for managing member types.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco objects.</param>
    public CompositionReferenceMemberTypeController(IMemberTypeService memberTypeService, IUmbracoMapper umbracoMapper)
    {
        _memberTypeService = memberTypeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves all member types that include the specified member type as a composition.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the member type to check for composition references.</param>
    /// <returns>An <see cref="IActionResult"/> containing a collection of <see cref="MemberTypeCompositionResponseModel"/> objects representing the referencing member types, or a problem response if not found.</returns>
    [HttpGet("{id:guid}/composition-references")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets composition references.")]
    [EndpointDescription("Gets a collection of member types that reference the specified member type as a composition.")]
    public async Task<IActionResult> CompositionReferences(CancellationToken cancellationToken, Guid id)
    {
        var memberType = await _memberTypeService.GetAsync(id);

        if (memberType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        IEnumerable<IMemberType> composedOf = _memberTypeService.GetComposedOf(memberType.Id);
        List<MemberTypeCompositionResponseModel> responseModels = _umbracoMapper.MapEnumerable<IMemberType, MemberTypeCompositionResponseModel>(composedOf);

        return Ok(responseModels);
    }
}
