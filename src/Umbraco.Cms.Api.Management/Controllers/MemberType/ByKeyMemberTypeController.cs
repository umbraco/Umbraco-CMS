using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

/// <summary>
/// Controller for managing member type operations by unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IMemberTypePresentationFactory _memberTypePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.MemberType.ByKeyMemberTypeController"/> class.
    /// </summary>
    /// <param name="memberTypeService">The service used to manage member types.</param>
    /// <param name="memberTypePresentationFactory">The factory used to create presentation models for member types.</param>
    public ByKeyMemberTypeController(IMemberTypeService memberTypeService, IMemberTypePresentationFactory memberTypePresentationFactory)
    {
        _memberTypeService = memberTypeService;
        _memberTypePresentationFactory = memberTypePresentationFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberTypeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a member type.")]
    [EndpointDescription("Gets a member type identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IMemberType? memberType = await _memberTypeService.GetAsync(id);
        if (memberType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        MemberTypeResponseModel model = await _memberTypePresentationFactory.CreateResponseModelAsync(memberType);
        return Ok(model);
    }
}
