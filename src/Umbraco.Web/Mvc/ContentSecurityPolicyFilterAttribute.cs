using System.Text;
using System.Web.Mvc;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Security;

namespace Umbraco.Web.Mvc
{
    internal class ContentSecurityPolicyFilterAttribute : ActionFilterAttribute
    {
        private readonly INonceProvider _nonceProvider;
        private readonly ISecuritySection _securitySection;

        public ContentSecurityPolicyFilterAttribute(INonceProvider nonceProvider,
            ISecuritySection securitySection)
        {
            _nonceProvider = nonceProvider;
            _securitySection = securitySection;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (_securitySection.ContentSecurityPolicy != null)
            {
                filterContext.HttpContext.Response.Headers.Add("Content-Security-Policy",
                    BuildCspHeader());
            }
        }

        private string BuildCspHeader()
        {
            var sb = new StringBuilder();
            var styleNonce = _securitySection.ContentSecurityPolicy.EnableNonce ? $"'nonce-{_nonceProvider.StyleNonce}'" : "";
            var scriptNonce = _securitySection.ContentSecurityPolicy.EnableNonce ? _nonceProvider.ScriptNonce : "";

            sb.Append(GetCspElementContent("default-src", _securitySection.ContentSecurityPolicy.DefaultSrc));
            if (_securitySection.ContentSecurityPolicy.StyleSrc != null)
            {
                sb.Append($"style-src: {_securitySection.ContentSecurityPolicy.StyleSrc} {styleNonce};");
            }
            if (_securitySection.ContentSecurityPolicy.ScriptSrc != null)
            {
                sb.Append($"script-src: {_securitySection.ContentSecurityPolicy.ScriptSrc} {scriptNonce};");
            }
            sb.Append(GetCspElementContent("font-src", _securitySection.ContentSecurityPolicy.FontSrc));
            sb.Append(GetCspElementContent("connect-src", _securitySection.ContentSecurityPolicy.ConnectSrc));
            sb.Append(GetCspElementContent("frame-src", _securitySection.ContentSecurityPolicy.FrameSrc));
            sb.Append(GetCspElementContent("img-src", _securitySection.ContentSecurityPolicy.ImgSrc));
            sb.Append(GetCspElementContent("manifest-src", _securitySection.ContentSecurityPolicy.ManifestSrc));
            sb.Append(GetCspElementContent("object-src", _securitySection.ContentSecurityPolicy.ObjectSrc));
            sb.Append(GetCspElementContent("prefetch-src", _securitySection.ContentSecurityPolicy.PrefetchSrc));
            sb.Append(GetCspElementContent("worker-src", _securitySection.ContentSecurityPolicy.PrefetchSrc));

            if (!string.IsNullOrEmpty(_securitySection.ContentSecurityPolicy.ReportUri))
            {
                sb.Append($"report-to: {_securitySection.ContentSecurityPolicy.ReportTo};");
            }
            if (!string.IsNullOrEmpty(_securitySection.ContentSecurityPolicy.ReportUri))
            {
                sb.Append($"report-uri: {_securitySection.ContentSecurityPolicy.ReportUri};");
            }
            return sb.ToString();
        }

        private string GetCspElementContent(string title, IContentSecurityPolicySrc cspElement)
        {
            return !string.IsNullOrEmpty(cspElement?.ToString())
                ? $"{title}: {cspElement};"
                : "";
        }

    }
}
