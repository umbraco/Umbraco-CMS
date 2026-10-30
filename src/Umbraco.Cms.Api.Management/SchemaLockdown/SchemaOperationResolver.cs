using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Infers the <see cref="SchemaOperation"/> implied by an HTTP verb.
/// </summary>
internal static class SchemaOperationResolver
{
    /// <summary>
    /// Resolves the operation inferred from an HTTP verb.
    /// </summary>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <returns>The resolved operation, or <see cref="SchemaOperation.Unknown"/> for an unrecognised verb.</returns>
    public static SchemaOperation Resolve(string? httpMethod)
        => httpMethod is null ? SchemaOperation.Unknown : FromVerb(httpMethod);

    /// <summary>
    /// Resolves the operation each of an action's entity types is subject to.
    /// </summary>
    /// <param name="entityTypes">The entity types the action's controller manages.</param>
    /// <param name="declared">The operations the action declares.</param>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <returns>Each entity type the controller declared, paired with its operation.</returns>
    /// <remarks>
    /// A declaration naming the entity type wins, then a declaration naming none, then the HTTP verb. Both enforcement
    /// and the OpenAPI document use this, so they can't disagree.
    /// </remarks>
    public static (string EntityType, SchemaOperation Operation)[] Resolve(
        IReadOnlyList<string> entityTypes,
        IReadOnlyList<SchemaOperationAttribute> declared,
        string? httpMethod)
        => entityTypes
            .Select(entityType => (EntityType: entityType, Operation: For(entityType, declared, httpMethod)))
            .ToArray();

    private static SchemaOperation For(
        string entityType,
        IReadOnlyList<SchemaOperationAttribute> declared,
        string? httpMethod)
    {
        SchemaOperationAttribute? match =
            declared.FirstOrDefault(x => string.Equals(x.EntityType, entityType, StringComparison.OrdinalIgnoreCase))
            ?? declared.FirstOrDefault(x => x.EntityType is null);

        return match?.Operation ?? Resolve(httpMethod);
    }

    private static SchemaOperation FromVerb(string httpMethod)
    {
        if (HttpMethods.IsGet(httpMethod) || HttpMethods.IsHead(httpMethod) || HttpMethods.IsOptions(httpMethod))
        {
            return SchemaOperation.Read;
        }

        if (HttpMethods.IsPost(httpMethod))
        {
            return SchemaOperation.Create;
        }

        if (HttpMethods.IsPut(httpMethod) || HttpMethods.IsPatch(httpMethod))
        {
            return SchemaOperation.Update;
        }

        if (HttpMethods.IsDelete(httpMethod))
        {
            return SchemaOperation.Delete;
        }

        return SchemaOperation.Unknown;
    }
}
