using System.Web;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the UserService, which is an easy access to operations involving <see cref="IProfile"/> and eventually Users and Members.
    /// </summary>
    internal interface IUserService : IService
    {
        /// <summary>
        /// Gets an <see cref="IProfile"/> for the current BackOffice User
        /// </summary>
        /// <param name="httpContext">HttpContext to fetch the user through</param>
        /// <returns><see cref="IProfile"/> containing the Name and Id of the logged in BackOffice User</returns>
        IProfile GetCurrentBackOfficeUser(HttpContextBase httpContext);

        /// <summary>
        /// Gets an <see cref="IProfile"/> for the current BackOffice User.
        /// </summary>
        /// <remarks>
        /// Requests the current HttpContext, so this method will only work in a web context.
        /// </remarks>
        /// <returns><see cref="IProfile"/> containing the Name and Id of the logged in BackOffice User</returns>
        IProfile GetCurrentBackOfficeUser();

        /// <summary>
        /// Gets an IProfile by User Id.
        /// </summary>
        /// <param name="id">Id of the User to retrieve</param>
        /// <returns><see cref="IProfile"/></returns>
        IProfile GetProfileById(int id);
    }
}