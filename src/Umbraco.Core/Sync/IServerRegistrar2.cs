namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Provides server registrations to the distributed cache.
    /// </summary>
    /// <remarks>This interface exists because IServerRegistrar could not be modified
    /// for backward compatibility reasons - but IServerRegistrar is broken because it
    /// does not support server role management. So ppl should really implement
    /// IServerRegistrar2, and the two interfaces will get merged in v8.</remarks>
    public interface IServerRegistrar2 : IServerRegistrar
    {
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
