// Copyright (c) Umbraco.
// See LICENSE for more details.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public abstract class ComplexPropertyEditorContentNotificationHandler :
    INotificationHandler<ContentSavingNotification>,
    INotificationHandler<ContentCopyingNotification>
{
    protected abstract string EditorAlias { get; }

    public void Handle(ContentCopyingNotification notification)
    {
        IEnumerable<IProperty> props = notification.Copy.GetPropertiesByEditor(EditorAlias);
        UpdatePropertyValues(props, false);
    }

    public void Handle(ContentSavingNotification notification)
    {
        foreach (IContent entity in notification.SavedEntities)
        {
            IEnumerable<IProperty> props = entity.GetPropertiesByEditor(EditorAlias);
            UpdatePropertyValues(props, true);
        }
    }

    protected abstract string FormatPropertyValue(string rawJson, bool onlyMissingKeys);

    private void UpdatePropertyValues(IEnumerable<IProperty> props, bool onlyMissingKeys)
    {
        foreach (IProperty prop in props)
        {
            // A Property may have one or more values due to cultures
            IReadOnlyCollection<IPropertyValue> propVals = prop.Values;
            foreach (IPropertyValue cultureVal in propVals)
            {
                // Remove keys from published value & any nested properties
                var publishedValue = cultureVal.PublishedValue is JToken jsonPublishedValue
                    ? jsonPublishedValue.ToString(Formatting.None)
                    : cultureVal.PublishedValue?.ToString();
                var updatedPublishedVal =
                    FormatPropertyValue(publishedValue!, onlyMissingKeys).NullOrWhiteSpaceAsNull();
                cultureVal.PublishedValue = updatedPublishedVal;

                // Remove keys from edited/draft value & any nested properties
                var editedValue = cultureVal.EditedValue is JToken jsonEditedValue
                    ? jsonEditedValue.ToString(Formatting.None)
                    : cultureVal.EditedValue?.ToString();
                var updatedEditedVal = FormatPropertyValue(editedValue!, onlyMissingKeys).NullOrWhiteSpaceAsNull();
                cultureVal.EditedValue = updatedEditedVal;
            }
        }
    }
}
