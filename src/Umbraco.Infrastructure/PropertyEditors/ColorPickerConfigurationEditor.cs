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
}
