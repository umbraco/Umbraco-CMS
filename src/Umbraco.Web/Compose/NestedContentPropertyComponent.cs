using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web.Compose
{
    public class NestedContentPropertyComponent : IComponent
    {
        public void Initialize()
        {
            ContentService.Copying += ContentService_Copying;
            ContentService.Saving += ContentService_Saving;
        }

        private void ContentService_Copying(IContentService sender, CopyEventArgs<IContent> e)
        {
            // When a content node contains nested content property
            // Check if the copied node contains a nested content
            var nestedContentProps = e.Copy.Properties.Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.NestedContent);
            UpdateNestedContentProperties(nestedContentProps, false);
        }

        private void ContentService_Saving(IContentService sender, ContentSavingEventArgs e)
        {
            // One or more content nodes could be saved in a bulk publish
            foreach (var entity in e.SavedEntities)
            {
                // When a content node contains nested content property
                // Check if the copied node contains a nested content
                var nestedContentProps = entity.Properties.Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.NestedContent);
                UpdateNestedContentProperties(nestedContentProps, true);
            }
        }

        public void Terminate()
        {
            ContentService.Copying -= ContentService_Copying;
            ContentService.Saving -= ContentService_Saving;
        }

        private void UpdateNestedContentProperties(IEnumerable<Property> nestedContentProps, bool onlyMissingKeys)
        {
            // Each NC Property on a doctype
            foreach (var nestedContentProp in nestedContentProps)
            {
                // A NC Prop may have one or more values due to cultures
                var propVals = nestedContentProp.Values;
                foreach (var cultureVal in propVals)
                {
                    // Remove keys from published value & any nested NC's
                    var updatedPublishedVal = CreateNestedContentKeys(cultureVal.PublishedValue?.ToString(), onlyMissingKeys);
                    cultureVal.PublishedValue = updatedPublishedVal;

                    // Remove keys from edited/draft value & any nested NC's
                    var updatedEditedVal = CreateNestedContentKeys(cultureVal.EditedValue?.ToString(), onlyMissingKeys);
                    cultureVal.EditedValue = updatedEditedVal;
                }
            }
        }

        
        // internal for tests
        internal string CreateNestedContentKeys(string rawJson, bool onlyMissingKeys, Func<Guid> createGuid = null)
        {
            // used so we can test nicely
            if (createGuid == null)
                createGuid = () => Guid.NewGuid();

            if (string.IsNullOrWhiteSpace(rawJson) || !rawJson.DetectIsJson())
                return rawJson;

            // Parse JSON            
            var complexEditorValue = JToken.Parse(rawJson);

            UpdateNestedContentKeysRecursively(complexEditorValue, onlyMissingKeys, createGuid);

            return complexEditorValue.ToString();
        }

        private void UpdateNestedContentKeysRecursively(JToken json, bool onlyMissingKeys, Func<Guid> createGuid)
        {
            // check if this is NC
            var isNestedContent = json.SelectTokens($"$..['{NestedContentPropertyEditor.ContentTypeAliasPropertyKey}']", false).Any();

            // select all values (flatten)
            var allProperties = json.SelectTokens("$..*").OfType<JValue>().Select(x => x.Parent as JProperty).WhereNotNull().ToList();
            foreach (var prop in allProperties)
            {
                if (prop.Name == NestedContentPropertyEditor.ContentTypeAliasPropertyKey)
                {
                    // get it's sibling 'key' property
                    var ncKeyVal = prop.Parent["key"] as JValue;
                    // TODO: This bool seems odd, if the key is null, shouldn't we fill it in regardless of onlyMissingKeys?
                    if ((onlyMissingKeys && ncKeyVal == null) || (!onlyMissingKeys && ncKeyVal != null))
                    {
                        // create or replace
                        prop.Parent["key"] = createGuid().ToString();
                    }                   
                }
                else if (!isNestedContent || prop.Name != "key")
                {
                    // this is an arbitrary property that could contain a nested complex editor
                    var propVal = prop.Value?.ToString();
                    // check if this might contain a nested NC
                    if (!propVal.IsNullOrWhiteSpace() && propVal.DetectIsJson() && propVal.InvariantContains(NestedContentPropertyEditor.ContentTypeAliasPropertyKey))
                    {
                        // recurse
                        var parsed = JToken.Parse(propVal);
                        UpdateNestedContentKeysRecursively(parsed, onlyMissingKeys, createGuid);
                        // set the value to the updated one
                        prop.Value = parsed.ToString();
                    }
                }
            }
        }

    }
}
