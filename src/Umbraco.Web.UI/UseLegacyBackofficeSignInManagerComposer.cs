using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Web.UI;

public class UseLegacyBackofficeSignInManagerComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<BackOfficeAuthenticationTypeSettings>(options =>
        {
            options.AuthenticationType = Constants.Security.BackOfficeAuthenticationType;
            options.ExternalAuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType;
            options.TwoFactorAuthenticationType = Constants.Security.BackOfficeTwoFactorAuthenticationType;
            options.TwoFactorRememberMeAuthenticationType = Constants.Security.BackOfficeTwoFactorRememberMeAuthenticationType;
        });
    }
}
