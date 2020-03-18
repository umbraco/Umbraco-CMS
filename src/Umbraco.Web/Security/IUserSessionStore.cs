using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// An IUserStore interface part to implement if the store supports validating user session Ids
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IUserSessionStore<TUser, in TKey> : IUserStore<TUser, TKey>, IDisposable
        where TUser : class, IUser<TKey>
    {
        Task<bool> ValidateSessionIdAsync(int userId, string sessionId);
    }
}