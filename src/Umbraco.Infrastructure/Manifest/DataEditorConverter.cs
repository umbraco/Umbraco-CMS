using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Provides a json read converter for <see cref="IDataEditor" /> in manifests.
/// </summary>
internal class DataEditorConverter : JsonReadConverter<IDataEditor>
{
    private readonly IDataValueEditorFactory _dataValueEditorFactory;
    private readonly IIOHelper _ioHelper;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILocalizedTextService _textService;
    private const string SupportsReadOnly = "supportsReadOnly";

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataEditorConverter" /> class.
    /// </summary>
    public DataEditorConverter(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        ILocalizedTextService textService,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer)
    {
        _dataValueEditorFactory = dataValueEditorFactory;
        _ioHelper = ioHelper;
        _textService = textService;
        _shortStringHelper = shortStringHelper;
        _jsonSerializer = jsonSerializer;
    }

    /// <inheritdoc />
    protected override IDataEditor Create(Type objectType, string path, JObject jobject)
    {
        // in PackageManifest, property editors are IConfiguredDataEditor[] whereas
        // parameter editors are IDataEditor[] - both will end up here because we handle
        // IDataEditor and IConfiguredDataEditor implements it, but we can check the
        // type to figure out what to create
        EditorType type = EditorType.PropertyValue;

        var isPropertyEditor = path.StartsWith("propertyEditors[");

        if (isPropertyEditor)
        {
            // property editor
            jobject["isPropertyEditor"] = JToken.FromObject(true);
            if (jobject["isParameterEditor"] is JToken jToken && jToken.Value<bool>())
            {
                type |= EditorType.MacroParameter;
            }
        }
        else
        {
            // parameter editor
            type = EditorType.MacroParameter;
        }

        return new DataEditor(_dataValueEditorFactory, type);
    }

    /// <inheritdoc />
    protected override void Deserialize(JObject jobject, IDataEditor target, JsonSerializer serializer)
    {
        // see Create above, target is either DataEditor (parameter) or ConfiguredDataEditor (property)
        if (!(target is DataEditor dataEditor))
        {
            throw new Exception("panic.");
        }

        if (jobject["isPropertyEditor"] is JToken jtoken && jtoken.Value<bool>())
        {
            PrepareForPropertyEditor(jobject, dataEditor);
        }
        else
        {
            PrepareForParameterEditor(jobject, dataEditor);
        }

        base.Deserialize(jobject, target, serializer);
    }

    private static JArray RewriteValidators(JObject validation)
    {
        var jarray = new JArray();

        foreach (KeyValuePair<string, JToken?> v in validation)
        {
            var key = v.Key;
            JToken? val = v.Value;
            var jo = new JObject { { "type", key }, { "configuration", val } };
            jarray.Add(jo);
        }

        return jarray;
    }

    private void PrepareForPropertyEditor(JObject jobject, DataEditor target)
    {
        if (jobject["editor"] == null)
        {
            throw new InvalidOperationException("Missing 'editor' value.");
        }

        if (jobject.Property(SupportsReadOnly) is null)
        {
            jobject[SupportsReadOnly] = false;
        }

        // explicitly assign a value editor of type ValueEditor
        // (else the deserializer will try to read it before setting it)
        // (and besides it's an interface)
        target.ExplicitValueEditor = new DataValueEditor(_textService, _shortStringHelper, _jsonSerializer);

        // in the manifest, validators are a simple dictionary eg
        // {
        //   required: true,
        //   regex: '\\d*'
        // }
        // and we need to turn this into a list of IPropertyValidator
        // so, rewrite the json structure accordingly
        if (jobject["editor"]?["validation"] is JObject validation)
        {
            jobject["editor"]!["validation"] = RewriteValidators(validation);
        }

        if (jobject["editor"]?["view"] is JValue view)
        {
            jobject["editor"]!["view"] = RewriteVirtualUrl(view);
        }

        var prevalues = jobject["prevalues"] as JObject;
        var defaultConfig = jobject["defaultConfig"] as JObject;
        if (prevalues != null || defaultConfig != null)
        {
            // explicitly assign a configuration editor of type ConfigurationEditor
            // (else the deserializer will try to read it before setting it)
            // (and besides it's an interface)
            target.ExplicitConfigurationEditor = new ConfigurationEditor();

            var config = new JObject();
            if (prevalues != null)
            {
                config = prevalues;

                // see note about validators, above - same applies to field validators
                if (config["fields"] is JArray jarray)
                {
                    foreach (JToken field in jarray)
                    {
                        if (field["validation"] is JObject fvalidation)
                        {
                            field["validation"] = RewriteValidators(fvalidation);
                        }

                        if (field["view"] is JValue fview)
                        {
                            field["view"] = RewriteVirtualUrl(fview);
                        }
                    }
                }
            }

            // in the manifest, default configuration is at editor level
            // move it down to configuration editor level so it can be deserialized properly
            if (defaultConfig != null)
            {
                config["defaultConfig"] = defaultConfig;
                jobject.Remove("defaultConfig");
            }

            // in the manifest, configuration is named 'prevalues', rename
            // it is important to do this LAST
            jobject["config"] = config;
            jobject.Remove("prevalues");
        }
    }

    private string? RewriteVirtualUrl(JValue view) => _ioHelper.ResolveRelativeOrVirtualUrl(view.Value as string);

    private void PrepareForParameterEditor(JObject jobject, DataEditor target)
    {
        // in a manifest, a parameter editor looks like:
        //
        // {
        //   "alias": "...",
        //   "name": "...",
        //   "view": "...",
        //   "config": { "key1": "value1", "key2": "value2" ... }
        // }
        //
        // the view is at top level, but should be down one level to be properly
        // deserialized as a ParameterValueEditor property -> need to move it
        if (jobject.Property("view") != null)
        {
            // explicitly assign a value editor of type ParameterValueEditor
            target.ExplicitValueEditor = new DataValueEditor(_textService, _shortStringHelper, _jsonSerializer);

            // move the 'view' property
            jobject["editor"] = new JObject { ["view"] = jobject["view"] };
            jobject.Property("view")?.Remove();
        }

        if (jobject.Property(SupportsReadOnly) is null)
        {
            jobject[SupportsReadOnly] = false;
        }

        // in the manifest, default configuration is named 'config', rename
        if (jobject["config"] is JObject config)
        {
            jobject["defaultConfig"] = config;
            jobject.Remove("config");
        }

        // We need to null check, if view do not exists, then editor do not exists
        if (jobject["editor"]?["view"] is JValue view)
        {
            jobject["editor"]!["view"] = RewriteVirtualUrl(view);
        }
    }
}
