using Umbraco.Web.Security;

namespace Umbraco.Core.Security
{
    public interface IBackofficeSecurityAccessor
    {
        IBackofficeSecurity BackofficeSecurity { get; set; }
    }
}
