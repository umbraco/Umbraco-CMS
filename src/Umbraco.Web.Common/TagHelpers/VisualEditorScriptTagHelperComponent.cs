using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Views;

namespace Umbraco.Cms.Web.Common.TagHelpers;

/// <summary>
/// Injects the visual editor guest script into the <c>&lt;body&gt;</c> tag when in preview mode.
/// </summary>
/// <remarks>
/// Uses the standard ASP.NET Core <see cref="ITagHelperComponent"/> extensibility point to append
/// the guest script to the body tag. The script runs inside the preview iframe and communicates
/// with the backoffice via postMessage.
/// </remarks>
internal sealed class VisualEditorScriptTagHelperComponent : TagHelperComponent
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ICspNonceService _cspNonceService;
    private readonly IHostingEnvironment _hostingEnvironment;

    public VisualEditorScriptTagHelperComponent(
        IUmbracoContextAccessor umbracoContextAccessor,
        ICspNonceService cspNonceService,
        IHostingEnvironment hostingEnvironment)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _cspNonceService = cspNonceService;
        _hostingEnvironment = hostingEnvironment;
    }

    /// <summary>
    /// Run after other tag helper components (e.g. the preview badge).
    /// </summary>
    public override int Order => 100;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!string.Equals(output.TagName, "body", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext)
            || !umbracoContext.InPreviewMode)
        {
            return;
        }

        var nonce = _cspNonceService.GetNonce();
        var backOfficePath = _hostingEnvironment.GetBackOfficePath();
        output.PostContent.AppendHtml(VisualEditorGuestScript.GetScriptTag(nonce, backOfficePath));
    }
}
