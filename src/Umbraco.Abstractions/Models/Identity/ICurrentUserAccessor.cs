using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models.Identity
{
    public interface ICurrentUserAccessor
    {
        /// <summary>
        /// Returns the current user or null if no user is currently authenticated.
        /// </summary>
        /// <returns>The current user or null</returns>
        IUser TryGetCurrentUser();
    }
}
