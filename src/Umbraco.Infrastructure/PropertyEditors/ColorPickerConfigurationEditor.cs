// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class ColorPickerConfigurationEditor : ConfigurationEditor<ColorPickerConfiguration>
{
    private readonly IJsonSerializer _jsonSerializer;

    public ColorPickerConfigurationEditor(IIOHelper ioHelper, IJsonSerializer jsonSerializer,
        IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
        _jsonSerializer = jsonSerializer;
        ConfigurationField items = Fields.First(x => x.Key == "items");

        // customize the items field
        items.View = "views/propertyeditors/colorpicker/colorpicker.prevalues.html";
        items.Description = "Add, remove or sort colors";
        items.Name = "Colors";
        items.Validators.Add(new ColorListValidator());
    }

    public override Dictionary<string, object> ToConfigurationEditor(ColorPickerConfiguration? configuration)
    {
        List<ValueListConfiguration.ValueListItem>? configuredItems = configuration?.Items; // ordered
        object editorItems;

        if (configuredItems == null)
        {
            editorItems = new object();
        }
        else
        {
            var d = new Dictionary<string, object>();
            editorItems = d;
            var sortOrder = 0;
            foreach (ValueListConfiguration.ValueListItem item in configuredItems)
            {
                d[item.Id.ToString()] = GetItemValue(item, configuration!.UseLabel, sortOrder++);
            }
        }

        var useLabel = configuration?.UseLabel ?? false;

        return new Dictionary<string, object> { { "items", editorItems }, { "useLabel", useLabel } };
    }

    // send: { "items": { "<id>": { "value": "<color>", "label": "<label>", "sortOrder": <sortOrder> } , ... }, "useLabel": <bool> }
    // recv: { "items": ..., "useLabel": <bool> }
    public override ColorPickerConfiguration FromConfigurationEditor(
        IDictionary<string, object?>? editorValues,
        ColorPickerConfiguration? configuration)
    {
        var output = new ColorPickerConfiguration();

        if (editorValues is null || !editorValues.TryGetValue("items", out var jjj) || !(jjj is JArray jItems))
        {
            return output; // oops
        }

        // handle useLabel
        if (editorValues.TryGetValue("useLabel", out var useLabelObj))
        {
            Attempt<bool> convertBool = useLabelObj.TryConvertTo<bool>();
            if (convertBool.Success)
            {
                output.UseLabel = convertBool.Result;
            }
        }

        // auto-assigning our ids, get next id from existing values
        var nextId = 1;
        if (configuration?.Items != null && configuration.Items.Count > 0)
        {
            nextId = configuration.Items.Max(x => x.Id) + 1;
        }

        // create ValueListItem instances - ordered (items get submitted in the sorted order)
        foreach (JObject item in jItems.OfType<JObject>())
        {
            // in:  { "value": "<color>", "id": <id>, "label": "<label>" }
            // out: ValueListItem, Id = <id>, Value = <color> | { "value": "<color>", "label": "<label>" }
            //                                        (depending on useLabel)
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

            var label = item.Property("label")?.Value.Value<string>();
            value = _jsonSerializer.Serialize(new { value, label });

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

    private object GetItemValue(ValueListConfiguration.ValueListItem item, bool useLabel, int sortOrder)
    {
        // in:  ValueListItem, Id = <id>, Value = <color> | { "value": "<color>", "label": "<label>" }
        //                                        (depending on useLabel)
        // out: { "value": "<color>", "label": "<label>", "sortOrder": <sortOrder> }
        var v = new ItemValue { Color = item.Value, Label = item.Value, SortOrder = sortOrder };

        if (item.Value?.DetectIsJson() ?? false)
        {
            try
            {
                ItemValue? o = _jsonSerializer.Deserialize<ItemValue>(item.Value);
                o!.SortOrder = sortOrder;
                return o;
            }
            catch
            {
                // parsing Json failed, don't do anything, get the value (sure?)
                return new ItemValue { Color = item.Value, Label = item.Value, SortOrder = sortOrder };
            }
        }

        return new ItemValue { Color = item.Value, Label = item.Value, SortOrder = sortOrder };
    }

    internal class ColorListValidator : IValueValidator
    {
        public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
        {
            if (!(value is JArray json))
            {
                yield break;
            }

            // validate each item which is a json object
            for (var index = 0; index < json.Count; index++)
            {
                JToken i = json[index];
                if (!(i is JObject jItem) || jItem["value"] == null)
                {
                    continue;
                }

                // NOTE: we will be removing empty values when persisting so no need to validate
                var asString = jItem["value"]?.ToString();
                if (asString.IsNullOrWhiteSpace())
                {
                    continue;
                }

                if (Regex.IsMatch(asString!, "^([0-9a-f]{3}|[0-9a-f]{6})$", RegexOptions.IgnoreCase) == false)
                {
                    yield return new ValidationResult("The value " + asString + " is not a valid hex color", new[]
                    {
                        // we'll make the server field the index number of the value so it can be wired up to the view
                        "item_" + index.ToInvariantString(),
                    });
                }
            }
        }
    }

    // represents an item we are exchanging with the editor
    [DataContract]
    private class ItemValue
    {
        [DataMember(Name = "value")]
        public string? Color { get; set; }

        [DataMember(Name = "label")]
        public string? Label { get; set; }

        [DataMember(Name = "sortOrder")]
        public int SortOrder { get; set; }
    }
}
