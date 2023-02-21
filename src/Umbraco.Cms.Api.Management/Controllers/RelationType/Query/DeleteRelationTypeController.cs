using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

public class DeleteRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;

    public DeleteRelationTypeController(IRelationService relationService) => _relationService = relationService;

    [HttpDelete("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid key)
    {
        IRelationType? relationType = _relationService.GetRelationTypeById(key);

        if (relationType == null)
        {
            ProblemDetailsBuilder problemDetails = new ProblemDetailsBuilder()
                .WithTitle("Could not find relation type")
                .WithDetail($"Relation type with key {key} could not be found");
            return NotFound(problemDetails);
        }

        _relationService.Delete(relationType);

        return await Task.FromResult(Ok());
    }
}
