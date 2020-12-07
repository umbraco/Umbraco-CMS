using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Members;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Infrastructure.Members
{
    /// <summary>
    /// A custom user store that uses Umbraco member data
    /// </summary>
    public class UmbracoMembersUserStore : DisposableObjectSlim,
        //IUserStore<UmbracoMembersIdentityUser>,
        IUserPasswordStore<UmbracoMembersIdentityUser>
        //IUserEmailStore<UmbracoMembersIdentityUser>
        //IUserLoginStore<UmbracoMembersIdentityUser>
        //IUserRoleStore<UmbracoMembersIdentityUser>,
        //IUserSecurityStampStore<UmbracoMembersIdentityUser>
        //IUserLockoutStore<UmbracoMembersIdentityUser>
        //IUserTwoFactorStore<UmbracoMembersIdentityUser>
        //IUserSessionStore<UmbracoMembersIdentityUser>
    {
        private readonly bool _disposed = false;
        private readonly IMemberService _memberService;
        private readonly UmbracoMapper _mapper;
        private readonly IScopeProvider _scopeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoMembersUserStore"/> class for the members identity store
        /// </summary>
        /// <param name="memberService">The member service</param>
        /// <param name="mapper">The mapper for properties</param>
        /// <param name="scopeProvider">The scope provider</param>
        public UmbracoMembersUserStore(IMemberService memberService, UmbracoMapper mapper, IScopeProvider scopeProvider)
        {
            _memberService = memberService;
            _mapper = mapper;
            _scopeProvider = scopeProvider;
        }

        /// <summary>
        /// Create the member as an identity user
        /// </summary>
        /// <param name="user">The identity user` for a member</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The identity result</returns>
        public Task<IdentityResult> CreateAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // create member
            // TODO: are we keeping this method, e.g. the Member Service?
            // The user service creates it directly, but this way we get the member type by alias first
            IMember member = _memberService.CreateMember(
                user.UserName,
                user.Email,
                user.Name.IsNullOrWhiteSpace() ? user.UserName : user.Name,
                user.MemberTypeAlias.IsNullOrWhiteSpace() ? Constants.Security.DefaultMemberTypeAlias : user.MemberTypeAlias);

            UpdateMemberProperties(member, user);

            // TODO: do we want to accept empty passwords here - if third-party for example?
            // In other method if so?
            _memberService.Save(member);

            // re-assign id
            user.Id = member.Id;

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

            if (!member.HasIdentity)
            {
                throw new DataException("Could not create the member, check logs for details");
            }

            return Task.FromResult(IdentityResult.Success);

            // TODO: confirm and implement
            //if (memberUser.LoginsChanged)
            //{
            //    var logins = await GetLoginsAsync(memberUser);
            //    _externalLoginStore.SaveUserLogins(member.Id, logins);
            //}

            // TODO: confirm and implement
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
            var anythingChanged = false;

            // don't assign anything if nothing has changed as this will trigger the track changes of the model
            if (memberIdentityUser.IsPropertyDirty(nameof(UmbracoMembersIdentityUser.Name)) &&
                member.Name != memberIdentityUser.Name && memberIdentityUser.Name.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Name = memberIdentityUser.Name;
            }

            if (memberIdentityUser.IsPropertyDirty(nameof(UmbracoMembersIdentityUser.Email)) &&
                member.Email != memberIdentityUser.Email && memberIdentityUser.Email.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Email = memberIdentityUser.Email;
            }

            if (member.IsLockedOut != memberIdentityUser.IsLockedOut)
            {
                anythingChanged = true;
                member.IsLockedOut = memberIdentityUser.IsLockedOut;

                if (member.IsLockedOut)
                {
                    // need to set the last lockout date
                    member.LastLockoutDate = DateTime.Now;
                }
            }

            if (memberIdentityUser.IsPropertyDirty(nameof(UmbracoMembersIdentityUser.UserName)) &&
                member.Username != memberIdentityUser.UserName && memberIdentityUser.UserName.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Username = memberIdentityUser.UserName;
            }
           
            if (memberIdentityUser.IsPropertyDirty(nameof(UmbracoMembersIdentityUser.PasswordHash))
                && member.RawPasswordValue != memberIdentityUser.PasswordHash && memberIdentityUser.PasswordHash.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.RawPasswordValue = memberIdentityUser.PasswordHash;
                member.PasswordConfiguration = memberIdentityUser.PasswordConfig;
            }

            // TODO: Roles
            // [Comment] Same comment as per BackOfficeUserStore: Fix this for Groups too
            //if (identityUser.IsPropertyDirty(nameof(UmbracoMembersIdentityUser.Roles)) || identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Groups)))
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

            memberIdentityUser.ResetDirtyProperties(false);
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

        public async Task<UmbracoMembersIdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            // TODO: confirm logic
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            IMember member = _memberService.GetByUsername(normalizedUserName);
            if (member == null)
            {
                return null;
            }

            UmbracoMembersIdentityUser result = _mapper.Map<UmbracoMembersIdentityUser>(member);

            return await Task.FromResult(result);
        }

        public Task<string> GetNormalizedUserNameAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken)
        {
            // TODO: unit tests for and implement all bodies  
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.UserName);
        }

        /// <summary>
        /// Sets the normalized user name
        /// </summary>
        /// <param name="user">The member identity user</param>
        /// <param name="normalizedName">The normalized member name</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task once complete</returns>
        public Task SetNormalizedUserNameAsync(UmbracoMembersIdentityUser user, string normalizedName, CancellationToken cancellationToken) => SetUserNameAsync(user, normalizedName, cancellationToken);

        /// <summary>
        /// Sets the user name as an async operation
        /// </summary>
        /// <param name="user">The member identity user</param>
        /// <param name="userName">The member user name</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task once complete</returns>
        public Task SetUserNameAsync(UmbracoMembersIdentityUser user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.UserName = userName;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Update the user asynchronously
        /// </summary>
        /// <param name="member">The member identity user</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>An identity result task</returns>
        public Task<IdentityResult> UpdateAsync(UmbracoMembersIdentityUser member, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            Attempt<int> asInt = member.Id.TryConvertTo<int>();
            if (asInt == false)
            {
                throw new InvalidOperationException("The member id must be an integer to work with the Umbraco");
            }

            using (IScope scope = _scopeProvider.CreateScope())
            {
                IMember found = _memberService.GetById(asInt.Result);
                if (found != null)
                {
                    // we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
                    // var isLoginsPropertyDirty = member.IsPropertyDirty(nameof(UmbracoMembersIdentityUser.Logins));

                    if (UpdateMemberProperties(found, member))
                    {
                        _memberService.Save(found);
                    }

                    //if (isLoginsPropertyDirty)
                    //{
                    //    _externalLoginService.Save(
                    //        found.Id,
                    //        member.Logins.Select(x => new ExternalLogin(
                    //            x.LoginProvider,
                    //            x.ProviderKey,
                    //            x.UserData)));
                    //}
                }

                scope.Complete();
            }

            return Task.FromResult(IdentityResult.Success);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// TODO: All from BackOfficeUserStore - same. Can we share?
        /// <summary>
        /// Set the user password hash
        /// </summary>
        /// <param name="user">The identity member user</param>
        /// <param name="passwordHash">The password hash</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="ArgumentException">Throws if the properties are null</exception>
        /// <returns>Returns asynchronously</returns>
        public Task SetPasswordHashAsync(UmbracoMembersIdentityUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (passwordHash == null)
            {
                throw new ArgumentNullException(nameof(passwordHash));
            }

            if (string.IsNullOrEmpty(passwordHash))
            {
                throw new ArgumentException("Value can't be empty.", nameof(passwordHash));
            }

            user.PasswordHash = passwordHash;

            // Clear this so that it's reset at the repository level
            user.PasswordConfig = null;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the user password hash
        /// </summary>
        /// <param name="user"/>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns/>
        public Task<string> GetPasswordHashAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PasswordHash);
        }

        /// <summary>
        /// Returns true if a user has a password set
        /// </summary>
        /// <param name="user">The identity user</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True if the user has a password</returns>
        public Task<bool> HasPasswordAsync(UmbracoMembersIdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(string.IsNullOrEmpty(user.PasswordHash) == false);
        }
    }
}
