using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security
{

    public sealed class ConfigureMemberIdentityOptions : IConfigureOptions<IdentityOptions>
    {
        private readonly MemberPasswordConfigurationSettings _memberPasswordConfiguration;

        public ConfigureMemberIdentityOptions(IOptions<MemberPasswordConfigurationSettings> memberPasswordConfiguration)
            => _memberPasswordConfiguration = memberPasswordConfiguration.Value;

        public void Configure(IdentityOptions options)
        {
            options.SignIn.RequireConfirmedAccount = true;          // uses our custom IUserConfirmation
            options.SignIn.RequireConfirmedEmail = false;           // not implemented
            options.SignIn.RequireConfirmedPhoneNumber = false;     // not implemented

            options.User.RequireUniqueEmail = true;

            options.Lockout.AllowedForNewUsers = true;
            // TODO: Implement this
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(30);

            options.Password.ConfigurePasswordOptions(_memberPasswordConfiguration);

            options.Lockout.MaxFailedAccessAttempts = _memberPasswordConfiguration.MaxFailedAccessAttemptsBeforeLockout;
        }
    }
}
