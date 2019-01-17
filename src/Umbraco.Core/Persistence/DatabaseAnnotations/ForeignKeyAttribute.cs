using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    /// <summary>
    /// Attribute that represents a Foreign Key reference
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ForeignKeyAttribute : ReferencesAttribute
    {
        public ForeignKeyAttribute(Type type) : base(type)
        {
        }

        internal string OnDelete { get; set; }
        internal string OnUpdate { get; set; }

        /// <summary>
        /// Gets or sets the name of the foreign key reference
        /// </summary>
        /// <remarks>
        /// Overrides the default naming of a foreign key reference:
        /// FK_thisTableName_refTableName
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the Column that this foreign key should reference.
        /// </summary>
        /// <remarks>PrimaryKey column is used by default</remarks>
        public string Column { get; set; }
    }
}
