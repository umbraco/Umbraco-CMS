using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Provides a cache for constructors used by block list property value converters, optimizing performance by avoiding repeated reflection or instantiation.
/// </summary>
public class BlockListPropertyValueConstructorCache : BlockEditorPropertyValueConstructorCacheBase<BlockListItem>
{
}
