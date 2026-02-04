namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property index value factory specifically for tag properties.
/// </summary>
/// <remarks>
///     This marker interface allows for specialized indexing of tag values,
///     enabling proper handling of multiple tag values per property.
/// </remarks>
public interface ITagPropertyIndexValueFactory : IPropertyIndexValueFactory
{
}
