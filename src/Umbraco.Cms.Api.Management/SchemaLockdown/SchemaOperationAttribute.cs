using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Overrides the permission verbs an action needs.
/// </summary>
/// <remarks>
/// By default, the verbs are inferred from the HTTP verb and the entity types of the controller's
/// <see cref="SchemaEntityTypeAttribute"/>. With this attribute, the action needs exactly the declared verbs, for
/// example <c>[SchemaOperation([Constants.Permissions.Template.Create, Constants.Permissions.DocumentType.Update])]</c>,
/// or none, as <c>[SchemaOperation([])]</c>, for an action that changes nothing.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SchemaOperationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaOperationAttribute"/> class.
    /// </summary>
    /// <param name="permissions">The permission verbs the action needs, for example from <see cref="Constants.Permissions"/>.</param>
    public SchemaOperationAttribute(string[] permissions)
    {
        foreach (var permission in permissions)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        }

        Permissions = permissions;
    }

    /// <summary>
    /// Gets the permission verbs the action needs.
    /// </summary>
    public IReadOnlyList<string> Permissions { get; }
}
