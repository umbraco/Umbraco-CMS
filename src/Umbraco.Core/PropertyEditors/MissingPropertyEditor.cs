using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a temporary representation of an editor for cases where a data type is created but not editor is
///     available.
/// </summary>
[HideFromTypeFinder]
public class MissingPropertyEditor : IDataEditor
{
    private const string EditorAlias = "Umbraco.Missing";
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingPropertyEditor"/> class.
    /// </summary>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 18.")]
    public MissingPropertyEditor()
        : this(
            EditorAlias,
            StaticServiceProvider.Instance.GetRequiredService<IDataValueEditorFactory>())
    {
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
            new DataEditorAttribute(EditorAlias));

    /// <inheritdoc />
    public IDataValueEditor GetValueEditor(object? configurationObject) => GetValueEditor();

    /// <inheritdoc />
    public IConfigurationEditor GetConfigurationEditor() => new ConfigurationEditor();

    /// <summary>
    /// Provides the property value editor for missing property editors.
    /// </summary>
    internal sealed class MissingPropertyValueEditor : DataValueEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingPropertyValueEditor"/> class.
        /// </summary>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        /// <param name="ioHelper">The IO helper.</param>
        /// <param name="attribute">The data editor attribute.</param>
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
