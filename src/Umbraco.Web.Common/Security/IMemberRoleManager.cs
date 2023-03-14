using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security;

public interface IMemberRoleManager
{
    IEnumerable<UmbracoIdentityRole> Roles { get; }
}
