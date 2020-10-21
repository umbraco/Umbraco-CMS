using Umbraco.Web.Security;

namespace Umbraco.Core.Security
{
    public interface IBackOfficeSecurityAccessor
    {
        IBackOfficeSecurity BackOfficeSecurity { get; set; }
    }
}
