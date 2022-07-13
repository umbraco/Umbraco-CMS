using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security;

public class MemberRoleManager : RoleManager<UmbracoIdentityRole>, IMemberRoleManager
{
    public MemberRoleManager(
        IRoleStore<UmbracoIdentityRole> store,
        IEnumerable<IRoleValidator<UmbracoIdentityRole>> roleValidators,
        IdentityErrorDescriber errors,
        ILogger<MemberRoleManager> logger)
        : base(store, roleValidators, new NoopLookupNormalizer(), errors, logger)
    {
    }

    IEnumerable<UmbracoIdentityRole> IMemberRoleManager.Roles => Roles.ToList();
}
