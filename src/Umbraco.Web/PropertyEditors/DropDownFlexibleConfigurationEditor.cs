using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    internal class DropDownFlexibleConfigurationEditor : ConfigurationEditor<DropDownFlexibleConfiguration>
    {
        public DropDownFlexibleConfigurationEditor(ILocalizedTextService textService)
        {
            var items = Fields.First(x => x.Key == "items");

            // customize the items field
            items.Name = textService.Localize("editdatatype/addPrevalue");
            items.Validators.Add(new ValueListUniqueValueValidator());
        }

        public override DropDownFlexibleConfiguration FromConfigurationEditor(Dictionary<string, object> editorValues, DropDownFlexibleConfiguration configuration)
        {
            var output = new DropDownFlexibleConfiguration();

            if (!editorValues.TryGetValue("items", out var jjj) || !(jjj is JArray jItems))
                return output; // oops

            // handle multiple
            if (editorValues.TryGetValue("multiple", out var multipleObj))
                output.Multiple = multipleObj.TryConvertTo<bool>();

            // auto-assigning our ids, get next id from existing values
            var nextId = 1;
            if (configuration?.Items != null && configuration.Items.Count > 0)
                nextId = configuration.Items.Max(x => x.Id) + 1;

            // create ValueListItem instances - sortOrder is ignored here
            foreach (var item in jItems.OfType<JObject>())
            {
                var value = item.Property("value")?.Value?.Value<string>();
                if (string.IsNullOrWhiteSpace(value)) continue;

                var id = item.Property("id")?.Value?.Value<int>() ?? 0;
                if (id >= nextId) nextId = id + 1;

                output.Items.Add(new ValueListConfiguration.ValueListItem { Id = id, Value = value });
            }

            // ensure ids
            foreach (var item in output.Items)
                if (item.Id == 0)
                    item.Id = nextId++;

            return output;
        }

        public override Dictionary<string, object> ToConfigurationEditor(DropDownFlexibleConfiguration configuration)
        {
            var items = configuration?.Items.ToDictionary(x => x.Id.ToString(), x => x.Value) ?? new object();
            var multiple = configuration?.Multiple ?? false;

            return new Dictionary<string, object>
            {
                { "items", items },
                { "multiple", multiple }
            };
        }

    }
}
