namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides the system default culture.
    /// </summary>
    public interface ISystemDefaultCultureProvider
    {
        /// <summary>
        /// Gets the system default culture.
        /// </summary>
        string DefaultCulture { get; }
    }
}
