using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Extensions;

public static class HtmlEncodedStringExtensions
{
    /// <summary>
    /// Checks if the specified <see cref="IHtmlEncodedString" /> is <c>null</c> or only contains whitespace, optionally after all HTML tags have been stripped/removed.
    /// </summary>
    /// <param name="htmlEncodedString">The encoded HTML string.</param>
    /// <param name="stripHtml">If set to <c>true</c> strips/removes all HTML tags.</param>
    /// <returns>
    /// Returns <c>true</c> if the HTML string is <c>null</c> or only contains whitespace, optionally after all HTML tags have been stripped/removed.
    /// </returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this IHtmlEncodedString? htmlEncodedString, bool stripHtml = false)
        => (htmlEncodedString?.ToHtmlString() is var htmlString && string.IsNullOrWhiteSpace(htmlString)) ||
           (stripHtml && string.IsNullOrWhiteSpace(htmlString.StripHtml()));
}
