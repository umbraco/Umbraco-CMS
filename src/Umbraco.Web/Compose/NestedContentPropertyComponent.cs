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

        private string CreateNestedContentKeys(string rawJson, bool onlyMissingKeys)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
                return rawJson;

            // Parse JSON
            var ncJson = JToken.Parse(rawJson);

            // NC prop contains one or more items/rows of things
            foreach (var nestedContentItem in ncJson.Children<JObject>())
            {
                // If saving/publishing - we only generate keys for NC items that are missing
                if (onlyMissingKeys)
                {
                    var ncKeyProp = nestedContentItem.Properties().SingleOrDefault(x => x.Name.InvariantEquals("key"));
                    if (ncKeyProp == null)
                    {
                        nestedContentItem.Properties().Append(new JProperty("key", Guid.NewGuid().ToString()));
                    }
                }

                foreach (var ncItemProp in nestedContentItem.Properties())
                {
                    if (onlyMissingKeys == false)
                    {
                        // Only when copying a node - we generate new keys for all NC items
                        if (ncItemProp.Name.InvariantEquals("key"))
                            ncItemProp.Value = Guid.NewGuid().ToString();
                    }

                    // No need to check this property for JSON - as this is a JSON prop we know
                    // That only contains the string of the doctype alias used as the NC item
                    if (ncItemProp.Name == NestedContentPropertyEditor.ContentTypeAliasPropertyKey)
                        continue;

                    // As we don't know what properties in the JSON may contain other complex editors or deep nested NC
                    // We are detecting if its value stores JSON to help filter the list AND that in its JSON it has ncContentTypeAlias prop
                    var ncItemPropVal = ncItemProp.Value?.ToString();

                    if (ncItemPropVal.DetectIsJson())
                    {
                        // Parse the nested JSON (complex editor)
                        var complexEditorJson = JToken.Parse(ncItemPropVal);

                        // Verify the complex editor is nested content (Will have one or more ncContentTypeAlias)
                        // One for each NC item/row
                        var hasNestedContentJsonProp = complexEditorJson.SelectTokens($"$..['{NestedContentPropertyEditor.ContentTypeAliasPropertyKey}']", false);
                        if (hasNestedContentJsonProp.Count() > 0)
                        {
                            // Recurse & update this JSON property
                            ncItemProp.Value = CreateNestedContentKeys(ncItemPropVal, onlyMissingKeys);
                        }
                    }
                }
            }

            return ncJson.ToString();
        }
    }
}
