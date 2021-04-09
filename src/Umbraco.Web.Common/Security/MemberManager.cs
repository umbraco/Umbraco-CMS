using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;


namespace Umbraco.Cms.Web.Common.Security
{

    public class MemberManager : UmbracoUserManager<MemberIdentityUser, MemberPasswordConfigurationSettings>, IMemberManager
    {
        public MemberManager(
            IIpResolver ipResolver,
            IUserStore<MemberIdentityUser> store,
            IOptions<MemberIdentityOptions> optionsAccessor,
            IPasswordHasher<MemberIdentityUser> passwordHasher,
            IEnumerable<IUserValidator<MemberIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<MemberIdentityUser>> passwordValidators,
            BackOfficeIdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<MemberIdentityUser>> logger,
            IOptions<MemberPasswordConfigurationSettings> passwordConfiguration)
            : base(ipResolver, store, optionsAccessor, passwordHasher, userValidators, passwordValidators, errors,
                services, logger, passwordConfiguration)
        {
        }

        public bool IsMemberAuthorized(IEnumerable<string> allowTypes = null, IEnumerable<string> allowGroups = null, IEnumerable<int> allowMembers = null)
            => true; // TODO: Implement!
    }
}
