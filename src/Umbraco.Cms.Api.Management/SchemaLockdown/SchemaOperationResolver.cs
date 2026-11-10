using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Resolves the permission verbs a governed action needs.
/// </summary>
internal static class SchemaOperationResolver
{
    /// <summary>
    /// Resolves the permission verbs an action needs.
    /// </summary>
    /// <param name="entityTypes">The entity types the action's controller manages.</param>
    /// <param name="declared">What the action declares, if anything.</param>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <returns>The declared verbs, or else the HTTP verb's change on each entity type, in order.</returns>
    /// <remarks>Both enforcement and the OpenAPI document use this, so they can't disagree.</remarks>
    public static string[] ResolvePermissions(
        IReadOnlyList<string> entityTypes,
        SchemaOperationAttribute? declared,
        string? httpMethod)
    {
        if (declared is not null)
        {
            return declared.Permissions.Distinct(StringComparer.Ordinal).ToArray();
        }

        return ResolveFromHttpMethod(httpMethod) is SchemaOperation operation
            ? entityTypes.Select(entityType => SchemaPermissions.For(entityType, operation)).ToArray()
            : [];
    }

    /// <summary>
    /// Resolves the change an HTTP verb makes.
    /// </summary>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <returns>
    /// <c>null</c> for GET, HEAD and OPTIONS, which change nothing. Otherwise the change: PUT, PATCH and any unrecognised
    /// verb are treated as <see cref="SchemaOperation.Update"/>.
    /// </returns>
    private static SchemaOperation? ResolveFromHttpMethod(string? httpMethod)
    {
        var method = httpMethod ?? string.Empty;

        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
        {
            return null;
        }

        if (HttpMethods.IsPost(method))
        {
            return SchemaOperation.Create;
        }

        if (HttpMethods.IsDelete(method))
        {
            return SchemaOperation.Delete;
        }

        return SchemaOperation.Update;
    }
}
