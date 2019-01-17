using System;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Marks a class that represents a data editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataEditorAttribute : Attribute
    {
        private string _valueType = ValueTypes.String;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEditorAttribute"/> class for a property editor.
        /// </summary>
        /// <param name="alias">The unique identifier of the editor.</param>
        /// <param name="name">The friendly name of the editor.</param>
        public DataEditorAttribute(string alias, string name)
            : this(alias, EditorType.PropertyValue, name, NullView)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEditorAttribute"/> class for a property editor.
        /// </summary>
        /// <param name="alias">The unique identifier of the editor.</param>
        /// <param name="name">The friendly name of the editor.</param>
        /// <param name="view">The view to use to render the editor.</param>
        public DataEditorAttribute(string alias, string name, string view)
            : this(alias, EditorType.PropertyValue, name, view)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEditorAttribute"/> class.
        /// </summary>
        /// <param name="alias">The unique identifier of the editor.</param>
        /// <param name="type">The type of the editor.</param>
        /// <param name="name">The friendly name of the editor.</param>
        public DataEditorAttribute(string alias, EditorType type, string name)
            : this(alias, type, name, NullView)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEditorAttribute"/> class.
        /// </summary>
        /// <param name="alias">The unique identifier of the editor.</param>
        /// <param name="type">The type of the editor.</param>
        /// <param name="name">The friendly name of the editor.</param>
        /// <param name="view">The view to use to render the editor.</param>
        /// <remarks>
        /// <para>Set <paramref name="view"/> to <see cref="NullView"/> to explicitly set the view to null.</para>
        /// <para>Otherwise, <paramref name="view"/> cannot be null nor empty.</para>
        /// </remarks>
        public DataEditorAttribute(string alias, EditorType type, string name, string view)
        {
            if ((type & ~(EditorType.PropertyValue | EditorType.MacroParameter)) > 0)
                throw new ArgumentOutOfRangeException(nameof(type), $"Not a valid {typeof(EditorType)} value.");
            Type = type;

            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullOrEmptyException(nameof(alias));
            Alias = alias;

            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));
            Name = name;

            if (string.IsNullOrWhiteSpace(view)) throw new ArgumentNullOrEmptyException(nameof(view));
            View = view == NullView ? null : view;
        }

        /// <summary>
        /// Gets a special value indicating that the view should be null.
        /// </summary>
        public const string NullView = "EXPLICITELY-SET-VIEW-TO-NULL-2B5B0B73D3DD47B28DDB84E02C349DFB"; // just a random string

        /// <summary>
        /// Gets the unique alias of the editor.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Gets the type of the editor.
        /// </summary>
        public EditorType Type { get; }

        /// <summary>
        /// Gets the friendly name of the editor.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the view to use to render the editor.
        /// </summary>
        public string View { get; }

        /// <summary>
        /// Gets or sets the type of the edited value.
        /// </summary>
        /// <remarks>Must be a valid <see cref="ValueTypes"/> value.</remarks>
        public string ValueType {
            get => _valueType;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullOrEmptyException(nameof(value));
                if (!ValueTypes.IsValue(value)) throw new ArgumentOutOfRangeException(nameof(value), $"Not a valid {typeof(ValueTypes)} value.");
                _valueType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the editor should be displayed without its label.
        /// </summary>
        public bool HideLabel { get; set; }

        /// <summary>
        /// Gets or sets an optional icon.
        /// </summary>
        /// <remarks>The icon can be used for example when presenting datatypes based upon the editor.</remarks>
        public string Icon { get; set; } = Constants.Icons.PropertyEditor;

        /// <summary>
        /// Gets or sets an optional group.
        /// </summary>
        /// <remarks>The group can be used for example to group the editors by category.</remarks>
        public string Group { get; set; } = "common";

        /// <summary>
        /// Gets or sets a value indicating whether the value editor is deprecated.
        /// </summary>
        /// <remarks>A deprecated editor is still supported but not proposed in the UI.</remarks>
        public bool IsDeprecated { get; set; }
    }
}
