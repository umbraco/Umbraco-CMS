using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// A custom user store that uses Umbraco member data
    /// </summary>
    public class MemberRolesUserStore : RoleStoreBase<IdentityRole<string>, string, IdentityUserRole<string>, IdentityRoleClaim<string>>
    {
        private readonly IMemberService _memberService;
        private readonly IMemberGroupService _memberGroupService;
        private readonly IScopeProvider _scopeProvider;

        public MemberRolesUserStore(IMemberService memberService, IMemberGroupService memberGroupService, IScopeProvider scopeProvider, IdentityErrorDescriber describer)
            : base(describer)
        {
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _memberGroupService = memberGroupService ?? throw new ArgumentNullException(nameof(memberGroupService));
            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
        }

        /// <inheritdoc />
        public override IQueryable<IdentityRole<string>> Roles { get; }

        /// <inheritdoc />
        public override Task<IdentityResult> CreateAsync(IdentityRole<string> role, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public override Task<IdentityResult> UpdateAsync(IdentityRole<string> role, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public override Task<IdentityResult> DeleteAsync(IdentityRole<string> role, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public override Task<IdentityRole<string>> FindByIdAsync(string id, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public override Task<IdentityRole<string>> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public override Task<IList<Claim>> GetClaimsAsync(IdentityRole<string> role, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public override Task AddClaimAsync(IdentityRole<string> role, Claim claim, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public override Task RemoveClaimAsync(IdentityRole<string> role, Claim claim, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();
    }
}
