namespace Umbraco.Core.PropertyEditors.ValueCorrectors
{
    using System;

    /// <summary>
    /// An attribute to decorate a <see cref="PropertyValueConverterBase"/> to associated a correction class during resolution.
    /// </summary>
    internal class DbPropertyValueCorrectionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbPropertyValueCorrectionAttribute"/> class.
        /// </summary>
        /// <param name="propertyEditorAlias">
        /// The property editor alias.
        /// </param>
        public DbPropertyValueCorrectionAttribute(string propertyEditorAlias)
        {
            this.PropertyEditorAlias = propertyEditorAlias;
        }

        /// <summary>
        /// Gets the property editor alias.
        /// </summary>
        public string PropertyEditorAlias { get; private set; }
    }
}