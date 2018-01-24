using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Marks a class that represents a data editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)] // fixme allow multiple?!
    public sealed class ParameterEditorAttribute : DataEditorAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterEditorAttribute"/> class.
        /// </summary>
        /// <param name="alias">The unique identifier of the editor.</param>
        /// <param name="name">The friendly name of the editor.</param>
        public ParameterEditorAttribute(string alias, string name)
            : this(alias, name, NullView)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterEditorAttribute"/> class.
        /// </summary>
        /// <param name="alias">The unique identifier of the editor.</param>
        /// <param name="name">The friendly name of the editor.</param>
        /// <param name="view">The view to use to render the editor.</param>
        public ParameterEditorAttribute(string alias, string name, string view)
            : base(alias, name, view)
        { }
    }
}
