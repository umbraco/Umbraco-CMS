// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Convertable block data from json
/// </summary>
public class BlockEditorData<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly string _propertyEditorAlias;

    public BlockEditorData(
        string propertyEditorAlias,
        IEnumerable<ContentAndSettingsReference> references,
        TValue blockValue)
    {
        if (string.IsNullOrWhiteSpace(propertyEditorAlias))
        {
            throw new ArgumentException($"'{nameof(propertyEditorAlias)}' cannot be null or whitespace", nameof(propertyEditorAlias));
        }

        _propertyEditorAlias = propertyEditorAlias;
        BlockValue = blockValue ?? throw new ArgumentNullException(nameof(blockValue));
        References = references != null
            ? new List<ContentAndSettingsReference>(references)
            : throw new ArgumentNullException(nameof(references));
    }

    private BlockEditorData()
    {
        _propertyEditorAlias = string.Empty;
        BlockValue = new TValue();
    }

    public static BlockEditorData<TValue, TLayout> Empty { get; } = new();

    /// <summary>
    ///     Returns the layout for this specific property editor
    /// </summary>
    public IEnumerable<TLayout>? Layout => BlockValue.GetLayouts(_propertyEditorAlias);

    /// <summary>
    ///     Returns the reference to the original BlockValue
    /// </summary>
    public TValue BlockValue { get; }

    public List<ContentAndSettingsReference> References { get; } = new();
}
