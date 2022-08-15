using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security;

public interface IBackOfficeSecurity
{
    /// <summary>
    ///     Gets the current user.
    /// </summary>
    /// <returns>The current user that has been authenticated for the request.</returns>
    /// <remarks>If authentication hasn't taken place this will be null.</remarks>
    // TODO: This is used a lot but most of it can be refactored to not use this at all since the IUser instance isn't
    // needed in most cases. Where an IUser is required this could be an ext method on the ClaimsIdentity/ClaimsPrincipal that passes in
    // an IUserService, like HttpContext.User.GetUmbracoUser(_userService);
    // This one isn't as easy to remove as the others below.
    IUser? CurrentUser { get; }

    /// <summary>
    ///     Gets the current user's id.
    /// </summary>
    /// <returns>The current user's Id that has been authenticated for the request.</returns>
    /// <remarks>If authentication hasn't taken place this will be unsuccessful.</remarks>
    // TODO: This should just be an extension method on ClaimsIdentity
    Attempt<int> GetUserId();

    /// <summary>
    ///     Checks if the specified user as access to the app
    /// </summary>
    /// <param name="section"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <remarks>If authentication hasn't taken place this will be unsuccessful.</remarks>
    // TODO: Should be part of IBackOfficeUserManager
    bool UserHasSectionAccess(string section, IUser user);

    /// <summary>
    ///     Ensures that a back office user is logged in
    /// </summary>
    /// <returns></returns>
    /// <remarks>This does not force authentication, that must be done before calls to this are made.</remarks>
    // TODO: Should be removed, this should not be necessary
    bool IsAuthenticated();
}
