using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Implements <see cref="IDataType" />.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class DataType : TreeEntityBase, IDataType
{
    private readonly IConfigurationEditorJsonSerializer _serializer;
    private object? _configuration;
    private string? _configurationJson;
    private ValueStorageType _databaseType;
    private IDataEditor? _editor;
    private bool _hasConfiguration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataType" /> class.
    /// </summary>
    public DataType(IDataEditor? editor, IConfigurationEditorJsonSerializer serializer, int parentId = -1)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(editor));
        ParentId = parentId;

        // set a default configuration
        Configuration = _editor.GetConfigurationEditor().DefaultConfigurationObject;
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public IDataEditor? Editor
    {
        get => _editor;
        set
        {
            // ignore if no change
            if (_editor?.Alias == value?.Alias)
            {
                return;
            }

            OnPropertyChanged(nameof(Editor));

            // try to map the existing configuration to the new configuration
            // simulate saving to db and reloading (ie go via json)
            var configuration = Configuration;
            var json = _serializer.Serialize(configuration);
            _editor = value;

            try
            {
                Configuration = _editor?.GetConfigurationEditor().FromDatabase(json, _serializer);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"The configuration for data type {Id} : {EditorAlias} is invalid (see inner exception)."
                    + " Please fix the configuration and ensure it is valid. The site may fail to start and / or load data types and run.",
                    e);
            }
        }
    }

    /// <inheritdoc />
    [DataMember]
    public string EditorAlias => _editor?.Alias ?? string.Empty;

    /// <inheritdoc />
    [DataMember]
    public ValueStorageType DatabaseType
    {
        get => _databaseType;
        set => SetPropertyValueAndDetectChanges(value, ref _databaseType, nameof(DatabaseType));
    }

    /// <inheritdoc />
    [DataMember]
    public object? Configuration
    {
        get
        {
            // if we know we have a configuration (which may be null), return it
            // if we don't have an editor, then we have no configuration, return null
            // else, use the editor to get the configuration object
            if (_hasConfiguration)
            {
                return _configuration;
            }

            try
            {
                _configuration = _editor?.GetConfigurationEditor().FromDatabase(_configurationJson, _serializer);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"The configuration for data type {Id} : {EditorAlias} is invalid (see inner exception)."
                    + " Please fix the configuration and ensure it is valid. The site may fail to start and / or load data types and run.",
                    e);
            }

            _hasConfiguration = true;
            _configurationJson = null;

            return _configuration;
        }

        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // we don't support re-assigning the same object
            // configurations are kinda non-mutable, mainly because detecting changes would be a pain
            // reference comparison
            if (_configuration == value)
            {
                throw new ArgumentException(
                    "Configurations are kinda non-mutable. Do not reassign the same object.",
                    nameof(value));
            }

            // validate configuration type
            if (!_editor?.GetConfigurationEditor().IsConfiguration(value) ?? true)
            {
                throw new ArgumentException(
                    $"Value of type {value.GetType().Name} cannot be a configuration for editor {_editor?.Alias}, expecting.",
                    nameof(value));
            }

            // extract database type from configuration object, if appropriate
            if (value is IConfigureValueType valueTypeConfiguration)
            {
                DatabaseType = ValueTypes.ToStorageType(valueTypeConfiguration.ValueType);
            }

            // extract database type from dictionary, if appropriate
            if (value is IDictionary<string, object> dictionaryConfiguration
                && dictionaryConfiguration.TryGetValue(
                    Constants.PropertyEditors.ConfigurationKeys.DataValueType,
                    out var valueTypeObject)
                && valueTypeObject is string valueTypeString
                && ValueTypes.IsValue(valueTypeString))
            {
                DatabaseType = ValueTypes.ToStorageType(valueTypeString);
            }

            _configuration = value;
            _hasConfiguration = true;
            _configurationJson = null;

            // it's always a change
            OnPropertyChanged(nameof(Configuration));
        }
    }

    /// <summary>
    ///     Lazily set the configuration as a serialized json string.
    /// </summary>
    /// <remarks>
    ///     <para>Will be de-serialized on-demand.</para>
    ///     <para>
    ///         This method is meant to be used when building entities from database, exclusively.
    ///         It does NOT register a property change to dirty. It ignores the fact that the configuration
    ///         may contain the database type, because the datatype DTO should also contain that database
    ///         type, and they should be the same.
    ///     </para>
    ///     <para>Think before using!</para>
    /// </remarks>
    public void SetLazyConfiguration(string? configurationJson)
    {
        _hasConfiguration = false;
        _configuration = null;
        _configurationJson = configurationJson;
    }

    /// <summary>
    ///     Gets a lazy configuration.
    /// </summary>
    /// <remarks>
    ///     <para>The configuration object will be lazily de-serialized.</para>
    ///     <para>This method is meant to be used when creating published datatypes, exclusively.</para>
    ///     <para>Think before using!</para>
    /// </remarks>
    internal Lazy<object?> GetLazyConfiguration()
    {
        // note: in both cases, make sure we capture what we need - we don't want
        // to capture a reference to this full, potentially heavy, DataType instance.
        if (_hasConfiguration)
        {
            // if configuration has already been de-serialized, return
            var capturedConfiguration = _configuration;
            return new Lazy<object?>(() => capturedConfiguration);
        }
        else
        {
            // else, create a Lazy de-serializer
            var capturedConfiguration = _configurationJson;
            IDataEditor? capturedEditor = _editor;
            return new Lazy<object?>(() =>
            {
                try
                {
                    return capturedEditor?.GetConfigurationEditor().FromDatabase(capturedConfiguration, _serializer);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        $"The configuration for data type {Id} : {EditorAlias} is invalid (see inner exception)."
                        + " Please fix the configuration and ensure it is valid. The site may fail to start and / or load data types and run.",
                        e);
                }
            });
        }
    }
}
