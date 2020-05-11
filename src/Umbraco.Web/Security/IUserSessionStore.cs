using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// An IUserStore interface part to implement if the store supports validating user session Ids
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public interface IUserSessionStore<TUser> : IUserStore<TUser>
        where TUser : class
    {
        Task<bool> ValidateSessionIdAsync(string userId, string sessionId);
    }
}
