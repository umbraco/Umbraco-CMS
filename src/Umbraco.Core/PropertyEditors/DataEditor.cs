using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a parameter editor. fixme rewrite
    /// </summary>
    /// <remarks>
    /// <para>Is not abstract because can be instanciated from manifests.</para>
    /// </remarks>
    [HideFromTypeFinder]
    public class DataEditor : IDataEditor
    {
        private IDataValueEditor _valueEditor;
        private IDataValueEditor _valueEditorAssigned;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEditor"/> class.
        /// </summary>
        public DataEditor(EditorType type = EditorType.PropertyValue)
        {
            // defaults
            Type = type;
            Icon = Constants.Icons.PropertyEditor;
            Group = "common";
            DefaultConfiguration = new Dictionary<string, object>();

            // assign properties based on the attribute, if it is found
            Attribute = GetType().GetCustomAttribute<DataEditorAttribute>(false);
            if (Attribute == null) return;

            Alias = Attribute.Alias;
            Type = Attribute.Type;
            Name = Attribute.Name;
            IsDeprecated = Attribute.IsDeprecated;
        }

        /// <summary>
        /// Gets the editor attribute.
        /// </summary>
        protected DataEditorAttribute Attribute { get; }

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
        public string Icon { get; }

        /// <inheritdoc />
        [JsonProperty("group")]
        public string Group { get; }

        /// <inheritdoc />
        [JsonIgnore]
        public bool IsDeprecated { get; }

        /// <inheritdoc />
        [JsonProperty("editor")]
        public IDataValueEditor ValueEditor
        {
            get => _valueEditor ?? (_valueEditor = CreateValueEditor());
            set
            {
                _valueEditorAssigned = value;
                _valueEditor = null;
            }
        }

        /// <inheritdoc />
        [JsonProperty("config")]
        public IDictionary<string, object> DefaultConfiguration { get; set;  }

        /// <summary>
        /// Creates a value editor instance.
        /// </summary>
        /// <returns></returns>
        protected virtual IDataValueEditor CreateValueEditor()
        {
            // handle assigned editor
            if (_valueEditorAssigned != null)
                return _valueEditorAssigned;

            // create a new editor
            var editor = new DataValueEditor();

            var view = Attribute?.View;
            if (string.IsNullOrWhiteSpace(view))
                throw new InvalidOperationException("The editor does not specify a view.");
            editor.View = view;

            return editor;
        }
    }
}
