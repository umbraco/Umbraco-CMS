using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a base generic ContentPropertyBasic from a Property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ContentPropertyBasicConverter<T> : TypeConverter<Property, T>
        where T : ContentPropertyBasic, new()
    {
        /// <summary>
        /// Assigns the PropertyEditor, Id, Alias and Value to the property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected override T ConvertCore(Property property)
        {
            var editor = PropertyEditorResolver.Current.GetById(property.PropertyType.DataTypeId);
            if (editor == null)
            {
                //TODO: Remove this check as we shouldn't support this at all!
                var legacyEditor = DataTypesResolver.Current.GetById(property.PropertyType.DataTypeId);
                if (legacyEditor == null)
                {
                    throw new NullReferenceException("The property editor with id " + property.PropertyType.DataTypeId + " does not exist");
                }

                var legacyResult = new T
                    {
                        Id = property.Id,
                        Value = property.Value == null ? "" : property.Value.ToString(),
                        Alias = property.Alias
                    };
                return legacyResult;
            }
            var result = new T
                {
                    Id = property.Id,
                    Value = editor.ValueEditor.SerializeValue(property.Value),
                    Alias = property.Alias
                };

            result.PropertyEditor = editor;

            return result;
        }
    }
}