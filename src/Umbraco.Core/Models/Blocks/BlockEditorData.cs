// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Convertable block data from json
/// </summary>
public class BlockEditorData<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : IBlockLayoutItem
{
    public BlockEditorData(IEnumerable<ContentAndSettingsReference> references, TValue blockValue)
    {
        BlockValue = blockValue ?? throw new ArgumentNullException(nameof(blockValue));
        References = references != null
            ? new List<ContentAndSettingsReference>(references)
            : throw new ArgumentNullException(nameof(references));
    }

    private BlockEditorData()
        => BlockValue = new TValue();

    public static BlockEditorData<TValue, TLayout> Empty { get; } = new();

    /// <summary>
    ///     Returns the layout for this specific property editor
    /// </summary>
    public IEnumerable<TLayout>? Layout => BlockValue.GetLayouts();

    /// <summary>
    ///     Returns the reference to the original BlockValue
    /// </summary>
    public TValue BlockValue { get; }

    public List<ContentAndSettingsReference> References { get; } = new();
}
