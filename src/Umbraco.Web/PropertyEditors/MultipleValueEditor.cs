using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A value editor to handle posted json array data and to return array data for the multiple selected csv items
    /// </summary>
    /// <remarks>
    /// This is re-used by editors such as the multiple drop down list or check box list
    /// </remarks>
    internal class MultipleValueEditor : DataValueEditor
    {
        private readonly ILogger _logger;

        internal MultipleValueEditor(ILogger logger, DataEditorAttribute attribute)
            : base(attribute)
        {
            _logger = logger;
        }

        /// <summary>
        /// Override so that we can return an array to the editor for multi-select values
        /// </summary>
        /// <param name="property"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="culture"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
        {
            var json = base.ToEditor(property, dataTypeService, culture, segment).ToString();
            return JsonConvert.DeserializeObject<string[]>(json) ?? Array.Empty<string>();
        }

        /// <summary>
        /// When multiple values are selected a json array will be posted back so we need to format for storage in
        /// the database which is a comma separated string value
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public override object FromEditor(Core.Models.Editors.ContentPropertyData editorValue, object currentValue)
        {
            var json = editorValue.Value as JArray;
            if (json == null || json.HasValues == false)
            {
                return null;
            }

            var values = json.Select(item => item.Value<string>()).ToArray();
            if (values.Length == 0)
            {
                return null;
            }

            return JsonConvert.SerializeObject(values, Formatting.None);
        }
    }
}
