using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// A custom user store that uses Umbraco member data
    /// </summary>
    public class MemberRoleStore<TRole> : IRoleStore<TRole> where TRole : IdentityRole
    {
        private readonly IMemberGroupService _memberGroupService;
        private bool _disposed;

        public MemberRoleStore(IMemberGroupService memberGroupService, IdentityErrorDescriber errorDescriber)
        {
            _memberGroupService = memberGroupService ?? throw new ArgumentNullException(nameof(memberGroupService));
            ErrorDescriber = errorDescriber ?? throw new ArgumentNullException(nameof(errorDescriber));
        }

        /// <summary>
        /// Gets or sets the <see cref="IdentityErrorDescriber"/> for any error that occurred with the current operation.
        /// </summary>
        public IdentityErrorDescriber ErrorDescriber { get; set; }
        
        /// <inheritdoc />
        public Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
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
        public Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (!int.TryParse(role.Id, out int roleId))
            {
                //TODO: what identity error should we return in this case?
                return Task.FromResult(IdentityResult.Failed(ErrorDescriber.DefaultError()));
            }

            IMemberGroup memberGroup = _memberGroupService.GetById(roleId);
            if (memberGroup != null)
            {
                if (MapToMemberGroup(role, memberGroup))
                {
                    _memberGroupService.Save(memberGroup);
                }
            }
            else
            {
                //TODO: throw exception when not found, or return failure? And is this the correct message
                return Task.FromResult(IdentityResult.Failed(ErrorDescriber.InvalidRoleName(role.Name)));
            }

            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (!int.TryParse(role.Id, out int roleId))
            {
                //TODO: what identity error should we return in this case?
                return Task.FromResult(IdentityResult.Failed(ErrorDescriber.DefaultError()));
            }

            IMemberGroup memberGroup = _memberGroupService.GetById(roleId);
            if (memberGroup != null)
            {
                _memberGroupService.Delete(memberGroup);
            }
            else
            {
                //TODO: throw exception when not found, or return failure? And is this the correct message
                return Task.FromResult(IdentityResult.Failed(ErrorDescriber.InvalidRoleName(role.Name)));
            }

            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (!int.TryParse(role.Id, out int roleId))
            {
                return null;
            }

            IMemberGroup memberGroup = _memberGroupService.GetById(roleId);

            return Task.FromResult(memberGroup?.Name);
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return null;
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return null;
        }

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return null;
        }

        /// <inheritdoc />
        public Task<TRole> FindByIdAsync(string id, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (!int.TryParse(id, out int roleId))
            {
                return null;
            }

            IMemberGroup memberGroup = _memberGroupService.GetById(roleId);

            return Task.FromResult(memberGroup == null ? null : MapFromMemberGroup(memberGroup));
        }

        /// <inheritdoc />
        public Task<TRole> FindByNameAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            IMemberGroup memberGroup = _memberGroupService.GetByName(name);

            return Task.FromResult(memberGroup == null ? null : MapFromMemberGroup(memberGroup));
        }

        /// <summary>
        /// Maps a member group to an identity role
        /// </summary>
        /// <param name="memberGroup"></param>
        /// <returns></returns>
        private TRole MapFromMemberGroup(IMemberGroup memberGroup)
        {
            var result = new IdentityRole
            {
                Id = memberGroup.Id.ToString(),
                Name = memberGroup.Name
                //TODO: Are we interested in NormalizedRoleName?
            };

            return result as TRole;
        }

        /// <summary>
        /// Map an identity role to a member group
        /// </summary>
        /// <param name="role"></param>
        /// <param name="memberGroup"></param>
        /// <returns></returns>
        private bool MapToMemberGroup(TRole role, IMemberGroup memberGroup)
        {
            var anythingChanged = false;

            if (!string.IsNullOrEmpty(role.Name) && memberGroup.Name != role.Name)
            {
                memberGroup.Name = role.Name;
                anythingChanged = true;
            }

            return anythingChanged;
        }

        public void Dispose()
        {
            //TODO: is any dispose action necessary here or is this all managed by the IOC container?
        }

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
