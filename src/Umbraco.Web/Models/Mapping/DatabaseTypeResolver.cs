using System;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Gets the DataTypeDatabaseType from the selected property editor for the data type
    /// </summary>
    internal class DatabaseTypeResolver : ValueResolver<DataTypeSave, DataTypeDatabaseType>
    {
        protected override DataTypeDatabaseType ResolveCore(DataTypeSave source)
        {
            var propertyEditor = PropertyEditorResolver.Current.GetByAlias(source.SelectedEditor);
            if (propertyEditor == null)
            {
                throw new InvalidOperationException("Could not find property editor with id " + source.SelectedEditor);
            }
            return propertyEditor.ValueEditor.GetDatabaseType();
        }
    }
}