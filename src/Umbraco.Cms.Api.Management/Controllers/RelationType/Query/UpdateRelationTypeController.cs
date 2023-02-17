using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

public class UpdateRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public UpdateRelationTypeController(IRelationService relationService, IUmbracoMapper umbracoMapper)
    {
        _relationService = relationService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RelationTypeViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid key, RelationTypeUpdatingViewModel relationTypeSavingViewModel)
    {
        IRelationType? persistedRelationType = _relationService.GetRelationTypeById(key);

        if (persistedRelationType is null)
        {
            ProblemDetails problemDetails = new ProblemDetailsBuilder()
                .WithTitle("Could not find relation type")
                .WithDetail($"Relation type with key {key} could not be found")
                .Build();
            return NotFound(problemDetails);
        }

        IRelationType updated = _umbracoMapper.Map(relationTypeSavingViewModel, persistedRelationType);

        _relationService.Save(updated);

        RelationTypeViewModel relationTypeViewModel = _umbracoMapper.Map<RelationTypeViewModel>(persistedRelationType)!;

        return await Task.FromResult(Ok(relationTypeViewModel));
    }
}
