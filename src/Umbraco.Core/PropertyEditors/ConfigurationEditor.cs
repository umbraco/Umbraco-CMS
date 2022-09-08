using System.Runtime.Serialization;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a data type configuration editor.
/// </summary>
[DataContract]
public class ConfigurationEditor : IConfigurationEditor
{
    private IDictionary<string, object> _defaultConfiguration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationEditor" /> class.
    /// </summary>
    public ConfigurationEditor()
    {
        Fields = new List<ConfigurationField>();
        _defaultConfiguration = new Dictionary<string, object>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationEditor" /> class.
    /// </summary>
    protected ConfigurationEditor(List<ConfigurationField> fields)
    {
        Fields = fields;
        _defaultConfiguration = new Dictionary<string, object>();
    }

    /// <summary>
    ///     Gets the fields.
    /// </summary>
    [DataMember(Name = "fields")]
    public List<ConfigurationField> Fields { get; }

    /// <inheritdoc />
    [DataMember(Name = "defaultConfig")]
    public virtual IDictionary<string, object> DefaultConfiguration
    {
        get => _defaultConfiguration;
        set => _defaultConfiguration = value;
    }

    /// <inheritdoc />
    public virtual object? DefaultConfigurationObject => DefaultConfiguration;

    /// <summary>
    ///     Converts a configuration object into a serialized database value.
    /// </summary>
    public static string? ToDatabase(
        object? configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        => configuration == null ? null : configurationEditorJsonSerializer.Serialize(configuration);

    /// <summary>
    ///     Gets the configuration as a typed object.
    /// </summary>
    public static TConfiguration? ConfigurationAs<TConfiguration>(object? obj)
    {
        if (obj == null)
        {
            return default;
        }

        if (obj is TConfiguration configuration)
        {
            return configuration;
        }

        throw new InvalidCastException(
            $"Cannot cast configuration of type {obj.GetType().Name} to {typeof(TConfiguration).Name}.");
    }

    /// <inheritdoc />
    public virtual bool IsConfiguration(object obj) => obj is IDictionary<string, object>;

    /// <inheritdoc />
    public virtual object FromDatabase(
        string? configurationJson,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        => string.IsNullOrWhiteSpace(configurationJson)
            ? new Dictionary<string, object>()
            : configurationEditorJsonSerializer.Deserialize<Dictionary<string, object>>(configurationJson)!;

    /// <inheritdoc />
    public virtual object? FromConfigurationEditor(IDictionary<string, object?>? editorValues, object? configuration)
    {
        // by default, return the posted dictionary
        // but only keep entries that have a non-null/empty value
        // rest will fall back to default during ToConfigurationEditor()
        var keys = editorValues?.Where(x =>
                x.Value == null || (x.Value is string stringValue && string.IsNullOrWhiteSpace(stringValue)))
            .Select(x => x.Key).ToList();

        if (keys is not null)
        {
            foreach (var key in keys)
            {
                editorValues?.Remove(key);
            }
        }

        return editorValues;
    }

    /// <inheritdoc />
    public virtual IDictionary<string, object> ToConfigurationEditor(object? configuration)
    {
        // editors that do not override ToEditor/FromEditor have their configuration
        // as a dictionary of <string, object> and, by default, we merge their default
        // configuration with their current configuration
        if (configuration == null)
        {
            configuration = new Dictionary<string, object>();
        }

        if (!(configuration is IDictionary<string, object> c))
        {
            throw new ArgumentException(
                $"Expecting a {typeof(Dictionary<string, object>).Name} instance but got {configuration.GetType().Name}.",
                nameof(configuration));
        }

        // clone the default configuration, and apply the current configuration values
        var d = new Dictionary<string, object>(DefaultConfiguration);
        foreach ((string key, object value) in c)
        {
            d[key] = value;
        }

        return d;
    }

    /// <inheritdoc />
    public virtual IDictionary<string, object> ToValueEditor(object? configuration)
        => ToConfigurationEditor(configuration);

    /// <summary>
    ///     Gets a field by its property name.
    /// </summary>
    /// <remarks>
    ///     Can be used in constructors to add infos to a field that has been defined
    ///     by a property marked with the <see cref="ConfigurationFieldAttribute" />.
    /// </remarks>
    protected ConfigurationField Field(string name)
        => Fields.First(x => x.PropertyName == name);
}
