namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Specifies the type of urls that the url provider should produce, Auto is the default
    /// </summary>
    /// <remarks>
    /// <para>The <c>Relative</c> option can lead to invalid results when combined with hostnames, but it is the only way to reproduce
    /// the true, pre-4.10, always-relative behavior of Umbraco.</para>
    /// </remarks>
    public enum UrlProviderMode
    {

        /// <summary>
        /// Indicates that the url provider should produce relative urls exclusively.
        /// </summary>
        Relative,

        /// <summary>
        /// Indicates that the url provider should produce absolute urls exclusively.
        /// </summary>
        Absolute,

        /// <summary>
        /// Indicates that the url provider should determine automatically whether to return relative or absolute urls.
        /// </summary>
        Auto
    }
}
