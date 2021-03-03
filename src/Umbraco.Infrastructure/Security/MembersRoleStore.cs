using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// A custom user store that uses Umbraco member data
    /// </summary>
    public class MembersRoleStore : RoleStoreBase<IdentityRole<string>, string, IdentityUserRole<string>, IdentityRoleClaim<string>>
    {
        private readonly IMemberService _memberService;
        private readonly IMemberGroupService _memberGroupService;
        private readonly IScopeProvider _scopeProvider;

        public MembersRoleStore(IMemberService memberService, IMemberGroupService memberGroupService, IScopeProvider scopeProvider, IdentityErrorDescriber describer)
            : base(describer)
        {
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _memberGroupService = memberGroupService ?? throw new ArgumentNullException(nameof(memberGroupService));
            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
        }

        /// <inheritdoc />
        public override IQueryable<IdentityRole<string>> Roles
        {
            get
            {
                IEnumerable<IMemberGroup> memberGroups = _memberGroupService.GetAll();
                var identityRoles = new List<IdentityRole<string>>();
                foreach (IMemberGroup group in memberGroups)
                {
                    IdentityRole<string> identityRole = MapFromMemberGroup(group);
                    identityRoles.Add(identityRole);
                }

                return identityRoles.AsQueryable();
            }
        }

        /// <inheritdoc />
        public override Task<IdentityResult> CreateAsync(
            IdentityRole<string> role,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var memberGroup = new MemberGroup
            {
                Name = role.Name
            };

            _memberGroupService.Save(memberGroup);

            role.Id = memberGroup.Id.ToString();

            return Task.FromResult(IdentityResult.Success);
        }


        /// <inheritdoc />
        public override Task<IdentityResult> UpdateAsync(IdentityRole<string> role,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (!int.TryParse(role.Id, out int roleId))
            {
                return new Task<IdentityResult>(() => IdentityResult.Failed());
            }

            IMemberGroup memberGroup = _memberGroupService.GetById(roleId);
            if (memberGroup != null)
            {
                if (MapToMemberGroup(role, memberGroup))
                {
                    _memberGroupService.Save(memberGroup);
                }
            }

            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public override Task<IdentityResult> DeleteAsync(IdentityRole<string> role,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (!int.TryParse(role.Id, out int roleId))
            {
                return new Task<IdentityResult>(() => IdentityResult.Failed());
            }

            IMemberGroup memberGroup = _memberGroupService.GetById(roleId);
            if (memberGroup != null)
            {
                _memberGroupService.Delete(memberGroup);
            }
            else
            {
                //TODO: throw exception when not found, or return failure?
                return Task.FromResult(IdentityResult.Failed());
            }

            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public override Task<IdentityRole<string>> FindByIdAsync(string id,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (!int.TryParse(id, out int roleId))
            {
                return null;
            }

            IMemberGroup memberGroup = _memberGroupService.GetById(roleId);

            return Task.FromResult(memberGroup == null ? null : MapFromMemberGroup(memberGroup));
        }

        /// <inheritdoc />
        public override Task<IdentityRole<string>> FindByNameAsync(string normalizedName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            IMemberGroup memberGroup = _memberGroupService.GetByName(normalizedName);

            return Task.FromResult(memberGroup == null ? null : MapFromMemberGroup(memberGroup));
        }

        /// <inheritdoc />
        public override Task<IList<Claim>> GetClaimsAsync(IdentityRole<string> role, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public override Task AddClaimAsync(IdentityRole<string> role, Claim claim, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public override Task RemoveClaimAsync(IdentityRole<string> role, Claim claim, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

        /// <summary>
        /// Maps a member group to an identity role
        /// </summary>
        /// <param name="memberGroup"></param>
        /// <returns></returns>
        private IdentityRole<string> MapFromMemberGroup(IMemberGroup memberGroup)
        {
            var result = new IdentityRole
            {
                Id = memberGroup.Id.ToString(),
                Name = memberGroup.Name
            };

            return result;
        }

        /// <summary>
        /// Map an identity role to a member group
        /// </summary>
        /// <param name="role"></param>
        /// <param name="memberGroup"></param>
        /// <returns></returns>
        private bool MapToMemberGroup(IdentityRole<string> role, IMemberGroup memberGroup)
        {
            var anythingChanged = false;

            if (!string.IsNullOrEmpty(role.Name) && memberGroup.Name != role.Name)
            {
                memberGroup.Name = role.Name;
                anythingChanged = true;
            }

            return anythingChanged;
        }
    }
}
