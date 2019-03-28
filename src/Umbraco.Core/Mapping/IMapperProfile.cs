namespace Umbraco.Core.Mapping
{
    /// <summary>
    /// Represents a mapper profile.
    /// </summary>
    public interface IMapperProfile
    {
        /// <summary>
        /// Defines maps.
        /// </summary>
        void DefineMaps(Mapper mapper);
    }
}
