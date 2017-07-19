using System;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Gets the DataTypeDatabaseType from the selected property editor for the data type
    /// </summary>
    internal class DatabaseTypeResolver
    {
        public DataTypeDatabaseType Resolve(DataTypeSave source)
        {
            var propertyEditor = Current.PropertyEditors[source.SelectedEditor];
            if (propertyEditor == null)
            {
                throw new InvalidOperationException("Could not find property editor with id " + source.SelectedEditor);
            }
            return propertyEditor.ValueEditor.GetDatabaseType();
        }
    }
}