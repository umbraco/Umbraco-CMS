using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

[ApiVersion("1.0")]
public class CreateRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateRelationTypeController(IRelationService relationService,
        IRelationTypePresentationFactory relationTypePresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _relationService = relationService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateRelationTypeRequestModel createRelationTypeRequestModel)
    {
        IRelationType relationTypePersisted = _relationTypePresentationFactory.CreateRelationType(createRelationTypeRequestModel);

        Attempt<IRelationType, RelationTypeOperationStatus> result = await _relationService.CreateAsync(relationTypePersisted, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? await Task.FromResult(CreatedAtAction<ByKeyRelationTypeController>(controller => nameof(controller.ByKey), relationTypePersisted.Key))
            : RelationTypeOperationStatusResult(result.Status);
    }
}
