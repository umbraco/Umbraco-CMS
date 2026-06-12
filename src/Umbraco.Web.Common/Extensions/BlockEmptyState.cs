using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

/// <summary>
/// Produces the annotated empty container the visual editor uses to offer an "add content"
/// affordance on an empty, editable block property. Returns empty content outside the visual editor.
/// </summary>
internal static class BlockEmptyState
{
    /// <summary>
    /// Returns an annotated empty container (<c>&lt;div class="{cssClass}" data-umb-block-property="{alias}"&gt;</c>)
    /// when the property is editable in the visual editor and the visual editor is active; otherwise empty content.
    /// </summary>
    public static IHtmlContent Container(string cssClass, string propertyAlias, bool editableInVisualEditor)
    {
        if (!editableInVisualEditor
            || string.IsNullOrEmpty(propertyAlias)
            || !VisualEditorPropertyTracker.IsEnabled)
        {
            return HtmlString.Empty;
        }

        var encodedClass = HtmlEncoder.Default.Encode(cssClass);
        var encodedAlias = HtmlEncoder.Default.Encode(propertyAlias);
        return new HtmlString($"<div class=\"{encodedClass}\" data-umb-block-property=\"{encodedAlias}\"></div>");
    }
}
