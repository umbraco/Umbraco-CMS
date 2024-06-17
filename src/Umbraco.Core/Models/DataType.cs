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
    private IDictionary<string, object> _configurationData;
    private ValueStorageType _databaseType;
    private IDataEditor? _editor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataType" /> class.
    /// </summary>
    public DataType(IDataEditor? editor, IConfigurationEditorJsonSerializer serializer, int parentId = -1)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
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
            _configurationData = value;

            OnPropertyChanged(nameof(ConfigurationData));
        }
    }
}
