using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

public class UpdateRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IRelationTypeViewModelFactory _relationTypeViewModelFactory;

    public UpdateRelationTypeController(IRelationService relationService, IUmbracoMapper umbracoMapper, IRelationTypeViewModelFactory relationTypeViewModelFactory)
    {
        _relationService = relationService;
        _umbracoMapper = umbracoMapper;
        _relationTypeViewModelFactory = relationTypeViewModelFactory;
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

        _relationTypeViewModelFactory.MapUpdateModelToRelationType(relationTypeSavingViewModel, persistedRelationType);

        _relationService.Save(persistedRelationType);

        return await Task.FromResult(Ok());
    }
}
