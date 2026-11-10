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
    private readonly ISchemaRestrictions _restrictions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownServerController"/> class.
    /// </summary>
    /// <param name="restrictions">The schema lockdown restrictions.</param>
    public SchemaLockdownServerController(ISchemaRestrictions restrictions)
        => _restrictions = restrictions;

    /// <summary>
    /// Retrieves the permission verbs schema lockdown denies.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="ServerSchemaLockdownResponseModel"/> with the resolved restrictions.</returns>
    [HttpGet("schema-lockdown")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerSchemaLockdownResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the schema lockdown state.")]
    [EndpointDescription("Gets the permission verbs schema lockdown denies.")]
    public Task<IActionResult> SchemaLockdown(CancellationToken cancellationToken)
    {
        var model = new ServerSchemaLockdownResponseModel
        {
            DeniedPermissions = _restrictions.DeniedPermissions.Order(StringComparer.Ordinal).ToArray(),
        };

        return Task.FromResult<IActionResult>(Ok(model));
    }
}
