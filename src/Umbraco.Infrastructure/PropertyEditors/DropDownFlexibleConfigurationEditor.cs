// Copyright (c) Umbraco.
// See LICENSE for more details.

using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class DropDownFlexibleConfigurationEditor : ConfigurationEditor<DropDownFlexibleConfiguration>
{
    public DropDownFlexibleConfigurationEditor(ILocalizedTextService textService, IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
        ConfigurationField items = Fields.First(x => x.Key == "items");

        // customize the items field
        items.Name = textService.Localize("editdatatype", "addPrevalue");
        items.Validators.Add(new ValueListUniqueValueValidator());
    }

    public override DropDownFlexibleConfiguration FromConfigurationEditor(
        IDictionary<string, object?>? editorValues,
        DropDownFlexibleConfiguration? configuration)
    {
        var output = new DropDownFlexibleConfiguration();

        if (editorValues is null || !editorValues.TryGetValue("items", out var jjj) || jjj is not JArray jItems)
        {
            return output; // oops
        }

        // handle multiple
        if (editorValues.TryGetValue("multiple", out var multipleObj))
        {
            Attempt<bool> convertBool = multipleObj.TryConvertTo<bool>();
            if (convertBool.Success)
            {
                output.Multiple = convertBool.Result;
            }
        }

        // auto-assigning our ids, get next id from existing values
        var nextId = 1;
        if (configuration?.Items != null && configuration.Items.Count > 0)
        {
            nextId = configuration.Items.Max(x => x.Id) + 1;
        }

        // create ValueListItem instances - sortOrder is ignored here
        foreach (JObject item in jItems.OfType<JObject>())
        {
            var value = item.Property("value")?.Value.Value<string>();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var id = item.Property("id")?.Value.Value<int>() ?? 0;
            if (id >= nextId)
            {
                nextId = id + 1;
            }

            output.Items.Add(new ValueListConfiguration.ValueListItem { Id = id, Value = value });
        }

        // ensure ids
        foreach (ValueListConfiguration.ValueListItem item in output.Items)
        {
            if (item.Id == 0)
            {
                item.Id = nextId++;
            }
        }

        return output;
    }

    public override Dictionary<string, object> ToConfigurationEditor(DropDownFlexibleConfiguration? configuration)
    {
        // map to what the editor expects
        var i = 1;
        var items =
            configuration?.Items.ToDictionary(x => x.Id.ToString(), x => new { value = x.Value, sortOrder = i++ }) ??
            new object();

        var multiple = configuration?.Multiple ?? false;

        return new Dictionary<string, object> { { "items", items }, { "multiple", multiple } };
    }
}
