using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents an editor for editing the configuration of editors.
/// </summary>
public interface IConfigurationEditor
{
    /// <summary>
    ///     Gets the fields.
    /// </summary>
    [DataMember(Name = "fields")]
    List<ConfigurationField> Fields { get; }

    /// <summary>
    ///     Gets the default configuration.
    /// </summary>
    [DataMember(Name = "defaultConfig")]
    IDictionary<string, object> DefaultConfiguration { get; }

    /// <summary>
    ///     Converts the configuration data to values for the configuration editor.
    /// </summary>
    /// <param name="configuration">The configuration data.</param>
    IDictionary<string, object> ToConfigurationEditor(IDictionary<string, object> configuration);

    /// <summary>
    ///     Converts values from the configuration editor to configuration data.
    /// </summary>
    /// <remarks>
    /// Consider this the reverse of <see cref="ToConfigurationEditor"/>.
    /// </remarks>
    /// <param name="configuration">Values from the configuration editor.</param>
    IDictionary<string, object> FromConfigurationEditor(IDictionary<string, object> configuration);

    /// <summary>
    ///     Converts the configuration data to values for the value editor.
    /// </summary>
    /// <param name="configuration">The configuration data.</param>
    IDictionary<string, object> ToValueEditor(IDictionary<string, object> configuration);

    /// <summary>
    ///     Creates a configuration object from the configuration data.
    /// </summary>
    /// <param name="configuration">The configuration data.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration serializer.</param>
    object ToConfigurationObject(
        IDictionary<string, object> configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer);

    /// <summary>
    ///     Creates configuration data from a configuration object.
    /// </summary>
    /// <param name="configuration">The configuration object.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration serializer.</param>
    IDictionary<string, object> FromConfigurationObject(
        object configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer);

    /// <summary>
    ///     Converts configuration data into a serialized database value.
    /// </summary>
    string ToDatabase(
        IDictionary<string, object> configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer);

    /// <summary>
    ///     Converts a serialized database value into configuration data.
    /// </summary>
    /// <param name="configuration">The serialized database value (JSON format).</param>
    /// <param name="configurationEditorJsonSerializer">The configuration serializer.</param>
    IDictionary<string, object> FromDatabase(
        string? configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer);

    /// <summary>
    ///     Performs validation of configuration data.
    /// </summary>
    /// <param name="configuration">The configuration data to validate.</param>
    /// <returns>One or more <see cref="ValidationResult"/> if the configuration data is invalid, an empty collection otherwise.</returns>
    IEnumerable<ValidationResult> Validate(IDictionary<string, object> configuration);
}
