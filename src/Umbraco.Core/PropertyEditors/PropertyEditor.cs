using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides a base class for property editors.
    /// </summary>
    /// <remarks>
    /// <para>Editors can be deserialized from manifests, which is why the Json serialization
    /// attributes are required, and the properties require an internal setter.</para>
    /// </remarks>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "(),nq}")]
    public class PropertyEditor : IParameterEditor
    {
        private ValueEditor _valueEditorAssigned;
        private ConfigurationEditor _configurationEditorAssigned;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEditor"/> class.
        /// </summary>
        public PropertyEditor(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // defaults
            Icon = Constants.Icons.PropertyEditor;
            Group = "common";

            // assign properties based on the attribute, if it is found
            Attribute = GetType().GetCustomAttribute<ValueEditorAttribute>(false);
            if (Attribute == null) return;

            Alias = Attribute.Alias;
            Name = Attribute.Name;
            IsParameterEditor = Attribute.IsMacroParameterEditor;
            Icon = Attribute.Icon;
            Group = Attribute.Group;
            IsDeprecated = Attribute.IsDeprecated;
        }

        /// <summary>
        /// Gets the editor attribute.
        /// </summary>
        protected ValueEditorAttribute Attribute { get; }

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this editor can be used as a parameter editor.
        /// </summary>
        [JsonProperty("isParameterEditor")]
        public bool IsParameterEditor { get; internal set; } // fixme understand + explain

        /// <summary>
        /// Gets or sets the unique alias of the property editor.
        /// </summary>
        [JsonProperty("alias", Required = Required.Always)]
        public string Alias { get; internal set; }

        /// <summary>
        /// Gets or sets the name of the property editor.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets or sets the icon of the property editor.
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; internal set; }

        /// <summary>
        /// Gets or sets the group of the property editor.
        /// </summary>
        /// <remarks>The group can be used to group editors by categories.</remarks>
        [JsonProperty("group")]
        public string Group { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property editor is deprecated.
        /// </summary>
        [JsonIgnore]
        public bool IsDeprecated { get; internal set; } // fixme - kill it all in v8

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
        /// <para>The property is marked as a Json property with ObjectCreationHandling
        /// set to Replace in order to prevent the Json deserializer to retrieve the
        /// value of the property before setting it.</para>
        /// </remarks>
        [JsonProperty("editor", Required = Required.Always, ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public ValueEditor ValueEditor
        {
            // create a new value editor each time - the property editor can be a
            // singleton, but the value editor will get a configuration which depends
            // on the datatype, so it cannot be a singleton really
            get => CreateValueEditor();
            set => _valueEditorAssigned = value;
        }

        [JsonIgnore]
        IValueEditor IParameterEditor.ValueEditor => ValueEditor; // fixme - because we must, but - bah

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
        /// <para>The property is marked as a Json property with ObjectCreationHandling
        /// set to Replace in order to prevent the Json deserializer to retrieve the
        /// value of the property before setting it.</para>
        /// </remarks>
        [JsonProperty("prevalues", ObjectCreationHandling = ObjectCreationHandling.Replace)] // changing the name would break manifests
        public ConfigurationEditor ConfigurationEditor
        {
            get => CreateConfigurationEditor();
            set => _configurationEditorAssigned = value;
        }

        // fixme explain what this is
        // makes sense only for dictionary-based configurationeditor, true conf editors have classes?
        [JsonProperty("defaultConfig")]
        public virtual IDictionary<string, object> DefaultConfiguration { get; set; }

        [JsonIgnore]
        IDictionary<string, object> IParameterEditor.Configuration => DefaultConfiguration; // fixme - because we must, but - bah

        /// <summary>
        /// Creates a value editor instance.
        /// </summary>
        protected virtual ValueEditor CreateValueEditor()
        {
            // handle assigned editor
            // or create a new editor
            return _valueEditorAssigned ?? new ValueEditor(Attribute);
        }

        /// <summary>
        /// Creates a configuration editor instance.
        /// </summary>
        protected virtual ConfigurationEditor CreateConfigurationEditor()
        {
            // handle assigned editor
            if (_configurationEditorAssigned != null)
                return _configurationEditorAssigned;

            // else return an empty one
            return new ConfigurationEditor();
        }

        protected bool Equals(PropertyEditor other)
        {
            return string.Equals(Alias, other.Alias);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PropertyEditor) obj);
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
            return $"Name: {Name}, Alias: {Alias}, IsParameterEditor: {IsParameterEditor}";
        }
    }
}
