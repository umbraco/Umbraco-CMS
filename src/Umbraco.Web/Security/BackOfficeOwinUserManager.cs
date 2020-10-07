using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Owin.Security.DataProtection;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Mapping;
using Umbraco.Core.Services;
using Umbraco.Net;

namespace Umbraco.Web.Security
{
    // TODO: Most of this is already migrated to netcore, there's probably not much more to go and then we can complete remove it
    public class BackOfficeOwinUserManager : BackOfficeUserManager
    {
        public const string OwinMarkerKey = "Umbraco.Web.Security.Identity.BackOfficeUserManagerMarker";

        public BackOfficeOwinUserManager(
            IOptions<UserPasswordConfigurationSettings> passwordConfiguration,
            IIpResolver ipResolver,
            IUserStore<BackOfficeIdentityUser> store,
            IOptions<BackOfficeIdentityOptions> optionsAccessor,
            IEnumerable<IUserValidator<BackOfficeIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<BackOfficeIdentityUser>> passwordValidators,
            BackOfficeLookupNormalizer keyNormalizer,
            BackOfficeIdentityErrorDescriber errors,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<UserManager<BackOfficeIdentityUser>> logger)
            : base(ipResolver, store, optionsAccessor, null, userValidators, passwordValidators, keyNormalizer, errors, null, logger, passwordConfiguration)
        {
            PasswordConfiguration = passwordConfiguration.Value;
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
            IOptions<GlobalSettings> globalSettings,
            UmbracoMapper mapper,
            IOptions<UserPasswordConfigurationSettings> passwordConfiguration,
            IIpResolver ipResolver,
            BackOfficeIdentityErrorDescriber errors,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<UserManager<BackOfficeIdentityUser>> logger)
        {
            var store = new BackOfficeUserStore(userService, entityService, externalLoginService, globalSettings, mapper);

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
            IOptions<UserPasswordConfigurationSettings> passwordConfiguration,
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
            options.Password.RequiredLength = passwordConfiguration.Value.RequiredLength;
            options.Password.RequireNonAlphanumeric = passwordConfiguration.Value.RequireNonLetterOrDigit;
            options.Password.RequireDigit = passwordConfiguration.Value.RequireDigit;
            options.Password.RequireLowercase = passwordConfiguration.Value.RequireLowercase;
            options.Password.RequireUppercase = passwordConfiguration.Value.RequireUppercase;

            // Ensure Umbraco security stamp claim type is used
            options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
            options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
            options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            options.ClaimsIdentity.SecurityStampClaimType = Constants.Security.SecurityStampClaimType;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = passwordConfiguration.Value.MaxFailedAccessAttemptsBeforeLockout;
            //NOTE: This just needs to be in the future, we currently don't support a lockout timespan, it's either they are locked
            // or they are not locked, but this determines what is set on the account lockout date which corresponds to whether they are
            // locked out or not.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(30);

            return new BackOfficeOwinUserManager(
                passwordConfiguration,
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
