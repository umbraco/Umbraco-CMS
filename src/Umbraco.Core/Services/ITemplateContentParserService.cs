namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for parsing template content to extract layout information.
/// </summary>
public interface ITemplateContentParserService
{
    /// <summary>
    /// Extracts the layout template alias from the view content.
    /// </summary>
    /// <param name="viewContent">The view content to parse.</param>
    /// <returns>The alias of the layout template, or <c>null</c> if not found.</returns>
    string? LayoutTemplateAlias(string? viewContent);

    /// <inheritdoc cref="LayoutTemplateAlias" />
    [Obsolete("Use LayoutTemplateAlias instead. Scheduled for removal in Umbraco 20.")]
    string? MasterTemplateAlias(string? viewContent) => LayoutTemplateAlias(viewContent);
}
