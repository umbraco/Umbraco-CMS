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
    /// <remarks>
    ///     <para>
    ///         For basic configuration editors, this will be a dictionary of key/values. For advanced editors
    ///         which inherit from <see cref="ConfigurationEditor{TConfiguration}" />, this will be the dictionary
    ///         equivalent of an actual configuration object (ie an instance of <c>TConfiguration</c>, obtained
    ///         via <see cref="ToConfigurationEditor" />.
    ///     </para>
    /// </remarks>
    [DataMember(Name = "defaultConfig")]
    IDictionary<string, object> DefaultConfiguration { get; }

    /// <summary>
    ///     Gets the default configuration object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For basic configuration editors, this will be <see cref="DefaultConfiguration" />, ie a
    ///         dictionary of key/values. For advanced editors which inherit from
    ///         <see cref="ConfigurationEditor{TConfiguration}" />,
    ///         this will be an actual configuration object (ie an instance of <c>TConfiguration</c>.
    ///     </para>
    /// </remarks>
    object? DefaultConfigurationObject { get; }

    /// <summary>
    ///     Determines whether a configuration object is of the type expected by the configuration editor.
    /// </summary>
    bool IsConfiguration(object obj);

    // notes
    // ToConfigurationEditor returns a dictionary, and FromConfigurationEditor accepts a dictionary.
    // this is due to the way our front-end editors work, see DataTypeController.PostSave
    // and DataTypeConfigurationFieldDisplayResolver - we are not going to change it now.

    /// <summary>
    ///     Converts the serialized database value into the actual configuration object.
    /// </summary>
    /// <remarks>
    ///     Converting the configuration object to the serialized database value is
    ///     achieved by simply serializing the configuration. See <see cref="ConfigurationEditor.ToDatabase" />.
    /// </remarks>
    object FromDatabase(
        string? configurationJson,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer);

    /// <summary>
    ///     Converts the values posted by the configuration editor into the actual configuration object.
    /// </summary>
    /// <param name="editorValues">The values posted by the configuration editor.</param>
    /// <param name="configuration">The current configuration object.</param>
    object? FromConfigurationEditor(IDictionary<string, object?>? editorValues, object? configuration);

    /// <summary>
    ///     Converts the configuration object to values for the configuration editor.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    [Obsolete("The value type parameter of the dictionary will be made nullable in V11, use ToConfigurationEditorNullable.")]
    IDictionary<string, object> ToConfigurationEditor(object? configuration);

    // TODO: Obsolete in V11.
    IDictionary<string, object?> ToConfigurationEditorNullable(object? configuration) =>
        ToConfigurationEditor(configuration)!;

    /// <summary>
    ///     Converts the configuration object to values for the value editor.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    IDictionary<string, object>? ToValueEditor(object? configuration);
}
