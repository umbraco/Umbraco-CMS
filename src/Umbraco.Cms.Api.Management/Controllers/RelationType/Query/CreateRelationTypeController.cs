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

public class CreateRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationTypeViewModelFactory _relationTypeViewModelFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateRelationTypeController(IRelationService relationService,
        IRelationTypeViewModelFactory relationTypeViewModelFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _relationService = relationService;
        _relationTypeViewModelFactory = relationTypeViewModelFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(RelationTypeSavingViewModel relationTypeSavingViewModel)
    {
        IRelationType relationTypePersisted = _relationTypeViewModelFactory.CreateRelationType(relationTypeSavingViewModel);

        Attempt<IRelationType, RelationTypeOperationStatus> result = await _relationService.CreateAsync(relationTypePersisted, CurrentUserId(_backOfficeSecurityAccessor));

        return result.Success
            ? await Task.FromResult(CreatedAtAction<ByKeyRelationTypeController>(controller => nameof(controller.ByKey), relationTypePersisted.Key))
            : RelationTypeOperationStatusResult(result.Status);
    }
}
