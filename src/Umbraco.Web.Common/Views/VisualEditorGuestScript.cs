namespace Umbraco.Cms.Web.Common.Views;

/// <summary>
/// Generates the script tag for the visual editor guest script.
/// The script file is served from the backoffice static assets at /umbraco/backoffice/visual-editor-guest.js.
/// </summary>
internal static class VisualEditorGuestScript
{
    public static string GetScriptTag(string? nonce, string backOfficePath = "/umbraco/backoffice")
    {
        var nonceAttr = string.IsNullOrEmpty(nonce) ? string.Empty : $" nonce=\"{nonce}\"";
        return $"<script data-umb-visual-editor src=\"{backOfficePath}/apps/visual-editor/injected.js\"{nonceAttr}></script>";
    }
}
