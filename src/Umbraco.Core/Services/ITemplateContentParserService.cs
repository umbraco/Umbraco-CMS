using System;

namespace Umbraco.Cms.Core.Services;

public interface ITemplateContentParserService
{
    /// <summary>
    ///     Parses the view content and extracts the layout alias from the Layout directive
    /// </summary>
    /// <param name="viewContent">The content of the Razor view</param>
    /// <returns>The layout alias if found, otherwise null</returns>
    string? LayoutAlias(string? viewContent);

    /// <summary>
    ///     Parses the view content and extracts the master template alias from the Layout directive
    /// </summary>
    /// <param name="viewContent">The content of the Razor view</param>
    /// <returns>The master template alias if found, otherwise null</returns>
    [Obsolete("Use LayoutAlias instead. This will be removed in Umbraco 19.")]
    string? MasterTemplateAlias(string? viewContent) => LayoutAlias(viewContent);
}
