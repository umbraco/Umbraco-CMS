namespace Umbraco.Cms.Core.Runtime;

/// <summary>
/// Provides permission checking for Umbraco boot operations.
/// </summary>
/// <remarks>
/// Implementations of this interface verify that the application has the necessary
/// permissions to start up correctly, such as file system access.
/// </remarks>
public interface IUmbracoBootPermissionChecker
{
    /// <summary>
    /// Throws an exception if the application does not have the required permissions to boot.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the application lacks required permissions for startup.
    /// </exception>
    void ThrowIfNotPermissions();
}
