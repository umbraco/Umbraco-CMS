using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A custom user store that uses Umbraco member data
/// </summary>
public class MemberRoleStore : IQueryableRoleStore<UmbracoIdentityRole>
{
    // TODO: Move into custom error describer.
    // TODO: How revealing can the error messages be?
    private readonly IdentityError _intParseError =
        new() { Code = "IdentityIdParseError", Description = "Cannot parse ID to int" };

    private readonly IdentityError _memberGroupNotFoundError =
        new() { Code = "IdentityMemberGroupNotFound", Description = "Member group not found" };

    private readonly IMemberGroupService _memberGroupService;
    private readonly IIdKeyMap _idKeyMap;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberRoleStore"/> class.
    /// </summary>
    /// <remarks>private const string genericIdentityErrorCode = "IdentityErrorUserStore";</remarks>
    /// <param name="memberGroupService">Service used to manage and retrieve member groups.</param>
    /// <param name="errorDescriber">Provides error messages for identity-related operations.</param>
    /// <param name="idKeyMap">The cached id-to-key map used to resolve int role IDs to GUID keys.</param>
    public MemberRoleStore(IMemberGroupService memberGroupService, IdentityErrorDescriber errorDescriber, IIdKeyMap idKeyMap)
    {
        _memberGroupService = memberGroupService ?? throw new ArgumentNullException(nameof(memberGroupService));
        ErrorDescriber = errorDescriber ?? throw new ArgumentNullException(nameof(errorDescriber));
        _idKeyMap = idKeyMap ?? throw new ArgumentNullException(nameof(idKeyMap));
    }

    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public MemberRoleStore(IMemberGroupService memberGroupService, IdentityErrorDescriber errorDescriber)
        : this(
            memberGroupService,
            errorDescriber,
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>())
    {
    }

    /// <summary>
    ///     Gets or sets the <see cref="IdentityErrorDescriber" /> for any error that occurred with the current operation.
    /// </summary>
    public IdentityErrorDescriber ErrorDescriber { get; set; }

    /// <inheritdoc/>
    public IQueryable<UmbracoIdentityRole> Roles =>
        _memberGroupService.GetAllAsync().GetAwaiter().GetResult().Select(MapFromMemberGroup).AsQueryable();

    /// <inheritdoc />
    public async Task<IdentityResult> CreateAsync(UmbracoIdentityRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        var memberGroup = new MemberGroup { Name = role.Name };

        await _memberGroupService.CreateAsync(memberGroup);

        role.Id = memberGroup.Id.ToString();

        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public async Task<IdentityResult> UpdateAsync(UmbracoIdentityRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (!int.TryParse(role.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var roleId))
        {
            return IdentityResult.Failed(_intParseError);
        }

        IMemberGroup? memberGroup = await GetMemberGroupByIdAsync(roleId);
        if (memberGroup != null)
        {
            if (MapToMemberGroup(role, memberGroup))
            {
                await _memberGroupService.UpdateAsync(memberGroup);
            }

            return IdentityResult.Success;
        }

        return IdentityResult.Failed(_memberGroupNotFoundError);
    }

    /// <inheritdoc />
    public async Task<IdentityResult> DeleteAsync(UmbracoIdentityRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (!int.TryParse(role.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var roleId))
        {
            throw new ArgumentException("The Id of the role is not an integer");
        }

        IMemberGroup? memberGroup = await GetMemberGroupByIdAsync(roleId);
        if (memberGroup == null)
        {
            return IdentityResult.Failed(_memberGroupNotFoundError);
        }

        await _memberGroupService.DeleteAsync(memberGroup.Key);
        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public Task<string> GetRoleIdAsync(UmbracoIdentityRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        return Task.FromResult(role.Id)!;
    }

    /// <inheritdoc />
    public Task<string?> GetRoleNameAsync(UmbracoIdentityRole role, CancellationToken cancellationToken = default)
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
    public Task SetRoleNameAsync(
        UmbracoIdentityRole role,
        string? roleName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }


            role.Name = roleName;
            return Task.CompletedTask;
        }

    /// <inheritdoc />
    public Task<string?> GetNormalizedRoleNameAsync(
        UmbracoIdentityRole role,
        CancellationToken cancellationToken = default)
        => GetRoleNameAsync(role, cancellationToken);

    /// <inheritdoc />
    public Task SetNormalizedRoleNameAsync(
        UmbracoIdentityRole role,
        string? normalizedName,
        CancellationToken cancellationToken = default)
        => SetRoleNameAsync(role, normalizedName, cancellationToken);

    /// <inheritdoc />
    public async Task<UmbracoIdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        IMemberGroup? memberGroup;

        // member group can be found by int or Guid, so try both
        if (!int.TryParse(roleId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            if (!Guid.TryParse(roleId, out Guid guid))
            {
                throw new ArgumentOutOfRangeException(nameof(roleId), $"{nameof(roleId)} is not a valid Guid");
            }

            memberGroup = await _memberGroupService.GetAsync(guid);
        }
        else
        {
            memberGroup = await GetMemberGroupByIdAsync(id);
        }

        return memberGroup == null ? null : MapFromMemberGroup(memberGroup);
    }

    /// <summary>
    ///     Resolves a member group by its integer id by mapping the id to its GUID key via the cached id-key map.
    /// </summary>
    private async Task<IMemberGroup?> GetMemberGroupByIdAsync(int id)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.MemberGroup);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        return await _memberGroupService.GetAsync(keyAttempt.Result);
    }

    /// <inheritdoc />
    public Task<UmbracoIdentityRole?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            IMemberGroup? memberGroup = _memberGroupService.GetByName(name);
            return Task.FromResult(memberGroup == null ? null : MapFromMemberGroup(memberGroup))!;
        }

    /// <summary>
    ///     Dispose the store
    /// </summary>
    public void Dispose() => _disposed = true;

    /// <summary>
    ///     Throws if this class has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>
    ///     Maps a member group to an identity role
    /// </summary>
    /// <param name="memberGroup">The member group to map.</param>
    /// <returns>The mapped identity role.</returns>
    private UmbracoIdentityRole MapFromMemberGroup(IMemberGroup memberGroup)
    {
        // NOTE: there is a ConcurrencyStamp property but we don't use it. The purpose
        // of this value is to try to prevent concurrent writes in the DB but this is
        // an implementation detail at the data source level that has leaked into the
        // model. A good writeup of that is here:
        // https://stackoverflow.com/a/37362173
        // For our purposes currently we won't worry about this.
        var result = new UmbracoIdentityRole { Id = memberGroup.Id.ToString(), Name = memberGroup.Name };
        return result;
    }

    /// <summary>
    ///     Map an identity role to a member group
    /// </summary>
    /// <param name="role">The identity role to map.</param>
    /// <param name="memberGroup">The member group to update.</param>
    /// <returns>True if any properties were changed.</returns>
    private bool MapToMemberGroup(UmbracoIdentityRole role, IMemberGroup memberGroup)
    {
        var anythingChanged = false;

        if (role.IsPropertyDirty(nameof(UmbracoIdentityRole.Name))
            && !string.IsNullOrEmpty(role.Name) && memberGroup.Name != role.Name)
        {
            memberGroup.Name = role.Name;
            anythingChanged = true;
        }

        return anythingChanged;
    }
}
