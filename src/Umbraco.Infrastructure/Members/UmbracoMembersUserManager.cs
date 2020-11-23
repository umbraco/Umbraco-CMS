using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using Umbraco.Core.Members;

namespace Umbraco.Infrastructure.Members
{
    /// <summary>
    /// A manager for the Umbraco members identity implementation
    /// </summary>
    public class UmbracoMembersUserManager : UmbracoMembersUserManager<UmbracoMembersIdentityUser>, IUmbracoMembersUserManager
    {
        public UmbracoMembersUserManager(
            IUserStore<UmbracoMembersIdentityUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<UmbracoMembersIdentityUser> passwordHasher,
            IEnumerable<IUserValidator<UmbracoMembersIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<UmbracoMembersIdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<UmbracoMembersIdentityUser>> logger) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }
    }

    public class UmbracoMembersUserManager<T> : UserManager<T>
       where T : UmbracoMembersIdentityUser
    {
        public UmbracoMembersUserManager(
            IUserStore<T> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<T> passwordHasher,
            IEnumerable<IUserValidator<T>> userValidators,
            IEnumerable<IPasswordValidator<T>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<T>> logger) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }
    }
}
