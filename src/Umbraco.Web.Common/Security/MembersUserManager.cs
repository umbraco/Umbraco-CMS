using System;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Extensions;
using Umbraco.Infrastructure.Security;
using Umbraco.Net;
using Umbraco.Web.Models.ContentEditing;


namespace Umbraco.Web.Common.Security
{
    public class MembersUserManager : UmbracoUserManager<MembersIdentityUser, MemberPasswordConfigurationSettings>, IMembersUserManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MembersUserManager(
            IIpResolver ipResolver,
            IUserStore<MembersIdentityUser> store,
            IOptions<MembersIdentityOptions> optionsAccessor,
            IPasswordHasher<MembersIdentityUser> passwordHasher,
            IEnumerable<IUserValidator<MembersIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<MembersIdentityUser>> passwordValidators,
            BackOfficeIdentityErrorDescriber errors,
            IServiceProvider services,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserManager<MembersIdentityUser>> logger,
            IOptions<MemberPasswordConfigurationSettings> passwordConfiguration)
            : base(ipResolver, store, optionsAccessor, passwordHasher, userValidators, passwordValidators, errors, services, logger, passwordConfiguration)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId(IPrincipal currentUser)
        {
            UmbracoBackOfficeIdentity umbIdentity = currentUser?.GetUmbracoIdentity();
            var currentUserId = umbIdentity?.GetUserId<string>() ?? Core.Constants.Security.SuperUserIdAsString;
            return currentUserId;
        }

        private IdentityAuditEventArgs CreateArgs(AuditEvent auditEvent, IPrincipal currentUser, string affectedUserId, string affectedUsername)
        {
            var currentUserId = GetCurrentUserId(currentUser);
            var ip = IpResolver.GetCurrentRequestIpAddress();
            return new IdentityAuditEventArgs(auditEvent, ip, currentUserId, string.Empty, affectedUserId, affectedUsername);
        }

        //TODO: have removed all other member audit events - can revisit if we need member auditing on a user level in future

        public void RaiseForgotPasswordRequestedEvent(IPrincipal currentUser, string userId) => throw new NotImplementedException();

        public void RaiseForgotPasswordChangedSuccessEvent(IPrincipal currentUser, string userId) => throw new NotImplementedException();

        public SignOutAuditEventArgs RaiseLogoutSuccessEvent(IPrincipal currentUser, string userId) => throw new NotImplementedException();

        public UserInviteEventArgs RaiseSendingUserInvite(IPrincipal currentUser, UserInvite invite, IUser createdUser) => throw new NotImplementedException();

        public bool HasSendingUserInviteEventHandler { get; }
    }
}
