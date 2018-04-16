using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a configuration editor for the "drop down list multiple" property editor.
    /// </summary>
    /// <remarks>
    /// <para>Ensures that 'multiple' is saved for the config in the db but is not a configuration field.</para>
    /// <para>This is mostly to maintain backwards compatibility with old property editors. Devs can now simply
    /// use the "drop down" property editor and check the "multiple" configuration checkbox</para>
    /// <para>fixme what is multiple exactly?!</para>
    /// </remarks>
    internal class DropDownMultipleConfigurationEditor : ValueListConfigurationEditor
    {
        public DropDownMultipleConfigurationEditor(ILocalizedTextService textService)
            : base(textService)
        {
            Fields.Add(new ConfigurationField
            {
                Key = "multiple",
                Name = "multiple",
                View = "hidden", // so it does not show in the configuration editor
                HideLabel = true
            });
        }

        // editor...
        //
        // receives:
        // "preValues":[
        //  {
        //    "label":"Add prevalue",
        //    "description":"Add and remove values for the list",
        //    "hideLabel":false,
        //    "view":"multivalues",
        //    "config":{},
        //    "key":"items",
        //    "value":{"169":{"value":"a","sortOrder":1},"170":{"value":"b","sortOrder":2},"171":{"value":"c","sortOrder":3}}
        //  },
        //  {
        //    "label":"multiple",
        //    "description":null,
        //    "hideLabel":true,
        //    "view":"hidden",
        //    "config":{},
        //    "key":"multiple",
        //    "value":"1"
        //  }]
        //
        // posts ('d' being a new value):
        // [{key: "items", value: [{value: "a", sortOrder: 1, id: "169"}, {value: "c", sortOrder: 3, id: "171"}, {value: "d"}]}, {key: "multiple", value: "1"}]
        //
        // the 'multiple' thing never goes to DB
        // values go to DB with alias 0, 1, 2 + their ID + value
        // the sort order that comes back makes no sense
        //
        // FromEditor can totally ignore 'multiple'

        /// <inheritdoc/>
        public override Dictionary<string, object> ToConfigurationEditor(ValueListConfiguration configuration)
        {
            var dictionary = base.ToConfigurationEditor(configuration);

            // always add the multiple field, as 'true'
            dictionary["multiple"] = 1;

            return dictionary;
        }
    }
}