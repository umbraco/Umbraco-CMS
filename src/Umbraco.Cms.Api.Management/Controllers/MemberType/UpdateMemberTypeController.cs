using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

/// <summary>
/// API controller responsible for handling HTTP requests to update member types in the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class UpdateMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeEditingPresentationFactory _memberTypeEditingPresentationFactory;
    private readonly IMemberTypeEditingService _memberTypeEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMemberTypeService _memberTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMemberTypeController"/> class.
    /// </summary>
    /// <param name="memberTypeEditingPresentationFactory">Factory for creating member type editing presentation models.</param>
    /// <param name="memberTypeEditingService">Service for handling member type editing operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="memberTypeService">Service for managing member types.</param>
    public UpdateMemberTypeController(
        IMemberTypeEditingPresentationFactory memberTypeEditingPresentationFactory,
        IMemberTypeEditingService memberTypeEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberTypeService memberTypeService)
    {
        _memberTypeEditingPresentationFactory = memberTypeEditingPresentationFactory;
        _memberTypeEditingService = memberTypeEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _memberTypeService = memberTypeService;
    }

    /// <summary>
    /// Asynchronously updates the member type identified by the specified <paramref name="id"/> using the details provided in the <paramref name="requestModel"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the member type to update.</param>
    /// <param name="requestModel">The model containing the updated member type details.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c> if the update was successful.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request model is invalid.</description></item>
    /// <item><description><c>404 Not Found</c> if a member type with the specified <paramref name="id"/> does not exist.</description></item>
    /// </list>
    /// </returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a member type.")]
    [EndpointDescription("Updates a member type identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateMemberTypeRequestModel requestModel)
    {
        IMemberType? memberType = await _memberTypeService.GetAsync(id);
        if (memberType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        MemberTypeUpdateModel model = _memberTypeEditingPresentationFactory.MapUpdateModel(requestModel);
        Attempt<IMemberType?, ContentTypeOperationStatus> result = await _memberTypeEditingService.UpdateAsync(memberType, model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }
}
