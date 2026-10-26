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
    /// <returns>The resolved operation.</returns>
    /// <remarks>
    /// <see cref="SchemaOperation.Read"/> is only ever produced by an explicit allowlist of safe verbs. Anything
    /// unrecognised, or absent, resolves to <see cref="SchemaOperation.Unknown"/>, which is denied on every entity
    /// type the restrictions speak to, so a new or custom verb cannot bypass lockdown.
    /// </remarks>
    public static SchemaOperation Resolve(string? httpMethod)
        => httpMethod is null ? SchemaOperation.Unknown : FromVerb(httpMethod);

    /// <summary>
    /// Resolves the operation each of an action's entity types is subject to.
    /// </summary>
    /// <param name="entityTypes">The entity types the action's controller manages.</param>
    /// <param name="declared">The operations the action declares.</param>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <returns>
    /// Every entity type the controller declared, in the casing it declared it, paired with the operation it is
    /// subject to.
    /// </returns>
    /// <remarks>
    /// A declaration naming an entity type decides that one, then a declaration naming none covers whatever is left,
    /// then the verb. Enforcement and the OpenAPI document both resolve through here, so what the document says a
    /// request may be denied for cannot drift from what would actually deny it.
    /// <para>
    /// A <see cref="SchemaOperationAttribute"/> matches the entity type it names case-insensitively, but only decides
    /// that type's operation. The name itself comes from the controller's declaration, which is the one thing that
    /// says what the controller manages.
    /// </para>
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
