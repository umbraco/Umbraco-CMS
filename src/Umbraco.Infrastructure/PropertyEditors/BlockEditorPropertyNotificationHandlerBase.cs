using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public abstract class BlockEditorPropertyNotificationHandlerBase<TBlockLayoutItem> : ComplexPropertyEditorContentNotificationHandler
    where TBlockLayoutItem : IBlockLayoutItem, new()
{
    protected BlockEditorDataConverter Converter { get; }

    protected ILogger<BlockEditorPropertyNotificationHandlerBase<TBlockLayoutItem>> Logger { get; }

    protected BlockEditorPropertyNotificationHandlerBase(
        BlockEditorDataConverter converter,
        ILogger<BlockEditorPropertyNotificationHandlerBase<TBlockLayoutItem>> logger)
    {
        Converter = converter;
        Logger = logger;
    }


    // internal for tests
    internal string ReplaceBlockEditorUdis(string rawJson, Func<Guid>? createGuid = null)
    {
        // used so we can test nicely
        if (createGuid == null)
        {
            createGuid = () => Guid.NewGuid();
        }

        if (string.IsNullOrWhiteSpace(rawJson) || !rawJson.DetectIsJson())
        {
            return rawJson;
        }

        // Parse JSON
        // This will throw a FormatException if there are null UDIs (expected)
        BlockEditorData blockEditorData = Converter.Deserialize(rawJson);

        UpdateBlocksRecursively(blockEditorData, createGuid);

        return JsonConvert.SerializeObject(blockEditorData.BlockValue, Formatting.None);
    }

    protected override string FormatPropertyValue(string rawJson, bool onlyMissingKeys)
    {
        // the block editor doesn't ever have missing UDIs so when this is true there's nothing to process
        if (onlyMissingKeys)
        {
            return rawJson;
        }

        return ReplaceBlockEditorUdis(rawJson);
    }

    protected virtual void UpdateBlocksRecursively(BlockEditorData blockEditorData, Func<Guid> createGuid)
    {
        var oldToNew = new Dictionary<Udi, Udi>();
        MapOldToNewUdis(oldToNew, blockEditorData.BlockValue.ContentData, createGuid);
        MapOldToNewUdis(oldToNew, blockEditorData.BlockValue.SettingsData, createGuid);

        for (var i = 0; i < blockEditorData.References.Count; i++)
        {
            ContentAndSettingsReference reference = blockEditorData.References[i];
            var hasContentMap = oldToNew.TryGetValue(reference.ContentUdi, out Udi? contentMap);
            Udi? settingsMap = null;
            var hasSettingsMap = reference.SettingsUdi is not null && oldToNew.TryGetValue(reference.SettingsUdi, out settingsMap);

            if (hasContentMap)
            {
                // replace the reference
                blockEditorData.References.RemoveAt(i);
                blockEditorData.References.Insert(i, new ContentAndSettingsReference(contentMap!, hasSettingsMap ? settingsMap : null));
            }
        }

        // build the layout with the new UDIs
        var layout = (JArray?)blockEditorData.Layout;
        layout?.Clear();
        foreach (ContentAndSettingsReference reference in blockEditorData.References)
        {
            layout?.Add(JObject.FromObject(new TBlockLayoutItem
            {
                ContentUdi = reference.ContentUdi,
                SettingsUdi = reference.SettingsUdi,
            }));
        }

        RecursePropertyValues(blockEditorData.BlockValue.ContentData, createGuid);
        RecursePropertyValues(blockEditorData.BlockValue.SettingsData, createGuid);
    }

    private void RecursePropertyValues(IEnumerable<BlockItemData> blockData, Func<Guid> createGuid)
    {
        foreach (BlockItemData data in blockData)
        {
            // check if we need to recurse (make a copy of the dictionary since it will be modified)
            foreach (KeyValuePair<string, object?> propertyAliasToBlockItemData in new Dictionary<string, object?>(data.RawPropertyValues))
            {
                if (propertyAliasToBlockItemData.Value is JToken jtoken)
                {
                    if (ProcessJToken(jtoken, createGuid, out JToken result))
                    {
                        // need to re-save this back to the RawPropertyValues
                        data.RawPropertyValues[propertyAliasToBlockItemData.Key] = result;
                    }
                }
                else
                {
                    var asString = propertyAliasToBlockItemData.Value?.ToString();

                    if (asString != null && asString.DetectIsJson())
                    {
                        // this gets a little ugly because there could be some other complex editor that contains another block editor
                        // and since we would have no idea how to parse that, all we can do is try JSON Path to find another block editor
                        // of our type
                        JToken? json = null;
                        try
                        {
                            json = JToken.Parse(asString);
                        }
                        catch (Exception)
                        {
                            // See issue https://github.com/umbraco/Umbraco-CMS/issues/10879
                            // We are detecting JSON data by seeing if a string is surrounded by [] or {}
                            // If people enter text like [PLACEHOLDER] JToken  parsing fails, it's safe to ignore though
                            // Logging this just in case in the future we find values that are not safe to ignore
                            Logger.LogWarning(
                                "The property {PropertyAlias} on content type {ContentTypeKey} has a value of: {BlockItemValue} - this was recognized as JSON but could not be parsed",
                                data.Key,
                                propertyAliasToBlockItemData.Key,
                                asString);
                        }

                        if (json != null && ProcessJToken(json, createGuid, out JToken result))
                        {
                            // need to re-save this back to the RawPropertyValues
                            data.RawPropertyValues[propertyAliasToBlockItemData.Key] = result;
                        }
                    }
                }
            }
        }
    }

    private bool ProcessJToken(JToken json, Func<Guid> createGuid, out JToken result)
    {
        var updated = false;
        result = json;

        // select all tokens (flatten)
        var allProperties = json.SelectTokens("$..*").Select(x => x.Parent as JProperty).WhereNotNull().ToList();
        foreach (JProperty prop in allProperties)
        {
            if (prop.Name == EditorAlias)
            {
                // get it's parent 'layout' and it's parent's container
                if (prop.Parent?.Parent is JProperty layout && layout.Parent is JObject layoutJson)
                {
                    // recurse
                    BlockEditorData blockEditorData = Converter.ConvertFrom(layoutJson);
                    UpdateBlocksRecursively(blockEditorData, createGuid);

                    // set new value
                    if (layoutJson.Parent != null)
                    {
                        // we can replace the object
                        layoutJson.Replace(JObject.FromObject(blockEditorData.BlockValue));
                        updated = true;
                    }
                    else
                    {
                        // if there is no parent it means that this json property was the root, in which case we just return
                        result = JObject.FromObject(blockEditorData.BlockValue);
                        return true;
                    }
                }
            }
            else if (prop.Name != "layout" && prop.Name != "contentData" && prop.Name != "settingsData" && prop.Name != "contentTypeKey")
            {
                // this is an arbitrary property that could contain a nested complex editor
                var propVal = prop.Value.ToString();

                // check if this might contain a nested Block Editor
                if (!propVal.IsNullOrWhiteSpace() && propVal.DetectIsJson() && propVal.InvariantContains(EditorAlias))
                {
                    if (Converter.TryDeserialize(propVal, out BlockEditorData? nestedBlockData))
                    {
                        // recurse
                        UpdateBlocksRecursively(nestedBlockData, createGuid);

                        // set the value to the updated one
                        prop.Value = JObject.FromObject(nestedBlockData.BlockValue);
                        updated = true;
                    }
                }
            }
        }

        return updated;
    }

    private void MapOldToNewUdis(Dictionary<Udi, Udi> oldToNew, IEnumerable<BlockItemData> blockData, Func<Guid> createGuid)
    {
        foreach (BlockItemData data in blockData)
        {
            // This should never happen since a FormatException will be thrown if one is empty but we'll keep this here
            if (data.Udi is null)
            {
                throw new InvalidOperationException("Block data cannot contain a null UDI");
            }

            // replace the UDIs
            var newUdi = Udi.Create(Constants.UdiEntityType.Element, createGuid());
            oldToNew[data.Udi] = newUdi;
            data.Udi = newUdi;
        }
    }
}
