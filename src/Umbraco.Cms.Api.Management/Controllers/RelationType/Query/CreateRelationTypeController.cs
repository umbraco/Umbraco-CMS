using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

public class CreateRelationTypeController : RelationTypeControllerBase
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IRelationService _relationService;

    public CreateRelationTypeController(IShortStringHelper shortStringHelper, IRelationService relationService)
    {
        _shortStringHelper = shortStringHelper;
        _relationService = relationService;
    }

    [HttpPost("create")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RelationTypeViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(RelationTypeSavingViewModel relationTypeSavingViewModel)
    {
        var relationTypePersisted = new Core.Models.RelationType(
            relationTypeSavingViewModel.Name,
            relationTypeSavingViewModel.Name.ToSafeAlias(_shortStringHelper, true),
            relationTypeSavingViewModel.IsBidirectional,
            relationTypeSavingViewModel.ParentObjectType,
            relationTypeSavingViewModel.ChildObjectType,
            relationTypeSavingViewModel.IsDependency);

        _relationService.Save(relationTypePersisted);

        return await Task.FromResult(CreatedAtAction<ByKeyRelationTypeController>(controller => nameof(controller.ByKey), relationTypePersisted.Key));
    }
}
