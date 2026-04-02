// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Handles nested Udi keys when
/// - saving: Empty keys get generated
/// - copy: keys get replaced by new ones while keeping references intact
/// - scaffolding: keys get replaced by new ones while keeping references intact
/// </summary>
public abstract class ComplexPropertyEditorContentNotificationHandler :
    INotificationHandler<ContentSavingNotification>,
    INotificationHandler<ContentCopyingNotification>,
    INotificationHandler<ContentScaffoldedNotification>
{
    protected abstract string EditorAlias { get; }

    /// <summary>
    /// Handles a <see cref="ContentCopyingNotification"/> by updating property values for all properties
    /// that use the configured editor alias on the copied content.
    /// </summary>
    /// <param name="notification">The notification containing information about the content being copied.</param>
    public void Handle(ContentCopyingNotification notification)
    {
        IEnumerable<IProperty> props = notification.Copy.GetPropertiesByEditor(EditorAlias);
        UpdatePropertyValues(props, false);
    }

    /// <summary>
    /// Handles a <see cref="ContentSavingNotification"/> by updating property values for all properties
    /// that use the configured editor alias on the content being saved.
    /// </summary>
    /// <param name="notification">The notification containing information about the content being saved.</param>
    /// <remarks>This ensures that property values are updated appropriately before the content is persisted.</remarks>
    public void Handle(ContentSavingNotification notification)
    {
        foreach (IContent entity in notification.SavedEntities)
        {
            IEnumerable<IProperty> props = entity.GetPropertiesByEditor(EditorAlias);
            UpdatePropertyValues(props, true);
        }
    }

    /// <summary>
    /// Handles a <see cref="ContentScaffoldedNotification"/> by updating property values for properties
    /// that use the complex property editor.
    /// </summary>
    /// <param name="notification">The <see cref="ContentScaffoldedNotification"/> containing event data for the content scaffolded operation.</param>
    public void Handle(ContentScaffoldedNotification notification)
    {
        IEnumerable<IProperty> props = notification.Scaffold.GetPropertiesByEditor(EditorAlias);
        UpdatePropertyValues(props, false);
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
                var publishedValue = cultureVal.PublishedValue?.ToString();
                var updatedPublishedVal =
                    FormatPropertyValue(publishedValue!, onlyMissingKeys).NullOrWhiteSpaceAsNull();
                cultureVal.PublishedValue = updatedPublishedVal;

                // Remove keys from edited/draft value & any nested properties
                var editedValue = cultureVal.EditedValue?.ToString();
                var updatedEditedVal = FormatPropertyValue(editedValue!, onlyMissingKeys).NullOrWhiteSpaceAsNull();
                cultureVal.EditedValue = updatedEditedVal;
            }
        }
    }
}
