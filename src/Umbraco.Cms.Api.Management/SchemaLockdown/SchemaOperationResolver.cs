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
    /// Resolves the operations each of an action's entity types is subject to.
    /// </summary>
    /// <param name="entityTypes">The entity types the action's controller manages.</param>
    /// <param name="declared">The operations the action declares.</param>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <returns>Each entity type the controller declared, paired with each of its operations.</returns>
    /// <remarks>
    /// Declarations naming the entity type win, then declarations naming none, then the HTTP verb. An entity type's
    /// operations are ordered by value, so which denial is reported doesn't depend on the order attributes are read in.
    /// Both enforcement and the OpenAPI document use this, so they can't disagree.
    /// </remarks>
    public static (string EntityType, SchemaOperation Operation)[] Resolve(
        IReadOnlyList<string> entityTypes,
        IReadOnlyList<SchemaOperationAttribute> declared,
        string? httpMethod)
        => entityTypes
            .SelectMany(entityType => For(entityType, declared, httpMethod)
                .Select(operation => (EntityType: entityType, Operation: operation)))
            .ToArray();

    private static SchemaOperation[] For(
        string entityType,
        IReadOnlyList<SchemaOperationAttribute> declared,
        string? httpMethod)
    {
        SchemaOperation[] named =
            Operations(declared, x => string.Equals(x.EntityType, entityType, StringComparison.OrdinalIgnoreCase));
        if (named.Length > 0)
        {
            return named;
        }

        SchemaOperation[] general = Operations(declared, x => x.EntityType is null);
        return general.Length > 0 ? general : [Resolve(httpMethod)];
    }

    private static SchemaOperation[] Operations(
        IReadOnlyList<SchemaOperationAttribute> declared,
        Func<SchemaOperationAttribute, bool> applies)
        => declared.Where(applies).Select(x => x.Operation).Distinct().Order().ToArray();

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
