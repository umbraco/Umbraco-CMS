using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Convertable block data from json
/// </summary>
public class BlockEditorData
{
    private readonly string _propertyEditorAlias;

    public BlockEditorData(
        string propertyEditorAlias,
        IEnumerable<ContentAndSettingsReference> references,
        BlockValue blockValue)
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
        BlockValue = new BlockValue();
    }

    public static BlockEditorData Empty { get; } = new();

    /// <summary>
    ///     Returns the layout for this specific property editor
    /// </summary>
    public JToken? Layout => BlockValue.Layout.TryGetValue(_propertyEditorAlias, out JToken? layout) ? layout : null;

    /// <summary>
    ///     Returns the reference to the original BlockValue
    /// </summary>
    public BlockValue BlockValue { get; }

    public List<ContentAndSettingsReference> References { get; } = new();
}
