using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Api.Management.Permissions;

/// <summary>
/// Resolves the permission verbs a governed action needs.
/// </summary>
internal static class RequiredPermissionsResolver
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
        RequiresPermissionsAttribute? declared,
        string? httpMethod)
    {
        if (declared is not null)
        {
            return declared.Permissions.Distinct(StringComparer.Ordinal).ToArray();
        }

        return ResolveFromHttpMethod(httpMethod) is PermissionOperation operation
            ? entityTypes.Select(entityType => EntityTypePermissions.For(entityType, operation)).ToArray()
            : [];
    }

    /// <summary>
    /// Resolves the change an HTTP verb makes.
    /// </summary>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <returns>
    /// <c>null</c> for GET, HEAD and OPTIONS, which change nothing. Otherwise the change: PUT, PATCH and any unrecognised
    /// verb are treated as <see cref="PermissionOperation.Update"/>.
    /// </returns>
    private static PermissionOperation? ResolveFromHttpMethod(string? httpMethod)
    {
        var method = httpMethod ?? string.Empty;

        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
        {
            return null;
        }

        if (HttpMethods.IsPost(method))
        {
            return PermissionOperation.Create;
        }

        if (HttpMethods.IsDelete(method))
        {
            return PermissionOperation.Delete;
        }

        return PermissionOperation.Update;
    }
}
