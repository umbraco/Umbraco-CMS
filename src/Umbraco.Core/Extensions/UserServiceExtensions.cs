using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Extensions;

public static class UserServiceExtensions
{
    public static async Task<IUser> GetRequiredUserAsync(this IUserService userService, Guid key)
        => await userService.GetAsync(key)
           ?? throw new InvalidOperationException($"Could not find user with key: {key}");
}
