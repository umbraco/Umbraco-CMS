using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Caches constructor instances used by the <see cref="RichTextBlockPropertyValueConverter"/> to optimize property value conversion.
/// </summary>
public class RichTextBlockPropertyValueConstructorCache : BlockEditorPropertyValueConstructorCacheBase<RichTextBlockItem>
{
}
