using Microsoft.AspNet.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// A password hasher that is based on the rules configured for a membership provider
    /// </summary>
    public interface IMembershipProviderPasswordHasher : IPasswordHasher
    {
        MembershipProviderBase MembershipProvider { get; }
    }
}