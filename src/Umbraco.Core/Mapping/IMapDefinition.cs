namespace Umbraco.Core.Mapping
{
    /// <summary>
    /// Defines maps for <see cref="UmbracoMapper"/>.
    /// </summary>
    public interface IMapDefinition
    {
        /// <summary>
        /// Defines maps.
        /// </summary>
        void DefineMaps(UmbracoMapper mapper);
    }
}
