using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Members;
using Umbraco.Core.Security;
using System.Threading;
using Umbraco.Core.Configuration.Models;


namespace Umbraco.Infrastructure.Members
{
    /// <summary>
    /// A manager for the Umbraco members identity implementation
    /// </summary>
    public class UmbracoMembersUserManager : UmbracoMembersUserManager<UmbracoMembersIdentityUser>, IUmbracoMembersUserManager
    {
        public UmbracoMembersUserManager(
            IUserStore<UmbracoMembersIdentityUser> store,
            IOptions<UmbracoMembersIdentityOptions> optionsAccessor,
            IPasswordHasher<UmbracoMembersIdentityUser> passwordHasher,
            IEnumerable<IUserValidator<UmbracoMembersIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<UmbracoMembersIdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<UmbracoMembersIdentityUser>> logger,
            IOptions<MemberPasswordConfigurationSettings> passwordConfiguration) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger, passwordConfiguration)
        {
        }
    }

    public class UmbracoMembersUserManager<T> : UserManager<T>
       where T : UmbracoMembersIdentityUser
    {
        public IPasswordConfiguration PasswordConfiguration { get; protected set; }

        private PasswordGenerator _passwordGenerator;

        public UmbracoMembersUserManager(
            IUserStore<T> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<T> passwordHasher,
            IEnumerable<IUserValidator<T>> userValidators,
            IEnumerable<IPasswordValidator<T>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<T>> logger,
            IOptions<MemberPasswordConfigurationSettings> passwordConfiguration) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            PasswordConfiguration = passwordConfiguration.Value ?? throw new ArgumentNullException(nameof(passwordConfiguration));
        }

        /// <summary>
        /// Replace the underlying options property with our own strongly typed version
        /// </summary>
        public new UmbracoMembersIdentityOptions Options
        {
            get => (UmbracoMembersIdentityOptions)base.Options;
            set => base.Options = value;
        }

        /// <summary>
        /// Gets/sets the default Umbraco member user password checker
        /// </summary>
        public IUmbracoMembersUserPasswordChecker UmbracoMembersUserPasswordChecker { get; set; }

        ///  <summary>
        /// [TODO: from BackOfficeUserManager duplicated, could be shared]
        ///  Override to determine how to hash the password
        ///  </summary>
        ///  <param name="memberUser"></param>
        ///  <param name="newPassword"></param>
        ///  <param name="validatePassword"></param>
        ///  <returns></returns>
        ///  <remarks>
        ///  This method is called anytime the password needs to be hashed for storage (i.e. including when reset password is used)
        ///  </remarks>
        protected override async Task<IdentityResult> UpdatePasswordHash(T memberUser, string newPassword, bool validatePassword)
        {
            memberUser.LastPasswordChangeDateUtc = DateTime.UtcNow;

            if (validatePassword)
            {
                IdentityResult validate = await ValidatePasswordAsync(memberUser, newPassword);
                if (!validate.Succeeded)
                {
                    return validate;
                }
            }

            var passwordStore = Store as IUserPasswordStore<T>;
            if (passwordStore == null) throw new NotSupportedException("The current user store does not implement " + typeof(IUserPasswordStore<>));

            var hash = newPassword != null ? PasswordHasher.HashPassword(memberUser, newPassword) : null;
            await passwordStore.SetPasswordHashAsync(memberUser, hash, CancellationToken);
            await UpdateSecurityStampInternal(memberUser);
            return IdentityResult.Success;
        }

        ///TODO: duplicated code from backofficeusermanager, could be shared?
        /// <summary>
        /// Logic used to validate a username and password
        /// </summary>
        /// <param name="member"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <remarks>
        /// By default this uses the standard ASP.Net Identity approach which is:
        /// * Get password store
        /// * Call VerifyPasswordAsync with the password store + user + password
        /// * Uses the PasswordHasher.VerifyHashedPassword to compare the stored password
        ///
        /// In some cases people want simple custom control over the username/password check, for simplicity
        /// sake, developers would like the users to simply validate against an LDAP directory but the user
        /// data remains stored inside of Umbraco.
        /// See: http://issues.umbraco.org/issue/U4-7032 for the use cases.
        ///
        /// We've allowed this check to be overridden with a simple callback so that developers don't actually
        /// have to implement/override this class.
        /// </remarks>
        public override async Task<bool> CheckPasswordAsync(T member, string password)
        {
            if (UmbracoMembersUserPasswordChecker != null)
            {
                UmbracoMembersUserPasswordCheckerResult result = await UmbracoMembersUserPasswordChecker.CheckPasswordAsync(member, password);

                if (member.HasIdentity == false)
                {
                    return false;
                }

                //if the result indicates to not fallback to the default, then return true if the credentials are valid
                if (result != UmbracoMembersUserPasswordCheckerResult.FallbackToDefaultChecker)
                {
                    return result == UmbracoMembersUserPasswordCheckerResult.ValidCredentials;
                }
            }

            //we cannot proceed if the user passed in does not have an identity
            if (member.HasIdentity == false)
                return false;

            //use the default behavior
            return await base.CheckPasswordAsync(member, password);
        }

        ///[TODO: from BackOfficeUserManager duplicated, could be shared]
        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task UpdateSecurityStampInternal(T user)
        {
            if (SupportsUserSecurityStamp == false) return;
            await GetSecurityStore().SetSecurityStampAsync(user, NewSecurityStamp(), CancellationToken.None);
        }

        ///[TODO: from BackOfficeUserManager duplicated, could be shared]
        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <returns></returns>
        private IUserSecurityStampStore<T> GetSecurityStore()
        {
            var store = Store as IUserSecurityStampStore<T>;
            if (store == null) throw new NotSupportedException("The current user store does not implement " + typeof(IUserSecurityStampStore<>));
            return store;
        }

        ///[TODO: from BackOfficeUserManager duplicated, could be shared]
        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <returns></returns>
        private static string NewSecurityStamp()
        {
            return Guid.NewGuid().ToString();
        }

        ///[TODO: from BackOfficeUserManager duplicated, could be shared]
        /// <summary>
        /// Helper method to generate a password for a member based on the current password validator
        /// </summary>
        /// <returns></returns>
        public string GeneratePassword()
        {
            if (_passwordGenerator == null)
            {
                _passwordGenerator = new PasswordGenerator(PasswordConfiguration);
            }
            string password = _passwordGenerator.GeneratePassword();
            return password;
        }
    }
}
