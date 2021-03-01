// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    // TODO: insert these notification handlers in core composition
    public abstract class ComplexPropertyEditorContentNotificationHandler :
        INotificationHandler<SavingNotification<IContent>>,
        INotificationHandler<CopyingNotification<IContent>>
    {
        protected abstract string EditorAlias { get; }

        protected abstract string FormatPropertyValue(string rawJson, bool onlyMissingKeys);

        public void Handle(SavingNotification<IContent> notification)
        {
            foreach (var entity in notification.SavedEntities)
            {
                var props = entity.GetPropertiesByEditor(EditorAlias);
                UpdatePropertyValues(props, true);
            }
        }

        public void Handle(CopyingNotification<IContent> notification)
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
                    var updatedPublishedVal = FormatPropertyValue(cultureVal.PublishedValue?.ToString(), onlyMissingKeys);
                    cultureVal.PublishedValue = updatedPublishedVal;

                    // Remove keys from edited/draft value & any nested properties
                    var updatedEditedVal = FormatPropertyValue(cultureVal.EditedValue?.ToString(), onlyMissingKeys);
                    cultureVal.EditedValue = updatedEditedVal;
                }
            }
        }
    }
}
