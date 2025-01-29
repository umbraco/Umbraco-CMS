using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

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
    public virtual IDictionary<string, object> ToConfigurationEditor(IDictionary<string, object> configuration)
        => configuration;

    /// <inheritdoc />
    public virtual IDictionary<string, object> FromConfigurationEditor(IDictionary<string, object> configuration)
        => configuration;

    /// <inheritdoc />
    public virtual IDictionary<string, object> ToValueEditor(IDictionary<string, object> configuration)
        => ToConfigurationEditor(configuration);

    /// <inheritdoc />
    public virtual object ToConfigurationObject(
        IDictionary<string, object> configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer) => configuration;

    /// <inheritdoc />
    public virtual IDictionary<string, object> FromConfigurationObject(
        object configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        => configurationEditorJsonSerializer.Deserialize<Dictionary<string, object>>(configurationEditorJsonSerializer.Serialize(configuration)) ?? new Dictionary<string, object>();

    /// <inheritdoc />
    public virtual string ToDatabase(
        IDictionary<string, object> configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        => configurationEditorJsonSerializer.Serialize(configuration);

    /// <inheritdoc />
    public virtual IDictionary<string, object> FromDatabase(
        string? configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        => configuration.IsNullOrWhiteSpace() ? new Dictionary<string, object>() : configurationEditorJsonSerializer.Deserialize<Dictionary<string, object>>(configuration) ?? new Dictionary<string, object>();

    /// <inheritdoc />
    public virtual IEnumerable<ValidationResult> Validate(IDictionary<string, object> configuration)
        => Fields
            .SelectMany(field =>
                configuration.TryGetValue(field.Key, out var value)
                    ? field.Validators.SelectMany(validator => validator.Validate(value, null, null, PropertyValidationContext.Empty()))
                    : Enumerable.Empty<ValidationResult>())
            .ToArray();

    /// <summary>
    ///     Gets a field by its property name.
    /// </summary>
    /// <remarks>
    ///     Can be used in constructors to add infos to a field that has been defined
    ///     by a property marked with the <see cref="ConfigurationFieldAttribute" />.
    /// </remarks>
    protected ConfigurationField Field(string name)
        => Fields.First(x => x.PropertyName == name);

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
}
