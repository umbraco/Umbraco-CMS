// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// A handler for Block editors used to bind to notifications, was not abstract enough so had to make this duplication, changes are highlighted with comments.
    /// </summary>
    public class BlockGridEditorPropertyHandler : ComplexPropertyEditorContentNotificationHandler
    {
        private readonly BlockListEditorDataConverter _converter = new BlockListEditorDataConverter();
        private readonly ILogger _logger;

        public BlockGridEditorPropertyHandler(ILogger<BlockGridEditorPropertyHandler> logger)
        {
            _logger = logger;
        }

        // Change: This is change dot the right alias.
        protected override string EditorAlias => Constants.PropertyEditors.Aliases.BlockGrid;

        protected override string FormatPropertyValue(string rawJson, bool onlyMissingKeys)
        {
            // the block editor doesn't ever have missing UDIs so when this is true there's nothing to process
            if (onlyMissingKeys)
                return rawJson;

            return ReplaceBlockListUdis(rawJson, null);
        }

        // internal for tests
        internal string ReplaceBlockListUdis(string rawJson, Func<Guid> createGuid = null)
        {
            // used so we can test nicely
            if (createGuid == null)
                createGuid = () => Guid.NewGuid();

            if (string.IsNullOrWhiteSpace(rawJson) || !rawJson.DetectIsJson())
                return rawJson;

            // Parse JSON
            // This will throw a FormatException if there are null UDIs (expected)
            var blockListValue = _converter.Deserialize(rawJson);

            UpdateBlockListRecursively(blockListValue, createGuid);

            return JsonConvert.SerializeObject(blockListValue.BlockValue, Formatting.None);
        }

        /*
        Niels:
        Change:
        TODO: Here we need to take children of layout item into account.
        */
        private void UpdateBlockListRecursively(BlockEditorData blockListData, Func<Guid> createGuid)
        {
            var oldToNew = new Dictionary<Udi, Udi>();
            MapOldToNewUdis(oldToNew, blockListData.BlockValue.ContentData, createGuid);
            MapOldToNewUdis(oldToNew, blockListData.BlockValue.SettingsData, createGuid);

            for (var i = 0; i < blockListData.References.Count; i++)
            {
                var reference = blockListData.References[i];
                var hasContentMap = oldToNew.TryGetValue(reference.ContentUdi, out var contentMap);
                Udi settingsMap = null;
                var hasSettingsMap = reference.SettingsUdi != null && oldToNew.TryGetValue(reference.SettingsUdi, out settingsMap);

                if (hasContentMap)
                {
                    // replace the reference
                    blockListData.References.RemoveAt(i);
                    blockListData.References.Insert(i, new ContentAndSettingsReference(contentMap, hasSettingsMap ? settingsMap : null));
                }
            }

            // build the layout with the new UDIs
            var layout = (JArray)blockListData.Layout;
            layout.Clear();
            foreach (var reference in blockListData.References)
            {
                layout.Add(JObject.FromObject(new BlockGridLayoutItem
                {
                    ContentUdi = reference.ContentUdi,
                    SettingsUdi = reference.SettingsUdi
                }));
            }


            RecursePropertyValues(blockListData.BlockValue.ContentData, createGuid);
            RecursePropertyValues(blockListData.BlockValue.SettingsData, createGuid);
        }

        private void RecursePropertyValues(IEnumerable<BlockItemData> blockData, Func<Guid> createGuid)
        {
            foreach (var data in blockData)
            {
                // check if we need to recurse (make a copy of the dictionary since it will be modified)
                foreach (var propertyAliasToBlockItemData in new Dictionary<string, object>(data.RawPropertyValues))
                {
                    if (propertyAliasToBlockItemData.Value is JToken jtoken)
                    {
                        if (ProcessJToken(jtoken, createGuid, out var result))
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
                            JToken json = null;
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
                                _logger.LogWarning(                                    "The property {PropertyAlias} on content type {ContentTypeKey} has a value of: {BlockItemValue} - this was recognized as JSON but could not be parsed",
                                    data.Key, propertyAliasToBlockItemData.Key, asString);
                            }

                            if (json != null && ProcessJToken(json, createGuid, out var result))
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
            foreach (var prop in allProperties)
            {
                // Cange: BlockList to BlockGrid.. WTF is this code, why is here some special treatment of BlockList, which is not general for handling 'inner' data og blocks. Should be easily to define aliases for blocks editors, as well we need extensions to registrer them selfs? So we can mix different block editors inside each other.
                if (prop.Name == Constants.PropertyEditors.Aliases.BlockGrid)
                {
                    // get it's parent 'layout' and it's parent's container
                    var layout = prop.Parent?.Parent as JProperty;
                    if (layout != null && layout.Parent is JObject layoutJson)
                    {
                        // recurse
                        var blockListValue = _converter.ConvertFrom(layoutJson);
                        UpdateBlockListRecursively(blockListValue, createGuid);

                        // set new value
                        if (layoutJson.Parent != null)
                        {
                            // we can replace the object
                            layoutJson.Replace(JObject.FromObject(blockListValue.BlockValue));
                            updated = true;
                        }
                        else
                        {
                            // if there is no parent it means that this json property was the root, in which case we just return
                            result = JObject.FromObject(blockListValue.BlockValue);
                            return true;
                        }
                    }
                }
                else if (prop.Name != "layout" && prop.Name != "contentData" && prop.Name != "settingsData" && prop.Name != "contentTypeKey")
                {
                    // this is an arbitrary property that could contain a nested complex editor
                    var propVal = prop.Value?.ToString();
                    // check if this might contain a nested Block Editor
                    if (!propVal.IsNullOrWhiteSpace() && propVal.DetectIsJson() && propVal.InvariantContains(Constants.PropertyEditors.Aliases.BlockGrid))
                    {
                        if (_converter.TryDeserialize(propVal, out var nestedBlockData))
                        {
                            // recurse
                            UpdateBlockListRecursively(nestedBlockData, createGuid);
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
            foreach (var data in blockData)
            {
                // This should never happen since a FormatException will be thrown if one is empty but we'll keep this here
                if (data.Udi == null)
                    throw new InvalidOperationException("Block data cannot contain a null UDI");

                // replace the UDIs
                var newUdi = GuidUdi.Create(Constants.UdiEntityType.Element, createGuid());
                oldToNew[data.Udi] = newUdi;
                data.Udi = newUdi;
            }
        }
    }
}
