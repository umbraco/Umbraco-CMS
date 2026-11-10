using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Names the permission verb for an operation on an entity type, as declared on a governed endpoint.
/// </summary>
/// <remarks>
/// A verb is named <c>Umb.{EntityType}.{Operation}</c>, with the entity type in Pascal case: creating a
/// <see cref="Constants.UdiEntityType.DocumentType"/> is <c>Umb.DocumentType.Create</c>. The verbs of the entity types
/// Umbraco governs are listed in <see cref="Constants.Permissions"/>.
/// </remarks>
internal static class SchemaPermissions
{
    /// <summary>
    /// Gets the permission verb for the supplied operation on the supplied entity type.
    /// </summary>
    /// <param name="entityType">The entity type, for example one of <see cref="Constants.UdiEntityType"/>.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The permission verb, for example <c>Umb.DocumentType.Create</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="operation"/> is not a defined operation.</exception>
    public static string For(string entityType, SchemaOperation operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        if (Enum.IsDefined(operation) is false)
        {
            throw new ArgumentOutOfRangeException(nameof(operation), operation, "Only a defined operation has a permission verb.");
        }

        return $"Umb.{ToPascalCase(entityType)}.{operation}";
    }

    private static string ToPascalCase(string entityType)
        => string.Concat(
            entityType
                .Split('-', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
}
