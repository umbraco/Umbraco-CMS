using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// An attribute used to define all of the basic properties of a property editor
    /// on the server side.
    /// </summary>
    public sealed class PropertyEditorAttribute : Attribute
    {
        public PropertyEditorAttribute(string id, string name, string editorView)
        {
            Mandate.ParameterNotNullOrEmpty(id, "id");
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Mandate.ParameterNotNullOrEmpty(editorView, "editorView");

            Id = id;
            Name = name;
            EditorView = editorView;

            //defaults
            ValueType = "string";
        }

        public PropertyEditorAttribute(string id, string name)
        {
            Mandate.ParameterNotNullOrEmpty(id, "id");
            Mandate.ParameterNotNullOrEmpty(name, "name");

            Id = id;
            Name = name;

            //defaults
            ValueType = "string";
        }

        public PropertyEditorAttribute(string id, string name, string valueType, string editorView)
        {
            Mandate.ParameterNotNullOrEmpty(id, "id");
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Mandate.ParameterNotNullOrEmpty(valueType, "valueType");
            Mandate.ParameterNotNullOrEmpty(editorView, "editorView");

            Id = id;
            Name = name;
            ValueType = valueType;
            EditorView = editorView;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string EditorView { get; private set; }
        public string ValueType { get; set; }
    }
}