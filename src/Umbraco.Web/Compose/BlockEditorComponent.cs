using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.Compose
{
    /// <summary>
    /// A component for Block editors used to bind to events
    /// </summary>
    public class BlockEditorComponent : IComponent
    {
        private ComplexPropertyEditorContentEventHandler _handler;
        private readonly BlockListEditorDataConverter _converter = new BlockListEditorDataConverter();
        private readonly ILogger _logger;

        [Obsolete("Use the ctor injecting dependencies.")]
        public BlockEditorComponent() : this(Current.Logger)
        {
        }

        public BlockEditorComponent(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            _handler = new ComplexPropertyEditorContentEventHandler(
                Constants.PropertyEditors.Aliases.BlockList,
                ReplaceBlockListUdis);
        }

        public void Terminate() => _handler?.Dispose();

        private string ReplaceBlockListUdis(string rawJson, bool onlyMissingUdis)
        {
            // the block editor doesn't ever have missing UDIs so when this is true there's nothing to process
            if (onlyMissingUdis) return rawJson;

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
                layout.Add(JObject.FromObject(new BlockListLayoutItem
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
                            catch (Exception e)
                            {
                                // See issue https://github.com/umbraco/Umbraco-CMS/issues/10879
                                // We are detecting JSON data by seeing if a string is surrounded by [] or {}
                                // If people enter text like [PLACEHOLDER] JToken  parsing fails, it's safe to ignore though
                                // Logging this just in case in the future we find values that are not safe to ignore
                                _logger.Warn<BlockEditorComponent>(
                                    "The property {PropertyAlias} on content type {ContentTypeKey} has a value of: {BlockItemValue} - this was recognized as JSON but could not be parsed",
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
                if (prop.Name == Constants.PropertyEditors.Aliases.BlockList)
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
                    if (!propVal.IsNullOrWhiteSpace() && propVal.DetectIsJson() && propVal.InvariantContains(Constants.PropertyEditors.Aliases.BlockList))
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
