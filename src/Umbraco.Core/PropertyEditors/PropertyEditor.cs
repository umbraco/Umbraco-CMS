using System;
using System.Linq;
using Umbraco.Core.PropertyEditors.Attributes;

namespace Umbraco.Core.PropertyEditors
{
    internal abstract class PropertyEditor
    {
        /// <summary>
        /// Constructor for a PropertyEditor
        /// </summary>
        protected PropertyEditor()
        {
            //Locate the metadata attribute
            var docTypeAttributes = GetType()
                .GetCustomAttributes(typeof(PropertyEditorAttribute), true)
                .OfType<PropertyEditorAttribute>();

            if (!docTypeAttributes.Any())
                throw new InvalidOperationException(
                    string.Format("The PropertyEditor of type {0} is missing the {1} attribute", GetType().FullName,
                                  typeof(PropertyEditorAttribute).FullName));

            //assign the properties of this object to those of the metadata attribute
            var attr = docTypeAttributes.First();
            Id = attr.Id;
            Name = attr.Name;
            Alias = attr.Alias;
        }

        public virtual Guid Id { get; protected set; }

        public virtual string Name { get; protected set; }

        public virtual string Alias { get; protected set; }
    }
}