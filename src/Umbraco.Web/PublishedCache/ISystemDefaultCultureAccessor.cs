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
        /// <remarks>
        /// <para>Implementations must NOT return a null value. Return an empty string for the invariant culture.</para>
        /// </remarks>
        string DefaultCulture { get; }
    }
}
