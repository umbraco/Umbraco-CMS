using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// An attribute used to define all of the basic properties of a parameter editor
    /// on the server side.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ParameterEditorAttribute : Attribute
    {
        public ParameterEditorAttribute(string alias, string name, string editorView)
        {
            Mandate.ParameterNotNullOrEmpty(alias, "alias");
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Mandate.ParameterNotNullOrEmpty(editorView, "editorView");

            Alias = alias;
            Name = name;
            EditorView = editorView;
        }

        public ParameterEditorAttribute(string alias, string name)
        {
            Mandate.ParameterNotNullOrEmpty(alias, "id");
            Mandate.ParameterNotNullOrEmpty(name, "name");

            Alias = alias;
            Name = name;
        }


        public string Alias { get; private set; }
        public string Name { get; private set; }
        public string EditorView { get; private set; }        
    }
}