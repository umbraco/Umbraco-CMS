using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Members;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Infrastructure.Members
{
    /// <summary>
    /// A custom user store that uses Umbraco member data
    /// </summary>
    public class UmbracoMembersUserStore : DisposableObjectSlim,
        IUserStore<UmbracoMembersIdentityUser>
    //IUserPasswordStore<UmbracoMembersIdentityUser>
    //IUserEmailStore<UmbracoMembersIdentityUser>
    //IUserLoginStore<UmbracoMembersIdentityUser>
    //IUserRoleStore<UmbracoMembersIdentityUser>,
    //IUserSecurityStampStore<UmbracoMembersIdentityUser>
    //IUserLockoutStore<UmbracoMembersIdentityUser>
    //IUserTwoFactorStore<UmbracoMembersIdentityUser>
    //IUserSessionStore<UmbracoMembersIdentityUser>
    {
        private bool _disposed = false;
        private IMemberService _memberService;

        public UmbracoMembersUserStore(IMemberService memberService)
        {
            _memberService = memberService;
        }


        public Task<IdentityResult> CreateAsync(UmbracoMembersIdentityUser memberUser, CancellationToken cancellationToken)
        {
            //TODO: cancellationToken.ThrowIfCancellationRequested();
            //TODO: ThrowIfDisposed();
            if (memberUser == null) throw new ArgumentNullException(nameof(memberUser));


            // [Comments from Identity package and BackOfficeUser]
            // the password must be 'something' it could be empty if authenticating
            // with an external provider so we'll just generate one and prefix it, the
            // prefix will help us determine if the password hasn't actually been specified yet.
            //this will hash the guid with a salt so should be nicely random

            var aspHasher = new PasswordHasher<UmbracoMembersIdentityUser>();
            var emptyPasswordValue =
                Constants.Security.EmptyPasswordPrefix +
                aspHasher.HashPassword(memberUser, Guid.NewGuid().ToString("N"));

            //create member
            //TODO: are we keeping this method, e.g. the Member Service?
            IMember memberEntity = _memberService.CreateMember(
                memberUser.UserName,
                memberUser.Email,
                memberUser.Name.IsNullOrWhiteSpace() ? memberUser.UserName : memberUser.Name,
                memberUser.MemberTypeAlias.IsNullOrWhiteSpace() ?
                    Constants.Security.DefaultMemberTypeAlias : memberUser.MemberTypeAlias);

           
            UpdateMemberProperties(memberEntity, memberUser);

            _memberService.Save(memberEntity);

            //re-assign id
            memberUser.Id = memberEntity.Id;

            // TODO: do we need this?
            // TODO: [from backofficeuser] we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
            //bool isLoginsPropertyDirty = member.IsPropertyDirty(nameof(UmbracoMembersIdentityUser.Logins));

            //if (isLoginsPropertyDirty)
            //{
            //    _externalLoginService.Save(
            //        user.Id,
            //        user.Logins.Select(x => new ExternalLogin(
            //            x.LoginProvider,
            //            x.ProviderKey,
            //            x.UserData)));
            //}

            if (!memberEntity.HasIdentity) throw new DataException("Could not create the user, check logs for details");

            return Task.FromResult(IdentityResult.Success);

            //TODO: confirm 
            //if (memberUser.LoginsChanged)
            //{
            //    var logins = await GetLoginsAsync(memberUser);
            //    _externalLoginStore.SaveUserLogins(member.Id, logins);
            //}

            //TODO: confirm 
            //if (memberUser.RolesChanged)
            //{
            //IMembershipRoleService<IMember> memberRoleService = _memberService;

            //var persistedRoles = memberRoleService.GetAllRoles(member.Id).ToArray();
            //var userRoles = memberUser.Roles.Select(x => x.RoleName).ToArray();

            //var keep = persistedRoles.Intersect(userRoles).ToArray();
            //var remove = persistedRoles.Except(keep).ToArray();
            //var add = userRoles.Except(persistedRoles).ToArray();

            //memberRoleService.DissociateRoles(new[] { member.Id }, remove);
            //memberRoleService.AssignRoles(new[] { member.Id }, add);
            //}
        }


        private bool UpdateMemberProperties(IMember member, UmbracoMembersIdentityUser memberIdentityUser)
        {
            //[Comments as per BackOfficeUserStore & identity package]
            var anythingChanged = false;
            //don't assign anything if nothing has changed as this will trigger the track changes of the model

            //if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.LastLoginDateUtc))
            //    || (member.LastLoginDate != default(DateTime) && identityUser.LastLoginDateUtc.HasValue == false)
            //    || identityUser.LastLoginDateUtc.HasValue && member.LastLoginDate.ToUniversalTime() != identityUser.LastLoginDateUtc.Value)
            //{
            //    anythingChanged = true;
            //    //if the LastLoginDate is being set to MinValue, don't convert it ToLocalTime
            //    var dt = identityUser.LastLoginDateUtc == DateTime.MinValue ? DateTime.MinValue : identityUser.LastLoginDateUtc.Value.ToLocalTime();
            //    member.LastLoginDate = dt;
            //}

            //if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.LastPasswordChangeDateUtc))
            //    || (member.LastPasswordChangeDate != default(DateTime) && identityUser.LastPasswordChangeDateUtc.HasValue == false)
            //    || identityUser.LastPasswordChangeDateUtc.HasValue && member.LastPasswordChangeDate.ToUniversalTime() != identityUser.LastPasswordChangeDateUtc.Value)
            //{
            //    anythingChanged = true;
            //    member.LastPasswordChangeDate = identityUser.LastPasswordChangeDateUtc.Value.ToLocalTime();
            //}

            //if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.EmailConfirmed))
            //    || (member.EmailConfirmedDate.HasValue && member.EmailConfirmedDate.Value != default(DateTime) && identityUser.EmailConfirmed == false)
            //    || ((member.EmailConfirmedDate.HasValue == false || member.EmailConfirmedDate.Value == default(DateTime)) && identityUser.EmailConfirmed))
            //{
            //    anythingChanged = true;
            //    member.EmailConfirmedDate = identityUser.EmailConfirmed ? (DateTime?)DateTime.Now : null;
            //}

            if (
                //memberIdentityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Name)) &&
                member.Name != memberIdentityUser.Name && memberIdentityUser.Name.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Name = memberIdentityUser.Name;
            }
            if (
                //memberIdentityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Email)) &&
                member.Email != memberIdentityUser.Email && memberIdentityUser.Email.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Email = memberIdentityUser.Email;
            }

            //TODO: AccessFailedCount
            //if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.AccessFailedCount))
            //    && member.FailedPasswordAttempts != identityUser.AccessFailedCount)
            //{
            //    anythingChanged = true;
            //    member.FailedPasswordAttempts = identityUser.AccessFailedCount;
            //}

            if (member.IsLockedOut != memberIdentityUser.IsLockedOut)
            {
                anythingChanged = true;
                member.IsLockedOut = memberIdentityUser.IsLockedOut;

                if (member.IsLockedOut)
                {
                    //need to set the last lockout date
                    member.LastLockoutDate = DateTime.Now;
                }
            }
            if (
                //memberIdentityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.UserName)) &&
                member.Username != memberIdentityUser.UserName && memberIdentityUser.UserName.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Username = memberIdentityUser.UserName;
            }

            //TODO: PasswordHash and PasswordConfig
            //if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.PasswordHash))
            //    && member.RawPasswordValue != identityUser.PasswordHash && identityUser.PasswordHash.IsNullOrWhiteSpace() == false)
            //{
            //    anythingChanged = true;
            //    member.RawPasswordValue = identityUser.PasswordHash;
            //    member.PasswordConfiguration = identityUser.PasswordConfig;
            //}

            //TODO: SecurityStamp
            //if (member.SecurityStamp != identityUser.SecurityStamp)
            //{
            //    anythingChanged = true;
            //    member.SecurityStamp = identityUser.SecurityStamp;
            //}

            // TODO: Roles
            // [Comment] Same comment as per BackOfficeUserStore: Fix this for Groups too
            //if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Roles)) || identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Groups)))
            //{
            //    var userGroupAliases = member.Groups.Select(x => x.Alias).ToArray();

            //    var identityUserRoles = identityUser.Roles.Select(x => x.RoleId).ToArray();
            //    var identityUserGroups = identityUser.Groups.Select(x => x.Alias).ToArray();

            //    var combinedAliases = identityUserRoles.Union(identityUserGroups).ToArray();

            //    if (userGroupAliases.ContainsAll(combinedAliases) == false
            //        || combinedAliases.ContainsAll(userGroupAliases) == false)
            //    {
            //        anythingChanged = true;

            //        //clear out the current groups (need to ToArray since we are modifying the iterator)
            //        member.ClearGroups();

            //        //go lookup all these groups
            //        var groups = _userService.GetUserGroupsByAlias(combinedAliases).Select(x => x.ToReadOnlyGroup()).ToArray();

            //        //use all of the ones assigned and add them
            //        foreach (var group in groups)
            //        {
            //            member.AddGroup(group);
            //        }

            //        //re-assign
            //        identityUser.Groups = groups;
            //    }
            //}

            //TODO: reset all changes
            //memberIdentityUser.ResetDirtyProperties(false);

            return anythingChanged;
        }


        public Task<IdentityResult> DeleteAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UmbracoMembersIdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UmbracoMembersIdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(UmbracoMembersIdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(UmbracoMembersIdentityUser user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
