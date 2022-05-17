namespace Umbraco.Cms.Core.Mapping;

/// <summary>
///     Defines maps for <see cref="IUmbracoMapper" />.
/// </summary>
public interface IMapDefinition
{
    /// <summary>
    ///     Defines maps.
    /// </summary>
    void DefineMaps(IUmbracoMapper mapper);
}
