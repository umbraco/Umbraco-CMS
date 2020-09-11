using Umbraco.Web.Security;

namespace Umbraco.Core
{
    /// <summary>
    /// Creates and manages <see cref="IWebSecurity"/> instances.
    /// </summary>
    public interface IWebSecurityFactory
    {
        /// <summary>
        /// Ensures that a current <see cref="IWebSecurity"/> exists.
        /// </summary>
        void EnsureWebSecurity();
    }
}
