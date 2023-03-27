using System.Diagnostics;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a data editor.
/// </summary>
/// <remarks>
///     <para>
///         Editors can be deserialized from e.g. manifests, which is. why the class is not abstract,
///         the json serialization attributes are required, and the properties have an internal setter.
///     </para>
/// </remarks>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + "(),nq}")]
[HideFromTypeFinder]
[DataContract]
public class DataEditor : IDataEditor
{
    private readonly bool _canReuseValueEditor;
    private IDataValueEditor? _reusableValueEditor;
    private IDictionary<string, object>? _defaultConfiguration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataEditor" /> class.
    /// </summary>
    public DataEditor(IDataValueEditorFactory dataValueEditorFactory, EditorType type = EditorType.PropertyValue)
    {
        // defaults
        DataValueEditorFactory = dataValueEditorFactory;
        Type = type;
        Icon = Constants.Icons.PropertyEditor;
        Group = Constants.PropertyEditors.Groups.Common;

        // assign properties based on the attribute, if it is found
        Attribute = GetType().GetCustomAttribute<DataEditorAttribute>(false);
        if (Attribute == null)
        {
            Alias = string.Empty;
            Name = string.Empty;
            return;
        }

        Alias = Attribute.Alias;
        Type = Attribute.Type;
        Name = Attribute.Name;
        Icon = Attribute.Icon;
        Group = Attribute.Group;
        IsDeprecated = Attribute.IsDeprecated;

        _canReuseValueEditor = Attribute.ValueEditorIsReusable;
    }

    /// <summary>
    ///     Gets or sets an explicit value editor.
    /// </summary>
    /// <remarks>Used for manifest data editors.</remarks>
    [DataMember(Name = "editor")]
    public IDataValueEditor? ExplicitValueEditor { get; set; }

    /// <summary>
    ///     Gets the editor attribute.
    /// </summary>
    protected DataEditorAttribute? Attribute { get; }

    protected IDataValueEditorFactory DataValueEditorFactory { get; }

    /// <summary>
    ///     Gets or sets an explicit configuration editor.
    /// </summary>
    /// <remarks>Used for manifest data editors.</remarks>
    [DataMember(Name = "config")]
    public IConfigurationEditor? ExplicitConfigurationEditor { get; set; }

    /// <inheritdoc />
    [DataMember(Name = "alias", IsRequired = true)]
    public string Alias { get; set; }

    /// <inheritdoc />
    [DataMember(Name = "supportsReadOnly", IsRequired = true)]
    public bool SupportsReadOnly { get; set; }

    /// <inheritdoc />
    [IgnoreDataMember]
    public EditorType Type { get; }

    /// <inheritdoc />
    [DataMember(Name = "name", IsRequired = true)]
    public string Name { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "icon")]
    public string Icon { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "group")]
    public string Group { get; internal set; }

    /// <inheritdoc />
    [IgnoreDataMember]
    public bool IsDeprecated { get; }

    /// <inheritdoc />
    [DataMember(Name = "defaultConfig")]
    public IDictionary<string, object> DefaultConfiguration
    {
        // for property value editors, get the ConfigurationEditor.DefaultConfiguration
        // else fallback to a default, empty dictionary
        get => _defaultConfiguration ?? ((Type & EditorType.PropertyValue) > 0
            ? GetConfigurationEditor().DefaultConfiguration
            : _defaultConfiguration = new Dictionary<string, object>());
        set => _defaultConfiguration = value;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para>
    ///         If an explicit value editor has been assigned, then this explicit
    ///         instance is returned. Otherwise, a new instance is created by CreateValueEditor.
    ///     </para>
    ///     <para>
    ///         The instance created by CreateValueEditor is cached if allowed by the DataEditor
    ///         attribute (<see cref="DataEditorAttribute.ValueEditorIsReusable"/> == true).
    ///     </para>
    /// </remarks>
    public IDataValueEditor GetValueEditor() => ExplicitValueEditor
                                                ?? (_canReuseValueEditor
                                                    ? _reusableValueEditor ??= CreateValueEditor()
                                                    : CreateValueEditor());

    /// <inheritdoc />
    /// <remarks>
    ///     <para>
    ///         If an explicit value editor has been assigned, then this explicit
    ///         instance is returned. Otherwise, a new instance is created by CreateValueEditor,
    ///         and configured with the configuration.
    ///     </para>
    ///     <para>
    ///         The instance created by CreateValueEditor is not cached, i.e.
    ///         a new instance is created each time the property value is retrieved. The
    ///         property editor is a singleton, and the value editor cannot be a singleton
    ///         since it depends on the datatype configuration.
    ///     </para>
    ///     <para>
    ///         Technically, it could be cached by datatype but let's keep things
    ///         simple enough for now.
    ///     </para>
    /// </remarks>
    public virtual IDataValueEditor GetValueEditor(object? configuration)
    {
        // if an explicit value editor has been set (by the manifest parser)
        // then return it, and ignore the configuration, which is going to be
        // empty anyways
        if (ExplicitValueEditor != null)
        {
            return ExplicitValueEditor;
        }

        IDataValueEditor editor = CreateValueEditor();
        if (configuration is not null)
        {
            ((DataValueEditor)editor).Configuration = configuration; // TODO: casting is bad
        }

        return editor;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para>
    ///         If an explicit configuration editor has been assigned, then this explicit
    ///         instance is returned. Otherwise, a new instance is created by CreateConfigurationEditor.
    ///     </para>
    ///     <para>
    ///         The instance created by CreateConfigurationEditor is not cached, i.e.
    ///         a new instance is created each time. The property editor is a singleton, and although the
    ///         configuration editor could technically be a singleton too, we'd rather not keep configuration editor
    ///         cached.
    ///     </para>
    /// </remarks>
    public IConfigurationEditor GetConfigurationEditor() => ExplicitConfigurationEditor ?? CreateConfigurationEditor();

    /// <inheritdoc />
    public virtual IPropertyIndexValueFactory PropertyIndexValueFactory => new DefaultPropertyIndexValueFactory();

    /// <summary>
    ///     Creates a value editor instance.
    /// </summary>
    /// <returns></returns>
    protected virtual IDataValueEditor CreateValueEditor()
    {
        if (Attribute == null)
        {
            throw new InvalidOperationException($"The editor is not attributed with {nameof(DataEditorAttribute)}");
        }

        return DataValueEditorFactory.Create<DataValueEditor>(Attribute);
    }

    /// <summary>
    ///     Creates a configuration editor instance.
    /// </summary>
    protected virtual IConfigurationEditor CreateConfigurationEditor()
    {
        var editor = new ConfigurationEditor();

        // pass the default configuration if this is not a property value editor
        if ((Type & EditorType.PropertyValue) == 0 && _defaultConfiguration is not null)
        {
            editor.DefaultConfiguration = _defaultConfiguration;
        }

        return editor;
    }

    /// <summary>
    ///     Provides a summary of the PropertyEditor for use with the <see cref="DebuggerDisplayAttribute" />.
    /// </summary>
    protected virtual string DebuggerDisplay() => $"Name: {Name}, Alias: {Alias}";
}
