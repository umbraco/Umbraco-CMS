namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the MemberService, which is an easy access to operations involving (umbraco) members.
    /// </summary>
    internal interface IMemberService : IMembershipMemberService
    {
         
    }

    /// <summary>
    /// Defines part of the MemberService, which is specific to methods used by the membership provider.
    /// </summary>
    /// <remarks>
    /// Idea is to have this is an isolated interface so that it can be easily 'replaced' in the membership provider impl.
    /// </remarks>
    internal interface IMembershipMemberService : IService
    {}
}