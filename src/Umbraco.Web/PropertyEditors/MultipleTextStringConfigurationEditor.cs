using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for a multiple testring value editor.
    /// </summary>
    internal class MultipleTextStringConfigurationEditor : ConfigurationEditor<MultipleTextStringConfiguration>
    {
        public MultipleTextStringConfigurationEditor()
        {
            Fields.Add(new ConfigurationField(new IntegerValidator())
            {
                Description = "Enter the minimum amount of text boxes to be displayed",
                Key = "min",
                View = "requiredfield",
                Name = "Minimum",
                PropertyName = nameof(MultipleTextStringConfiguration.Minimum)
            });

            Fields.Add(new ConfigurationField(new IntegerValidator())
            {
                Description = "Enter the maximum amount of text boxes to be displayed, enter 0 for unlimited",
                Key = "max",
                View = "requiredfield",
                Name = "Maximum",
                PropertyName = nameof(MultipleTextStringConfiguration.Maximum)
            });
        }

        /// <inheritdoc />
        public override MultipleTextStringConfiguration FromConfigurationEditor(IDictionary<string, object> editorValues, MultipleTextStringConfiguration configuration)
        {
            // todo this isn't pretty
            //the values from the editor will be min/max fieds and we need to format to json in one field
            // is the editor sending strings or ints or?!
            var min = (editorValues.ContainsKey("min") ? editorValues["min"].ToString() : "0").TryConvertTo<int>();
            var max = (editorValues.ContainsKey("max") ? editorValues["max"].ToString() : "0").TryConvertTo<int>();

            return new MultipleTextStringConfiguration
            {
                Minimum = min ? min.Result : 0,
                Maximum = max ? max.Result : 0
            };
        }

        /// <inheritdoc />
        public override Dictionary<string, object> ToConfigurationEditor(MultipleTextStringConfiguration configuration)
        {
            return new Dictionary<string, object>
            {
                { "min", configuration.Minimum },
                { "max", configuration.Maximum }
            };
        }
    }
}
