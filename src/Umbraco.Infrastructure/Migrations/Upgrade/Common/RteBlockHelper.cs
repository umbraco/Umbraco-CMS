using System.Text.RegularExpressions;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.Common;

/// <summary>
/// Provides helper methods for processing rich text editor (RTE) blocks containing local links during the upgrade to Umbraco version 15.0.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public static partial class RteBlockHelper
{
    /// <summary>
    /// Returns a <see cref="Regex"/> that matches <c>umb-rte-block</c> elements containing a <c>data-content-udi</c> attribute in the input HTML.
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance for identifying <c>umb-rte-block</c> elements with a <c>data-content-udi</c> attribute.</returns>
    // Non-greedy on both [^>]*? and .*? so consecutive sibling <umb-rte-block> elements are matched
    // individually rather than collapsed into one span (which left all-but-last sibling UDIs un-converted).
    [GeneratedRegex("<umb-rte-block\\b[^>]*?(?<attribute>data-content-udi)=\"(?<udi>[^\"]+)\"[^>]*>.*?<\\/umb-rte-block>")]
    public static partial Regex BlockRegex();

    /// <summary>
    /// Rewrites every <c>&lt;umb-rte-block&gt;</c> element in <paramref name="markup"/> from the legacy
    /// <c>data-content-udi="umb://element/..."</c> form to the v15+ <c>data-content-key="&lt;guid&gt;"</c>
    /// form. Blocks whose UDI fails to parse are dropped (mirrors the original behaviour of
    /// <c>ConvertRichTextEditorProperties</c>).
    /// </summary>
    /// <param name="markup">The RTE markup to convert.</param>
    /// <returns>The converted markup, or the input unchanged if no convertible blocks are present.</returns>
    public static string ConvertBlockUdisToKeys(string markup) =>
        BlockRegex().Replace(
            markup,
            match => UdiParser.TryParse(match.Groups["udi"].Value, out GuidUdi? guidUdi)
                ? match.Value
                    .Replace(match.Groups["attribute"].Value, "data-content-key")
                    .Replace(match.Groups["udi"].Value, guidUdi.Guid.ToString("D"))
                : string.Empty);
}
