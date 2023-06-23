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
    private object? _configurationObject;
    private IDictionary<string, object> _configurationData;
    private ValueStorageType _databaseType;
    private IDataEditor? _editor;
    private bool _hasConfigurationObject;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataType" /> class.
    /// </summary>
    public DataType(IDataEditor? editor, IConfigurationEditorJsonSerializer serializer, int parentId = -1)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(editor));
        ParentId = parentId;

        // set a default configuration
        _configurationData = _editor.GetConfigurationEditor().DefaultConfiguration;
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

            // reset the configuration (force reload on next access using the new editor)
            _configurationObject = null;
            _hasConfigurationObject = false;
            _editor = value;

            OnPropertyChanged(nameof(Editor));
        }
    }

    /// <inheritdoc />
    [DataMember]
    public string EditorAlias => _editor?.Alias ?? string.Empty;

    /// <inheritdoc />
    [DataMember]
    public string? EditorUiAlias { get; set; }

    /// <inheritdoc />
    [DataMember]
    public ValueStorageType DatabaseType
    {
        get => _databaseType;
        set => SetPropertyValueAndDetectChanges(value, ref _databaseType, nameof(DatabaseType));
    }

    /// <inheritdoc />
    public IDictionary<string, object> ConfigurationData
    {
        get => _configurationData;
        set
        {
            _configurationObject = null;
            _hasConfigurationObject = false;
            _configurationData = value;

            OnPropertyChanged(nameof(ConfigurationObject));
            OnPropertyChanged(nameof(ConfigurationData));
        }
    }

    /// <inheritdoc />
    [DataMember]
    public object? ConfigurationObject
    {
        get
        {
            // if we know we have a configuration (which may be null), return it
            // if we don't have an editor, then we have no configuration, return null
            // else, use the editor to get the configuration object
            if (_hasConfigurationObject)
            {
                return _configurationObject;
            }

            try
            {
                _configurationObject = _editor?.GetConfigurationEditor().ToConfigurationObject(_configurationData, _serializer);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"The configuration for data type {Id} : {EditorAlias} is invalid (see inner exception)."
                    + " Please fix the configuration and ensure it is valid. The site may fail to start and / or load data types and run.",
                    e);
            }

            _hasConfigurationObject = true;

            return _configurationObject;
        }
    }

    /// <summary>
    ///     Sets the configuration without invoking property changed events.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is meant to be used when building entities from database, exclusively.
    ///         It does NOT register a property change to dirty. It ignores the fact that the configuration
    ///         may contain the database type, because the datatype DTO should also contain that database
    ///         type, and they should be the same.
    ///     </para>
    ///     <para>Think before using!</para>
    /// </remarks>
    public void SetConfigurationData(IDictionary<string, object> configurationData)
    {
        _hasConfigurationObject = false;
        _configurationObject = null;
        _configurationData = configurationData;
    }

    /// <summary>
    ///     Gets a lazy configuration.
    /// </summary>
    /// <remarks>
    ///     <para>The configuration object will be lazily de-serialized.</para>
    ///     <para>This method is meant to be used when creating published datatypes, exclusively.</para>
    ///     <para>Think before using!</para>
    /// </remarks>
    internal Lazy<object?> GetLazyConfigurationObject()
    {
        // note: in both cases, make sure we capture what we need - we don't want
        // to capture a reference to this full, potentially heavy, DataType instance.
        if (_hasConfigurationObject)
        {
            // if configuration has already been de-serialized, return
            var capturedConfiguration = _configurationObject;
            return new Lazy<object?>(() => capturedConfiguration);
        }
        else
        {
            // else, create a Lazy de-serializer
            IDictionary<string, object> capturedConfiguration = _configurationData;
            IDataEditor? capturedEditor = _editor;
            return new Lazy<object?>(() =>
            {
                try
                {
                    return capturedEditor?.GetConfigurationEditor().ToConfigurationObject(capturedConfiguration, _serializer);
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
