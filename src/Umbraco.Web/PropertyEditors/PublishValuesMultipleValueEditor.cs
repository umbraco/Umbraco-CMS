using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

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

        internal PublishValuesMultipleValueEditor(bool publishIds, ILogger logger, DataEditorAttribute attribute)
            : base(attribute, logger)
        {
            _publishIds = publishIds;
        }

        public PublishValuesMultipleValueEditor(bool publishIds, DataEditorAttribute attribute)
            : this(publishIds, Current.Logger, attribute)
        { }

        /// <summary>
        /// If publishing ids, we don't need to do anything, otherwise we need to look up the pre-values and get the string values
        /// </summary>
        /// <param name="propertyType"></param>
        /// <param name="propertyValue"></param>
        /// <param name="dataTypeService"></param>
        /// <returns></returns>
        public override string ConvertDbToString(PropertyType propertyType, object propertyValue, IDataTypeService dataTypeService)
        {
            if (propertyValue == null)
                return null;

            //publishing ids, so just need to return the value as-is
            if (_publishIds)
            {
                return propertyValue.ToString();
            }

            // get the multiple ids
            // if none, fallback to base
            var selectedIds = propertyValue.ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            if (selectedIds.Any() == false)
                return base.ConvertDbToString(propertyType, propertyValue, dataTypeService);

            // get the configuration items
            // if none, fallback to base
            var configuration = dataTypeService.GetDataType(propertyType.DataTypeId).ConfigurationAs<ValueListConfiguration>();
            if (configuration == null)
                return base.ConvertDbToString(propertyType, propertyValue, dataTypeService);

            var items = configuration.Items.Where(x => selectedIds.Contains(x.Id.ToInvariantString())).Select(x => x.Value);
            return string.Join(",", items);
        }

        /// <summary>
        /// Override so that we can return a json array to the editor for multi-select values
        /// </summary>
        /// <param name="property"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="languageId"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        public override object ToEditor(Property property, IDataTypeService dataTypeService, int? languageId = null, string segment = null)
        {
            var delimited = base.ToEditor(property, dataTypeService, languageId, segment).ToString();
            return delimited.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// When multiple values are selected a json array will be posted back so we need to format for storage in
        /// the database which is a comma separated ID value
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public override object FromEditor(Core.Models.Editors.ContentPropertyData editorValue, object currentValue)
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
