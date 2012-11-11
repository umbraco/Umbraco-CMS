using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

namespace Umbraco.Web.Services
{
    /// <summary>
    /// Represents the UserService, which is an easy access to operations involving <see cref="IProfile"/> and eventually Users and Members.
    /// </summary>
    internal class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWorkProvider provider)
        {
            _unitOfWork = provider.GetUnitOfWork();
        }

        #region Implementation of IUserService

        /// <summary>
        /// Gets an <see cref="IProfile"/> for the current BackOffice User
        /// </summary>
        /// <param name="httpContext">HttpContext to fetch the user through</param>
        /// <returns><see cref="IProfile"/> containing the Name and Id of the logged in BackOffice User</returns>
        public IProfile GetCurrentBackOfficeUser(HttpContextBase httpContext)
        {
            Mandate.That(httpContext != null,
                         () =>
                         new ArgumentException(
                             "The HttpContext which is used to retrieve information about the currently logged in backoffice user was null and can therefor not be used",
                             "HttpContextBase"));
            if (httpContext == null) return null;

            var cookie = httpContext.Request.Cookies["UMB_UCONTEXT"];
            Mandate.That(cookie != null, () => new ArgumentException("The Cookie containing the UserContext Guid Id was null", "Cookie"));
            if (cookie == null) return null;

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
                                         new TimeSpan(0, (int)(Umbraco.Core.Configuration.GlobalSettings.TimeOutInMinutes / 10), 0));
            }
            else
            {
                userId = (int) HttpRuntime.Cache[cacheKey];
            }

            var profile = GetProfileById(userId);
            return profile;
        }

        /// <summary>
        /// Gets an IProfile by User Id.
        /// </summary>
        /// <param name="id">Id of the User to retrieve</param>
        /// <returns><see cref="IProfile"/></returns>
        public IProfile GetProfileById(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IUserRepository, IUser, int>(_unitOfWork);
            return repository.GetProfileById(id);
        }

        #endregion
    }
}