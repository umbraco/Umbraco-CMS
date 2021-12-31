using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Utility class for dealing with <see cref="ContentService"/> Copying/Saving events for complex editors
    /// </summary>
    internal class ComplexPropertyEditorContentEventHandler : IDisposable
    {
        private readonly string _editorAlias;
        private readonly Func<string, bool, string> _formatPropertyValue;
        private bool _disposedValue;

        public ComplexPropertyEditorContentEventHandler(string editorAlias,
            Func<string, bool, string> formatPropertyValue)
        {
            _editorAlias = editorAlias;
            _formatPropertyValue = formatPropertyValue;
            ContentService.Copying += ContentService_Copying;
            ContentService.Saving += ContentService_Saving;
        }

        /// <summary>
        /// <see cref="ContentService"/> Copying event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentService_Copying(IContentService sender, CopyEventArgs<IContent> e)
        {
            var props = e.Copy.GetPropertiesByEditor(_editorAlias);
            UpdatePropertyValues(props, false);
        }

        /// <summary>
        /// <see cref="ContentService"/> Saving event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentService_Saving(IContentService sender, ContentSavingEventArgs e)
        {
            foreach (var entity in e.SavedEntities)
            {
                var props = entity.GetPropertiesByEditor(_editorAlias);
                UpdatePropertyValues(props, true);
            }
        }

        private void UpdatePropertyValues(IEnumerable<Property> props, bool onlyMissingKeys)
        {
            foreach (var prop in props)
            {
                // A Property may have one or more values due to cultures
                var propVals = prop.Values;
                foreach (var cultureVal in propVals)
                {
                    // Remove keys from published value & any nested properties
                    var publishedValue = cultureVal.PublishedValue is JToken jsonPublishedValue ? jsonPublishedValue.ToString(Formatting.None) : cultureVal.PublishedValue?.ToString();
                    var updatedPublishedVal = _formatPropertyValue(publishedValue, onlyMissingKeys).NullOrWhiteSpaceAsNull();
                    cultureVal.PublishedValue = updatedPublishedVal;

                    // Remove keys from edited/draft value & any nested properties
                    var editedValue = cultureVal.EditedValue is JToken jsonEditedValue ? jsonEditedValue.ToString(Formatting.None) : cultureVal.EditedValue?.ToString();
                    var updatedEditedVal = _formatPropertyValue(editedValue, onlyMissingKeys).NullOrWhiteSpaceAsNull();
                    cultureVal.EditedValue = updatedEditedVal;
                }
            }
        }

        /// <summary>
        /// Unbinds from events
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    ContentService.Copying -= ContentService_Copying;
                    ContentService.Saving -= ContentService_Saving;
                }
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Unbinds from events
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
