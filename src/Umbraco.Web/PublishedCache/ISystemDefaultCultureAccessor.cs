namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides the system default culture.
    /// </summary>
    public interface ISystemDefaultCultureAccessor
    {
        /// <summary>
        /// Gets the system default culture.
        /// </summary>
        string DefaultCulture { get; }
    }
}
