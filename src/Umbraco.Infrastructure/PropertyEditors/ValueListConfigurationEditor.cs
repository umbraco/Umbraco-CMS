// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Pre-value editor used to create a list of items
/// </summary>
/// <remarks>
///     This pre-value editor is shared with editors like drop down, checkbox list, etc....
/// </remarks>
public class ValueListConfigurationEditor : ConfigurationEditor<ValueListConfiguration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public ValueListConfigurationEditor(ILocalizedTextService textService, IIOHelper ioHelper)
        : this(textService, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public ValueListConfigurationEditor(ILocalizedTextService textService, IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
        ConfigurationField items = Fields.First(x => x.Key == "items");

        // customize the items field
        items.Name = textService.Localize("editdatatype", "addPrevalue");
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
    public override Dictionary<string, object> ToConfigurationEditor(ValueListConfiguration? configuration)
    {
        if (configuration == null)
        {
            return new Dictionary<string, object> { { "items", new object() } };
        }

        // map to what the (still v7) editor expects
        // {"item":{"169":{"value":"a","sortOrder":1},"170":{"value":"b","sortOrder":2},"171":{"value":"c","sortOrder":3}}}
        var i = 1;
        return new Dictionary<string, object>
        {
            {
                "items",
                configuration.Items.ToDictionary(x => x.Id.ToString(), x => new { value = x.Value, sortOrder = i++ })
            },
        };
    }

    /// <inheritdoc />
    public override ValueListConfiguration FromConfigurationEditor(
        IDictionary<string, object?>? editorValues,
        ValueListConfiguration? configuration)
    {
        var output = new ValueListConfiguration();

        if (editorValues is null || !editorValues.TryGetValue("items", out var jjj) || !(jjj is JArray jItems))
        {
            return output; // oops
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
}
