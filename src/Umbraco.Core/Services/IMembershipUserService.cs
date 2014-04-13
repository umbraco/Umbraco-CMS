using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines part of the UserService, which is specific to methods used by the membership provider.
    /// </summary>
    /// <remarks>
    /// Idea is to have this is an isolated interface so that it can be easily 'replaced' in the membership provider impl.
    /// </remarks>
    public interface IMembershipUserService : IMembershipMemberService<IUser>
    {
        IUser CreateUserWithIdentity(string username, string email, IUserType userType);
    }
}