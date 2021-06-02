using System.Collections.Generic;

namespace Umbraco.Core.PropertyEditors
{
    public static partial class ConfigurationFieldsExtensions
    {
        /// <summary>
        /// Adds a configuration field.
        /// </summary>
        /// <param name="fields">The list of configuration fields.</param>
        /// <param name="key">The key (alias) of the field.</param>
        /// <param name="name">The name (label) of the field.</param>
        /// <param name="description">The description for the field.</param>
        /// <param name="view">The path to the editor view to be used for the field.</param>
        /// <param name="config">Optional configuration used for field's editor.</param>
        public static void Add(
            this List<ConfigurationField> fields,
            string key,
            string name,
            string description,
            string view,
            IDictionary<string, object> config = null)
        {
            fields.Add(new ConfigurationField
            {
                Key = key,
                Name = name,
                Description = description,
                View = view,
                Config = config,
            });
        }
    }
}
