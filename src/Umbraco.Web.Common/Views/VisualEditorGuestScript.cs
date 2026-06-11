namespace Umbraco.Cms.Web.Common.Views;

/// <summary>
/// Generates the script tag for the visual editor guest script.
/// The script is built from <c>src/apps/visual-editor/injected.ts</c> in the backoffice client
/// and served from the backoffice static assets under <c>apps/visual-editor/injected.js</c>.
/// </summary>
internal static class VisualEditorGuestScript
{
    /// <summary>
    /// Builds the script tag referencing the visual editor guest script asset.
    /// </summary>
    /// <param name="nonce">The CSP nonce for the current request, or <c>null</c>/empty when CSP is not in use.</param>
    /// <param name="backOfficePath">The (cache-busted) backoffice assets path the script is served under.</param>
    /// <returns>The <c>&lt;script&gt;</c> tag markup.</returns>
    public static string GetScriptTag(string? nonce, string backOfficePath = "/umbraco/backoffice")
    {
        var nonceAttr = string.IsNullOrEmpty(nonce) ? string.Empty : $" nonce=\"{nonce}\"";
        return $"<script data-umb-visual-editor src=\"{backOfficePath}/apps/visual-editor/injected.js\"{nonceAttr}></script>";
    }
}
