using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.IO;

/// <summary>
/// Provides helper methods for working with template view files.
/// </summary>
public interface IViewHelper
{
    /// <summary>
    /// Determines whether a view file exists for the specified template.
    /// </summary>
    /// <param name="t">The template to check.</param>
    /// <returns><c>true</c> if the view file exists; otherwise, <c>false</c>.</returns>
    bool ViewExists(ITemplate t);

    /// <summary>
    /// Gets the file contents of the specified template's view file.
    /// </summary>
    /// <param name="t">The template to get contents for.</param>
    /// <returns>The contents of the view file, or an empty string if the file does not exist.</returns>
    string GetFileContents(ITemplate t);

    /// <summary>
    /// Creates a view file for the specified template.
    /// </summary>
    /// <param name="t">The template to create a view for.</param>
    /// <param name="overWrite">Whether to overwrite an existing view file.</param>
    /// <returns>The contents of the view file.</returns>
    string CreateView(ITemplate t, bool overWrite = false);

    /// <summary>
    /// Updates the view file for the specified template.
    /// </summary>
    /// <param name="t">The template to update the view for.</param>
    /// <param name="currentAlias">The current alias of the template, used to rename the file if the alias has changed.</param>
    /// <returns>The content of the updated view file.</returns>
    string? UpdateViewFile(ITemplate t, string? currentAlias = null);

    /// <summary>
    /// Gets the relative path to a view file based on the template alias.
    /// </summary>
    /// <param name="alias">The template alias.</param>
    /// <returns>The relative path to the view file.</returns>
    string ViewPath(string alias);
}
