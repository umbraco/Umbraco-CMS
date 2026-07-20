// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents convertible block data from JSON.
/// </summary>
/// <typeparam name="TValue">The type of the block value.</typeparam>
/// <typeparam name="TLayout">The type of the layout item.</typeparam>
public class BlockEditorData<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : IBlockLayoutItem
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockEditorData{TValue, TLayout}" /> class.
    /// </summary>
    /// <param name="references">The content and settings references.</param>
    /// <param name="blockValue">The block value.</param>
    /// <exception cref="ArgumentNullException">Thrown when references or blockValue is null.</exception>
    public BlockEditorData(IEnumerable<ContentAndSettingsReference> references, TValue blockValue)
    {
        BlockValue = blockValue ?? throw new ArgumentNullException(nameof(blockValue));
        References = references != null
            ? new List<ContentAndSettingsReference>(references)
            : throw new ArgumentNullException(nameof(references));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockEditorData{TValue, TLayout}" /> class.
    /// </summary>
    private BlockEditorData()
        => BlockValue = new TValue();

    /// <summary>
    ///     Gets the empty instance of <see cref="BlockEditorData{TValue, TLayout}" />.
    /// </summary>
    /// <value>
    ///     The empty instance.
    /// </value>
    public static BlockEditorData<TValue, TLayout> Empty { get; } = new();

    /// <summary>
    ///     Gets the layout for this specific property editor.
    /// </summary>
    /// <value>
    ///     The layout.
    /// </value>
    public IEnumerable<TLayout>? Layout => BlockValue.GetLayouts();

    /// <summary>
    ///     Gets the reference to the original block value.
    /// </summary>
    /// <value>
    ///     The block value.
    /// </value>
    public TValue BlockValue { get; }

    /// <summary>
    ///     Gets the content and settings references.
    /// </summary>
    /// <value>
    ///     The references.
    /// </value>
    public List<ContentAndSettingsReference> References { get; } = new();
}
