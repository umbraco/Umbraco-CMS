using System;

namespace Umbraco.Core.PropertyEditors.Attributes
{
    /// <summary>
    /// Defines a PropertyEditor
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class PropertyEditorAttribute : Attribute
    {
        public PropertyEditorAttribute(string id, string alias, string name)
        {
            Mandate.ParameterNotNullOrEmpty(id, "id");
            Mandate.ParameterNotNullOrEmpty(alias, "alias");
            Mandate.ParameterNotNullOrEmpty(name, "name");
            
            Id = Guid.Parse(id);
            Alias = alias;
            Name = name;

            IsContentPropertyEditor = true;
        }

        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; set; }

        /// <summary>
        /// Flag determining if this property editor is used to edit content
        /// </summary>
        public bool IsContentPropertyEditor { get; set; }

        /// <summary>
        /// Flag determining if this property editor is used to edit parameters
        /// </summary>
        public bool IsParameterEditor { get; set; }
    }
}