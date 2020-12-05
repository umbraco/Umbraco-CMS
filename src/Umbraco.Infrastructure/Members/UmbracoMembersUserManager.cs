using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Members;
using Umbraco.Core.Security;


namespace Umbraco.Infrastructure.Members
{
    /// <summary>
    /// A manager for the Umbraco members identity implementation
    /// </summary>
    public class UmbracoMembersUserManager : UmbracoMembersUserManager<UmbracoMembersIdentityUser>, IUmbracoMembersUserManager
    {
        ///<inheritdoc />
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

    /// <summary>
    /// Manager for the member identity user
    /// </summary>
    /// <typeparam name="T">The identity user</typeparam>
    public class UmbracoMembersUserManager<T> : UserManager<T>
       where T : UmbracoMembersIdentityUser
    {
        private PasswordGenerator _passwordGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoMembersUserManager"/> class.
        /// </summary>
        /// <param name="store">The members store</param>
        /// <param name="optionsAccessor">The identity options accessor</param>
        /// <param name="passwordHasher">The password hasher</param>
        /// <param name="userValidators">The user validators</param>
        /// <param name="passwordValidators">The password validators</param>
        /// <param name="keyNormalizer">The keep lookup normalizer</param>
        /// <param name="errors">The error display messages</param>
        /// <param name="services">The service provider</param>
        /// <param name="logger">The logger</param>
        /// <param name="passwordConfiguration">The password configuration</param>
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
            IOptions<MemberPasswordConfigurationSettings> passwordConfiguration) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) =>
            PasswordConfiguration = passwordConfiguration.Value ?? throw new ArgumentNullException(nameof(passwordConfiguration));


        /// <summary>
        /// Gets or sets the password configuration
        /// </summary>
        public IPasswordConfiguration PasswordConfiguration { get; protected set; }

        /// <summary>
        /// gets or sets the underlying options property with our own strongly typed version
        /// </summary>
        public new UmbracoMembersIdentityOptions Options
        {
            get => (UmbracoMembersIdentityOptions)base.Options;
            set => base.Options = value;
        }

        /// <summary>
        /// Gets or sets the default Umbraco member user password checker
        /// </summary>
        public IUmbracoMembersUserPasswordChecker UmbracoMembersUserPasswordChecker { get; set; }

        /// <summary>
        /// TODO: from BackOfficeUserManager duplicated, could be shared
        /// Override to determine how to hash the password
        /// </summary>
        /// <param name="memberUser">The member to validate</param>
        /// <param name="newPassword">The new password</param>
        /// <param name="validatePassword">Whether to validate the password</param>
        /// <returns>The identity result of updating the password hash</returns>
        /// <remarks>
        /// This method is called anytime the password needs to be hashed for storage (i.e. including when reset password is used)
        /// </remarks>
        protected override async Task<IdentityResult> UpdatePasswordHash(T memberUser, string newPassword, bool validatePassword)
        {
            // memberUser.LastPasswordChangeDateUtc = DateTime.UtcNow;

            if (validatePassword)
            {
                IdentityResult validate = await ValidatePasswordAsync(memberUser, newPassword);
                if (!validate.Succeeded)
                {
                    return validate;
                }
            }

            if (!(Store is IUserPasswordStore<T> passwordStore))
            {
                throw new NotSupportedException("The current user store does not implement " + typeof(IUserPasswordStore<>));
            }

            var hash = newPassword != null ? PasswordHasher.HashPassword(memberUser, newPassword) : null;
            await passwordStore.SetPasswordHashAsync(memberUser, hash, CancellationToken);
            await UpdateSecurityStampInternal(memberUser);
            return IdentityResult.Success;
        }

        /// TODO: duplicated code from backofficeusermanager, could be shared
        /// <summary>
        /// Logic used to validate a username and password
        /// </summary>
        /// <param name="member">The member to validate</param>
        /// <param name="password">The password to validate</param>
        /// <returns>Whether the password is the correct password for this member</returns>
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

                // if the result indicates to not fallback to the default, then return true if the credentials are valid
                if (result != UmbracoMembersUserPasswordCheckerResult.FallbackToDefaultChecker)
                {
                    return result == UmbracoMembersUserPasswordCheckerResult.ValidCredentials;
                }
            }

            // we cannot proceed if the user passed in does not have an identity
            if (member.HasIdentity == false)
            {
                return false;
            }

            // use the default behavior
            return await base.CheckPasswordAsync(member, password);
        }

        /// TODO: from BackOfficeUserManager duplicated, could be shared
        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <param name="user">The user to update the security stamp for</param>
        /// <returns>Task returns</returns>
        private async Task UpdateSecurityStampInternal(T user)
        {
            if (SupportsUserSecurityStamp == false)
            {
                return;
            }

            await GetSecurityStore().SetSecurityStampAsync(user, NewSecurityStamp(), CancellationToken.None);
        }

        /// TODO: from BackOfficeUserManager duplicated, could be shared
        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <returns>Return a user security stamp</returns>
        private IUserSecurityStampStore<T> GetSecurityStore()
        {
            if (!(Store is IUserSecurityStampStore<T> store))
            {
                throw new NotSupportedException("The current user store does not implement " + typeof(IUserSecurityStampStore<>));
            }

            return store;
        }

        /// TODO: from BackOfficeUserManager duplicated, could be shared
        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <returns>Returns a new security stamp</returns>
        private static string NewSecurityStamp() => Guid.NewGuid().ToString();

        /// <summary>
        /// TODO: from BackOfficeUserManager duplicated, could be shared
        /// Helper method to generate a password for a member based on the current password validator
        /// </summary>
        /// <returns>The generated password</returns>
        public string GeneratePassword()
        {
            _passwordGenerator ??= new PasswordGenerator(PasswordConfiguration);
            string password = _passwordGenerator.GeneratePassword();
            return password;
        }

        /// <summary>
        /// Helper method to validate a password based on the current password validator
        /// </summary>
        /// <param name="password">The password to update</param>
        /// <returns>The validated password</returns>
        public async Task<List<IdentityResult>> ValidatePassword(string password)
        {
            var passwordValidators = new List<IdentityResult>();
            foreach(IPasswordValidator<T> validator in PasswordValidators)
            {
                IdentityResult result = await validator.ValidateAsync(this, null, password);
                passwordValidators.Add(result);
            }

            return passwordValidators;
        }
    }
}
