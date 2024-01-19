using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Web.UI;

public class UseLegacyBackofficeSignInManagerComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
            builder.Services.RemoveAll<IBackOfficeSignInManager>();
            builder.Services.RemoveAll<BackOfficeSignInManager>();
            builder.BuildUmbracoBackOfficeIdentity().AddSignInManager<IBackOfficeSignInManager, LegacyBackOfficeSignInManager>();
            builder.BuildUmbracoBackOfficeIdentity().AddSignInManager<BackOfficeSignInManager, LegacyBackOfficeSignInManager>();
    }
}

public class LegacyBackOfficeSignInManager : BackOfficeSignInManager
{
    protected override string AuthenticationType => Constants.Security.BackOfficeAuthenticationType;

    protected override string ExternalAuthenticationType => Constants.Security.BackOfficeExternalAuthenticationType;

    protected override string TwoFactorAuthenticationType => Constants.Security.BackOfficeTwoFactorAuthenticationType;

    protected override string TwoFactorRememberMeAuthenticationType =>
        Constants.Security.BackOfficeTwoFactorRememberMeAuthenticationType;

    public LegacyBackOfficeSignInManager(BackOfficeUserManager userManager, IHttpContextAccessor contextAccessor, IBackOfficeExternalLoginProviders externalLogins, IUserClaimsPrincipalFactory<BackOfficeIdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, IOptions<GlobalSettings> globalSettings, ILogger<SignInManager<BackOfficeIdentityUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<BackOfficeIdentityUser> confirmation, IEventAggregator eventAggregator, IOptions<SecuritySettings> securitySettings) : base(userManager, contextAccessor, externalLogins, claimsFactory, optionsAccessor, globalSettings, logger, schemes, confirmation, eventAggregator, securitySettings)
    {
    }

    public LegacyBackOfficeSignInManager(BackOfficeUserManager userManager, IHttpContextAccessor contextAccessor, IBackOfficeExternalLoginProviders externalLogins, IUserClaimsPrincipalFactory<BackOfficeIdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, IOptions<GlobalSettings> globalSettings, ILogger<SignInManager<BackOfficeIdentityUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<BackOfficeIdentityUser> confirmation, IEventAggregator eventAggregator) : base(userManager, contextAccessor, externalLogins, claimsFactory, optionsAccessor, globalSettings, logger, schemes, confirmation, eventAggregator)
    {
    }

    public LegacyBackOfficeSignInManager(BackOfficeUserManager userManager, IHttpContextAccessor contextAccessor, IBackOfficeExternalLoginProviders externalLogins, IUserClaimsPrincipalFactory<BackOfficeIdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, IOptions<GlobalSettings> globalSettings, ILogger<SignInManager<BackOfficeIdentityUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<BackOfficeIdentityUser> confirmation) : base(userManager, contextAccessor, externalLogins, claimsFactory, optionsAccessor, globalSettings, logger, schemes, confirmation)
    {
    }
}
