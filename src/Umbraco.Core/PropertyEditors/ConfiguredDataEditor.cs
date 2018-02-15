using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides a base class for property editors. fixme rewrite
    /// </summary>
    /// <remarks>
    /// <para>Editors can be deserialized from manifests, which is why the Json serialization
    /// attributes are required, and the properties require an internal setter.</para>
    /// </remarks>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "(),nq}")]
    [HideFromTypeFinder]
    public class ConfiguredDataEditor : IConfiguredDataEditor
    {
        private IDataValueEditor _valueEditorAssigned;
        private IConfigurationEditor _configurationEditorAssigned;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredDataEditor"/> class.
        /// </summary>
        public ConfiguredDataEditor(ILogger logger, EditorType type = EditorType.PropertyValue)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // defaults
            Type = type;
            Icon = Constants.Icons.PropertyEditor;
            Group = "common";

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

        /// <summary>
        /// Gets or sets a value indicating whether the property editor is deprecated.
        /// </summary>
        /// <remarks>A deprecated editor does not show up in the list of available editors for a datatype,
        /// unless it is the current editor for the datatype.</remarks>
        [JsonIgnore]
        public bool IsDeprecated { get; internal set; } // fixme on interface?

        [JsonProperty("preValues")]
        public IDictionary<string, object> DefaultConfiguration => ConfigurationEditor.DefaultConfiguration;

        /// <summary>
        /// Gets or sets the value editor.
        /// </summary>
        /// <remarks>
        /// <para>If an instance of a value editor is assigned to the property,
        /// then this instance is returned when getting the property value. Otherwise, a
        /// new instance is created by CreateValueEditor.</para>
        /// <para>The instance created by CreateValueEditor is not cached, i.e.
        /// a new instance is created each time the property value is retrieved. The
        /// property editor is a singleton, and the value editor cannot be a singleton
        /// since it depends on the datatype configuration.</para>
        /// <para>Technically, it could be cached by datatype but let's keep things
        /// simple enough for now.</para>
        /// <para>The property is *not* marked with json ObjectCreationHandling = ObjectCreationHandling.Replace,
        /// so by default the deserializer will first try to read it before assigning it, which is why
        /// all deserialization *should* set the property before anything (see manifest deserializer).</para>
        /// </remarks>
        [JsonProperty("editor", Required = Required.Always)]
        public IDataValueEditor ValueEditor
        {
            // create a new value editor each time - the property editor can be a
            // singleton, but the value editor will get a configuration which depends
            // on the datatype, so it cannot be a singleton really
            get => CreateValueEditor();
            set => _valueEditorAssigned = value;
        }

        /// <summary>
        /// Gets or sets the configuration editor.
        /// </summary>
        /// <remarks>
        /// <para>If an instance of a configuration editor is assigned to the property,
        /// then this instance is returned when getting the property value. Otherwise, a
        /// new instance is created by CreateConfigurationEditor.</para>
        /// <para>The instance created by CreateConfigurationEditor is not cached, i.e.
        /// a new instance is created each time the property value is retrieved. The
        /// property editor is a singleton, and although the configuration editor could
        /// technically be a singleton too, we'd rather not keep configuration editor
        /// cached.</para>
        /// <para>The property is *not* marked with json ObjectCreationHandling = ObjectCreationHandling.Replace,
        /// so by default the deserializer will first try to read it before assigning it, which is why
        /// all deserialization *should* set the property before anything (see manifest deserializer).</para>
        /// </remarks>
        [JsonProperty("prevalues")] // changing the name would break manifests
        public IConfigurationEditor ConfigurationEditor
        {
            get => CreateConfigurationEditor();
            set => _configurationEditorAssigned = value;
        }

        /// <summary>
        /// Creates a value editor instance.
        /// </summary>
        protected virtual IDataValueEditor CreateValueEditor()
        {
            // handle assigned editor
            // or create a new editor
            return _valueEditorAssigned ?? new DataValueEditor(Attribute);
        }

        /// <summary>
        /// Creates a configuration editor instance.
        /// </summary>
        protected virtual IConfigurationEditor CreateConfigurationEditor()
        {
            // handle assigned editor
            if (_configurationEditorAssigned != null)
                return _configurationEditorAssigned;

            // else return an empty one
            return new ConfigurationEditor();
        }

        protected bool Equals(ConfiguredDataEditor other)
        {
            return string.Equals(Alias, other.Alias);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ConfiguredDataEditor) obj);
        }

        public override int GetHashCode()
        {
            // an internal setter is required for de-serialization from manifests
            // but we are never going to change the alias once the editor exists
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Alias.GetHashCode();
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
