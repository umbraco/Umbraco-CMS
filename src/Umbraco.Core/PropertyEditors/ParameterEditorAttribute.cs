using System;
using Umbraco.Core.Exceptions;

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
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullOrEmptyException(nameof(alias));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));
            if (string.IsNullOrWhiteSpace(editorView)) throw new ArgumentNullOrEmptyException(nameof(editorView));

            Alias = alias;
            Name = name;
            EditorView = editorView;
        }

        public ParameterEditorAttribute(string alias, string name)
        {
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullOrEmptyException(nameof(alias));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));

            Alias = alias;
            Name = name;
        }

        public string Alias { get; }
        public string Name { get; }
        public string EditorView { get; }
    }
}
