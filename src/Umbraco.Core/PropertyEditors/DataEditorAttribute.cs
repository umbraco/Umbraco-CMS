using System;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Marks a class that represents a data editor.
    /// </summary>
    public abstract class DataEditorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterEditorAttribute"/> class.
        /// </summary>
        /// <param name="alias">The unique identifier of the editor.</param>
        /// <param name="name">The friendly name of the editor.</param>
        /// <param name="view">The view to use to render the editor.</param>
        /// <remarks>
        /// <para>Set <paramref name="view"/> to <see cref="NullView"/> to explicitely set the view to null.</para>
        /// <para>Otherwise, <paramref name="view"/> cannot be null nor empty.</para>
        /// </remarks>
        protected DataEditorAttribute(string alias, string name, string view)
        {
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
        protected static string NullView = "EXPLICITELY-SET-VIEW-TO-NULL-2B5B0B73D3DD47B28DDB84E02C349DFB"; // just a random string

        /// <summary>
        /// Gets the unique alias of the editor.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Gets the friendly name of the editor.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the view to use to render the editor.
        /// </summary>
        public string View { get; }
    }
}