using System;
using System.Collections.Generic;

namespace umbraco.DataLayer.Utility.Table
{
    /// <summary>
    /// Interface for classes that represent a data source table.
    /// </summary>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public interface ITable : IEnumerable<IField>
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        string Name { get; }

        /// <summary>
        /// Adds the field to the table.
        /// </summary>
        /// <param name="field">The field.</param>
        void AddField(IField field);

        /// <summary>
        /// Creates a new field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">The field data type.</param>
        /// <returns>A new field.</returns>
        IField CreateField(string name, Type dataType);

        /// <summary>
        /// Creates a new field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">The field data type.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>A new field.</returns>
        IField CreateField(string name, Type dataType, FieldProperties properties);

        /// <summary>
        /// Creates a new field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">The field data type.</param>
        /// <param name="size">The size.</param>
        /// <returns>A new field.</returns>
        IField CreateField(string name, Type dataType, int size);

        /// <summary>
        /// Creates a new field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">The field data type.</param>
        /// <param name="size">The size.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>A new field.</returns>
        IField CreateField(string name, Type dataType, int size, FieldProperties properties);

        /// <summary>
        /// Finds the field with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The field, or <c>null</c> if a field with the specified name doesn't exist.</returns>
        IField FindField(string name);

        /// <summary>
        /// Finds the first field satisfiying the matcher.
        /// </summary>
        /// <param name="matcher">The matcher.</param>
        /// <returns>The first field found, or <c>null</c> if no field matches.</returns>
        IField FindField(Predicate<IField> matcher);

        /// <summary>
        /// Finds all fields satisfiying the matcher.
        /// </summary>
        /// <param name="matcher">The matcher.</param>
        /// <returns>A list of all matching fields.</returns>
        IList<IField> FindFields(Predicate<IField> matcher);
    }
}
