using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

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
        private readonly PropertyEditorAttribute _attribute;

        private PropertyValueEditor _valueEditor;
        private PropertyValueEditor _valueEditorAssigned;
        private PreValueEditor _preValueEditor;
        private PreValueEditor _preValueEditorAssigned;

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
            _attribute = GetType().GetCustomAttribute<PropertyEditorAttribute>(false);
            if (_attribute == null) return;

            Alias = _attribute.Alias;
            Name = _attribute.Name;
            IsParameterEditor = _attribute.IsParameterEditor;
            Icon = _attribute.Icon;
            Group = _attribute.Group;
            IsDeprecated = _attribute.IsDeprecated;
        }

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
        [JsonProperty("group")]
        public string Group { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property editor is deprecated.
        /// </summary>
        [JsonIgnore]
        public bool IsDeprecated { get; internal set; } // fixme - kill it all in v8

        [JsonProperty("editor", Required = Required.Always)]
        public PropertyValueEditor ValueEditor
        {
            get => _valueEditor ?? (_valueEditor = CreateValueEditor());
            set
            {
                _valueEditorAssigned = value;
                _valueEditor = null;
            }
        }

        [JsonIgnore]
        IValueEditor IParameterEditor.ValueEditor => ValueEditor; // fixme - because we must, but - bah

        [JsonProperty("prevalues")]
        public PreValueEditor PreValueEditor
        {
            get => _preValueEditor ?? (_preValueEditor = CreatePreValueEditor());
            set
            {
                _preValueEditorAssigned = value;
                _preValueEditor = null;
            }
        }

        [JsonProperty("defaultConfig")]
        public virtual IDictionary<string, object> DefaultPreValues { get; set; }

        [JsonIgnore]
        IDictionary<string, object> IParameterEditor.Configuration => DefaultPreValues; // fixme - because we must, but - bah

        /// <summary>
        /// Creates a value editor instance.
        /// </summary>
        protected virtual PropertyValueEditor CreateValueEditor()
        {
            // handle assigned editor
            if (_valueEditorAssigned != null)
                return _valueEditorAssigned;

            // create a new editor
            var editor = new PropertyValueEditor();

            var view = _attribute?.EditorView;
            if (string.IsNullOrWhiteSpace(view))
                throw new InvalidOperationException("The editor does not specify a view.");
            if (view.StartsWith("~/"))
                view = IOHelper.ResolveUrl(view);
            editor.View = view;

            editor.ValueType = _attribute.ValueType;
            editor.HideLabel = _attribute.HideLabel;
            return editor;
        }

        /// <summary>
        /// Creates a configuration editor instance.
        /// </summary>
        protected virtual PreValueEditor CreateConfigurationEditor()
        {
            // handle assigned editor
            if (_preValueEditorAssigned != null)
                return _preValueEditorAssigned;

            // else return an empty one
            return new PreValueEditor();
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

        /// <summary>
        /// Maps configuration to a strongly typed object.
        /// </summary>
        public virtual object DeserializeConfiguration(string json) // fixme
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
    }
}
