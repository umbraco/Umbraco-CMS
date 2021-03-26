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
    public class MemberManager : UmbracoUserManager<MembersIdentityUser, MemberPasswordConfigurationSettings>, IMemberManager
    {

        public MemberManager(
            IIpResolver ipResolver,
            IUserStore<MembersIdentityUser> store,
            IOptions<MembersIdentityOptions> optionsAccessor,
            IPasswordHasher<MembersIdentityUser> passwordHasher,
            IEnumerable<IUserValidator<MembersIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<MembersIdentityUser>> passwordValidators,
            BackOfficeIdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<MembersIdentityUser>> logger,
            IOptions<MemberPasswordConfigurationSettings> passwordConfiguration)
            : base(ipResolver, store, optionsAccessor, passwordHasher, userValidators, passwordValidators, errors, services, logger, passwordConfiguration)
        {
        }
    }
}
