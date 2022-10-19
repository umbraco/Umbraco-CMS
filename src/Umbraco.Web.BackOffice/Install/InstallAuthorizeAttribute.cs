using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Install;

/// <summary>
/// Specifies the authorization filter that verifies whether the runtime level is <see cref="RuntimeLevel.Install" />, or <see cref="RuntimeLevel.Upgrade" /> and a user is logged in.
/// </summary>
public class InstallAuthorizeAttribute : TypeFilterAttribute
{
    public InstallAuthorizeAttribute()
        : base(typeof(InstallAuthorizeFilter))
    { }

    private class InstallAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly ILogger<InstallAuthorizeFilter> _logger;
        private readonly IRuntimeState _runtimeState;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHostingEnvironment _hostingEnvironment;

        public InstallAuthorizeFilter(IRuntimeState runtimeState, ILogger<InstallAuthorizeFilter> logger, LinkGenerator linkGenerator, IHostingEnvironment hostingEnvironment)
        {
            _runtimeState = runtimeState;
            _logger = logger;
            _linkGenerator = linkGenerator;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (_runtimeState.EnableInstaller() == false)
            {
                // Only authorize when the installer is enabled
                context.Result = new ForbidResult(new AuthenticationProperties()
                {
                    RedirectUri = _linkGenerator.GetBackOfficeUrl(_hostingEnvironment)
                });
            }
            else if (_runtimeState.Level == RuntimeLevel.Upgrade && (await context.HttpContext.AuthenticateBackOfficeAsync()).Succeeded == false)
            {
                // Redirect to authorize upgrade
                var authorizeUpgradePath = _linkGenerator.GetPathByAction(nameof(BackOfficeController.AuthorizeUpgrade), ControllerExtensions.GetControllerName<BackOfficeController>(), new
                {
                    area = Constants.Web.Mvc.BackOfficeArea,
                    redir = _linkGenerator.GetInstallerUrl()
                });
                context.Result = new LocalRedirectResult(authorizeUpgradePath ?? "/");
            }
        }
    }
}
