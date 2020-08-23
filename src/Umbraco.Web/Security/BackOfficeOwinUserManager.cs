using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Owin.Security.DataProtection;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Mapping;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Net;
using Umbraco.Web.Configuration;

namespace Umbraco.Web.Security
{
    public class BackOfficeOwinUserManager : BackOfficeUserManager
    {
        public const string OwinMarkerKey = "Umbraco.Web.Security.Identity.BackOfficeUserManagerMarker";

        public BackOfficeOwinUserManager(
            IOptions<BackOfficeIdentityOptions> passwordConfiguration,
            IIpResolver ipResolver,
            IUserStore<BackOfficeIdentityUser> store,
            IOptions<BackOfficeIdentityOptions> optionsAccessor,
            IEnumerable<IUserValidator<BackOfficeIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<BackOfficeIdentityUser>> passwordValidators,
            BackOfficeLookupNormalizer keyNormalizer,
            BackOfficeIdentityErrorDescriber errors,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<UserManager<BackOfficeIdentityUser>> logger)
            : base(ipResolver, store, optionsAccessor, null, userValidators, passwordValidators, keyNormalizer, errors, null, logger, ConfigModelConversions.ConvertToOptionsOfUserPasswordConfigurationSettings(passwordConfiguration))
        {
            PasswordConfiguration = ConfigModelConversions.ConvertToOptionsOfUserPasswordConfigurationSettings(passwordConfiguration).Value;
            InitUserManager(this, dataProtectionProvider);
        }

        #region Static Create methods

        /// <summary>
        /// Creates a BackOfficeUserManager instance with all default options and the default BackOfficeUserManager
        /// </summary>
        public static BackOfficeOwinUserManager Create(
            IUserService userService,
            IEntityService entityService,
            IExternalLoginService externalLoginService,
            IGlobalSettings globalSettings,
            UmbracoMapper mapper,
            IUserPasswordConfiguration passwordConfiguration,
            IIpResolver ipResolver,
            BackOfficeIdentityErrorDescriber errors,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<UserManager<BackOfficeIdentityUser>> logger)
        {
            var store = new BackOfficeUserStore(userService, entityService, externalLoginService, ConfigModelConversions.ConvertGlobalSettings(globalSettings), mapper);

            return Create(
                passwordConfiguration,
                ipResolver,
                store,
                errors,
                dataProtectionProvider,
                logger);
        }

        /// <summary>
        /// Creates a BackOfficeUserManager instance with all default options and a custom BackOfficeUserManager instance
        /// </summary>
        public static BackOfficeOwinUserManager Create(
            IUserPasswordConfiguration passwordConfiguration,
            IIpResolver ipResolver,
            IUserStore<BackOfficeIdentityUser> customUserStore,
            BackOfficeIdentityErrorDescriber errors,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<UserManager<BackOfficeIdentityUser>> logger)
        {
            var options = new BackOfficeIdentityOptions();

            // Configure validation logic for usernames
            var userValidators = new List<UserValidator<BackOfficeIdentityUser>> { new BackOfficeUserValidator<BackOfficeIdentityUser>() };
            options.User.RequireUniqueEmail = true;

            // Configure validation logic for passwords
            var passwordValidators = new List<IPasswordValidator<BackOfficeIdentityUser>> { new PasswordValidator<BackOfficeIdentityUser>() };
            options.Password.RequiredLength = passwordConfiguration.RequiredLength;
            options.Password.RequireNonAlphanumeric = passwordConfiguration.RequireNonLetterOrDigit;
            options.Password.RequireDigit = passwordConfiguration.RequireDigit;
            options.Password.RequireLowercase = passwordConfiguration.RequireLowercase;
            options.Password.RequireUppercase = passwordConfiguration.RequireUppercase;

            // Ensure Umbraco security stamp claim type is used
            options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
            options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
            options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            options.ClaimsIdentity.SecurityStampClaimType = Constants.Security.SecurityStampClaimType;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = passwordConfiguration.MaxFailedAccessAttemptsBeforeLockout;
            //NOTE: This just needs to be in the future, we currently don't support a lockout timespan, it's either they are locked
            // or they are not locked, but this determines what is set on the account lockout date which corresponds to whether they are
            // locked out or not.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(30);

            return new BackOfficeOwinUserManager(
                ConfigModelConversions.ConvertToOptionsOfBackOfficeIdentityOptions(passwordConfiguration),
                ipResolver,
                customUserStore,
                new OptionsWrapper<BackOfficeIdentityOptions>(options),
                userValidators,
                passwordValidators,
                new BackOfficeLookupNormalizer(),
                errors,
                dataProtectionProvider,
                logger);
        }

        #endregion

        protected override IPasswordHasher<BackOfficeIdentityUser> GetDefaultPasswordHasher(IPasswordConfiguration passwordConfiguration)
        {
            return new UserAwarePasswordHasher<BackOfficeIdentityUser>(new LegacyPasswordSecurity(passwordConfiguration));
        }

        protected void InitUserManager(BackOfficeOwinUserManager manager, IDataProtectionProvider dataProtectionProvider)
        {
            // use a custom hasher based on our membership provider
            PasswordHasher = GetDefaultPasswordHasher(PasswordConfiguration);

            // set OWIN data protection token provider as default
            if (dataProtectionProvider != null)
            {
                manager.RegisterTokenProvider(
                    TokenOptions.DefaultProvider,
                    new OwinDataProtectorTokenProvider<BackOfficeIdentityUser>(dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        TokenLifespan = TimeSpan.FromDays(3)
                    });
            }

            // register ASP.NET Core Identity token providers
            manager.RegisterTokenProvider(TokenOptions.DefaultEmailProvider, new EmailTokenProvider<BackOfficeIdentityUser>());
            manager.RegisterTokenProvider(TokenOptions.DefaultPhoneProvider, new PhoneNumberTokenProvider<BackOfficeIdentityUser>());
            manager.RegisterTokenProvider(TokenOptions.DefaultAuthenticatorProvider, new AuthenticatorTokenProvider<BackOfficeIdentityUser>());
        }
    }
}
