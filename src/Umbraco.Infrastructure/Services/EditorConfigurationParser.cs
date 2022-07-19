using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Services;

public class EditorConfigurationParser : IEditorConfigurationParser
{
    public TConfiguration? ParseFromConfigurationEditor<TConfiguration>(IDictionary<string, object?>? editorValues, IEnumerable<ConfigurationField> fields)
    {
        // note - editorValue contains a mix of CLR types (string, int...) and JToken
        // turning everything back into a JToken... might not be fastest but is simplest
        // for now
        var o = new JObject();

        foreach (ConfigurationField field in fields)
        {
            // field only, JsonPropertyAttribute is ignored here
            // only keep fields that have a non-null/empty value
            // rest will fall back to default during ToObject()
            if (editorValues is not null && editorValues.TryGetValue(field.Key, out var value) && value != null &&
                (!(value is string stringValue) || !string.IsNullOrWhiteSpace(stringValue)))
            {
                if (value is JToken jtoken)
                {
                    // If it's a jtoken then set it
                    o[field.PropertyName!] = jtoken;
                }
                else if (field.PropertyType == typeof(bool) && value is string sBool)
                {
                    // If it's a boolean property type but a string is found, try to do a conversion
                    Attempt<bool> converted = sBool.TryConvertTo<bool>();
                    if (converted.Success)
                    {
                        o[field.PropertyName!] = converted.Result;
                    }
                }
                else
                {
                    // Default behavior
                    o[field.PropertyName!] = JToken.FromObject(value);
                }
            }
        }

        return o.ToObject<TConfiguration>();
    }

    public Dictionary<string, object> ParseToConfigurationEditor<TConfiguration>(TConfiguration? configuration)
    {
        string FieldNamer(PropertyInfo property)
        {
            // try the field
            ConfigurationFieldAttribute? field = property.GetCustomAttribute<ConfigurationFieldAttribute>();
            if (field is not null)
            {
                return field.Key;
            }

            // but the property may not be a field just an extra thing
            JsonPropertyAttribute? json = property.GetCustomAttribute<JsonPropertyAttribute>();
            return json?.PropertyName ?? property.Name;
        }

        return ObjectJsonExtensions.ToObjectDictionary(configuration, FieldNamer);
    }
}
