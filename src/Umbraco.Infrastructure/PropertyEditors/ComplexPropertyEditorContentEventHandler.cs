// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Services.Implement;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Utility class for dealing with <see cref="ContentService"/> Copying/Saving events for complex editors
    /// </summary>
    public class ComplexPropertyEditorContentEventHandler : IDisposable
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

        private void UpdatePropertyValues(IEnumerable<IProperty> props, bool onlyMissingKeys)
        {
            foreach (var prop in props)
            {
                // A Property may have one or more values due to cultures
                var propVals = prop.Values;
                foreach (var cultureVal in propVals)
                {
                    // Remove keys from published value & any nested properties
                    var updatedPublishedVal = _formatPropertyValue(cultureVal.PublishedValue?.ToString(), onlyMissingKeys);
                    cultureVal.PublishedValue = updatedPublishedVal;

                    // Remove keys from edited/draft value & any nested properties
                    var updatedEditedVal = _formatPropertyValue(cultureVal.EditedValue?.ToString(), onlyMissingKeys);
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
