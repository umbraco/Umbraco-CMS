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
    /// Custom value editor to handle posted json data and to return json data for the multiple selected items.
    /// </summary>
    internal class DropDownMultipleValueEditor : DropDownValueEditor
    {
        private readonly bool _publishIds;

        internal DropDownMultipleValueEditor(bool publishIds, IDataTypeService dataTypeService, ValueEditor wrapped)
            : base(dataTypeService, wrapped)
        {
            _publishIds = publishIds;
        }

        public DropDownMultipleValueEditor(bool publishIds, ValueEditor wrapped)
            : this(publishIds, ApplicationContext.Current.Services.DataTypeService, wrapped)
        {
        }

        /// <summary>
        /// If publishing ids, we don't need to do anything, otherwise we need to look up the pre-values and get the string values
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public override object FormatValueForCache(Property property)
        {
            if (_publishIds)
            {
                return base.FormatValueForCache(property);
            }

            var selectedIds = property.Value.ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            if (selectedIds.Any() == false)
            {
                return base.FormatValueForCache(property);
            }

            var preValues = GetPreValues(property);
            if (preValues != null)
            {
                //get all pre-values matching our Ids
                return string.Join(",", 
                                   preValues.Where(x => selectedIds.Contains(x.Value.Id.ToInvariantString())).Select(x => x.Value.Value));
            }

            return base.FormatValueForCache(property);
        }

        /// <summary>
        /// Override so that we can return a json array to the editor for multi-select values
        /// </summary>
        /// <param name="dbValue"></param>
        /// <returns></returns>
        public override object FormatDataForEditor(object dbValue)
        {
            var delimited = base.FormatDataForEditor(dbValue).ToString();
            return delimited.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// When multiple values are selected a json array will be posted back so we need to format for storage in 
        /// the database which is a comma separated ID value
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public override object FormatDataForPersistence(Core.Models.Editors.ContentPropertyData editorValue, object currentValue)
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