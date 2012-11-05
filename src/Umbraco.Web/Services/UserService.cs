using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using umbraco;

namespace Umbraco.Web.Services
{
    /// <summary>
    /// Represents the UserService, which is an easy access to operations involving <see cref="IProfile"/> and eventually Users and Members.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly HttpContextBase _httpContext;

        public UserService(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        #region Implementation of IUserService

        /// <summary>
        /// Gets an <see cref="IProfile"/> for the current BackOffice User
        /// </summary>
        /// <returns><see cref="IProfile"/> containing the Name and Id of the logged in BackOffice User</returns>
        public IProfile GetCurrentBackOfficeUser()
        {
            var cookie = _httpContext.Request.Cookies["UMB_UCONTEXT"];

            Mandate.That(cookie != null, () => new ArgumentException("The Cookie containing the UserContext Guid Id was null", "Cookie"));

            string contextId = cookie.Value;
            string cacheKey = string.Concat("UmbracoUserContext", contextId);

            int userId = 0;

            if(HttpRuntime.Cache[cacheKey] == null)
            {
                userId =
                    DatabaseFactory.Current.Database.ExecuteScalar<int>(
                        "select userID from umbracoUserLogins where contextID = @ContextId",
                        new {ContextId = new Guid(contextId)});

                HttpRuntime.Cache.Insert(cacheKey, userId,
                                         null,
                                         System.Web.Caching.Cache.NoAbsoluteExpiration,
                                         new TimeSpan(0, (int) (GlobalSettings.TimeOutInMinutes/10), 0));
            }
            else
            {
                userId = (int) HttpRuntime.Cache[cacheKey];
            }

            //Not too happy with this db lookup, but it'll improve once there is a UserRepository with caching
            var userDto = GetUserById(userId);

            var profile = new Profile(userId, userDto.UserName);
            return profile;
        }

        /// <summary>
        /// Gets a User dto from the database.
        /// </summary>
        /// <param name="id">Id of the User to retrieve</param>
        /// <returns><see cref="UserDto"/></returns>
        private UserDto GetUserById(int id)
        {
            return DatabaseFactory.Current.Database.FirstOrDefault<UserDto>("WHERE id = @Id", new {Id = id});
        }

        #endregion
    }
}