namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Specifies the type of urls that the url provider should produce, Auto is the default.
    /// </summary>
    public enum UrlMode
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
