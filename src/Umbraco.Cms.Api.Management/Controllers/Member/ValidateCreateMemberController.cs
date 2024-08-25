using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

[ApiVersion("1.0")]
public class ValidateCreateMemberController : MemberControllerBase
{
    private readonly IMemberEditingService _memberEditingService;
    private readonly IMemberEditingPresentationFactory _memberEditingPresentationFactory;

    public ValidateCreateMemberController(
        IMemberEditingService memberEditingService,
        IMemberEditingPresentationFactory memberEditingPresentationFactory)
    {
        _memberEditingService = memberEditingService;
        _memberEditingPresentationFactory = memberEditingPresentationFactory;
    }

    [HttpPost("validate")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Validate(
        CancellationToken cancellationToken,
        CreateMemberRequestModel requestModel)
    {
        MemberCreateModel model = _memberEditingPresentationFactory.MapCreateModel(requestModel);
        Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await _memberEditingService.ValidateCreateAsync(model);

        return result.Success
            ? Ok()
            : MemberEditingOperationStatusResult(result.Status, requestModel, result.Result);
    }
}
