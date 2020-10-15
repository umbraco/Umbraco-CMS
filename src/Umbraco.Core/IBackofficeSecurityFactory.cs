using Umbraco.Web.Security;

namespace Umbraco.Core
{
    /// <summary>
    /// Creates and manages <see cref="IBackofficeSecurity"/> instances.
    /// </summary>
    public interface IBackofficeSecurityFactory
    {
        /// <summary>
        /// Ensures that a current <see cref="IBackofficeSecurity"/> exists.
        /// </summary>
        void EnsureBackofficeSecurity();
    }
}
