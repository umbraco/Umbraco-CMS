using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.TagHelpers;

/// <summary>
/// Injects the front-end preview badge into the <c>&lt;body&gt;</c> element while the current request is in preview mode.
/// </summary>
/// <remarks>
/// This runs for every Razor view, regardless of the page's base class, so the badge is rendered even when the
/// <c>&lt;body&gt;</c> element lives in a layout that does not inherit <see cref="Views.UmbracoViewPage" />
/// (see https://github.com/umbraco/Umbraco-CMS/issues/23505).
/// </remarks>
public class PreviewBadgeTagHelperComponent : TagHelperComponent
{
    private const string BodyTagName = "body";

    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IOptionsMonitor<ContentSettings> _contentSettings;
    private readonly ICspNonceService? _cspNonceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewBadgeTagHelperComponent" /> class.
    /// </summary>
    public PreviewBadgeTagHelperComponent(
        IUmbracoContextAccessor umbracoContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<ContentSettings> contentSettings,
        ICspNonceService? cspNonceService = null)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _httpContextAccessor = httpContextAccessor;
        _hostingEnvironment = hostingEnvironment;
        _contentSettings = contentSettings;
        _cspNonceService = cspNonceService;
    }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (output.TagName?.InvariantEquals(BodyTagName) is not true)
        {
            return;
        }

        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Response.ContentType?.InvariantContains("text/html") is not true)
        {
            return;
        }

        if (TryGetPreviewingUmbracoContext(out IUmbracoContext? umbracoContext) is false)
        {
            return;
        }

        var badge = _contentSettings.CurrentValue.PreviewBadge;
        if (badge.IsNullOrWhiteSpace())
        {
            return;
        }

        output.PostContent.AppendHtml(string.Format(
            badge,
            _hostingEnvironment.GetBackOfficePath(),

            // Belt and braces - via a browser at least it doesn't seem possible to have anything other than
            // a valid culture code provided in the querystring of this URL. But just to be sure of prevention
            // of an XSS vulnerability we'll HTML encode here too. An expected URL is untouched by this encoding.
            System.Web.HttpUtility.HtmlEncode(httpContext.Request.GetEncodedUrl()),
            umbracoContext.PublishedRequest?.PublishedContent?.Key,
            _cspNonceService?.GetNonceAttribute() ?? string.Empty));
    }

    private bool TryGetPreviewingUmbracoContext([NotNullWhen(true)] out IUmbracoContext? umbracoContext)
        => _umbracoContextAccessor.TryGetUmbracoContext(out umbracoContext)
           && umbracoContext.InPreviewMode
           && umbracoContext.PublishedRequest is not null;
}
