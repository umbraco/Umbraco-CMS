using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="RazorPage" />
/// </summary>
public static class RazorPageExtensions
{
    /// <summary>
    ///     Renders a section with default content if the section isn't defined
    /// </summary>
    public static HtmlString? RenderSection(this RazorPage webPage, string name, HtmlString defaultContents)
        => webPage.IsSectionDefined(name) ? webPage.RenderSection(name) : defaultContents;

    /// <summary>
    ///     Renders a section with default content if the section isn't defined
    /// </summary>
    public static HtmlString? RenderSection(this RazorPage webPage, string name, string defaultContents)
        => webPage.IsSectionDefined(name) ? webPage.RenderSection(name) : new HtmlString(defaultContents);
}
