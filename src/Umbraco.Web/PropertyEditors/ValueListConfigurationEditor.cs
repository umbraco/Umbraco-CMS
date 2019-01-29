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
            var items = Fields.First(x => x.Key == "items");

            // customize the items field
            items.Name = textService.Localize("editdatatype/addPrevalue");
            items.Validators.Add(new ValueListUniqueValueValidator());
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
        public override Dictionary<string, object> ToConfigurationEditor(ValueListConfiguration configuration)
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
        public override ValueListConfiguration FromConfigurationEditor(IDictionary<string, object> editorValues, ValueListConfiguration configuration)
        {
            var output = new ValueListConfiguration();

            if (editorValues.ContainsKey("items") == false)
                return output;

            var jsonObject = (JObject) editorValues["items"];
            if (jsonObject == null)
                return output;

            // auto-assigning our ids, get next id from existing values
            var nextId = 1;
            if (configuration?.Items != null && configuration.Items.Count > 0)
                nextId = configuration.Items.Max(x => x.Id) + 1;

            foreach (var itemValue in jsonObject.Children())
            {
                var listItem = itemValue.First;
                var value = listItem["value"].Value<string>();
                if(string.IsNullOrEmpty(value))
                    continue;;

                int.TryParse(itemValue.Path, out var id);
                if (id >= nextId)
                    nextId = id + 1;

                output.Items.Add(new ValueListConfiguration.ValueListItem
                {
                    Id = id,
                    Value = value
                });
            }

            output.Items.ForEach(item =>
            {
                if (item.Id == 0)
                    item.Id = nextId++;
            });

            return output;
        }
    }
}
