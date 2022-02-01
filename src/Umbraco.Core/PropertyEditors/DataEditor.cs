using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a data editor.
    /// </summary>
    /// <remarks>
    /// <para>Editors can be deserialized from e.g. manifests, which is. why the class is not abstract,
    /// the json serialization attributes are required, and the properties have an internal setter.</para>
    /// </remarks>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "(),nq}")]
    [HideFromTypeFinder]
    public class DataEditor : IDataEditor
    {
        private IDictionary<string, object> _defaultConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEditor"/> class.
        /// </summary>
        public DataEditor(ILogger logger, EditorType type = EditorType.PropertyValue)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // defaults
            Type = type;
            Icon = Constants.Icons.PropertyEditor;
            Group = Constants.PropertyEditors.Groups.Common;

            // assign properties based on the attribute, if it is found
            Attribute = GetType().GetCustomAttribute<DataEditorAttribute>(false);
            if (Attribute == null) return;

            Alias = Attribute.Alias;
            Type = Attribute.Type;
            Name = Attribute.Name;
            Icon = Attribute.Icon;
            Group = Attribute.Group;
            IsDeprecated = Attribute.IsDeprecated;
        }

        /// <summary>
        /// Gets the editor attribute.
        /// </summary>
        protected DataEditorAttribute Attribute { get; }

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc />
        [JsonProperty("alias", Required = Required.Always)]
        public string Alias { get; internal set; }

        /// <inheritdoc />
        [JsonIgnore]
        public EditorType Type { get; }

        /// <inheritdoc />
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("icon")]
        public string Icon { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("group")]
        public string Group { get; internal set; }

        /// <inheritdoc />
        [JsonIgnore]
        public bool IsDeprecated { get; }

        /// <inheritdoc />
        /// <remarks>
        /// <para>If an explicit value editor has been assigned, then this explicit
        /// instance is returned. Otherwise, a new instance is created by CreateValueEditor.</para>
        /// <para>The instance created by CreateValueEditor is not cached, i.e.
        /// a new instance is created each time the property value is retrieved. The
        /// property editor is a singleton, and the value editor cannot be a singleton
        /// since it depends on the datatype configuration.</para>
        /// <para>Technically, it could be cached by datatype but let's keep things
        /// simple enough for now.</para>
        /// </remarks>
        // TODO: point of that one? shouldn't we always configure?
        public IDataValueEditor GetValueEditor() => ExplicitValueEditor ?? CreateValueEditor();

        /// <inheritdoc />
        /// <remarks>
        /// <para>If an explicit value editor has been assigned, then this explicit
        /// instance is returned. Otherwise, a new instance is created by CreateValueEditor,
        /// and configured with the configuration.</para>
        /// <para>The instance created by CreateValueEditor is not cached, i.e.
        /// a new instance is created each time the property value is retrieved. The
        /// property editor is a singleton, and the value editor cannot be a singleton
        /// since it depends on the datatype configuration.</para>
        /// <para>Technically, it could be cached by datatype but let's keep things
        /// simple enough for now.</para>
        /// </remarks>
        public virtual IDataValueEditor GetValueEditor(object configuration)
        {
            // if an explicit value editor has been set (by the manifest parser)
            // then return it, and ignore the configuration, which is going to be
            // empty anyways
            if (ExplicitValueEditor != null)
                return ExplicitValueEditor;

            var editor = CreateValueEditor();
            ((DataValueEditor)editor).Configuration = configuration; // TODO: casting is bad
            return editor;
        }

        /// <summary>
        /// Gets or sets an explicit value editor.
        /// </summary>
        /// <remarks>Used for manifest data editors.</remarks>
        [JsonProperty("editor")]
        public IDataValueEditor ExplicitValueEditor { get; set; }

        /// <inheritdoc />
        /// <remarks>
        /// <para>If an explicit configuration editor has been assigned, then this explicit
        /// instance is returned. Otherwise, a new instance is created by CreateConfigurationEditor.</para>
        /// <para>The instance created by CreateConfigurationEditor is not cached, i.e.
        /// a new instance is created each time. The property editor is a singleton, and although the
        /// configuration editor could technically be a singleton too, we'd rather not keep configuration editor
        /// cached.</para>
        /// </remarks>
        public IConfigurationEditor GetConfigurationEditor() => ExplicitConfigurationEditor ?? CreateConfigurationEditor();

        /// <summary>
        /// Gets or sets an explicit configuration editor.
        /// </summary>
        /// <remarks>Used for manifest data editors.</remarks>
        [JsonProperty("config")]
        public IConfigurationEditor ExplicitConfigurationEditor { get; set; }

        /// <inheritdoc />
        [JsonProperty("defaultConfig")]
        public IDictionary<string, object> DefaultConfiguration
        {
            // for property value editors, get the ConfigurationEditor.DefaultConfiguration
            // else fallback to a default, empty dictionary

            get => _defaultConfiguration ?? ((Type & EditorType.PropertyValue) > 0 ? GetConfigurationEditor().DefaultConfiguration : (_defaultConfiguration = new Dictionary<string, object>()));
            set => _defaultConfiguration = value;
        }

        /// <inheritdoc />
        public virtual IPropertyIndexValueFactory PropertyIndexValueFactory => new DefaultPropertyIndexValueFactory();

        /// <summary>
        /// Creates a value editor instance.
        /// </summary>
        /// <returns></returns>
        protected virtual IDataValueEditor CreateValueEditor()
        {
            if (Attribute == null)
                throw new InvalidOperationException($"The editor is not attributed with {nameof(DataEditorAttribute)}");

            return new DataValueEditor(Attribute);
        }

        /// <summary>
        /// Creates a configuration editor instance.
        /// </summary>
        protected virtual IConfigurationEditor CreateConfigurationEditor()
        {
            var editor = new ConfigurationEditor();
            // pass the default configuration if this is not a property value editor
            if ((Type & EditorType.PropertyValue) == 0)
            {
                editor.DefaultConfiguration = _defaultConfiguration;
            }
            return editor;
        }

        /// <summary>
        /// Provides a summary of the PropertyEditor for use with the <see cref="DebuggerDisplayAttribute"/>.
        /// </summary>
        protected virtual string DebuggerDisplay()
        {
            return $"Name: {Name}, Alias: {Alias}";
        }
    }
}
