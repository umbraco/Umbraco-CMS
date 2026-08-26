using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Declares which schema entity type a controller manages, and thereby authorizes every action on that controller
/// against the schema lockdown restrictions.
/// </summary>
/// <remarks>
/// Declaring the entity type is all a controller has to do: because the requirement is carried as authorization
/// metadata, a newly added endpoint is governed without anyone having to remember to tag it. Whether the entity type
/// is actually locked down is decided solely by the registered <see cref="ISchemaLockdownConfigurator"/> instances,
/// never by this attribute.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class SchemaEntityTypeAttribute : Attribute, IAuthorizationRequirementData, IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaEntityTypeAttribute"/> class.
    /// </summary>
    /// <param name="entityType">The entity type the controller manages.</param>
    public SchemaEntityTypeAttribute(string entityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        EntityType = entityType;
    }

    /// <summary>
    /// Gets the entity type the controller manages.
    /// </summary>
    public string EntityType { get; }

    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}
