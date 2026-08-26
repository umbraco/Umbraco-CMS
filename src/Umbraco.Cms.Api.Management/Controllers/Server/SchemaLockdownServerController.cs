using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

/// <summary>
/// API controller that provides the schema lockdown restrictions so the backoffice can reflect them.
/// </summary>
[ApiVersion("1.0")]
public class SchemaLockdownServerController : ServerControllerBase
{
    private readonly IReadOnlySchemaRestrictions _restrictions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownServerController"/> class, which provides the schema lockdown endpoint for the Umbraco management API.
    /// </summary>
    /// <param name="restrictions">The frozen decision table.</param>
    public SchemaLockdownServerController(IReadOnlySchemaRestrictions restrictions)
        => _restrictions = restrictions;

    /// <summary>
    /// Retrieves which schema operations are permitted for each entity type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="ServerSchemaLockdownResponseModel"/> with the resolved restrictions.</returns>
    [HttpGet("schema-lockdown")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerSchemaLockdownResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the schema lockdown state.")]
    [EndpointDescription("Gets which schema operations are permitted for each entity type.")]
    public Task<IActionResult> SchemaLockdown(CancellationToken cancellationToken)
    {
        var model = new ServerSchemaLockdownResponseModel
        {
            // The restrictions match entity types case-insensitively and report back whatever casing a configurator wrote.
            // The backoffice looks its own entity types up against this, so the casing is settled here instead.
            RestrictedEntityTypes = _restrictions.RestrictedEntityTypes
                .Select(entityType => new ServerSchemaLockdownEntityTypeResponseModel
                {
                    EntityType = entityType.ToLowerInvariant(),
                    Create = _restrictions.IsAllowed(entityType, SchemaOperation.Create),
                    Update = _restrictions.IsAllowed(entityType, SchemaOperation.Update),
                    Delete = _restrictions.IsAllowed(entityType, SchemaOperation.Delete),
                })
                .ToArray(),
        };

        return Task.FromResult<IActionResult>(Ok(model));
    }
}
