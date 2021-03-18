using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Identity;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// A custom user store that uses Umbraco member data
    /// </summary>
    public class MemberUserStore : UserStoreBase<MemberIdentityUser, IdentityRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
    {
        private const string genericIdentityErrorCode = "IdentityErrorUserStore";
        private readonly IMemberService _memberService;
        private readonly UmbracoMapper _mapper;
        private readonly IScopeProvider _scopeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberUserStore"/> class for the members identity store
        /// </summary>
        /// <param name="memberService">The member service</param>
        /// <param name="mapper">The mapper for properties</param>
        /// <param name="scopeProvider">The scope provider</param>
        /// <param name="describer">The error describer</param>
        public MemberUserStore(IMemberService memberService, UmbracoMapper mapper, IScopeProvider scopeProvider, IdentityErrorDescriber describer)
        : base(describer)
        {
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
        }

        //TODO: why is this not supported?
        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IQueryable<MemberIdentityUser> Users => throw new NotImplementedException();

        /// <inheritdoc />
        public override Task<string> GetNormalizedUserNameAsync(MemberIdentityUser user, CancellationToken cancellationToken = default) => GetUserNameAsync(user, cancellationToken);

        /// <inheritdoc />
        public override Task SetNormalizedUserNameAsync(MemberIdentityUser user, string normalizedName, CancellationToken cancellationToken = default) => SetUserNameAsync(user, normalizedName, cancellationToken);

        /// <inheritdoc />
        public override Task<IdentityResult> CreateAsync(MemberIdentityUser user, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                ThrowIfDisposed();
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user));
                }

                // create member
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


                return Task.FromResult(IdentityResult.Success);
            }
            catch (Exception ex)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Code = genericIdentityErrorCode, Description = ex.Message }));
            }
        }

        /// <inheritdoc />
        public override Task<IdentityResult> UpdateAsync(MemberIdentityUser user, CancellationToken cancellationToken = default)
        {
            try
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
                    //TODO: should this be thrown, or an identity result?
                    throw new InvalidOperationException("The user id must be an integer to work with Umbraco");
                }

                using (IScope scope = _scopeProvider.CreateScope())
                {
                    IMember found = _memberService.GetById(asInt.Result);
                    if (found != null)
                    {
                        // we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
                        var isLoginsPropertyDirty = user.IsPropertyDirty(nameof(MemberIdentityUser.Logins));

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

                    return Task.FromResult(IdentityResult.Success);
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Code = genericIdentityErrorCode, Description = ex.Message }));
            }
        }

        /// <inheritdoc />
        public override Task<IdentityResult> DeleteAsync(MemberIdentityUser user, CancellationToken cancellationToken = default)
        {
            try
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
            catch (Exception ex)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Code = genericIdentityErrorCode, Description = ex.Message }));
            }
        }

        /// <inheritdoc />
        public override Task<MemberIdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default) => FindUserAsync(userId, cancellationToken);

        /// <inheritdoc />
        protected override Task<MemberIdentityUser> FindUserAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            IMember user = _memberService.GetById(UserIdToInt(userId));
            if (user == null)
            {
                return Task.FromResult((MemberIdentityUser)null);
            }

            return Task.FromResult(AssignLoginsCallback(_mapper.Map<MemberIdentityUser>(user)));
        }

        /// <inheritdoc />
        public override Task<MemberIdentityUser> FindByNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            IMember user = _memberService.GetByUsername(userName);
            if (user == null)
            {
                return Task.FromResult((MemberIdentityUser)null);
            }

            MemberIdentityUser result = AssignLoginsCallback(_mapper.Map<MemberIdentityUser>(user));

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public override async Task SetPasswordHashAsync(MemberIdentityUser user, string passwordHash, CancellationToken cancellationToken = default)
        {
            await base.SetPasswordHashAsync(user, passwordHash, cancellationToken);

            // Clear this so that it's reset at the repository level
            user.PasswordConfig = null;
            user.LastPasswordChangeDateUtc = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public override async Task<bool> HasPasswordAsync(MemberIdentityUser user, CancellationToken cancellationToken = default)
        {
            // This checks if it's null
            bool result = await base.HasPasswordAsync(user, cancellationToken);
            if (result)
            {
                // we also want to check empty
                return string.IsNullOrEmpty(user.PasswordHash) == false;
            }

            return false;
        }

        /// <inheritdoc />
        public override Task<MemberIdentityUser> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            IMember member = _memberService.GetByEmail(email);
            MemberIdentityUser result = member == null
                ? null
                : _mapper.Map<MemberIdentityUser>(member);

            return Task.FromResult(AssignLoginsCallback(result));
        }

        /// <inheritdoc />
        public override Task<string> GetNormalizedEmailAsync(MemberIdentityUser user, CancellationToken cancellationToken)
            => GetEmailAsync(user, cancellationToken);

        /// <inheritdoc />
        public override Task SetNormalizedEmailAsync(MemberIdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
            => SetEmailAsync(user, normalizedEmail, cancellationToken);

        /// <inheritdoc />
        public override Task AddLoginAsync(MemberIdentityUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
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

            if (string.IsNullOrWhiteSpace(login.LoginProvider))
            {
                throw new ArgumentNullException(nameof(login.LoginProvider));
            }

            if (string.IsNullOrWhiteSpace(login.ProviderKey))
            {
                throw new ArgumentNullException(nameof(login.ProviderKey));
            }

            ICollection<IIdentityUserLogin> logins = user.Logins;
            var instance = new IdentityUserLogin(
                login.LoginProvider,
                login.ProviderKey,
                user.Id.ToString());

            IdentityUserLogin userLogin = instance;
            logins.Add(userLogin);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task RemoveLoginAsync(MemberIdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(loginProvider))
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }

            if (string.IsNullOrWhiteSpace(providerKey))
            {
                throw new ArgumentNullException(nameof(providerKey));
            }

            IIdentityUserLogin userLogin = user.Logins.SingleOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
            if (userLogin != null)
            {
                user.Logins.Remove(userLogin);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task<IList<UserLoginInfo>> GetLoginsAsync(MemberIdentityUser user, CancellationToken cancellationToken = default)
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

            MemberIdentityUser user = await FindUserAsync(userId, cancellationToken);
            if (user == null)
            {
                //TODO: error throw or null result?
                return await Task.FromResult((IdentityUserLogin<string>)null);
            }

            if (string.IsNullOrWhiteSpace(loginProvider))
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }

            if (string.IsNullOrWhiteSpace(providerKey))
            {
                throw new ArgumentNullException(nameof(providerKey));
            }

            IList<UserLoginInfo> logins = await GetLoginsAsync(user, cancellationToken);
            UserLoginInfo found = logins.FirstOrDefault(x => x.ProviderKey == providerKey && x.LoginProvider == loginProvider);
            if (found == null)
            {
                //TODO: error throw or null result?
                return await Task.FromResult((IdentityUserLogin<string>)null);
            }

            return new IdentityUserLogin<string>
            {
                LoginProvider = found.LoginProvider,
                ProviderKey = found.ProviderKey,
                // TODO: We don't store this value so it will be null
                ProviderDisplayName = found.ProviderDisplayName,
                UserId = user.Id
            };
        }

        /// <inheritdoc />
        protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(loginProvider))
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }

            if (string.IsNullOrWhiteSpace(providerKey))
            {
                throw new ArgumentNullException(nameof(providerKey));
            }

            var logins = new List<IIdentityUserLogin>();

            // TODO: external login needed
            //_externalLoginService.Find(loginProvider, providerKey).ToList();
            if (logins.Count == 0)
            {
                return Task.FromResult((IdentityUserLogin<string>)null);
            }

            IIdentityUserLogin found = logins[0];
            return Task.FromResult(new IdentityUserLogin<string>
            {
                LoginProvider = found.LoginProvider,
                ProviderKey = found.ProviderKey,
                // TODO: We don't store this value so it will be null
                ProviderDisplayName = null,
                UserId = found.UserId
            });
        }

        /// <inheritdoc />
        public override Task AddToRoleAsync(MemberIdentityUser user, string role, CancellationToken cancellationToken = default)
        {
            if (cancellationToken != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(role));
            }

            IdentityUserRole<string> userRole = user.Roles.SingleOrDefault(r => r.RoleId == role);

            if (userRole == null)
            {
                _memberService.AssignRole(user.UserName, role);
                user.AddRole(role);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task RemoveFromRoleAsync(MemberIdentityUser user, string role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(role));
            }

            IdentityUserRole<string> userRole = user.Roles.SingleOrDefault(r => r.RoleId == role);

            if (userRole != null)
            {
                _memberService.DissociateRole(user.UserName, userRole.RoleId);
                user.Roles.Remove(userRole);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets a list of role names the specified user belongs to.
        /// </summary>
        public override Task<IList<string>> GetRolesAsync(MemberIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IEnumerable<string> currentRoles = _memberService.GetAllRoles(user.UserName);
            ICollection<IdentityUserRole<string>> roles = currentRoles.Select(role => new IdentityUserRole<string>
            {
                RoleId = role,
                UserId = user.Id
            }).ToList();

            user.Roles = roles;
            return Task.FromResult((IList<string>)user.Roles.Select(x => x.RoleId).ToList());
        }

        /// <summary>
        /// Returns true if a user is in the role
        /// </summary>
        public override Task<bool> IsInRoleAsync(MemberIdentityUser user, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            return Task.FromResult(user.Roles.Select(x => x.RoleId).InvariantContains(roleName));
        }

        /// <summary>
        /// Lists all users of a given role.
        /// </summary>
        public override Task<IList<MemberIdentityUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            IEnumerable<IMember> members = _memberService.GetMembersByMemberType(roleName);

            IList<MemberIdentityUser> membersIdentityUsers = members.Select(x => _mapper.Map<MemberIdentityUser>(x)).ToList();

            return Task.FromResult(membersIdentityUsers);
        }

        /// <inheritdoc/>
        protected override Task<IdentityRole> FindRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            IMemberGroup group = _memberService.GetAllRoles().SingleOrDefault(x => x.Name == roleName);
            if (group == null)
            {
                return Task.FromResult((IdentityRole)null);
            }

            return Task.FromResult(new IdentityRole(group.Name)
            {
                //TODO: what should the alias be?
                Id = group.Id.ToString()
            });
        }

        /// <inheritdoc/>
        protected override async Task<IdentityUserRole<string>> FindUserRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
        {
            MemberIdentityUser user = await FindUserAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            IdentityUserRole<string> found = user.Roles.FirstOrDefault(x => x.RoleId.InvariantEquals(roleId));
            return found;
        }

        /// <inheritdoc />
        public override Task<string> GetSecurityStampAsync(MemberIdentityUser user, CancellationToken cancellationToken = default)
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

        private MemberIdentityUser AssignLoginsCallback(MemberIdentityUser user)
        {
            if (user != null)
            {
                //TODO: implement
                //user.SetLoginsCallback(new Lazy<IEnumerable<IIdentityUserLogin>>(() => _externalLoginService.GetAll(UserIdToInt(user.Id))));
            }

            return user;
        }

        private bool UpdateMemberProperties(IMember member, MemberIdentityUser identityUserMember)
        {
            var anythingChanged = false;

            // don't assign anything if nothing has changed as this will trigger the track changes of the model
            if (identityUserMember.IsPropertyDirty(nameof(MemberIdentityUser.LastLoginDateUtc))
                || (member.LastLoginDate != default && identityUserMember.LastLoginDateUtc.HasValue == false)
                || (identityUserMember.LastLoginDateUtc.HasValue && member.LastLoginDate.ToUniversalTime() != identityUserMember.LastLoginDateUtc.Value))
            {
                anythingChanged = true;

                // if the LastLoginDate is being set to MinValue, don't convert it ToLocalTime
                DateTime dt = identityUserMember.LastLoginDateUtc == DateTime.MinValue ? DateTime.MinValue : identityUserMember.LastLoginDateUtc.Value.ToLocalTime();
                member.LastLoginDate = dt;
            }

            if (identityUserMember.IsPropertyDirty(nameof(MemberIdentityUser.LastPasswordChangeDateUtc))
                || (member.LastPasswordChangeDate != default && identityUserMember.LastPasswordChangeDateUtc.HasValue == false)
                || (identityUserMember.LastPasswordChangeDateUtc.HasValue && member.LastPasswordChangeDate.ToUniversalTime() != identityUserMember.LastPasswordChangeDateUtc.Value))
            {
                anythingChanged = true;
                member.LastPasswordChangeDate = identityUserMember.LastPasswordChangeDateUtc.Value.ToLocalTime();
            }

            if (identityUserMember.IsPropertyDirty(nameof(MemberIdentityUser.EmailConfirmed))
                || (member.EmailConfirmedDate.HasValue && member.EmailConfirmedDate.Value != default && identityUserMember.EmailConfirmed == false)
                || ((member.EmailConfirmedDate.HasValue == false || member.EmailConfirmedDate.Value == default) && identityUserMember.EmailConfirmed))
            {
                anythingChanged = true;
                member.EmailConfirmedDate = identityUserMember.EmailConfirmed ? (DateTime?)DateTime.Now : null;
            }

            if (identityUserMember.IsPropertyDirty(nameof(MemberIdentityUser.Name))
                && member.Name != identityUserMember.Name && identityUserMember.Name.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Name = identityUserMember.Name;
            }

            if (identityUserMember.IsPropertyDirty(nameof(MemberIdentityUser.Email))
                && member.Email != identityUserMember.Email && identityUserMember.Email.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Email = identityUserMember.Email;
            }

            if (identityUserMember.IsPropertyDirty(nameof(MemberIdentityUser.AccessFailedCount))
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

            if (member.IsApproved != identityUserMember.IsApproved)
            {
                anythingChanged = true;
                member.IsApproved = identityUserMember.IsApproved;
            }

            if (identityUserMember.IsPropertyDirty(nameof(MemberIdentityUser.UserName))
                && member.Username != identityUserMember.UserName && identityUserMember.UserName.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.Username = identityUserMember.UserName;
            }

            if (identityUserMember.IsPropertyDirty(nameof(MemberIdentityUser.PasswordHash))
                && member.RawPasswordValue != identityUserMember.PasswordHash && identityUserMember.PasswordHash.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                member.RawPasswordValue = identityUserMember.PasswordHash;
                member.PasswordConfiguration = identityUserMember.PasswordConfig;
            }

            if (member.SecurityStamp != identityUserMember.SecurityStamp)
            {
                anythingChanged = true;
                member.SecurityStamp = identityUserMember.SecurityStamp;
            }

            // TODO: Fix this for Groups too (as per backoffice comment)
            if (identityUserMember.IsPropertyDirty(nameof(MemberIdentityUser.Roles)) || identityUserMember.IsPropertyDirty(nameof(MemberIdentityUser.Groups)))
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
        public override Task<IList<Claim>> GetClaimsAsync(MemberIdentityUser user, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task AddClaimsAsync(MemberIdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task ReplaceClaimAsync(MemberIdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task RemoveClaimsAsync(MemberIdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task<IList<MemberIdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override Task<IdentityUserToken<string>> FindTokenAsync(MemberIdentityUser user, string loginProvider, string name, CancellationToken cancellationToken) => throw new NotImplementedException();

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
