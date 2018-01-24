using System;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Marks a class that represents a value editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ValueEditorAttribute : DataEditorAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueEditorAttribute"/> class.
        /// </summary>
        /// <param name="alias">The unique identifier of the editor.</param>
        /// <param name="name">The friendly name of the editor.</param>
        public ValueEditorAttribute(string alias, string name)
            : this(alias, name, NullView)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueEditorAttribute"/> class.
        /// </summary>
        /// <param name="alias">The unique identifier of the editor.</param>
        /// <param name="name">The friendly name of the editor.</param>
        /// <param name="view">The view to use to render the editor.</param>
        public ValueEditorAttribute(string alias, string name, string view)
            : base(alias, name, view)
        {
            // defaults
            ValueType = ValueTypes.String;
            Icon = Constants.Icons.PropertyEditor;
            Group = "common";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueEditorAttribute"/> class.
        /// </summary>
        /// <param name="alias">The unique identifier of the editor.</param>
        /// <param name="name">The friendly name of the editor.</param>
        /// <param name="view">The view to use to render the editor.</param>
        /// <param name="valueType">The type of the edited value.</param>
        /// <remarks>The <paramref name="valueType"/> must be a valid <see cref="ValueTypes"/> value.</remarks>
        public ValueEditorAttribute(string alias, string name, string view, string valueType)
            : this(alias, name, view)
        {
            if (string.IsNullOrWhiteSpace(valueType)) throw new ArgumentNullOrEmptyException(nameof(valueType));
            if (!ValueTypes.IsValue(valueType)) throw new ArgumentOutOfRangeException(nameof(valueType), "Not a valid ValueTypes.");
            ValueType = valueType;
        }

        /// <summary>
        /// Gets or sets the type of the edited value.
        /// </summary>
        /// <remarks>Must be a valid <see cref="ValueTypes"/> value.</remarks>
        public string ValueType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the editor type.
        /// </summary>
        public EditorType EditorType { get; set; } // fixme should be the attribute 1st ctor parameter?

        public bool IsPropertyValueEditor => (EditorType & EditorType.PropertyValue) != 0;

        /// <summary>
        /// Gets or sets a value indicating whether the editor is a macro parameter editor.
        /// </summary>
        public bool IsMacroParameterEditor { get; set; } // => (EditorType & EditorType.MacroParameter) != 0;

        /// <summary>
        /// If set to true, this property editor will not show up in the DataType's drop down list
        /// if there is not already one of them chosen for a DataType
        /// </summary>
        public bool IsDeprecated { get; set; } // fixme should just kill in v8

        /// <summary>
        /// Gets or sets a value indicating whether the editor should be displayed without its label.
        /// </summary>
        public bool HideLabel { get; set; }

        /// <summary>
        /// Gets or sets an optional icon.
        /// </summary>
        /// <remarks>The icon can be used for example when presenting datatypes based upon the editor.</remarks>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets an optional group.
        /// </summary>
        /// <remarks>The group can be used for example to group the editors by category.</remarks>
        public string Group { get; set; }
    }
}
