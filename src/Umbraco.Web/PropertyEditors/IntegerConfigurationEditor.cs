using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A custom pre-value editor class to deal with the legacy way that the pre-value data is stored.
    /// </summary>
    internal class IntegerConfigurationEditor : ConfigurationEditor
    {
        private readonly Dictionary<string, object> _config = new Dictionary<string, object>() { { "integer", true } };

        public IntegerConfigurationEditor()
        {
            Fields.Add(new ConfigurationField(new IntegerValidator())
            {
                Description = "Enter the minimum amount of number to be entered",
                Key = "min",
                View = "number",
                Name = "Minimum",
                Config = _config
            });

            Fields.Add(new ConfigurationField(new IntegerValidator())
            {
                Description = "Enter the intervals amount between each step of number to be entered",
                Key = "step",
                View = "number",
                Name = "Step Size",
                Config = _config
            });

            Fields.Add(new ConfigurationField(new IntegerValidator())
            {
                Description = "Enter the maximum amount of number to be entered",
                Key = "max",
                View = "number",
                Name = "Maximum",
                Config = _config
            });
        }
    }
}
