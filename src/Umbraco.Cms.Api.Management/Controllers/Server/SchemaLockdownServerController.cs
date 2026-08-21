using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

/// <summary>
/// API controller that provides the schema lockdown decision matrix so the backoffice can reflect it.
/// </summary>
[ApiVersion("1.0")]
public class SchemaLockdownServerController : ServerControllerBase
{
    private readonly ISchemaLockdownMatrixAccessor _matrixAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownServerController"/> class, which provides the schema lockdown endpoint for the Umbraco management API.
    /// </summary>
    /// <param name="matrixAccessor">Provides the frozen decision matrix.</param>
    public SchemaLockdownServerController(ISchemaLockdownMatrixAccessor matrixAccessor)
        => _matrixAccessor = matrixAccessor;

    /// <summary>
    /// Retrieves which schema operations are permitted for each entity type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="ServerSchemaLockdownResponseModel"/> with the decision matrix.</returns>
    [HttpGet("schema-lockdown")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerSchemaLockdownResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the schema lockdown state.")]
    [EndpointDescription("Gets which schema operations are permitted for each entity type.")]
    public Task<IActionResult> SchemaLockdown(CancellationToken cancellationToken)
    {
        SchemaLockdownMatrix matrix = _matrixAccessor.Matrix;

        ServerSchemaLockdownEntityTypeResponseModel[] entityTypes = SchemaEntityTypes.All
            .Select(entityType => new ServerSchemaLockdownEntityTypeResponseModel
            {
                EntityType = entityType,
                Create = matrix.IsAllowed(entityType, SchemaOperation.Create),
                Update = matrix.IsAllowed(entityType, SchemaOperation.Update),
                Delete = matrix.IsAllowed(entityType, SchemaOperation.Delete),
            })
            .ToArray();

        var model = new ServerSchemaLockdownResponseModel
        {
            Enabled = entityTypes.Any(entityType =>
                entityType.Create is false || entityType.Update is false || entityType.Delete is false),
            EntityTypes = entityTypes,
        };

        return Task.FromResult<IActionResult>(Ok(model));
    }
}
