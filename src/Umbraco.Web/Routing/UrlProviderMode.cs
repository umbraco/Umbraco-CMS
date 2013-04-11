namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Specifies the type of urls that the url provider should produce.
    /// </summary>
    /// <remarks>
    /// <para>The <c>AutoLegacy</c> option is equivalent to <c>Auto</c> but it also respects the legacy <c>useDomainPrefixes</c> setting. 
    /// When that setting is true, then all urls are absolute. Otherwise, urls will be relative or absolute, depending on hostnames.</para>
    /// <para>The <c>Relative</c> option can lead to invalid results when combined with hostnames, but it is the only way to reproduce
    /// the true, pre-4.10, always-relative behavior of Umbraco.</para>
    /// <para>For the time being, the default option is <c>AutoLegacy</c> although in the future it will be <c>Auto</c>.</para>
    /// </remarks>
    public enum UrlProviderMode
    {
        /// <summary>
        /// Indicates that the url provider should determine automatically whether to return relative or absolute urls,
        /// and also respect the legacy <c>useDomainPrefixes</c> setting.
        /// </summary>
        AutoLegacy,

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
