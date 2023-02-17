using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Services;


namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

public class CreateRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationTypeViewModelFactory _relationTypeViewModelFactory;

    public CreateRelationTypeController(IRelationService relationService, IRelationTypeViewModelFactory relationTypeViewModelFactory)
    {
        _relationService = relationService;
        _relationTypeViewModelFactory = relationTypeViewModelFactory;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(RelationTypeSavingViewModel relationTypeSavingViewModel)
    {
        Core.Models.RelationType relationTypePersisted = _relationTypeViewModelFactory.CreateRelationType(relationTypeSavingViewModel);

        _relationService.Save(relationTypePersisted);

        return await Task.FromResult(CreatedAtAction<ByKeyRelationTypeController>(controller => nameof(controller.ByKey), relationTypePersisted.Key));
    }
}
