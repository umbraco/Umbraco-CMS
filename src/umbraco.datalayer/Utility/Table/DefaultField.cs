using System;

namespace umbraco.DataLayer.Utility.Table
{
    /// <summary>
    /// Default implementation of the <see cref="IField"/> interface.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public class DefaultField : IField
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultField"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="size">The size of the field.</param>
        /// <param name="properties">The properties.</param>
        public DefaultField(string name, Type dataType, int size, FieldProperties properties)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (dataType == null)
                throw new ArgumentNullException("dataType");

            Name = name;
            DataType = dataType;
            Properties = properties;
            Size = size;
        }

        #endregion

        #region IField Members

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Gets the data type of the field.
        /// </summary>
        /// <value>The field data type.</value>
        public virtual Type DataType { get; protected set; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public virtual FieldProperties Properties { get; protected set; }

        /// <summary>
        /// Gets a value indicating the field size.
        /// </summary>
        /// <value>The size.</value>
        public virtual int Size { get; protected set; }

        /// <summary>
        /// Determines whether the field has the specified property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if the field has the property; otherwise, <c>false</c>.</returns>
        public virtual bool HasProperty(FieldProperties property)
        {
            return (Properties & property) == property;
        }

        #endregion

        #region object Members

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0} ({1})", Name, DataType.FullName);
        }

        #endregion
    }
}
