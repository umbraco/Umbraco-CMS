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

        //TODO: Move into custom error describer.
        //TODO: How revealing can the error messages be?
        private readonly IdentityError _intParseError = new IdentityError { Code = "IdentityIdParseError", Description = "Cannot parse ID to int" };
        private readonly IdentityError _memberGroupNotFoundError = new IdentityError { Code = "IdentityMemberGroupNotFound", Description = "Member group not found" };

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
        public Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            try
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
            catch (Exception ex)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Code = ex.Message, Description = ex.Message }));
            }
        }


        /// <inheritdoc />
        public Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (role == null)
                {
                    throw new ArgumentNullException(nameof(role));
                }

                ThrowIfDisposed();

                if (!int.TryParse(role.Id, out int roleId))
                {
                    return Task.FromResult(IdentityResult.Failed(_intParseError));
                }

                IMemberGroup memberGroup = _memberGroupService.GetById(roleId);
                if (memberGroup != null)
                {
                    if (MapToMemberGroup(role, memberGroup))
                    {
                        _memberGroupService.Save(memberGroup);
                    }
                    //TODO: if nothing changed, do we need to report this?
                    return Task.FromResult(IdentityResult.Success);
                }
                else
                {
                    //TODO: throw exception when not found, or return failure?
                    return Task.FromResult(IdentityResult.Failed(_memberGroupNotFoundError));
                }

            }
            catch (Exception ex)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Code = ex.Message, Description = ex.Message }));
            }
        }

        /// <inheritdoc />
        public Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default)
        {
            try
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
                    return Task.FromResult(IdentityResult.Failed(_intParseError));
                }

                IMemberGroup memberGroup = _memberGroupService.GetById(roleId);
                if (memberGroup != null)
                {
                    _memberGroupService.Delete(memberGroup);
                }
                else
                {
                    return Task.FromResult(IdentityResult.Failed(_memberGroupNotFoundError));
                }

                return Task.FromResult(IdentityResult.Success);
            }
            catch (Exception ex)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Code = ex.Message, Description = ex.Message }));
            }
        }

        /// <inheritdoc />

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Id);
        }

        /// <inheritdoc />
        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Name);
        }

        /// <inheritdoc />
        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken = default)
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
                //TODO: confirm logic
                memberGroup.Name = roleName;
                _memberGroupService.Save(memberGroup);
                role.Name = roleName;
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(_memberGroupNotFoundError));
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            //TODO: are we utilising NormalizedRoleName?
            return Task.FromResult(role.NormalizedName);
        }

        /// <inheritdoc />
        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            //TODO: are we utilising NormalizedRoleName and do we need to set it in the memberGroupService?
            role.NormalizedName = normalizedName;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(roleId))
            {
                throw new ArgumentNullException(nameof(roleId));
            }

            if (!int.TryParse(roleId, out int id))
            {
                throw new ArgumentOutOfRangeException(nameof(roleId), $"{nameof(roleId)} is not a valid Int");
            }

            IMemberGroup memberGroup = _memberGroupService.GetById(id);
            return Task.FromResult(memberGroup == null ? null : MapFromMemberGroup(memberGroup));
        }

        /// <inheritdoc />
        public Task<TRole> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            IMemberGroup memberGroup = _memberGroupService.GetByName(name);
            //TODO: throw exception when not found?

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

        //TODO: is any dispose action necessary here?

        /// <summary>
        /// Dispose the store
        /// </summary>
        public void Dispose() => _disposed = true;

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
