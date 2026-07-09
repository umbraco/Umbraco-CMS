namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property index value factory specifically for block-based property values.
/// </summary>
/// <remarks>
///     This marker interface allows for specialized indexing of block content,
///     such as Block List, Block Grid, and Rich Text block values.
/// </remarks>
public interface IBlockValuePropertyIndexValueFactory : IPropertyIndexValueFactory
{
}
