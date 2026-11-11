namespace Umbraco.Cms.Core.Permissions;

/// <summary>
/// Builds permission verbs, so every verb follows the same <c>{Resource}.{Operation}</c> format.
/// </summary>
/// <remarks>
/// Umbraco's own verbs are listed in <see cref="Constants.Permissions.Entities"/>. A package or product builds its verbs
/// from its own resource and one of <see cref="Constants.Permissions.Operations"/>, for example
/// <c>PermissionVerbs.For("My.Package.Widget", Constants.Permissions.Operations.Create)</c>, or an operation of its
/// own, such as <c>"Publish"</c>.
/// </remarks>
public static class PermissionVerbs
{
    /// <summary>
    /// Gets the permission verb for an operation.
    /// </summary>
    /// <param name="resource">
    /// The resource the verbs are named after, for example
    /// <see cref="Constants.Permissions.Entities.DocumentType.Resource"/>. A trailing dot is ignored.
    /// </param>
    /// <param name="operation">
    /// The operation, for example one of <see cref="Constants.Permissions.Operations"/>.
    /// </param>
    /// <returns>The permission verb, for example <c>Umb.DocumentType.Create</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="resource"/> or <paramref name="operation"/> is empty.</exception>
    public static string For(string resource, string operation)
    {
        var trimmed = resource?.TrimEnd('.');
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmed, nameof(resource));
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        return $"{trimmed}.{operation}";
    }
}
