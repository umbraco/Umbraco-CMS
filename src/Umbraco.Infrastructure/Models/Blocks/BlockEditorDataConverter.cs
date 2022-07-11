using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Converts the block json data into objects
/// </summary>
public abstract class BlockEditorDataConverter
{
    private readonly string _propertyEditorAlias;

    protected BlockEditorDataConverter(string propertyEditorAlias) => _propertyEditorAlias = propertyEditorAlias;

    public BlockEditorData ConvertFrom(JToken json)
    {
        BlockValue? value = json.ToObject<BlockValue>();
        return Convert(value);
    }

    public bool TryDeserialize(string json, [MaybeNullWhen(false)] out BlockEditorData blockEditorData)
    {
        try
        {
            BlockValue? value = JsonConvert.DeserializeObject<BlockValue>(json);
            blockEditorData = Convert(value);
            return true;
        }
        catch (Exception)
        {
            blockEditorData = null;
            return false;
        }
    }

    public BlockEditorData Deserialize(string json)
    {
        BlockValue? value = JsonConvert.DeserializeObject<BlockValue>(json);
        return Convert(value);
    }

    /// <summary>
    ///     Return the collection of <see cref="IBlockReference" /> from the block editor's Layout (which could be an array or
    ///     an object depending on the editor)
    /// </summary>
    /// <param name="jsonLayout"></param>
    /// <returns></returns>
    protected abstract IEnumerable<ContentAndSettingsReference>? GetBlockReferences(JToken jsonLayout);

    private BlockEditorData Convert(BlockValue? value)
    {
        if (value?.Layout == null)
        {
            return BlockEditorData.Empty;
        }

        IEnumerable<ContentAndSettingsReference>? references =
            value.Layout.TryGetValue(_propertyEditorAlias, out JToken? layout)
                ? GetBlockReferences(layout)
                : Enumerable.Empty<ContentAndSettingsReference>();

        return new BlockEditorData(_propertyEditorAlias, references!, value);
    }
}
