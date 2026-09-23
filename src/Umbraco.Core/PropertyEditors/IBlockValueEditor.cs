using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Capabilities specific to value editors backed by a block value (Block List, Block Grid, blocks-in-RTE
///     and the single-block editor).
/// </summary>
public interface IBlockValueEditor
{
    /// <summary>
    ///     Gets the block value behind a property value.
    /// </summary>
    /// <param name="value">The property value to read the block value from.</param>
    /// <returns>
    ///     The block value, or <c>null</c> if the property value is empty or holds no block layout.
    /// </returns>
    /// <remarks>
    ///     The value is deserialized and cleaned on every call, so each caller gets its own instance and is
    ///     free to read whatever it needs from it - <see cref="BlockValue.ContentData" />,
    ///     <see cref="BlockValue.SettingsData" />, <see cref="BlockValue.Expose" /> or the layout. Each block
    ///     property carries its resolved <see cref="BlockPropertyValue.PropertyType" />, so a caller can tell
    ///     which of them are themselves backed by a block value and read those in turn.
    /// </remarks>
    BlockValue? GetBlockValue(object? value);
}
