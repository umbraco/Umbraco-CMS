using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the tag value editor.
    /// </summary>
    public class TagConfigurationEditor : ConfigurationEditor<TagConfiguration>
    {
        public TagConfigurationEditor(ManifestValueValidatorCollection validators)
        {
            Field(nameof(TagConfiguration.Group)).Validators.Add(new RequiredValidator());
            Field(nameof(TagConfiguration.StorageType)).Validators.Add(new RequiredValidator());
        }

        public override Dictionary<string, object> ToConfigurationEditor(TagConfiguration configuration)
        {
            var dictionary = base.ToConfigurationEditor(configuration);

            // the front-end editor expects the string value of the storage type
            if (!dictionary.TryGetValue("storageType", out var storageType))
                storageType = TagsStorageType.Json; //default to Json
            dictionary["storageType"] = storageType.ToString();

            return dictionary;
        }

        public override TagConfiguration FromConfigurationEditor(IDictionary<string, object> editorValues, TagConfiguration configuration)
        {
            // the front-end editor retuns the string value of the storage type
            // pure Json could do with
            // [JsonConverter(typeof(StringEnumConverter))]
            // but here we're only deserializing to object and it's too late

            editorValues["storageType"] = Enum.Parse(typeof(TagsStorageType), (string) editorValues["storageType"]);
            return base.FromConfigurationEditor(editorValues, configuration);
        }
    }
}
