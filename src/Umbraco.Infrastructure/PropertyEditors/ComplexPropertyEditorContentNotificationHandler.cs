// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    public abstract class ComplexPropertyEditorContentNotificationHandler :
        INotificationHandler<ContentSavingNotification>,
        INotificationHandler<ContentCopyingNotification>
    {
        protected abstract string EditorAlias { get; }

        protected abstract string FormatPropertyValue(string rawJson, bool onlyMissingKeys);

        public void Handle(ContentSavingNotification notification)
        {
            foreach (var entity in notification.SavedEntities)
            {
                var props = entity.GetPropertiesByEditor(EditorAlias);
                UpdatePropertyValues(props, true);
            }
        }

        public void Handle(ContentCopyingNotification notification)
        {
            var props = notification.Copy.GetPropertiesByEditor(EditorAlias);
            UpdatePropertyValues(props, false);
        }

        private void UpdatePropertyValues(IEnumerable<IProperty> props, bool onlyMissingKeys)
        {
            foreach (var prop in props)
            {
                // A Property may have one or more values due to cultures
                var propVals = prop.Values;
                foreach (var cultureVal in propVals)
                {
                    // Remove keys from published value & any nested properties
                    var publishedValue = cultureVal.PublishedValue is JToken jsonPublishedValue ? jsonPublishedValue.ToString(Formatting.None) : cultureVal.PublishedValue?.ToString();
                    var updatedPublishedVal = FormatPropertyValue(publishedValue!, onlyMissingKeys).NullOrWhiteSpaceAsNull();
                    cultureVal.PublishedValue = updatedPublishedVal;

                    // Remove keys from edited/draft value & any nested properties
                    var editedValue = cultureVal.EditedValue is JToken jsonEditedValue ? jsonEditedValue.ToString(Formatting.None) : cultureVal.EditedValue?.ToString();
                    var updatedEditedVal = FormatPropertyValue(editedValue!, onlyMissingKeys).NullOrWhiteSpaceAsNull();
                    cultureVal.EditedValue = updatedEditedVal;
                }
            }
        }
    }
}
