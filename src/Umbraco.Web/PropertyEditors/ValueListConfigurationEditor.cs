using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Services;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Pre-value editor used to create a list of items
    /// </summary>
    /// <remarks>
    /// This pre-value editor is shared with editors like drop down, checkbox list, etc....
    /// </remarks>
    internal class ValueListConfigurationEditor : ConfigurationEditor<ValueListConfiguration>
    {
        public ValueListConfigurationEditor(ILocalizedTextService textService)
        {
            Fields.Add(new ConfigurationField(new ValueListUniqueValueValidator())
            {
                Description = "Add and remove values for the list",
                Key = "items",
                Name = textService.Localize("editdatatype/addPrevalue"),
                View = "multivalues"
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
        //  }]
        //
        // posts ('d' being a new value):
        // [{key: "items", value: [{value: "a", sortOrder: 1, id: "169"}, {value: "c", sortOrder: 3, id: "171"}, {value: "d"}]}]
        //
        // values go to DB with alias 0, 1, 2 + their ID + value
        // the sort order that comes back makes no sense

        /// <inheritdoc />
        public override Dictionary<string, object> ToEditor(ValueListConfiguration configuration)
        {
            if (configuration == null)
                return new Dictionary<string, object>
                {
                    { "items", new object() }
                };

            // map to what the (still v7) editor expects
            // {"item":{"169":{"value":"a","sortOrder":1},"170":{"value":"b","sortOrder":2},"171":{"value":"c","sortOrder":3}}}

            var i = 1;
            return new Dictionary<string, object>
            {
                { "items", configuration.Items.ToDictionary(x => x.Id.ToString(), x => new { value = x.Value, sortOrder = i++ }) }
            };
        }

        /// <inheritdoc />
        public override ValueListConfiguration FromEditor(Dictionary<string, object> editorValue, ValueListConfiguration configuration)
        {
            var output = new ValueListConfiguration();

            if (!editorValue.TryGetValue("items", out var jjj) || !(jjj is JArray jItems))
                return output; // oops

            // auto-assigning our ids, get next id from existing values
            var nextId = configuration.Items.Max(x => x.Id) + 1;

            // create ValueListItem instances - sortOrder is ignored here
            foreach (var item in jItems.OfType<JObject>())
            {
                var valueProp = item.Property("value");
                if (valueProp == null || valueProp.Type != JTokenType.String) continue;
                var value = valueProp.Value<string>();
                if (string.IsNullOrWhiteSpace(value)) continue;

                var idProp = item.Property("id");
                var id = idProp != null && idProp.Type == JTokenType.Integer ? idProp.Value<int>() : 0;

                if (id >= nextId) nextId = id + 1;

                output.Items.Add(new ValueListConfiguration.ValueListItem { Id = id, Value = value });
            }

            // ensure ids
            foreach (var item in output.Items)
                if (item.Id == 0)
                    item.Id = nextId++;

            return output;
        }
    }
}
