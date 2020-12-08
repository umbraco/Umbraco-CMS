using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Umbraco.Core.Security;

namespace Umbraco.Infrastructure.Security
{
    /// <summary>
    /// The user manager for members
    /// </summary>
    public interface IMembersUserManager : IUmbracoUserManager<MembersIdentityUser>
    {
    }
}
