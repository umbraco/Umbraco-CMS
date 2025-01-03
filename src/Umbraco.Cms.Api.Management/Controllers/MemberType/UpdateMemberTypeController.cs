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

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class UpdateMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeEditingPresentationFactory _memberTypeEditingPresentationFactory;
    private readonly IMemberTypeEditingService _memberTypeEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMemberTypeService _memberTypeService;

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

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
