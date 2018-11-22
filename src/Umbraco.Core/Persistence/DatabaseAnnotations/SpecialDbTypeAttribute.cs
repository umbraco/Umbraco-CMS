using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    /// <summary>
    /// Attribute that represents the usage of a special type
    /// </summary>
    /// <remarks>
    /// Should only be used when the .NET type can't be directly translated to a DbType.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class SpecialDbTypeAttribute : Attribute
    {
        public SpecialDbTypeAttribute(SpecialDbTypes databaseType)
        {
            DatabaseType = databaseType;
        }

        /// <summary>
        /// Gets or sets the <see cref="SpecialDbTypes"/> for this column
        /// </summary>
        public SpecialDbTypes DatabaseType { get; private set; }
    }
}
