using System.Text.RegularExpressions;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

/// <summary>
/// Provides helper methods for processing rich text editor (RTE) block markup by rewriting block UDIs to keys.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public static partial class RteBlockHelper
{
    /// <summary>
    /// Returns a <see cref="Regex"/> that matches <c>umb-rte-block</c> and <c>umb-rte-block-inline</c>
    /// elements containing a <c>data-content-udi</c> attribute in the input HTML.
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance for identifying <c>umb-rte-block</c>/<c>umb-rte-block-inline</c> elements with a <c>data-content-udi</c> attribute.</returns>
    // Non-greedy on both [^>]*? and .*? so consecutive sibling blocks are matched individually rather
    // than collapsed into one span (which left all-but-last sibling UDIs un-converted). The (?:-inline)?
    // on both the opening and closing tag lets .*? stop at the nearest close of either variant, so a
    // mix of block and inline siblings never collapse together and isolated inline blocks still match.
    [GeneratedRegex("<umb-rte-block(?:-inline)?\\b[^>]*?(?<attribute>data-content-udi)=\"(?<udi>[^\"]+)\"[^>]*>.*?<\\/umb-rte-block(?:-inline)?>")]
    public static partial Regex BlockRegex();

    /// <summary>
    /// Rewrites every <c>&lt;umb-rte-block&gt;</c> element in <paramref name="markup"/> from the legacy
    /// <c>data-content-udi="umb://element/..."</c> form to the v15+ <c>data-content-key="&lt;guid&gt;"</c>
    /// form.
    /// </summary>
    /// <remarks>
    /// Blocks whose UDI fails to parse are <b>dropped</b> from the output rather than preserved.
    /// This mirrors the original behaviour of <c>ConvertRichTextEditorProperties</c> and should not be
    /// changed without considering migrated content that may contain malformed UDIs.
    /// </remarks>
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
