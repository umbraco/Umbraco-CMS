using Umbraco.Web.Security;

namespace Umbraco.Core.Security
{
    public interface IWebSecurityAccessor
    {
        IWebSecurity WebSecurity { get; set; }
    }
}
