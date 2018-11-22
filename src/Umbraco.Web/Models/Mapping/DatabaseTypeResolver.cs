using System;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Gets the DataTypeDatabaseType from the selected property editor for the data type
    /// </summary>
    internal class DatabaseTypeResolver
    {
        public ValueStorageType Resolve(DataTypeSave source)
        {
            if (!Current.PropertyEditors.TryGet(source.EditorAlias, out var editor))
                throw new InvalidOperationException($"Could not find property editor \"{source.EditorAlias}\".");

            // fixme - what about source.PropertyEditor? can we get the configuration here? 'cos it may change the storage type?!
            var valueType = editor.GetValueEditor().ValueType;
            return ValueTypes.ToStorageType(valueType);
        }
    }
}
