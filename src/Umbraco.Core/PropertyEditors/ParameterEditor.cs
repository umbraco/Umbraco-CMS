using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Basic definition of a macro parameter editor
    /// </summary>
    public class ParameterEditor : IParameterEditor
    {
        private readonly ParameterEditorAttribute _attribute;

        private ParameterValueEditor _valueEditor;
        private ParameterValueEditor _valueEditorAssigned;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public ParameterEditor()
        {
            Configuration = new Dictionary<string, object>();

            // fixme ParameterEditorAttribute is AllowMultiple
            // then how can this ever make sense?
            // only DropDownMultiplePropertyEditor has multiple [ParameterEditor]
            // is exactly the same in v7 now
            // makes no sense at all?!

            // assign properties based on the attribute, if it is found
            _attribute = GetType().GetCustomAttribute<ParameterEditorAttribute>(false);
            if (_attribute == null) return;

            Alias = _attribute.Alias;
            Name = _attribute.Name;
        }

        /// <summary>
        /// The id  of the property editor
        /// </summary>
        [JsonProperty("alias", Required = Required.Always)]
        public string Alias { get; internal set; }

        /// <summary>
        /// The name of the property editor
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; internal set; }

        /// <summary>
        /// Allows a parameter editor to be re-used based on the configuration specified.
        /// </summary>
        [JsonProperty("config")]
        public IDictionary<string, object> Configuration { get; set; }

        [JsonProperty("editor")]
        public ParameterValueEditor ValueEditor
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

        /// <summary>
        /// Creates a value editor instance
        /// </summary>
        /// <returns></returns>
        protected virtual ParameterValueEditor CreateValueEditor()
        {
            // handle assigned editor
            if (_valueEditorAssigned != null)
                return _valueEditorAssigned;

            // create a new editor
            var editor = new ParameterValueEditor();

            var view = _attribute?.View;
            if (string.IsNullOrWhiteSpace(view))
                throw new InvalidOperationException("The editor does not specify a view.");
            editor.View = view;

            return editor;
        }
    }
}
