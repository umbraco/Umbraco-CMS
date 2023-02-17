using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Api.Management.ViewModels.Telemetry;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

public class ObjectTypesRelationTypeController : RelationTypeControllerBase
{
    private readonly IObjectTypeViewModelFactory _objectTypeViewModelFactory;

    public ObjectTypesRelationTypeController(IObjectTypeViewModelFactory objectTypeViewModelFactory) => _objectTypeViewModelFactory = objectTypeViewModelFactory;

    [HttpGet("object-types")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TelemetryViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObjectTypes(int skip = 0, int take = 100)
    {
        IEnumerable<ObjectTypeViewModel> objectTypes = _objectTypeViewModelFactory.Create();

        return await Task.FromResult(Ok(objectTypes.Skip(skip).Take(take)));
    }
}
