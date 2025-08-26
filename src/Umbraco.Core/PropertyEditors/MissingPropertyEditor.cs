using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a temporary representation of an editor for cases where a data type is created but not editor is
///     available.
/// </summary>
[HideFromTypeFinder]
public class MissingPropertyEditor : IDataEditor
{
    private readonly IDataValueEditorFactory _dataValueEditorFactory;
    private IDataValueEditor? _valueEditor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MissingPropertyEditor"/> class.
    /// </summary>
    public MissingPropertyEditor(
        string missingEditorAlias,
        IDataValueEditorFactory dataValueEditorFactory)
    {
        _dataValueEditorFactory = dataValueEditorFactory;
        Alias = missingEditorAlias;
    }

    /// <inheritdoc />
    public string Alias { get; }

    /// <summary>
    /// Gets the name of the editor.
    /// </summary>
    public string Name => "Missing property editor";

    /// <inheritdoc />
    public bool IsDeprecated => false;

    /// <inheritdoc />
    public bool SupportsReadOnly => true;

    /// <inheritdoc />
    public IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>();

    /// <inheritdoc />
    public IPropertyIndexValueFactory PropertyIndexValueFactory => new DefaultPropertyIndexValueFactory();

    /// <inheritdoc />
    public IDataValueEditor GetValueEditor() => _valueEditor
        ??= _dataValueEditorFactory.Create<MissingPropertyValueEditor>(
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.Missing));

    /// <inheritdoc />
    public IDataValueEditor GetValueEditor(object? configurationObject) => GetValueEditor();

    /// <inheritdoc />
    public IConfigurationEditor GetConfigurationEditor() => new ConfigurationEditor();

    // provides the property value editor
    internal sealed class MissingPropertyValueEditor : DataValueEditor
    {
        public MissingPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
        }

        /// <inheritdoc />
        public override bool IsReadOnly => true;
    }
}
