namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for parsing template content to extract layout information.
/// </summary>
public interface ITemplateContentParserService
{
    /// <summary>
    /// Extracts the master template alias from the view content.
    /// </summary>
    /// <param name="viewContent">The view content to parse.</param>
    /// <returns>The alias of the master template, or <c>null</c> if not found.</returns>
    string? MasterTemplateAlias(string? viewContent);
}
