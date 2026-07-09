using System.ComponentModel;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Represents a user store for Umbraco identity management, providing operations for managing users and their roles.
/// </summary>
/// <typeparam name="TUser">The type of the user entity.</typeparam>
/// <typeparam name="TRole">The type of the role entity.</typeparam>
public abstract class UmbracoUserStore<TUser, TRole>
    : UserStoreBase<TUser, TRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>,
        IdentityUserToken<string>, IdentityRoleClaim<string>>
    where TUser : UmbracoIdentityUser
    where TRole : IdentityRole<string>
{
    protected UmbracoUserStore(IdentityErrorDescriber describer)
        : base(describer)
    {
    }

    /// <summary>
    ///     Not supported in Umbraco
    /// </summary>
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override IQueryable<TUser> Users => throw new NotImplementedException();

    /// <summary>
    ///     Not supported in Umbraco
    /// </summary>
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    protected abstract Task<int> ResolveEntityIdFromIdentityId(string? identityId);

    protected static bool TryConvertIdentityIdToInt(string? userId, out int intId)
    {
        // The userId can in this case be one of three things
        // 1. An int - this means that the user logged in normally, this is fine, we parse it and return it.
        // 2. A fake Guid - this means that the user logged in using an external login provider, but we haven't migrated the users to have a key yet, so we need to convert it to an int.
        // 3. A Guid - this means that the user logged in using an external login provider, so we have to resolve the user by key.

        if (int.TryParse(userId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            intId = result;
            return true;
        }

        if (Guid.TryParse(userId, out Guid key))
        {
            if (key.IsFakeGuid())
            {
                intId = key.ToInt();
                return true;
            }
        }

        intId = default;
        return false;
    }

    protected static string UserIdToString(int userId) => string.Intern(userId.ToString(CultureInfo.InvariantCulture));

    /// <summary>
    ///     Asynchronously adds the specified user to the given role (user group).
    /// </summary>
    /// <param name="user">The user to add to the role.</param>
    /// <param name="normalizedRoleName">The normalized name of the role to add the user to.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous add-to-role operation.</returns>
    public override Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
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
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(normalizedRoleName));
        }

        IdentityUserRole<string>? userRole = user.Roles.SingleOrDefault(r => r.RoleId == normalizedRoleName);

        if (userRole == null)
        {
            user.AddRole(normalizedRoleName);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default) =>
        FindUserAsync(userId, cancellationToken);

    /// <summary>
    ///     Not supported in Umbraco
    /// </summary>
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Task<string?> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        => GetEmailAsync(user, cancellationToken);

    /// <inheritdoc />
    public override Task<string?> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = default)
        => GetUserNameAsync(user, cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves the list of role names that the specified user belongs to.
    /// </summary>
    /// <param name="user">The user for whom to retrieve role names.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of role names associated with the user.</returns>
    public override Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult((IList<string>)user.Roles.Select(x => x.RoleId).ToList());
    }

    /// <inheritdoc />
    public override Task<string?> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        // the stamp cannot be null, so if it is currently null then we'll just return a hash of the password
        return Task.FromResult(user.SecurityStamp.IsNullOrWhiteSpace()
            ? user.PasswordHash?.GenerateHash()
            : user.SecurityStamp);
    }

    /// <summary>
    ///     Not supported in Umbraco
    /// </summary>
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override Task<IList<TUser>>
        GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override async Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = default)
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

    /// <summary>
    ///     Determines whether the specified user is a member of the given role.
    /// </summary>
    /// <param name="user">The user whose membership is to be checked.</param>
    /// <param name="normalizedRoleName">The normalized name of the role to check membership for.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>True if the user is in the specified role; otherwise, false.</returns>
    public override Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
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
    ///     Not supported in Umbraco
    /// </summary>
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <summary>
    ///     Removes a role (user group) from the specified user asynchronously.
    /// </summary>
    /// <param name="user">The user from whom to remove the role.</param>
    /// <param name="normalizedRoleName">The normalized name of the role to remove from the user.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous remove operation.</returns>
    public override Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
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
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(normalizedRoleName));
        }

        IdentityUserRole<string>? userRole = user.Roles.SingleOrDefault(r => r.RoleId == normalizedRoleName);

        if (userRole != null)
        {
            user.Roles.Remove(userRole);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Not supported in Umbraco
    /// </summary>
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc />
    public override Task SetNormalizedEmailAsync(TUser user, string? normalizedEmail, CancellationToken cancellationToken)
        => SetEmailAsync(user, normalizedEmail, cancellationToken);

    /// <inheritdoc />
    public override Task SetNormalizedUserNameAsync(TUser user, string? normalizedName, CancellationToken cancellationToken = default)
        => SetUserNameAsync(user, normalizedName, cancellationToken);

    /// <inheritdoc />
    public override async Task SetPasswordHashAsync(TUser user, string? passwordHash, CancellationToken cancellationToken = default)
    {
        await base.SetPasswordHashAsync(user, passwordHash, cancellationToken);
        user.LastPasswordChangeDate = DateTime.UtcNow;
    }

    /// <summary>
    ///     Not supported in Umbraco, see comments above on GetTokenAsync, RemoveTokenAsync, SetTokenAsync
    /// </summary>
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected override Task AddUserTokenAsync(IdentityUserToken<string> token) => throw new NotImplementedException();

    /// <summary>
    ///     Not supported in Umbraco, see comments above on GetTokenAsync, RemoveTokenAsync, SetTokenAsync
    /// </summary>
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected override Task<IdentityUserToken<string>?> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <summary>
    ///     Not supported in Umbraco, see comments above on GetTokenAsync, RemoveTokenAsync, SetTokenAsync
    /// </summary>
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected override Task RemoveUserTokenAsync(IdentityUserToken<string> token) =>
        throw new NotImplementedException();
}
