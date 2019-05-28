using System.Collections.Generic;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Provides server registrations to the distributed cache.
    /// </summary>
    public interface IServerRegistrar
    {
        /// <summary>
        /// Gets the server registrations.
        /// </summary>
        IEnumerable<IServerAddress> Registrations { get; }

        /// <summary>
        /// Gets the role of the current server in the application environment.
        /// </summary>
        ServerRole GetCurrentServerRole();

        /// <summary>
        /// Gets the current umbraco application url.
        /// </summary>
        /// <remarks>
        /// <para>If the registrar does not provide the umbraco application url, should return null.</para>
        /// <para>Must return null, or a url that ends with SystemDirectories.Umbraco, and contains a scheme, eg "http://www.mysite.com/umbraco".</para>
        /// </remarks>
        string GetCurrentServerUmbracoApplicationUrl();
    }
}
