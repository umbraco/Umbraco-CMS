using System;
using System.Collections;
using System.Collections.Generic;

namespace umbraco.DataLayer.Utility.Table
{
    /// <summary>
    /// Default implementation of the <see cref="DefaultTable"/> interface.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public class DefaultTable : ITable
    {
        #region Private Fields

        /// <summary>List of table fields.</summary>
        private List<IField> m_Fields;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTable"/> class.
        /// </summary>
        /// <param name="name">The table name.</param>
        public DefaultTable(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Name = name;
            m_Fields = new List<IField>();
        }

        #endregion

        #region ITable Members

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Adds the field to the table.
        /// </summary>
        /// <param name="field">The field.</param>
        public virtual void AddField(IField field)
        {
            if (field == null)
                throw new ArgumentNullException("field");
            if (FindField(field.Name) != null)
                throw new ArgumentException(String.Format("A field named '{0}' already exists in table '{1}'.",
                                                          field.Name, this.Name));
            m_Fields.Add(field);
        }

        /// <summary>
        /// Creates a new field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">The field data type.</param>
        /// <returns>A new field.</returns>
        public virtual IField CreateField(string name, Type dataType)
        {
            return CreateField(name, dataType, 0);
        }

        /// <summary>
        /// Creates a new field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">The field data type.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>A new field.</returns>
        public virtual IField CreateField(string name, Type dataType, FieldProperties properties)
        {
            return CreateField(name, dataType, 0, properties);
        }

        /// <summary>
        /// Creates a new field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">The field data type.</param>
        /// <param name="size">The size.</param>
        /// <returns>A new field.</returns>
        public virtual IField CreateField(string name, Type dataType, int size)
        {
            return CreateField(name, dataType, size, FieldProperties.None);
        }

        /// <summary>
        /// Creates a new field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">The field data type.</param>
        /// <param name="size">The size.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>A new field.</returns>
        public virtual IField CreateField(string name, Type dataType, int size, FieldProperties properties)
        {
            return new DefaultField(name, dataType, size, properties);
        }

        /// <summary>
        /// Finds the field with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The field, or <c>null</c> if a field with the specified name doesn't exist.</returns>
        public IField FindField(string name)
        {
            return FindField(f => f.Name == name);
        }

        /// <summary>
        /// Finds the first field satisfiying the matcher.
        /// </summary>
        /// <param name="matcher">The matcher.</param>
        /// <returns>
        /// The first field found, or <c>null</c> if no field matches.
        /// </returns>
        public IField FindField(Predicate<IField> matcher)
        {
            foreach (IField field in this)
                if (matcher(field))
                    return field;
            return null;
        }

        /// <summary>
        /// Finds all fields satisfiying the matcher.
        /// </summary>
        /// <param name="matcher">The matcher.</param>
        /// <returns>A list of all matching fields.</returns>
        public IList<IField> FindFields(Predicate<IField> matcher)
        {
            List<IField> matchingFields = new List<IField>();
            foreach (IField field in this)
                if (matcher(field))
                    matchingFields.Add(field);
            return matchingFields;
        }

        #endregion

        #region IEnumerable<IField> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<IField> GetEnumerator()
        {
            return m_Fields.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            return String.Format("{0} ({1} fields)", Name, m_Fields.Count);
        }

        #endregion
    }
}


