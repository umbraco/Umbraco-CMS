using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IUserService"/>.
/// </summary>
public static class UserServiceExtensions
{
    /// <summary>
    /// Gets a user by key, throwing an exception if not found.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="key">The unique key of the user.</param>
    /// <returns>The user with the specified key.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the user with the specified key cannot be found.</exception>
    public static async Task<IUser> GetRequiredUserAsync(this IUserService userService, Guid key)
        => await userService.GetAsync(key)
           ?? throw new InvalidOperationException($"Could not find user with key: {key}");
}
