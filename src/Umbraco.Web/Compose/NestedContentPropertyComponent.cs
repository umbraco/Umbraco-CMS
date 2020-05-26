using System;
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
            ContentService.Publishing += ContentService_Publishing;
        }

        private void ContentService_Copying(IContentService sender, CopyEventArgs<IContent> e)
        {
            // When a content node contains nested content property
            // Check if the copied node contains a nested content
            var nestedContentProps = e.Copy.Properties.Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.NestedContent);

            // Each NC Property on a doctype
            foreach (var nestedContentProp in nestedContentProps)
            {
                // A NC Prop may have one or more values due to cultures
                var propVals = nestedContentProp.Values;
                foreach (var cultureVal in propVals)
                {
                    // Remove keys from published value & any nested NC's
                    var updatedPublishedVal = CreateNewNestedContentKeys(cultureVal.PublishedValue.ToString());
                    cultureVal.PublishedValue = updatedPublishedVal;

                    // Remove keys from edited/draft value & any nested NC's
                    var updatedEditedVal = CreateNewNestedContentKeys(cultureVal.EditedValue.ToString());
                    cultureVal.EditedValue = updatedEditedVal;
                }
            }
        }

        private void ContentService_Saving(IContentService sender, ContentSavingEventArgs e)
        {
            // One or more content nodes could be saved in a bulk publish
            foreach(var entity in e.SavedEntities)
            {
                // When a content node contains nested content property
                // Check if the copied node contains a nested content
                var nestedContentProps = entity.Properties.Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.NestedContent);

                // Each NC Property on a doctype
                foreach (var nestedContentProp in nestedContentProps)
                {
                    // A NC Prop may have one or more values due to cultures
                    var propVals = nestedContentProp.Values;
                    foreach (var cultureVal in propVals)
                    {
                        // Remove keys from published value & any nested NC's
                        var updatedPublishedVal = CreateMissingNestedContentKeys(cultureVal.PublishedValue.ToString());
                        cultureVal.PublishedValue = updatedPublishedVal;

                        // Remove keys from edited/draft value & any nested NC's
                        var updatedEditedVal = CreateMissingNestedContentKeys(cultureVal.EditedValue.ToString());
                        cultureVal.EditedValue = updatedEditedVal;
                    }
                }
            }
        }

        private void ContentService_Publishing(IContentService sender, ContentPublishingEventArgs e)
        {
            // One or more content nodes could be saved in a bulk publish
            foreach (var entity in e.PublishedEntities)
            {
                // When a content node contains nested content property
                // Check if the copied node contains a nested content
                var nestedContentProps = entity.Properties.Where(x => x.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.NestedContent);

                // Each NC Property on a doctype
                foreach (var nestedContentProp in nestedContentProps)
                {
                    // A NC Prop may have one or more values due to cultures
                    var propVals = nestedContentProp.Values;
                    foreach (var cultureVal in propVals)
                    {
                        // Remove keys from published value & any nested NC's
                        var updatedPublishedVal = CreateMissingNestedContentKeys(cultureVal.PublishedValue.ToString());
                        cultureVal.PublishedValue = updatedPublishedVal;

                        // Remove keys from edited/draft value & any nested NC's
                        var updatedEditedVal = CreateMissingNestedContentKeys(cultureVal.EditedValue.ToString());
                        cultureVal.EditedValue = updatedEditedVal;
                    }
                }
            }
        }

        public void Terminate()
        {
            ContentService.Copying -= ContentService_Copying;
            ContentService.Saving -= ContentService_Saving;
            ContentService.Publishing -= ContentService_Publishing;
        }

        private string CreateNewNestedContentKeys(string ncJson)
        {
            // Try & convert JSON to JArray (two props we will know should exist are key & ncContentTypeAlias)
            var ncItems = JArray.Parse(ncJson);

            // NC prop contains one or more items/rows of things
            foreach (var nestedContentItem in ncItems.Children<JObject>())
            {
                foreach (var ncItemProp in nestedContentItem.Properties())
                {
                    if(ncItemProp.Name.ToLowerInvariant() == "key")
                        ncItemProp.Value = Guid.NewGuid().ToString();

                    // No need to check this property for JSON - as this is a JSON prop we know
                    // That onyl contains the string of the doctype alias used as the NC item
                    if (ncItemProp.Name == NestedContentPropertyEditor.ContentTypeAliasPropertyKey)
                        continue;

                    // As we don't know what properties in the JSON may contain the nested NC
                    // We are detecting if its value stores JSON to help filter the list AND that in its JSON it has ncContentTypeAlias prop
                    if (ncItemProp.Value.ToString().DetectIsJson() && ncItemProp.Value.ToString().Contains(NestedContentPropertyEditor.ContentTypeAliasPropertyKey))
                    {
                        // Recurse & update this JSON property
                        ncItemProp.Value = CreateNewNestedContentKeys(ncItemProp.Value.ToString());
                    }
                }
            }

            return ncItems.ToString();
        }

        private string CreateMissingNestedContentKeys(string ncJson)
        {
            // Try & convert JSON to JArray (two props we will know should exist are key & ncContentTypeAlias)
            var ncItems = JArray.Parse(ncJson);

            // NC prop contains one or more items/rows of things
            foreach (var nestedContentItem in ncItems.Children<JObject>())
            {
                var ncKeyProp = nestedContentItem.Properties().SingleOrDefault(x => x.Name.ToLowerInvariant() == "key");
                if(ncKeyProp == null)
                {
                    nestedContentItem.Properties().Append(new JProperty("key", Guid.NewGuid().ToString()));
                }

                foreach (var ncItemProp in nestedContentItem.Properties())
                {
                    // No need to check this property for JSON (Its the key OR ncContentTypeAlias) which has no JSON
                    if (ncItemProp.Name == NestedContentPropertyEditor.ContentTypeAliasPropertyKey || ncItemProp.Name.ToLowerInvariant() == "key")
                        continue;

                    // As we don't know what properties in the JSON may contain the nested NC
                    // We are detecting if its value stores JSON to help filter the list AND that in its JSON it has ncContentTypeAlias prop
                    if (ncItemProp.Value.ToString().DetectIsJson() && ncItemProp.Value.ToString().Contains(NestedContentPropertyEditor.ContentTypeAliasPropertyKey))
                    {
                        // Recurse & update this JSON property
                        ncItemProp.Value = CreateMissingNestedContentKeys(ncItemProp.Value.ToString());
                    }
                }
            }

            return ncItems.ToString();
        }
    }
}
