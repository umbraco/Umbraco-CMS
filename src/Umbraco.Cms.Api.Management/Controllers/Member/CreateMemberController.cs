using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

[ApiVersion("1.0")]
public class CreateMemberController : MemberControllerBase
{
    private readonly IMemberEditingPresentationFactory _memberEditingPresentationFactory;
    private readonly IMemberEditingService _memberEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateMemberController(
        IMemberEditingPresentationFactory memberEditingPresentationFactory,
        IMemberEditingService memberEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _memberEditingPresentationFactory = memberEditingPresentationFactory;
        _memberEditingService = memberEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateMemberRequestModel createRequestModel)
    {
        MemberCreateModel model = _memberEditingPresentationFactory.MapCreateModel(createRequestModel);
        Attempt<MemberCreateResult, MemberEditingStatus> result = await _memberEditingService.CreateAsync(model, CurrentUser(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyMemberController>(controller => nameof(controller.ByKey), result.Result.Content!.Key)
            : MemberEditingStatusResult(result.Status);
    }
}
