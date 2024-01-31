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
public class UpdateMemberController : MemberControllerBase
{
    private readonly IMemberEditingService _memberEditingService;
    private readonly IMemberEditingPresentationFactory _memberEditingPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateMemberController(
        IMemberEditingService memberEditingService,
        IMemberEditingPresentationFactory memberEditingPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _memberEditingService = memberEditingService;
        _memberEditingPresentationFactory = memberEditingPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateMemberRequestModel updateRequestModel)
    {
        IMember? member = await _memberEditingService.GetAsync(id);
        if (member == null)
        {
            return MemberNotFound();
        }

        MemberUpdateModel model = _memberEditingPresentationFactory.MapUpdateModel(updateRequestModel);
        Attempt<IMember, MemberEditingStatus> result = await _memberEditingService.UpdateAsync(member, model, CurrentUser(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : MemberEditingStatusResult(result.Status);
    }
}
