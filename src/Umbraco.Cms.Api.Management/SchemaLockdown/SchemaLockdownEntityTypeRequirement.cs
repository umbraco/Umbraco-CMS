using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
///     Authorization requirement for the <see cref="SchemaLockdownAuthorizationHandler" />.
/// </summary>
/// <remarks>
/// Carries only the entity type, because it is declared once for a whole controller. Which operation a given request
/// performs is not knowable until that request is being authorized, so the handler resolves it.
/// </remarks>
internal sealed class SchemaLockdownEntityTypeRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownEntityTypeRequirement"/> class.
    /// </summary>
    /// <param name="entityType">The entity type the action's controller manages.</param>
    public SchemaLockdownEntityTypeRequirement(string entityType) => EntityType = entityType;

    /// <summary>
    /// Gets the entity type the action's controller manages.
    /// </summary>
    public string EntityType { get; }
}
