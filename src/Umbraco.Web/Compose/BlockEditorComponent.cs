using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web.Compose
{


    /// <summary>
    /// A component for Block editors used to bind to events
    /// </summary>
    public class BlockEditorComponent : IComponent
    {
        private ComplexPropertyEditorContentEventHandler _handler;
        private readonly BlockListEditorDataConverter _converter = new BlockListEditorDataConverter();

        public void Initialize()
        {
            _handler = new ComplexPropertyEditorContentEventHandler(
                Constants.PropertyEditors.Aliases.BlockList,
                CreateNestedContentKeys);
        }

        public void Terminate() => _handler?.Dispose();

        private string CreateNestedContentKeys(string rawJson, bool onlyMissingKeys) => CreateNestedContentKeys(rawJson, onlyMissingKeys, null);

        // internal for tests
        internal string CreateNestedContentKeys(string rawJson, bool onlyMissingKeys, Func<Guid> createGuid = null, JsonSerializerSettings serializerSettings = null)
        {
            // used so we can test nicely
            if (createGuid == null)
                createGuid = () => Guid.NewGuid();

            if (string.IsNullOrWhiteSpace(rawJson) || !rawJson.DetectIsJson())
                return rawJson;

            // Parse JSON
            var blockListValue = _converter.Deserialize(rawJson);

            UpdateBlockListRecursively(blockListValue, onlyMissingKeys, createGuid, serializerSettings);

            return JsonConvert.SerializeObject(blockListValue.BlockValue, serializerSettings);
        }

        private void UpdateBlockListRecursively(BlockEditorData blockListData, bool onlyMissingKeys, Func<Guid> createGuid, JsonSerializerSettings serializerSettings)
        {
            var oldToNew = new Dictionary<Udi, Udi>();
            MapOldToNewUdis(oldToNew, blockListData.BlockValue.ContentData, onlyMissingKeys, createGuid);
            MapOldToNewUdis(oldToNew, blockListData.BlockValue.SettingsData, onlyMissingKeys, createGuid);

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


            RecursePropertyValues(blockListData.BlockValue.ContentData, onlyMissingKeys, createGuid, serializerSettings);
            RecursePropertyValues(blockListData.BlockValue.SettingsData, onlyMissingKeys, createGuid, serializerSettings);
        }

        private void RecursePropertyValues(IEnumerable<BlockItemData> blockData, bool onlyMissingKeys, Func<Guid> createGuid, JsonSerializerSettings serializerSettings)
        {
            foreach (var data in blockData)
            {
                // check if we need to recurse (make a copy of the dictionary since it will be modified)
                foreach (var propertyAliasToBlockItemData in new Dictionary<string, object>(data.RawPropertyValues))
                {
                    var asString = propertyAliasToBlockItemData.Value?.ToString();

                    //// if this is a nested block list
                    //if (propertyAliasToBlockItemData.Value.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.BlockList)
                    //{
                    //    // recurse
                    //    var blockListValue = _converter.Deserialize(asString);
                    //    UpdateBlockListRecursively(blockListValue, onlyMissingKeys, createGuid);
                    //    // set new value
                    //    data.RawPropertyValues[propertyAliasToBlockItemData.Key] = JsonConvert.SerializeObject(blockListValue.BlockValue);
                    //}

                    if (asString != null && asString.DetectIsJson())
                    {
                        // this gets a little ugly because there could be some other complex editor that contains another block editor
                        // and since we would have no idea how to parse that, all we can do is try JSON Path to find another block editor
                        // of our type
                        var json = JToken.Parse(asString);

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
                                    UpdateBlockListRecursively(blockListValue, onlyMissingKeys, createGuid, serializerSettings);

                                    // set new value
                                    if (layoutJson.Parent != null)
                                    {
                                        // we can replace the sub string
                                        layoutJson.Replace(JsonConvert.SerializeObject(blockListValue.BlockValue, serializerSettings));
                                    }
                                    else
                                    {
                                        // this was the root string
                                        data.RawPropertyValues[propertyAliasToBlockItemData.Key] = JsonConvert.SerializeObject(blockListValue.BlockValue, serializerSettings);
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
                                        UpdateBlockListRecursively(nestedBlockData, onlyMissingKeys, createGuid, serializerSettings);
                                        // set the value to the updated one
                                        prop.Value = JsonConvert.SerializeObject(nestedBlockData.BlockValue, serializerSettings);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void MapOldToNewUdis(Dictionary<Udi, Udi> oldToNew, IEnumerable<BlockItemData> blockData, bool onlyMissingKeys, Func<Guid> createGuid)
        {
            foreach (var data in blockData)
            {
                if (data.Udi == null)
                    throw new InvalidOperationException("Block data cannot contain a null UDI");

                // replace the UDIs
                if (!onlyMissingKeys)
                {
                    var newUdi = GuidUdi.Create(Constants.UdiEntityType.Element, createGuid());
                    oldToNew[data.Udi] = newUdi;
                    data.Udi = newUdi;
                }
            }
        }
    }
}
