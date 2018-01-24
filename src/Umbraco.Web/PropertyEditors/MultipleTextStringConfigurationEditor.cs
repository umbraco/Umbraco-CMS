using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A custom pre-value editor class to deal with the legacy way that the pre-value data is stored.
    /// </summary>
    internal class MultipleTextStringConfigurationEditor : ConfigurationEditor
    {
        private readonly ILogger _logger;

        public MultipleTextStringConfigurationEditor(ILogger logger)
        {
            _logger = logger;

            Fields.Add(new ConfigurationField(new IntegerValidator())
            {
                Description = "Enter the minimum amount of text boxes to be displayed",
                Key = "min",
                View = "requiredfield",
                Name = "Minimum"
            });

            Fields.Add(new ConfigurationField(new IntegerValidator())
            {
                Description = "Enter the maximum amount of text boxes to be displayed, enter 0 for unlimited",
                Key = "max",
                View = "requiredfield",
                Name = "Maximum"
            });
        }

        /// <summary>
        /// Need to change how we persist the values so they are compatible with the legacy way we store values
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public override IDictionary<string, PreValue> ConvertEditorToDb(IDictionary<string, object> editorValue, PreValueCollection currentValue)
        {
            //the values from the editor will be min/max fieds and we need to format to json in one field
            var min = (editorValue.ContainsKey("min") ? editorValue["min"].ToString() : "0").TryConvertTo<int>();
            var max = (editorValue.ContainsKey("max") ? editorValue["max"].ToString() : "0").TryConvertTo<int>();

            var json = JObject.FromObject(new {Minimum = min.Success ? min.Result : 0, Maximum = max.Success ? max.Result : 0});

            return new Dictionary<string, PreValue> { { "0", new PreValue(json.ToString(Formatting.None)) } };
        }

        /// <summary>
        /// Need to deal with the legacy way of storing pre-values and turn them into nice values for the editor
        /// </summary>
        /// <param name="defaultPreVals"></param>
        /// <param name="persistedPreVals"></param>
        /// <returns></returns>
        public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
        {
            var preVals = persistedPreVals.FormatAsDictionary();
            var stringVal = preVals.Any() ? preVals.First().Value.Value : "";
            var returnVal = new Dictionary<string, object> { { "min", 0 }, { "max", 0 } };
            if (stringVal.IsNullOrWhiteSpace() == false)
            {
                try
                {
                    var json = JsonConvert.DeserializeObject<JObject>(stringVal);
                    if (json["Minimum"] != null)
                    {
                        //by default pre-values are sent out with an id/value pair
                        returnVal["min"] = json["Minimum"].Value<int>();
                    }
                    if (json["Maximum"] != null)
                    {
                        returnVal["max"] = json["Maximum"].Value<int>();
                    }
                }
                catch (Exception e)
                {
                    // this shouldn't happen unless there's already a bad formatted pre-value
                    _logger.Warn<MultipleTextStringConfigurationEditor>(e, "Could not deserialize value to json " + stringVal);
                    return returnVal;
                }
            }

            return returnVal;
        }
    }
}