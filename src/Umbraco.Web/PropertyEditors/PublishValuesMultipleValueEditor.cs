using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Custom value editor to handle posted json data and to return json data for the multiple selected items as well as ensuring
    /// that the multiple selected int IDs are published to cache as a delimited string (values)
    /// </summary>
    /// <remarks>
    /// This is re-used by editors such as the multiple drop down list or check box list
    /// </remarks>
    internal class PublishValuesMultipleValueEditor : PublishValueValueEditor
    {
        private readonly bool _publishIds;

        internal PublishValuesMultipleValueEditor(bool publishIds, IDataTypeService dataTypeService, PropertyValueEditor wrapped)
            : base(dataTypeService, wrapped)
        {
            _publishIds = publishIds;
        }

        public PublishValuesMultipleValueEditor(bool publishIds, PropertyValueEditor wrapped)
            : this(publishIds, ApplicationContext.Current.Services.DataTypeService, wrapped)
        {
        }

        /// <summary>
        /// If publishing ids, we don't need to do anything, otherwise we need to look up the pre-values and get the string values
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyType"></param>
        /// <param name="dataTypeService"></param>
        /// <returns></returns>
        public override string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            if (property.Value == null)
                return null;

            //publishing ids, so just need to return the value as-is
            if (_publishIds)
            {
                return property.Value.ToString();
            }

            //get the multiple ids
            var selectedIds = property.Value.ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            if (selectedIds.Any() == false)
            {
                //nothing there
                return base.ConvertDbToString(property, propertyType, dataTypeService);
            }

            var preValues = GetPreValues(property);
            if (preValues != null)
            {
                //get all pre-values matching our Ids
                return string.Join(",", 
                                   preValues.Where(x => selectedIds.Contains(x.Value.Id.ToInvariantString())).Select(x => x.Value.Value));
            }

            return base.ConvertDbToString(property, propertyType, dataTypeService);
        }

        /// <summary>
        /// Override so that we can return a json array to the editor for multi-select values
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyType"></param>
        /// <param name="dataTypeService"></param>
        /// <returns></returns>
        public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            var delimited = base.ConvertDbToEditor(property, propertyType, dataTypeService).ToString();
            return delimited.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// When multiple values are selected a json array will be posted back so we need to format for storage in 
        /// the database which is a comma separated ID value
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public override object ConvertEditorToDb(Core.Models.Editors.ContentPropertyData editorValue, object currentValue)
        {
            var json = editorValue.Value as JArray;
            if (json == null)
            {
                return null;
            }

            var values = json.Select(item => item.Value<string>()).ToList();
            //change to delimited
            return string.Join(",", values);
        }
    }
}