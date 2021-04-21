using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// Umbraco back office specific <see cref="IdentityErrorDescriber"/>
    /// </summary>
    public class BackOfficeErrorDescriber : IdentityErrorDescriber
    {
        // TODO: Override all the methods in order to provide our own translated error messages
    }

    public class MembersErrorDescriber : IdentityErrorDescriber
    {
        // TODO: Override all the methods in order to provide our own translated error messages
    }
}
