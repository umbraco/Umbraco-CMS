using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines part of the UserService, which is specific to methods used by the membership provider.
/// </summary>
/// <remarks>
///     Idea is to have this is an isolated interface so that it can be easily 'replaced' in the membership provider impl.
/// </remarks>
public interface IMembershipUserService : IMembershipMemberService<IUser>
{
    /// <summary>
    ///     Creates and persists a new User
    /// </summary>
    /// <remarks>
    ///     The user will be saved in the database and returned with an Id.
    ///     This method is convenient when you need to perform operations, which needs the
    ///     Id of the user once its been created.
    /// </remarks>
    /// <param name="username">Username of the User to create</param>
    /// <param name="email">Email of the User to create</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    IUser CreateUserWithIdentity(string username, string email);
}
