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
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

/// <summary>
/// API controller responsible for handling requests to create new member types in Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class CreateMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeEditingPresentationFactory _memberTypeEditingPresentationFactory;
    private readonly IMemberTypeEditingService _memberTypeEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMemberTypeController"/> class with the specified dependencies for member type editing and security.
    /// </summary>
    /// <param name="memberTypeEditingPresentationFactory">Factory for creating member type editing presentation models.</param>
    /// <param name="memberTypeEditingService">Service for handling member type editing operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public CreateMemberTypeController(
        IMemberTypeEditingPresentationFactory memberTypeEditingPresentationFactory,
        IMemberTypeEditingService memberTypeEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _memberTypeEditingPresentationFactory = memberTypeEditingPresentationFactory;
        _memberTypeEditingService = memberTypeEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new member type.")]
    [EndpointDescription("Creates a new member type with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateMemberTypeRequestModel requestModel)
    {
        MemberTypeCreateModel model = _memberTypeEditingPresentationFactory.MapCreateModel(requestModel);
        Attempt<IMemberType?, ContentTypeOperationStatus> result = await _memberTypeEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyMemberTypeController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : OperationStatusResult(result.Status);
    }
}
