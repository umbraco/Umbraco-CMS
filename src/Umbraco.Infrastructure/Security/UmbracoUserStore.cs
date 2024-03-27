using System.ComponentModel;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

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

    [Obsolete("Use TryConvertIdentityIdToInt instead. Scheduled for removal in V15.")]
    protected static int UserIdToInt(string? userId)
    {
        if (int.TryParse(userId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        if (Guid.TryParse(userId, out Guid key))
        {
            // Reverse the IntExtensions.ToGuid
            return BitConverter.ToInt32(key.ToByteArray(), 0);
        }

        throw new InvalidOperationException($"Unable to convert user ID ({userId})to int using InvariantCulture");
    }

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
    ///     Adds a user to a role (user group)
    /// </summary>
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
    ///     Gets a list of role names the specified user belongs to.
    /// </summary>
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
    ///     Returns true if a user is in the role
    /// </summary>
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
    ///     Removes the role (user group) for the user
    /// </summary>
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
        user.LastPasswordChangeDateUtc = DateTime.UtcNow;
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
