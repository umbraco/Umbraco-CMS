namespace Umbraco.Core
{
    public enum SQLRetryPolicyBehaviour
    {
        /// <summary>
        /// Default umbraco behavior 
        /// - Chooses retry behaviour based on whether or not an Azure connection is detected
        /// </summary>
        Default,

        /// <summary>
        /// Minimal transient network error detection for retries. The default for non-cloud installations
        /// </summary>
        Basic,

        /// <summary>
        /// Suitable for cloud installations of Umbraco where transient network errors are more common 
        /// - Azure transient network error detection for retries
        /// </summary>
        Azure
    }
}
