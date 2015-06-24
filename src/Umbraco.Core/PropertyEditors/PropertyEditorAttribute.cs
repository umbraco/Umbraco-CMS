using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// An attribute used to define all of the basic properties of a property editor
    /// on the server side.
    /// </summary>
    public sealed class PropertyEditorAttribute : Attribute
    {
        public PropertyEditorAttribute(string alias, string name, string editorView)
        {
            Mandate.ParameterNotNullOrEmpty(alias, "alias");
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Mandate.ParameterNotNullOrEmpty(editorView, "editorView");

            Alias = alias;
            Name = name;
            EditorView = editorView;

            //defaults
            ValueType = "string";
            Icon = Constants.Icons.PropertyEditor;
            Group = "common";
        }

        public PropertyEditorAttribute(string alias, string name)
        {
            Mandate.ParameterNotNullOrEmpty(alias, "id");
            Mandate.ParameterNotNullOrEmpty(name, "name");

            Alias = alias;
            Name = name;

            //defaults
            ValueType = "string";
            Icon = Constants.Icons.PropertyEditor;
            Group = "common";
        }

        public PropertyEditorAttribute(string alias, string name, string valueType, string editorView)
        {
            Mandate.ParameterNotNullOrEmpty(alias, "alias");
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Mandate.ParameterNotNullOrEmpty(valueType, "valueType");
            Mandate.ParameterNotNullOrEmpty(editorView, "editorView");

            Alias = alias;
            Name = name;
            ValueType = valueType;
            EditorView = editorView;

            Icon = Constants.Icons.PropertyEditor;
            Group = "common";
        }

        public string Alias { get; private set; }
        public string Name { get; private set; }
        public string EditorView { get; private set; }
        public string ValueType { get; set; }
        public bool IsParameterEditor { get; set; }

        /// <summary>
        /// If this is is true than the editor will be displayed full width without a label
        /// </summary>
        public bool HideLabel { get; set; }

        /// <summary>
        /// Optional, If this is set, datatypes using the editor will display this icon instead of the default system one.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Optional - if this is set, the datatype ui will display the editor in this group instead of the default one, by default an editor does not have a group.
        /// The group has no effect on how a property editor is stored or referenced.
        /// </summary>
        public string Group { get; set; }
    }
}