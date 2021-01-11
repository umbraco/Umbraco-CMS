using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Infrastructure.Security
{
    /// <summary>
    /// A custom user store that uses Umbraco member data
    /// </summary>
    public class MembersUserStore : UserStoreBase<MembersIdentityUser, IdentityRole<string>, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
    {
        private readonly IMemberService _memberService;
        private readonly UmbracoMapper _mapper;
        private readonly IScopeProvider _scopeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MembersUserStore"/> class for the members identity store
        /// </summary>
        /// <param name="memberService">The member service</param>
        /// <param name="mapper">The mapper for properties</param>
        /// <param name="scopeProvider">The scope provider</param>
        /// <param name="describer">The error describer</param>
        /// 
        public MembersUserStore(IMemberService memberService, UmbracoMapper mapper, IScopeProvider scopeProvider, IdentityErrorDescriber describer)
        : base(describer)
        {
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _mapper = mapper;
            _scopeProvider = scopeProvider;
        }

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IQueryable<MembersIdentityUser> Users => throw new NotImplementedException();

        /// <inheritdoc />
        public override Task<string> GetNormalizedUserNameAsync(MembersIdentityUser user, CancellationToken cancellationToken) => GetUserNameAsync(user, cancellationToken);

        /// <inheritdoc />
        public override Task SetNormalizedUserNameAsync(MembersIdentityUser user, string normalizedName, CancellationToken cancellationToken) => SetUserNameAsync(user, normalizedName, cancellationToken);

        /// <inheritdoc />
        public override Task<IdentityResult> CreateAsync(MembersIdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // create member
            // TODO: are we keeping this method? The user service creates the member directly
            // but this way we get the member type by alias first
            IMember memberEntity = _memberService.CreateMember(
                user.UserName,
                user.Email,
                user.Name.IsNullOrWhiteSpace() ? user.UserName : user.Name,
                user.MemberTypeAlias.IsNullOrWhiteSpace() ? Constants.Security.DefaultMemberTypeAlias : user.MemberTypeAlias);


            UpdateMemberProperties(memberEntity, user);

            // create the member
            _memberService.Save(memberEntity);

            if (!memberEntity.HasIdentity)
            {
                throw new DataException("Could not create the member, check logs for details");
            }

            // re-assign id
            user.Id = UserIdToString(memberEntity.Id);

            // [from backofficeuser] we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
            // var isLoginsPropertyDirty = user.IsPropertyDirty(nameof(MembersIdentityUser.Logins));
            // TODO: confirm re externallogins implementation
            //if (isLoginsPropertyDirty)
            //{
            //    _externalLoginService.Save(
            //        user.Id,
            //        user.Logins.Select(x => new ExternalLogin(
            //            x.LoginProvider,
            //            x.ProviderKey,
            //            x.UserData)));
            //}

            // TODO: confirm re roles implementations

            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public override Task<IdentityResult> UpdateAsync(MembersIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            Attempt<int> asInt = user.Id.TryConvertTo<int>();
            if (asInt == false)
            {
                throw new InvalidOperationException("The user id must be an integer to work with the Umbraco");
            }

            using (IScope scope = _scopeProvider.CreateScope())
            {
                IMember found = _memberService.GetById(asInt.Result);
                if (found != null)
                {
                    // we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
                    var isLoginsPropertyDirty = user.IsPropertyDirty(nameof(MembersIdentityUser.Logins));

                    if (UpdateMemberProperties(found, user))
                    {
                        _memberService.Save(found);
                    }

                    // TODO: when to implement external login service?

                    //if (isLoginsPropertyDirty)
                    //{
                    //    _externalLoginService.Save(
                    //        found.Id,
                    //        user.Logins.Select(x => new ExternalLogin(
                    //            x.LoginProvider,
                    //            x.ProviderKey,
                    //            x.UserData)));
                    //}
                }

                scope.Complete();
            }

            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public override Task<IdentityResult> DeleteAsync(MembersIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IMember found = _memberService.GetById(UserIdToInt(user.Id));
            if (found != null)
            {
                _memberService.Delete(found);
            }

            // TODO: when to implement external login service?
            //_externalLoginService.DeleteUserLogins(UserIdToInt(user.Id));

            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public override Task<MembersIdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default) => FindUserAsync(userId, cancellationToken);

        /// <inheritdoc />
        protected override Task<MembersIdentityUser> FindUserAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            IMember user = _memberService.GetById(UserIdToInt(userId));
            if (user == null)
            {
                return Task.FromResult((MembersIdentityUser)null);
            }

            return Task.FromResult(AssignLoginsCallback(_mapper.Map<MembersIdentityUser>(user)));
        }

        /// <inheritdoc />
        public override Task<MembersIdentityUser> FindByNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            IMember user = _memberService.GetByUsername(userName);
            if (user == null)
            {
                return Task.FromResult((MembersIdentityUser)null);
            }

            MembersIdentityUser result = AssignLoginsCallback(_mapper.Map<MembersIdentityUser>(user));

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public override async Task SetPasswordHashAsync(MembersIdentityUser user, string passwordHash, CancellationToken cancellationToken = default)
        {
            await base.SetPasswordHashAsync(user, passwordHash, cancellationToken);

            user.PasswordConfig = null; // Clear this so that it's reset at the repository level
            user.LastPasswordChangeDateUtc = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public override async Task<bool> HasPasswordAsync(MembersIdentityUser user, CancellationToken cancellationToken = default)
        {
            // This checks if it's null
            var result = await base.HasPasswordAsync(user, cancellationToken);
            if (result)
            {
                // we also want to check empty
                return string.IsNullOrEmpty(user.PasswordHash) == false;
            }

            return false;
        }

        /// <inheritdoc />
        public override Task<MembersIdentityUser> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            IMember member = _memberService.GetByEmail(email);
            MembersIdentityUser result = member == null
                ? null
                : _mapper.Map<MembersIdentityUser>(member);

            return Task.FromResult(AssignLoginsCallback(result));
        }

        /// <inheritdoc />
        public override Task<string> GetNormalizedEmailAsync(MembersIdentityUser user, CancellationToken cancellationToken)
            => GetEmailAsync(user, cancellationToken);

        /// <inheritdoc />
        public override Task SetNormalizedEmailAsync(MembersIdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
            => SetEmailAsync(user, normalizedEmail, cancellationToken);

        /// <inheritdoc />
        public override Task AddLoginAsync(MembersIdentityUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            ICollection<IIdentityUserLogin> logins = user.Logins;
            var instance = new IdentityUserLogin(login.LoginProvider, login.ProviderKey, user.Id.ToString());
            IdentityUserLogin userLogin = instance;
            logins.Add(userLogin);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task RemoveLoginAsync(MembersIdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IIdentityUserLogin userLogin = user.Logins.SingleOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
            if (userLogin != null)
            {
                user.Logins.Remove(userLogin);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task<IList<UserLoginInfo>> GetLoginsAsync(MembersIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult((IList<UserLoginInfo>)user.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.LoginProvider)).ToList());
        }
        
        /// <inheritdoc />
        protected override async Task<IdentityUserLogin<string>> FindUserLoginAsync(string userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            MembersIdentityUser user = await FindUserAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            IList<UserLoginInfo> logins = await GetLoginsAsync(user, cancellationToken);
            UserLoginInfo found = logins.FirstOrDefault(x => x.ProviderKey == providerKey && x.LoginProvider == loginProvider);
            if (found == null)
            {
                return null;
            }

            return new IdentityUserLogin<string>
            {
                LoginProvider = found.LoginProvider,
                ProviderKey = found.ProviderKey,
                ProviderDisplayName = found.ProviderDisplayName, // TODO: We don't store this value so it will be null
                UserId = user.Id
            };
        }

        /// <inheritdoc />
        protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            // TODO: external login needed?
            var logins = new List<IIdentityUserLogin>(); //_externalLoginService.Find(loginProvider, providerKey).ToList();
            if (logins.Count == 0)
            {
                return Task.FromResult((IdentityUserLogin<string>)null);
            }

            IIdentityUserLogin found = logins[0];
            return Task.FromResult(new IdentityUserLogin<string>
            {
                LoginProvider = found.LoginProvider,
                ProviderKey = found.ProviderKey,
                ProviderDisplayName = null, // TODO: We don't store this value so it will be null
                UserId = found.UserId
            });
        }

        /// <summary>
        /// Adds a user to a role (user group)
        /// </summary>
        public override Task AddToRoleAsync(MembersIdentityUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (normalizedRoleName == null)
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(normalizedRoleName));
            }

            IdentityUserRole<string> userRole = user.Roles.SingleOrDefault(r => r.RoleId == normalizedRoleName);

            if (userRole == null)
            {
                user.AddRole(normalizedRoleName);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the role (user group) for the user
        /// </summary>
        public override Task RemoveFromRoleAsync(MembersIdentityUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (normalizedRoleName == null)
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(normalizedRoleName));
            }

            IdentityUserRole<string> userRole = user.Roles.SingleOrDefault(r => r.RoleId == normalizedRoleName);

            if (userRole != null)
            {
                user.Roles.Remove(userRole);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns the roles (user groups) for this user
        /// </summary>
        public override Task<IList<string>> GetRolesAsync(MembersIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult((IList<string>)user.Roles.Select(x => x.RoleId).ToList());
        }

        /// <summary>
        /// Returns true if a user is in the role
        /// </summary>
        public override Task<bool> IsInRoleAsync(MembersIdentityUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Roles.Select(x => x.RoleId).InvariantContains(normalizedRoleName));
        }

        /// <summary>
        /// Lists all users of a given role.
        /// </summary>
        public override Task<IList<MembersIdentityUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (normalizedRoleName == null)
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            IEnumerable<IMember> members = _memberService.GetMembersByMemberType(normalizedRoleName);

            IList<MembersIdentityUser> membersIdentityUsers = members.Select(x => _mapper.Map<MembersIdentityUser>(x)).ToList();

            return Task.FromResult(membersIdentityUsers);
        }

        /// <inheritdoc/>
        protected override Task<IdentityRole<string>> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            IMemberGroup group = _memberService.GetAllRoles().SingleOrDefault(x => x.Name == normalizedRoleName);
            if (group == null)
            {
                return Task.FromResult((IdentityRole<string>)null);
            }

            return Task.FromResult(new IdentityRole<string>(group.Name)
            {
                //TODO: what should the alias be?
                Id = @group.Id.ToString()
            });
        }

        /// <inheritdoc/>
        protected override async Task<IdentityUserRole<string>> FindUserRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
        {
            MembersIdentityUser user = await FindUserAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            IdentityUserRole<string> found = user.Roles.FirstOrDefault(x => x.RoleId.InvariantEquals(roleId));
            return found;
        }

        /// <inheritdoc />
        public override Task<string> GetSecurityStampAsync(MembersIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // the stamp cannot be null, so if it is currently null then we'll just return a hash of the password
            return Task.FromResult(user.SecurityStamp.IsNullOrWhiteSpace()
                ? user.PasswordHash.GenerateHash()
                : user.SecurityStamp);
        }

        // TODO: share all possible between backoffice user

        private MembersIdentityUser AssignLoginsCallback(MembersIdentityUser user)
        {
            if (user != null)
            {
                //TODO: when to 
                //user.SetLoginsCallback(new Lazy<IEnumerable<IIdentityUserLogin>>(() => _externalLoginService.GetAll(UserIdToInt(user.Id))));
            }

            return user;
        }

        private bool UpdateMemberProperties(IMember member, MembersIdentityUser identityUserMember)
        {
            var anythingChanged = false;

            // don't assign anything if nothing has changed as this will trigger the track changes of the model
            if (identityUserMember.IsPropertyDirty(nameof(MembersIdentityUser.LastLoginDateUtc))
                || (member.LastLoginDate != default && identityUserMember.LastLoginDateUtc.HasValue == false)
                || (identityUserMember.LastLoginDateUtc.HasValue && member.LastLoginDate.ToUniversalTime() != identityUserMember.LastLoginDateUtc.Value))
            {
                anythingChanged = true;

                // if the LastLoginDate is being set to MinValue, don't convert it ToLocalTime
                DateTime dt = identityUserMember.LastLoginDateUtc == DateTime.MinValue ? DateTime.MinValue : identityUserMember.LastLoginDateUtc.Value.ToLocalTime();
                member.LastLoginDate = dt;
            }

            if (identityUserMember.IsPropertyDirty(nameof(MembersIdentityUser.LastPasswordChangeDateUtc))
                || (member.LastPasswordChangeDate != default && identityUserMember.LastPasswordChangeDateUtc.HasValue == false)
                || (identityUserMember.LastPasswordChangeDateUtc.HasValue && member.LastPasswordChangeDate.ToUniversalTime() != identityUserMember.LastPasswordChangeDateUtc.Value))
            {
                anythingChanged = true;
                member.LastPasswordChangeDate = identityUserMember.LastPasswordChangeDateUtc.Value.ToLocalTime();
            }

            //if (identityUser.IsPropertyDirty(nameof(MembersIdentityUser.EmailConfirmed))
            //    || (user.EmailConfirmedDate.HasValue && user.EmailConfirmedDate.Value != default && identityUser.EmailConfirmed == false)
            //    || ((user.EmailConfirmedDate.HasValue == false || user.EmailConfirmedDate.Value == default) && identityUser.EmailConfirmed))
            //{
            //    anythingChanged = true;
            //    user.EmailConfirmedDate = identityUser.EmailConfirmed ? (DateTime?)DateTime.Now : null;
            //}

            if (identityUserMember.IsPropertyDirty(nameof(MembersIdentityUser.Name))
                && member.Name != identityUserMember.Name && identityUserMember.Name.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Name = identityUserMember.Name;
            }

            if (identityUserMember.IsPropertyDirty(nameof(MembersIdentityUser.Email))
                && member.Email != identityUserMember.Email && identityUserMember.Email.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Email = identityUserMember.Email;
            }

            if (identityUserMember.IsPropertyDirty(nameof(MembersIdentityUser.AccessFailedCount))
                && member.FailedPasswordAttempts != identityUserMember.AccessFailedCount)
            {
                anythingChanged = true;
                member.FailedPasswordAttempts = identityUserMember.AccessFailedCount;
            }

            if (member.IsLockedOut != identityUserMember.IsLockedOut)
            {
                anythingChanged = true;
                member.IsLockedOut = identityUserMember.IsLockedOut;

                if (member.IsLockedOut)
                {
                    // need to set the last lockout date
                    member.LastLockoutDate = DateTime.Now;
                }
            }

            if (identityUserMember.IsPropertyDirty(nameof(MembersIdentityUser.UserName))
                && member.Username != identityUserMember.UserName && identityUserMember.UserName.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Username = identityUserMember.UserName;
            }

            if (identityUserMember.IsPropertyDirty(nameof(MembersIdentityUser.PasswordHash))
                && member.RawPasswordValue != identityUserMember.PasswordHash && identityUserMember.PasswordHash.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.RawPasswordValue = identityUserMember.PasswordHash;
                member.PasswordConfiguration = identityUserMember.PasswordConfig;
            }

            //if (user.SecurityStamp != identityUser.SecurityStamp)
            //{
            //    anythingChanged = true;
            //    user.SecurityStamp = identityUser.SecurityStamp;
            //}

            // TODO: Fix this for Groups too (as per backoffice comment)
            if (identityUserMember.IsPropertyDirty(nameof(MembersIdentityUser.Roles)) || identityUserMember.IsPropertyDirty(nameof(MembersIdentityUser.Groups)))
            {
            }

            // reset all changes
            identityUserMember.ResetDirtyProperties(false);

            return anythingChanged;
        }

        private static int UserIdToInt(string userId)
        {
            Attempt<int> attempt = userId.TryConvertTo<int>();
            if (attempt.Success)
            {
                return attempt.Result;
            }

            throw new InvalidOperationException("Unable to convert user ID to int", attempt.Exception);
        }

        private static string UserIdToString(int userId) => string.Intern(userId.ToString());

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task<IList<Claim>> GetClaimsAsync(MembersIdentityUser user, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task AddClaimsAsync(MembersIdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task ReplaceClaimAsync(MembersIdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task RemoveClaimsAsync(MembersIdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task<IList<MembersIdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        // TODO: We should support these

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override Task<IdentityUserToken<string>> FindTokenAsync(MembersIdentityUser user, string loginProvider, string name, CancellationToken cancellationToken) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override Task AddUserTokenAsync(IdentityUserToken<string> token) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override Task RemoveUserTokenAsync(IdentityUserToken<string> token) => throw new NotImplementedException();

    }
}
