using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Relation;

namespace Umbraco.Cms.Api.Management.Controllers.Relation;

[ApiVersion("1.0")]
public class ByIdRelationController : RelationControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationPresentationFactory _relationPresentationFactory;

    public ByIdRelationController(IRelationService relationService, IRelationPresentationFactory relationPresentationFactory)
    {
        _relationService = relationService;
        _relationPresentationFactory = relationPresentationFactory;
    }

    [HttpGet("{id:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RelationResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ById(int id)
    {
        IRelation? relation = _relationService.GetById(id);
        if (relation is null)
        {
            return RelationNotFound();
        }

        return await Task.FromResult(Ok(_relationPresentationFactory.Create(relation)));
    }
}
