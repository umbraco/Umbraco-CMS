using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.TagHelpers;

/// <summary>
/// A tag helper that adds a CSP nonce attribute to script and style elements.
/// </summary>
/// <remarks>
/// Use the <c>asp-csp-nonce</c> attribute on script or style elements to automatically
/// inject the CSP nonce for the current request.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// <script asp-csp-nonce src="~/scripts/app.js"></script>
/// <style asp-csp-nonce>body { color: red; }</style>
/// ]]></code>
/// </example>
[HtmlTargetElement("script", Attributes = AttributeName)]
[HtmlTargetElement("style", Attributes = AttributeName)]
public class CspNonceTagHelper : TagHelper
{
    private const string AttributeName = "asp-csp-nonce";

    private readonly ICspNonceService _cspNonceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CspNonceTagHelper"/> class.
    /// </summary>
    /// <param name="cspNonceService">The CSP nonce service.</param>
    public CspNonceTagHelper(ICspNonceService cspNonceService)
        => _cspNonceService = cspNonceService;

    /// <summary>
    /// Gets or sets a value indicating whether to add the CSP nonce.
    /// </summary>
    /// <remarks>
    /// This attribute is only used as a marker. The presence of the attribute
    /// triggers the tag helper; the value is ignored.
    /// </remarks>
    [HtmlAttributeName(AttributeName)]
    public bool AddNonce { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var nonce = _cspNonceService.GetNonce();

        if (!string.IsNullOrEmpty(nonce))
        {
            output.Attributes.SetAttribute("nonce", nonce);
        }
    }
}
