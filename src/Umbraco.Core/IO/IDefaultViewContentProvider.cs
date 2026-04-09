namespace Umbraco.Cms.Core.IO;

/// <summary>
/// Provides default content for Razor view files.
/// </summary>
public interface IDefaultViewContentProvider
{
    /// <summary>
    /// Gets the default file content for a Razor view.
    /// </summary>
    /// <param name="layoutPageAlias">The optional alias of the layout page to inherit from.</param>
    /// <param name="modelClassName">The optional class name of the model.</param>
    /// <param name="modelNamespace">The optional namespace of the model.</param>
    /// <param name="modelNamespaceAlias">The optional alias for the model namespace (defaults to "ContentModels").</param>
    /// <returns>The default Razor view content as a string.</returns>
    string GetDefaultFileContent(string? layoutPageAlias = null, string? modelClassName = null, string? modelNamespace = null, string? modelNamespaceAlias = null);
}
